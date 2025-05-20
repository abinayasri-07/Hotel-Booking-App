using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
namespace CozyHavenStay.Interfaces
{
    public interface IHotelManagerService
    {
        Task<CreateHotelManagerResponse> CreateHotelManager(CreateHotelManagerRequest request);
        Task<IEnumerable<HotelManagerDTO>> GetAllHotelManagers();
        Task<HotelManagerDTO?> GetManagerById(int id);
        Task<HotelManagerDTO?> UpdateHotelManager(int id, UpdateHotelManagerRequest request);
        Task<HotelManagerDTO?> DeleteHotelManager(int id);
    }
}

