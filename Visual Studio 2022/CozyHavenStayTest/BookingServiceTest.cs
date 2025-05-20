using Moq;
using NUnit.Framework;
using CozyHavenStay.Services;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CozyHavenStay.Tests
{
    [TestFixture]
    public class BookingServiceTest
    {
        private Mock<IRepository<int, Booking>> _mockBookingRepository;
        private Mock<IRepository<int, Room>> _mockRoomRepository;
        private Mock<IRepository<int, BookingRoom>> _mockBookingRoomRepository;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<BookingService>> _mockLogger;
        private BookingService _bookingService;

        [SetUp]
        public void Setup()
        {
            _mockBookingRepository = new Mock<IRepository<int, Booking>>();
            _mockRoomRepository = new Mock<IRepository<int, Room>>();
            _mockBookingRoomRepository = new Mock<IRepository<int, BookingRoom>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<BookingService>>();

            _bookingService = new BookingService(
                _mockBookingRepository.Object,
                _mockRoomRepository.Object,
                _mockBookingRoomRepository.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public void AddBooking_ShouldThrowException_WhenBookingConflictOccurs()
        {
            // Arrange
            var request = new CreateBookingRequest
            {
                RoomIds = new List<int> { 1 },
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(3)
            };

            var existingBooking = new Booking
            {
                BookingId = 10,
                CheckInDate = DateTime.UtcNow.AddDays(2),
                CheckOutDate = DateTime.UtcNow.AddDays(4),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomId = 1 }
                }
            };

            _mockBookingRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<Booking> { existingBooking });

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _bookingService.AddBooking(request));
            Assert.That(ex.Message, Does.Contain("Booking dates conflict with an existing booking."));
        }

        [Test]
        public async Task AddBooking_ShouldReturnBookingConfirmed_WhenBookingIsValid()
        {
            // Arrange
            var request = new CreateBookingRequest
            {
                RoomIds = new List<int> { 1 },
                CheckInDate = DateTime.UtcNow.AddDays(5),
                CheckOutDate = DateTime.UtcNow.AddDays(7)
            };

            _mockBookingRepository.Setup(x => x.GetAll()).ReturnsAsync(new List<Booking>());

            var room = new Room { RoomId = 1, IsAvailable = true };
            _mockRoomRepository.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(room);

            var booking = new Booking { BookingId = 1 };
            _mockBookingRepository.Setup(x => x.Add(It.IsAny<Booking>())).ReturnsAsync(booking);
            _mockBookingRoomRepository.Setup(x => x.Add(It.IsAny<BookingRoom>())).ReturnsAsync(new BookingRoom());

            _mockMapper.Setup(x => x.Map<Booking>(It.IsAny<CreateBookingRequest>())).Returns(booking);

            // Act
            var result = await _bookingService.AddBooking(request);

            // Assert
            Assert.AreEqual("Booking confirmed", result.Message);
            Assert.AreEqual(1, result.BookingId);
        }

        [Test]
        public async Task GetBookingById_ShouldReturnBooking_WhenBookingExists()
        {
            var booking = new Booking
            {
                BookingId = 1,
                Status = "Confirmed"
            };

            var response = new BookingResponse
            {
                BookingId = 1,
                Status = "Confirmed"
            };

            _mockBookingRepository.Setup(x => x.GetById(1)).ReturnsAsync(booking);
            _mockMapper.Setup(x => x.Map<BookingResponse>(booking)).Returns(response);

            var result = await _bookingService.GetBookingById(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.BookingId);
        }

        [Test]
        public async Task GetBookingsByCustomer_ShouldReturnBookingList()
        {
            var bookings = new List<Booking>
            {
                new Booking { BookingId = 1, CustomerId = 1 },
                new Booking { BookingId = 2, CustomerId = 1 }
            };

            var bookingDTOs = new List<BookingDTO>
            {
                new BookingDTO { BookingId = 1 },
                new BookingDTO { BookingId = 2 }
            };

            _mockBookingRepository.Setup(x => x.GetAll()).ReturnsAsync(bookings);
            _mockMapper.Setup(x => x.Map<IEnumerable<BookingDTO>>(It.IsAny<IEnumerable<Booking>>())).Returns(bookingDTOs);

            var result = await _bookingService.GetBookingsByCustomer(1);

            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task CancelBooking_ShouldReturnSuccess_WhenBookingCancelled()
        {
            var request = new CancelBookingRequest { BookingId = 1 };
            var booking = new Booking
            {
                BookingId = 1,
                Status = "Confirmed",
                BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 1 } }
            };

            var room = new Room { RoomId = 1, IsAvailable = false };

            _mockBookingRepository.Setup(x => x.GetById(1)).ReturnsAsync(booking);
            _mockRoomRepository.Setup(x => x.GetById(1)).ReturnsAsync(room);
            _mockBookingRepository.Setup(x => x.Update(1, It.IsAny<Booking>())).ReturnsAsync(booking);
            _mockRoomRepository.Setup(x => x.Update(1, It.IsAny<Room>())).ReturnsAsync(room);

            var result = await _bookingService.CancelBooking(request);

            Assert.AreEqual("Cancelled", result.Status);
            Assert.AreEqual("Booking has been cancelled successfully", result.Message);
        }

        [Test]
        public async Task CancelBooking_ShouldReturnAlreadyCancelled_WhenStatusIsCancelled()
        {
            var request = new CancelBookingRequest { BookingId = 1 };
            var booking = new Booking { BookingId = 1, Status = "Cancelled" };

            _mockBookingRepository.Setup(x => x.GetById(1)).ReturnsAsync(booking);

            var result = await _bookingService.CancelBooking(request);

            Assert.AreEqual("Cancelled", result.Status);
            Assert.AreEqual("Booking is already cancelled", result.Message);
        }
    }
}
