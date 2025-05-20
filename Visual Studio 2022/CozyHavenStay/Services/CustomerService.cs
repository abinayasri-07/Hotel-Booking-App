using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace CozyHavenStay.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<int, Customer> _customerRepository;
        private readonly IMapper _mapper;

        public CustomerService(IRepository<string, User> userRepository, IRepository<int, Customer> customerRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
        }

        public async Task<CreateCustomerResponse> AddCustomer(CreateCustomerRequest request)
        {
            using var hmac = new HMACSHA512();
            byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

            var user = new User
            {
                Email = request.Email,
                Password = passwordHash,
                HashKey = hmac.Key,
                Role = "Customer"
            };

            var userResult = await _userRepository.Add(user);
            if (userResult == null)
                throw new Exception("Failed to create user");

            var customer = _mapper.Map<Customer>(request);
            customer.User = userResult;

            var result = await _customerRepository.Add(customer);
            if (result == null)
                throw new Exception("Failed to create customer");

            return new CreateCustomerResponse { Id = result.Id };
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomers()
        {
            var customers = await _customerRepository.GetAll();
            return _mapper.Map<IEnumerable<CustomerDTO>>(customers);
        }

        public async Task<CustomerDTO?> GetCustomerById(int id)
        {
            var customer = await _customerRepository.GetById(id);
            return customer == null ? null : _mapper.Map<CustomerDTO>(customer);
        }

        public async Task<CustomerDTO?> UpdateCustomer(int id, UpdateCustomerRequest request)
        {
            var customer = await _customerRepository.GetById(id);
            if (customer == null) return null;

            _mapper.Map(request, customer);

            var updatedCustomer = await _customerRepository.Update(id, customer);
            return updatedCustomer == null ? null : _mapper.Map<CustomerDTO>(updatedCustomer);
        }

        public async Task<CustomerDTO?> DeleteCustomer(int id)
        {
            var deletedCustomer = await _customerRepository.Delete(id);
            return deletedCustomer == null ? null : _mapper.Map<CustomerDTO>(deletedCustomer);
        }
    }
}
