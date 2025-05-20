using AutoMapper;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;

namespace CozyHavenStay.Misc
{
    public class BookingMapper : Profile
    {
        public BookingMapper()
        {
            CreateMap<CreateBookingRequest, Booking>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)); // Set CreatedAt as current time

            CreateMap<Booking, CreateBookingResponse>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "Booking confirmed")); // Adding custom message

            CreateMap<Booking, BookingDTO>()
                // Removed RoomType and HotelName mappings from here
                .ForMember(dest => dest.RoomIds, opt => opt.MapFrom(src => src.BookingRooms.Select(br => br.RoomId).ToList())); // Mapping RoomIds from BookingRooms

            CreateMap<Booking, BookingResponse>()
                .ForMember(dest => dest.RoomIds, opt => opt.MapFrom(src => src.BookingRooms.Select(br => br.RoomId).ToList())); // Mapping RoomIds from BookingRooms
        }
    }
}
