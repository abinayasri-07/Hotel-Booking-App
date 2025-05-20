
using AutoMapper;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;

namespace CozyHavenStay.Misc
{
    public class CustomerMapper : Profile
    {
        public CustomerMapper()
        {
            CreateMap<Customer, CustomerDTO>()
                .ForMember(dest => dest.IdProofFileName, opt => opt.MapFrom(src => src.IdProofFileName));

            CreateMap<CreateCustomerRequest, Customer>()
                .ForMember(dest => dest.User, opt => opt.Ignore());

            CreateMap<UpdateCustomerRequest, Customer>();
        }
    }
}
