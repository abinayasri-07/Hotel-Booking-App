using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace CozyHavenStay.Services
{
    public class HotelManagerService : IHotelManagerService
    {
        private readonly IRepository<int, HotelManager> _managerRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IMapper _mapper;

        public HotelManagerService(IRepository<int, HotelManager> managerRepository, IRepository<string, User> userRepository, IMapper mapper)
        {
            _managerRepository = managerRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CreateHotelManagerResponse> CreateHotelManager(CreateHotelManagerRequest request)
        {
            var existingUser = await _userRepository.GetAll();
            if (existingUser.Any(u => u.Email == request.Email))
                throw new Exception("A user with this email already exists.");

            using var hmac = new HMACSHA512();
            byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

            var user = new User
            {
                Email = request.Email,
                Password = passwordHash,
                HashKey = hmac.Key,
                Role = "HotelManager"
            };

            var userResult = await _userRepository.Add(user);
            if (userResult == null)
                throw new Exception("Failed to create user");

            var manager = new HotelManager
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(request.Password)), // optional
                User = userResult
            };

            var result = await _managerRepository.Add(manager);
            if (result == null)
                throw new Exception("Failed to create manager");

            return new CreateHotelManagerResponse { Id = result.Id };
        }

        public async Task<IEnumerable<HotelManagerDTO>> GetAllHotelManagers()
        {
            var managers = await _managerRepository.GetAll();
            return _mapper.Map<IEnumerable<HotelManagerDTO>>(managers);
        }

        public async Task<HotelManagerDTO?> GetManagerById(int id)
        {
            var manager = await _managerRepository.GetById(id);
            return manager != null ? _mapper.Map<HotelManagerDTO>(manager) : null;
        }

        public async Task<HotelManagerDTO?> UpdateHotelManager(int id, UpdateHotelManagerRequest request)
        {
            var manager = await _managerRepository.GetById(id);
            if (manager == null) return null;

            manager.Name = request.Name;
            manager.Phone = request.Phone;

            var updated = await _managerRepository.Update(id, manager);
            return updated != null ? _mapper.Map<HotelManagerDTO>(updated) : null;
        }

        public async Task<HotelManagerDTO?> DeleteHotelManager(int id)
        {
            var deleted = await _managerRepository.Delete(id);
            return deleted != null ? _mapper.Map<HotelManagerDTO>(deleted) : null;
        }
    }
}