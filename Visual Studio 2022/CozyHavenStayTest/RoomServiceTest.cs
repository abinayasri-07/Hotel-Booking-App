using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CozyHavenStay.Tests.Services
{
    [TestFixture]
    public class RoomServiceTest
    {
        private Mock<IRepository<int, Room>> _mockRoomRepo;
        private Mock<IMapper> _mockMapper;
        private RoomService _roomService;

        [SetUp]
        public void Setup()
        {
            _mockRoomRepo = new Mock<IRepository<int, Room>>();
            _mockMapper = new Mock<IMapper>();
            _roomService = new RoomService(_mockRoomRepo.Object, _mockMapper.Object);
        }

        [Test]
        public async Task AddRoom_ValidRequest_ReturnsResponse()
        {
            var request = new CreateRoomRequest
            {
                HotelId = 1,
                Type = "Deluxe",
                Capacity = 2,
                PricePerNight = 100,
                IsAvailable = true
            };

            var room = new Room { RoomId = 1, HotelId = request.HotelId, Type = request.Type, Capacity = request.Capacity, PricePerNight = request.PricePerNight, IsAvailable = request.IsAvailable };
            _mockMapper.Setup(m => m.Map<Room>(request)).Returns(room);
            _mockRoomRepo.Setup(r => r.Add(It.IsAny<Room>())).ReturnsAsync(room);

            var result = await _roomService.AddRoom(request);

            Assert.That(result.RoomId, Is.EqualTo(1));
            Assert.That(result.Message, Is.EqualTo("Room created successfully"));
        }

        [Test]
        public async Task GetAllRooms_ReturnsMappedDTOs()
        {
            var rooms = new List<Room>
            {
                new Room { RoomId = 1, Type = "Standard", HotelId = 1, Capacity = 2, PricePerNight = 80, IsAvailable = true }
            };
            var roomDTOs = new List<RoomDTO> { new RoomDTO { RoomId = 1, Type = "Standard", Capacity = 2, PricePerNight = 80, IsAvailable = true } };

            _mockRoomRepo.Setup(r => r.GetAll()).ReturnsAsync(rooms);
            _mockMapper.Setup(m => m.Map<IEnumerable<RoomDTO>>(rooms)).Returns(roomDTOs);

            var result = await _roomService.GetAllRooms();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Type, Is.EqualTo("Standard"));
        }

        [Test]
        public async Task GetRoomById_ValidId_ReturnsRoomResponse()
        {
            var room = new Room { RoomId = 1, Type = "Deluxe", HotelId = 1, Capacity = 2, PricePerNight = 100, IsAvailable = true };
            var roomResponse = new RoomResponse { RoomId = 1, Type = "Deluxe", Capacity = 2, PricePerNight = 100, IsAvailable = true };

            _mockRoomRepo.Setup(r => r.GetById(1)).ReturnsAsync(room);
            _mockMapper.Setup(m => m.Map<RoomResponse>(room)).Returns(roomResponse);

            var result = await _roomService.GetRoomById(1);

            Assert.IsNotNull(result);
            Assert.That(result?.Type, Is.EqualTo("Deluxe"));
        }

        [Test]
        public async Task GetRoomById_InvalidId_ReturnsNull()
        {
            _mockRoomRepo.Setup(r => r.GetById(999)).ReturnsAsync((Room?)null);

            var result = await _roomService.GetRoomById(999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetRoomsByHotel_ValidHotelId_ReturnsFilteredRooms()
        {
            var rooms = new List<Room>
            {
                new Room { RoomId = 1, HotelId = 1, Type = "Deluxe", Capacity = 2, PricePerNight = 100, IsAvailable = true },
                new Room { RoomId = 2, HotelId = 1, Type = "Standard", Capacity = 2, PricePerNight = 80, IsAvailable = true }
            };
            var roomDTOs = new List<RoomDTO>
            {
                new RoomDTO { RoomId = 1, Type = "Deluxe", Capacity = 2, PricePerNight = 100, IsAvailable = true },
                new RoomDTO { RoomId = 2, Type = "Standard", Capacity = 2, PricePerNight = 80, IsAvailable = true }
            };

            _mockRoomRepo.Setup(r => r.GetAll()).ReturnsAsync(rooms);
            _mockMapper.Setup(m => m.Map<IEnumerable<RoomDTO>>(It.IsAny<IEnumerable<Room>>())).Returns(roomDTOs);

            var result = await _roomService.GetRoomsByHotel(1);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Type, Is.EqualTo("Deluxe"));
        }

        [Test]
        public async Task UpdateRoom_InvalidId_ReturnsNull()
        {
            var updatedRoomRequest = new CreateRoomRequest { Type = "Suite", Capacity = 4, PricePerNight = 150, IsAvailable = true };

            _mockRoomRepo.Setup(r => r.GetById(999)).ReturnsAsync((Room?)null);

            var result = await _roomService.UpdateRoom(999, updatedRoomRequest);

            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteRoom_ValidId_ReturnsDeletedRoom()
        {
            var room = new Room { RoomId = 1, Type = "Standard", HotelId = 1, Capacity = 2, PricePerNight = 80, IsAvailable = true };

            _mockRoomRepo.Setup(r => r.Delete(1)).ReturnsAsync(room);

            var result = await _roomService.DeleteRoom(1);

            Assert.IsNotNull(result);
            Assert.That(result?.Type, Is.EqualTo("Standard"));
        }

        [Test]
        public async Task DeleteRoom_InvalidId_ReturnsNull()
        {
            _mockRoomRepo.Setup(r => r.Delete(999)).ReturnsAsync((Room?)null);

            var result = await _roomService.DeleteRoom(999);

            Assert.IsNull(result);
        }

    }
}
