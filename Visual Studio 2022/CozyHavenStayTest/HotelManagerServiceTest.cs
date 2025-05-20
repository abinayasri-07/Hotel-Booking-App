using NUnit.Framework;
using Moq;
using AutoMapper;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Services;
using CozyHavenStay.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CozyHavenStayTest
{
    public class HotelManagerServiceTest
    {
        private Mock<IRepository<int, HotelManager>> _mockManagerRepo;
        private Mock<IRepository<string, User>> _mockUserRepo;
        private IMapper _mapper;
        private HotelManagerService _service;

        [SetUp]
        public void Setup()
        {
            _mockManagerRepo = new Mock<IRepository<int, HotelManager>>();
            _mockUserRepo = new Mock<IRepository<string, User>>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateHotelManagerRequest, HotelManager>();
                cfg.CreateMap<HotelManager, HotelManagerDTO>();
                cfg.CreateMap<HotelManager, CreateHotelManagerResponse>()
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            });
            _mapper = config.CreateMapper();

            _service = new HotelManagerService(_mockManagerRepo.Object, _mockUserRepo.Object, _mapper);
        }

        [Test]
        public async Task CreateHotelManager_ValidRequest_ReturnsResponse()
        {
            // Arrange
            var request = new CreateHotelManagerRequest
            {
                Name = "Test Manager",
                Email = "manager@example.com",
                Phone = "1234567890",
                Password = "password123"
            };

            _mockUserRepo.Setup(repo => repo.GetAll())
                         .ReturnsAsync(new List<User>());

            _mockUserRepo.Setup(repo => repo.Add(It.IsAny<User>()))
                         .ReturnsAsync((User u) => u);

            _mockManagerRepo.Setup(repo => repo.Add(It.IsAny<HotelManager>()))
                            .ReturnsAsync((HotelManager h) =>
                            {
                                h.Id = 1;
                                return h;
                            });

            // Act
            var result = await _service.CreateHotelManager(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public void CreateHotelManager_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var request = new CreateHotelManagerRequest
            {
                Name = "Duplicate",
                Email = "duplicate@example.com",
                Phone = "1111111111",
                Password = "test"
            };

            _mockUserRepo.Setup(repo => repo.GetAll())
                         .ReturnsAsync(new List<User>
                         {
                             new User { Email = "duplicate@example.com" }
                         });

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _service.CreateHotelManager(request));
            Assert.That(ex.Message, Is.EqualTo("A user with this email already exists."));
        }

        [Test]
        public async Task GetAllHotelManagers_ReturnsList()
        {
            // Arrange
            var managers = new List<HotelManager>
            {
                new HotelManager { Id = 1, Name = "Manager 1", Email = "m1@example.com" },
                new HotelManager { Id = 2, Name = "Manager 2", Email = "m2@example.com" }
            };

            _mockManagerRepo.Setup(r => r.GetAll()).ReturnsAsync(managers);

            // Act
            var result = await _service.GetAllHotelManagers();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetManagerById_ExistingId_ReturnsManager()
        {
            // Arrange
            var manager = new HotelManager { Id = 1, Name = "Single Manager", Email = "single@example.com" };

            _mockManagerRepo.Setup(r => r.GetById(1)).ReturnsAsync(manager);

            // Act
            var result = await _service.GetManagerById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Single Manager", result.Name);
        }

        [Test]
        public async Task UpdateHotelManager_ValidUpdate_ReturnsUpdated()
        {
            // Arrange
            var original = new HotelManager { Id = 1, Name = "Old Name", Phone = "000" };
            var updatedRequest = new UpdateHotelManagerRequest { Name = "New Name", Phone = "111" };

            _mockManagerRepo.Setup(r => r.GetById(1)).ReturnsAsync(original);
            _mockManagerRepo.Setup(r => r.Update(1, It.IsAny<HotelManager>()))
                            .ReturnsAsync((int id, HotelManager h) => h);

            // Act
            var result = await _service.UpdateHotelManager(1, updatedRequest);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("New Name", result.Name);
        }

        [Test]
        public async Task DeleteHotelManager_ExistingId_ReturnsDeleted()
        {
            // Arrange
            var manager = new HotelManager { Id = 1, Name = "To Delete" };
            _mockManagerRepo.Setup(r => r.Delete(1)).ReturnsAsync(manager);

            // Act
            var result = await _service.DeleteHotelManager(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("To Delete", result.Name);
        }

        [Test]
        public async Task DeleteHotelManager_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockManagerRepo.Setup(r => r.Delete(999)).ReturnsAsync((HotelManager)null);

            // Act
            var result = await _service.DeleteHotelManager(999);

            // Assert
            Assert.IsNull(result);
        }
    }
}

