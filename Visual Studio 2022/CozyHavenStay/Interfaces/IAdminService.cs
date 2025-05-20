using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Models;
using CozyHavenStay.Repositories;
namespace CozyHavenStay.Interfaces
{
    public interface IAdminService
    {
        Task<CreateAdminResponse> CreateAdmin(CreateAdminRequest request);
        Task<IEnumerable<AdminDTO>> GetAllAdmins();
        Task<AdminDTO?> GetAdminById(int id);
        Task<AdminDTO?> UpdateAdmin(int id, UpdateAdminRequest request);
        Task<AdminDTO?> DeleteAdmin(int id);
    }
}
