using AutoMapper;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;

namespace CozyHavenStay.Misc
{
    public class AdminMapper : Profile
    {
        public AdminMapper()
        {
            CreateMap<Admin, AdminDTO>();
            CreateMap<CreateAdminRequest, Admin>();
            CreateMap<UpdateAdminRequest, Admin>();
        }
    }
}
