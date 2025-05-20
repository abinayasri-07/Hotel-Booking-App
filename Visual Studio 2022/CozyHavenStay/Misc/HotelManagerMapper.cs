using AutoMapper;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;

namespace CozyHavenStay.Misc
{
    public class HotelManagerMapper : Profile
    {
        public HotelManagerMapper()
        {
            CreateMap<HotelManager, HotelManagerDTO>()
               .ForMember(dest => dest.HotelName,
                          opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.HotelName : null));

            CreateMap<CreateHotelManagerRequest, HotelManager>();
            CreateMap<UpdateHotelManagerRequest, HotelManager>();
        }
    }
}
