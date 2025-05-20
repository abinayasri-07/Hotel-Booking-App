using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Models;
using System.Threading.Tasks;
namespace CozyHavenStay.Interfaces
{
    public interface ICustomerService
    { 
        Task<CreateCustomerResponse> AddCustomer(CreateCustomerRequest request);
        Task<IEnumerable<CustomerDTO>> GetAllCustomers();
        Task<CustomerDTO?> GetCustomerById(int id);
        Task<CustomerDTO?> UpdateCustomer(int id, UpdateCustomerRequest request);
        Task<CustomerDTO?> DeleteCustomer(int id);
    }
}
