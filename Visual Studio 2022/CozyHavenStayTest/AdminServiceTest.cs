using AutoMapper;
using CozyHavenStay.Contexts;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Repositories;
using CozyHavenStay.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CozyHavenStayTest
{
    public class AdminServiceTest
    {
        private CozyHavenStayContext _context;
        private IRepository<string, User> _userRepository;
        private IRepository<int, Admin> _adminRepository;
        private IMapper _mapper;
        private AdminService _adminService;

        [SetUp]
        public void Setup()
        {
            // Initialize in-memory DB
            var options = new DbContextOptionsBuilder<CozyHavenStayContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // ensure isolation per test
                .Options;

            _context = new CozyHavenStayContext(options);

            // Real repositories
            _userRepository = new UserRepository(_context);
            _adminRepository = new AdminRepository(_context);

            // Configure mapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Admin, AdminDTO>();
            });

            _mapper = config.CreateMapper();

            // Service instance
            _adminService = new AdminService(_userRepository, _adminRepository, _mapper);
        }
        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreateAdmin_Success_ReturnsResponseWithId()
        {
            var request = new CreateAdminRequest
            {
                Email = "admin1@example.com",
                Password = "SecurePass123!",
                Name = "Admin Test"
            };

            var result = await _adminService.CreateAdmin(request);

            Assert.IsNotNull(result);
            Assert.Greater(result.Id, 0);
        }

        [Test]
        public void GetAllAdmins_WhenNoAdmins_ThrowsException()
        {
            Assert.ThrowsAsync<Exception>(async () => await _adminService.GetAllAdmins());
        }

        [Test]
        public async Task GetAdminById_ExistingAdmin_ReturnsAdminDTO()
        {
            var request = new CreateAdminRequest
            {
                Email = "admin2@example.com",
                Password = "Pass123!",
                Name = "Test Admin"
            };

            var created = await _adminService.CreateAdmin(request);

            var result = await _adminService.GetAdminById(created.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Test Admin", result.Name);
            Assert.AreEqual("admin2@example.com", result.Email);
        }

        [Test]
        public async Task UpdateAdmin_ExistingAdmin_ReturnsUpdatedAdminDTO()
        {
            // Create
            var createRequest = new CreateAdminRequest
            {
                Email = "admin3@example.com",
                Password = "Admin3Pass",
                Name = "Admin 3"
            };

            var created = await _adminService.CreateAdmin(createRequest);

            // Update
            var updateRequest = new UpdateAdminRequest
            {
                Name = "Updated Admin 3",
                Email = "updated3@example.com"
            };

            var updated = await _adminService.UpdateAdmin(created.Id, updateRequest);

            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Admin 3", updated.Name);
            Assert.AreEqual("updated3@example.com", updated.Email);
        }

        [Test]
        public async Task DeleteAdmin_ExistingAdmin_ReturnsDeletedAdminDTO()
        {
            var request = new CreateAdminRequest
            {
                Email = "admin4@example.com",
                Password = "PassDel123",
                Name = "To Be Deleted"
            };

            var created = await _adminService.CreateAdmin(request);

            var deleted = await _adminService.DeleteAdmin(created.Id);

            Assert.IsNotNull(deleted);
            Assert.AreEqual("To Be Deleted", deleted.Name);
        }

        [Test]
        public void DeleteAdmin_NonExistingId_ThrowsException()
        {
            Assert.ThrowsAsync<Exception>(async () => await _adminService.DeleteAdmin(999));
        }
    }
}
