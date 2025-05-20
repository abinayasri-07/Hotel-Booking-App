using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace CozyHavenStay.Services
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<int, Admin> _adminRepository;
        private readonly IMapper _mapper;

        public AdminService(IRepository<string, User> userRepository, IRepository<int, Admin> adminRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _adminRepository = adminRepository;
            _mapper = mapper;
        }

        public async Task<CreateAdminResponse> CreateAdmin(CreateAdminRequest request)
        {
            using var hmac = new HMACSHA512();
            byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

            var user = new User
            {
                Email = request.Email,
                Password = passwordHash,
                HashKey = hmac.Key,
                Role = "Admin"
            };

            var userResult = await _userRepository.Add(user);
            if (userResult == null)
                throw new Exception("Failed to create user");

            var admin = new Admin
            {
                Name = request.Name,
                Email = request.Email,
                User = userResult
            };

            var adminResult = await _adminRepository.Add(admin);
            if (adminResult == null)
                throw new Exception("Failed to create admin");

            return new CreateAdminResponse { Id = adminResult.Id };
        }

        public async Task<IEnumerable<AdminDTO>> GetAllAdmins()
        {
            var admins = await _adminRepository.GetAll();

            if (admins == null || !admins.Any())
            {
                throw new Exception("No admins found");
            }

            return _mapper.Map<IEnumerable<AdminDTO>>(admins);
        }

        public async Task<AdminDTO?> GetAdminById(int id)
        {
            var admin = await _adminRepository.GetById(id);
            return admin != null ? _mapper.Map<AdminDTO>(admin) : null;
        }

        public async Task<AdminDTO?> UpdateAdmin(int id, UpdateAdminRequest request)
        {
            var admin = await _adminRepository.GetById(id);
            if (admin == null) return null;

            admin.Name = request.Name;
            admin.Email = request.Email;

            var updatedAdmin = await _adminRepository.Update(id, admin);
            return updatedAdmin != null ? _mapper.Map<AdminDTO>(updatedAdmin) : null;
        }

        public async Task<AdminDTO?> DeleteAdmin(int id)
        {
            var deleted = await _adminRepository.Delete(id);
            return deleted != null ? _mapper.Map<AdminDTO>(deleted) : null;
        }
    }
}
