using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Services;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CozyHavenStayTest
{
    public class HotelServiceTest
    {
        private IMapper _mapper;
        private Mock<IRepository<int, Hotel>> _hotelRepositoryMock;
        private IHotelService _hotelService;

        [SetUp]
        public void Setup()
        {
            var mockMapper = new Mock<IMapper>();

            mockMapper.Setup(m => m.Map<HotelDTO>(It.IsAny<Hotel>()))
                .Returns<Hotel>(h => new HotelDTO
                {
                    HotelId = h.HotelId,
                    HotelName = h.HotelName,
                    Location = h.Location,
                    StarRating = h.StarRating
                });

            mockMapper.Setup(m => m.Map<IEnumerable<HotelDTO>>(It.IsAny<IEnumerable<Hotel>>()))
                .Returns<IEnumerable<Hotel>>(hotels => hotels.Select(h => new HotelDTO
                {
                    HotelId = h.HotelId,
                    HotelName = h.HotelName,
                    Location = h.Location,
                    StarRating = h.StarRating
                }));

            mockMapper.Setup(m => m.Map<Hotel>(It.IsAny<CreateHotelRequest>()))
                .Returns<CreateHotelRequest>(req => new Hotel
                {
                    HotelName = req.HotelName,
                    Location = req.Location,
                    StarRating = req.StarRating
                });

            _mapper = mockMapper.Object;
            _hotelRepositoryMock = new Mock<IRepository<int, Hotel>>();
            _hotelService = new HotelService(_hotelRepositoryMock.Object, _mapper);
        }

        [Test]
        public async Task AddHotel_Success_ReturnsCreateHotelResponse()
        {
            var request = new CreateHotelRequest
            {
                HotelName = "Test Hotel",
                Location = "Test Location",
                StarRating = 5
            };

            var hotel = new Hotel
            {
                HotelId = 1,
                HotelName = "Test Hotel",
                Location = "Test Location",
                StarRating = 5
            };

            _hotelRepositoryMock.Setup(repo => repo.Add(It.IsAny<Hotel>()))
                .ReturnsAsync(hotel);

            var result = await _hotelService.AddHotel(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HotelId, Is.EqualTo(1));
            Assert.That(result.Message, Is.EqualTo("Hotel created successfully"));
        }

        [Test]
        public async Task GetAllHotels_ReturnsListOfHotelDTOs()
        {
            var hotels = new List<Hotel>
            {
                new Hotel { HotelId = 1, HotelName = "Hotel 1", Location = "Location 1", StarRating = 4 },
                new Hotel { HotelId = 2, HotelName = "Hotel 2", Location = "Location 2", StarRating = 5 }
            };

            _hotelRepositoryMock.Setup(repo => repo.GetAll())
                .ReturnsAsync(hotels);

            var result = await _hotelService.GetAllHotels();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().HotelName, Is.EqualTo("Hotel 1"));
        }

        [Test]
        public async Task UpdateHotel_ExistingHotel_ReturnsUpdatedHotel()
        {
            var hotel = new Hotel
            {
                HotelId = 1,
                HotelName = "Old Hotel",
                Location = "Old Location",
                StarRating = 3,
                HotelManager = new HotelManager
                {
                    Id = 1,
                    Name = "Manager",
                    Email = "m@example.com",
                    Phone = "9876543210"
                },
                Rooms = new List<Room>(),
                Reviews = new List<Review>()
            };

            var updateRequest = new CreateHotelRequest
            {
                HotelName = "Updated Hotel",
                Location = "Updated Location",
                StarRating = 5
            };

            _hotelRepositoryMock.Setup(repo => repo.GetById(1))
                .ReturnsAsync(hotel);

            _hotelRepositoryMock.Setup(repo => repo.Update(1, It.IsAny<Hotel>()))
                .ReturnsAsync((int id, Hotel h) =>
                {
                    h.HotelId = id;
                    return h;
                });

            var result = await _hotelService.UpdateHotel(1, updateRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HotelName, Is.EqualTo("Updated Hotel"));
            Assert.That(result.Location, Is.EqualTo("Updated Location"));
            Assert.That(result.StarRating, Is.EqualTo(5));
        }

        [Test]
        public async Task DeleteHotel_ExistingHotel_ReturnsDeletedHotel()
        {
            var hotel = new Hotel
            {
                HotelId = 1,
                HotelName = "Hotel to Delete",
                Location = "Location",
                StarRating = 3,
                HotelManager = new HotelManager
                {
                    Id = 2,
                    Name = "To Delete",
                    Email = "del@example.com",
                    Phone = "0000000000"
                }
            };

            _hotelRepositoryMock.Setup(repo => repo.GetById(1))
                .ReturnsAsync(hotel);

            _hotelRepositoryMock.Setup(repo => repo.Delete(1))
                .ReturnsAsync(hotel);

            var result = await _hotelService.DeleteHotel(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.HotelId, Is.EqualTo(1));
            Assert.That(result.HotelName, Is.EqualTo("Hotel to Delete"));
        }
        [Test]
        public async Task GetHotelsByFilter_ValidFilters_ReturnsFilteredHotels()
        {
            // Arrange: Set up mock hotels and filter request
            var hotels = new[]
            {
        new Hotel { HotelId = 1, HotelName = "Hotel A", Location = "New York", StarRating = 4 },
        new Hotel { HotelId = 2, HotelName = "Hotel B", Location = "New York", StarRating = 3 },
        new Hotel { HotelId = 3, HotelName = "Hotel C", Location = "London", StarRating = 5 }
    };

            var filterRequest = new HotelRequest
            {
                Filters = new HotelFilter
                {
                    Location = "New York"
                }
            };

            _hotelRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(hotels);

            // Act: Call the method with the filter request
            var result = await _hotelService.GetHotelsByFilter(filterRequest);

            var filteredHotel = result.First();
            Assert.AreEqual("New York", filteredHotel.Location);

        }

    }
}
