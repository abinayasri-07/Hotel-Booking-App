using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CozyHavenStay.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<int, Booking> _bookingRepository;
        private readonly IRepository<int, Room> _roomRepository;
        private readonly IRepository<int, BookingRoom> _bookingRoomRepository;
        private readonly IMapper _mapper;

        public BookingService(IRepository<int, Booking> bookingRepository,
                              IRepository<int, Room> roomRepository,
                              IRepository<int, BookingRoom> bookingRoomRepository,
                              IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _bookingRoomRepository = bookingRoomRepository;
            _mapper = mapper;
        }

        // Add a new booking
        public async Task<CreateBookingResponse> AddBooking(CreateBookingRequest request)
        {
            // Check for booking conflict first
            var conflict = await CheckBookingConflict(new BookingConflictCheckRequest
            {
                RoomIds = request.RoomIds,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate
            });

            if (conflict.HasConflict)
                throw new Exception("Booking dates conflict with an existing booking.");

            // Fetch rooms and check availability
            var rooms = new List<Room>();
            foreach (var roomId in request.RoomIds)
            {
                var room = await _roomRepository.GetById(roomId);
                if (room == null || !room.IsAvailable)
                    throw new Exception($"Room {roomId} not available");

                rooms.Add(room);
            }

            // Create the booking
            var booking = _mapper.Map<Booking>(request);
            booking.Status = "Confirmed";
            booking.CreatedAt = DateTime.UtcNow;

            // Add the booking
            var result = await _bookingRepository.Add(booking);

            // Create BookingRoom entries for each room
            foreach (var room in rooms)
            {
                var bookingRoom = new BookingRoom
                {
                    BookingId = result.BookingId,
                    RoomId = room.RoomId
                };

                // Add BookingRoom entries
                await _bookingRoomRepository.Add(bookingRoom);

                // Mark the room as unavailable
                room.IsAvailable = false;
                await _roomRepository.Update(room.RoomId, room);
            }

            return new CreateBookingResponse
            {
                BookingId = result.BookingId,
                Message = "Booking confirmed"
            };
        }

        // Get a booking by ID
        public async Task<BookingResponse?> GetBookingById(int bookingId)
        {
            var booking = await _bookingRepository.GetById(bookingId);
            if (booking == null) return null;

            var dto = _mapper.Map<BookingResponse>(booking);

            // Get the room details
            var bookingRooms = booking.BookingRooms?.ToList();
            if (bookingRooms != null && bookingRooms.Any())
            {
                var room = await _roomRepository.GetById(bookingRooms.First().RoomId);
                dto.Type = room?.Type ?? string.Empty;
                dto.HotelName = room?.Hotel?.HotelName ?? string.Empty;
            }

            return dto;
        }

        // Get all bookings
        public async Task<IEnumerable<BookingDTO>> GetAllBookings()
        {
            var allBookings = await _bookingRepository.GetAll();
            return _mapper.Map<IEnumerable<BookingDTO>>(allBookings);
        }

        // Get bookings by customer ID
        public async Task<IEnumerable<BookingDTO>> GetBookingsByCustomer(int customerId)
        {
            var all = await _bookingRepository.GetAll();
            var filtered = all.Where(b => b.CustomerId == customerId);
            return _mapper.Map<IEnumerable<BookingDTO>>(filtered);
        }

        // Get bookings by hotel ID
        public async Task<IEnumerable<BookingDTO>> GetBookingsByHotel(int hotelId)
        {
            var all = await _bookingRepository.GetAll();
            var filtered = all.Where(b => b.BookingRooms != null &&
                                           b.BookingRooms.Any(br => br.Room?.HotelId == hotelId));
            return _mapper.Map<IEnumerable<BookingDTO>>(filtered);
        }

        // Cancel a booking
        public async Task<CancelBookingResponse> CancelBooking(CancelBookingRequest request)
        {
            var booking = await _bookingRepository.GetById(request.BookingId);
            if (booking == null)
                throw new Exception("Booking not found");

            if (booking.Status == "Cancelled")
                return new CancelBookingResponse
                {
                    BookingId = booking.BookingId,
                    Status = "Cancelled",
                    Message = "Booking is already cancelled"
                };

            booking.Status = "Cancelled";
            await _bookingRepository.Update(booking.BookingId, booking);

            // Revert room availability
            foreach (var bookingRoom in booking.BookingRooms ?? new List<BookingRoom>())
            {
                var room = await _roomRepository.GetById(bookingRoom.RoomId);
                if (room != null)
                {
                    room.IsAvailable = true;
                    await _roomRepository.Update(room.RoomId, room);
                }
            }

            return new CancelBookingResponse
            {
                BookingId = booking.BookingId,
                Status = "Cancelled",
                Message = "Booking has been cancelled successfully"
            };
        }

        // Check if there is a booking conflict
        public async Task<BookingConflictCheckResponse> CheckBookingConflict(BookingConflictCheckRequest request)
        {
            var bookings = await _bookingRepository.GetAll();

            var hasConflict = bookings.Any(b =>
                b.BookingRooms?.Any(br => br.RoomId == request.RoomIds.First()) ?? false &&
                b.Status != "Cancelled" &&
                request.CheckInDate < b.CheckOutDate &&
                request.CheckOutDate > b.CheckInDate
            );

            return new BookingConflictCheckResponse
            {
                HasConflict = hasConflict,
                Message = hasConflict ? "Conflicting booking exists" : "Room is available"
            };
        }

        // Get bookings by filter
        public async Task<IEnumerable<BookingDTO>> GetBookingsByFilter(BookingRequest request)
        {
            var bookings = (await _bookingRepository.GetAll()).ToList();

            if (!bookings.Any())
                throw new Exception("No bookings found");

            if (request.Filters != null)
                bookings = ApplyFilters(request.Filters, bookings);

            if (request.SortBy != null)
                bookings = ApplySort(request.SortBy.Value, bookings);

            if (request.Pagination != null)
                bookings = ApplyPagination(request.Pagination, bookings);

            return _mapper.Map<IEnumerable<BookingDTO>>(bookings);
        }

        // Apply filters to bookings
        private List<Booking> ApplyFilters(BookingFilter filters, List<Booking> bookings)
        {
            if (!string.IsNullOrEmpty(filters.Status))
                bookings = bookings.Where(b => b.Status.ToLower() == filters.Status.ToLower()).ToList();

            if (filters.FromDate.HasValue)
                bookings = bookings.Where(b => b.CheckInDate >= filters.FromDate.Value).ToList();

            if (filters.ToDate.HasValue)
                bookings = bookings.Where(b => b.CheckOutDate <= filters.ToDate.Value).ToList();

            if (filters.CustomerId.HasValue)
                bookings = bookings.Where(b => b.CustomerId == filters.CustomerId.Value).ToList();

            if (filters.HotelId.HasValue)
                bookings = bookings.Where(b => b.BookingRooms != null &&
                                                b.BookingRooms.Any(br => br.Room?.HotelId == filters.HotelId.Value)).ToList();

            return bookings;
        }

        // Apply sorting to bookings
        private List<Booking> ApplySort(int sortBy, List<Booking> bookings)
        {
            return sortBy switch
            {
                1 => bookings.OrderBy(b => b.CheckInDate).ToList(),
                -1 => bookings.OrderByDescending(b => b.CheckInDate).ToList(),
                2 => bookings.OrderBy(b => b.Status).ToList(),
                -2 => bookings.OrderByDescending(b => b.Status).ToList(),
                3 => bookings.OrderBy(b => b.BookingId).ToList(),
                _ => bookings
            };
        }

        // Apply pagination to bookings
        private List<Booking> ApplyPagination(Pagination pagination, List<Booking> bookings)
        {
            return bookings
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();
        }

        // Update room availability for expired bookings
        public async Task<int> UpdateRoomAvailabilityAsync()
        {
            var now = DateTime.UtcNow;

            var bookings = await _bookingRepository.GetAll();
            var expiredBookings = bookings
                .Where(b => b.Status == "Confirmed" && b.CheckOutDate <= now && b.BookingRooms != null)
                .ToList();

            foreach (var booking in expiredBookings)
            {
                foreach (var bookingRoom in booking.BookingRooms ?? new List<BookingRoom>())
                {
                    var room = await _roomRepository.GetById(bookingRoom.RoomId);
                    if (room != null)
                    {
                        room.IsAvailable = true;
                        await _roomRepository.Update(room.RoomId, room);
                    }
                }

                booking.Status = "Completed";
                await _bookingRepository.Update(booking.BookingId, booking);
            }

            return expiredBookings.Count;
        }
    }
}