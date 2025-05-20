using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Services;
using Moq;
using NUnit.Framework;

namespace CozyHavenStayTest
{
    [TestFixture]
    public class ReviewServiceTest
    {
        private Mock<IRepository<int, Review>> _reviewRepositoryMock;
        private Mock<IRepository<int, Customer>> _customerRepositoryMock;
        private IMapper _mapper;
        private ReviewService _reviewService;

        [SetUp]
        public void SetUp()
        {
            // Setting up AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateReviewRequest, Review>();
                cfg.CreateMap<Review, ReviewResponse>();
                cfg.CreateMap<Review, ReviewDTO>();
            });

            _mapper = config.CreateMapper();

            // Mocking repositories
            _reviewRepositoryMock = new Mock<IRepository<int, Review>>();
            _customerRepositoryMock = new Mock<IRepository<int, Customer>>();

            // Creating the ReviewService instance with mocked dependencies
            _reviewService = new ReviewService(_reviewRepositoryMock.Object, _customerRepositoryMock.Object, _mapper);
        }

        [Test]
        public async Task AddReview_ValidRequest_ReturnsSuccessMessage()
        {
            // Arrange
            var request = new CreateReviewRequest
            {
                HotelId = 1,
                CustomerId = 1,
                Rating = 5,
                Comment = "Great stay!"
            };

            var review = new Review
            {
                ReviewId = 1,
                HotelId = 1,
                CustomerId = 1,
                Rating = 5,
                Comment = "Great stay!",
                Date = DateTime.UtcNow
            };

            _reviewRepositoryMock.Setup(repo => repo.Add(It.IsAny<Review>()))
                .ReturnsAsync(review);

            // Act
            var result = await _reviewService.AddReview(request);

            // Assert
            Assert.AreEqual("Review added successfully", result.Message);
            Assert.AreEqual(1, result.ReviewId);
        }

        [Test]
        public async Task GetReviewById_ExistingReview_ReturnsReviewResponse()
        {
            // Arrange
            var review = new Review
            {
                ReviewId = 1,
                HotelId = 1,
                CustomerId = 1,
                Rating = 5,
                Comment = "Great stay!",
                Date = DateTime.UtcNow
            };

            var customer = new Customer
            {
                Id = 1,
                Name = "John Doe"
            };

            _reviewRepositoryMock.Setup(repo => repo.GetById(1))
                .ReturnsAsync(review);

            _customerRepositoryMock.Setup(repo => repo.GetById(1))
                .ReturnsAsync(customer);

            // Act
            var result = await _reviewService.GetReviewById(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("John Doe", result.CustomerName);
            Assert.AreEqual(1, result.ReviewId);
            Assert.AreEqual("Great stay!", result.Comment);
        }

        [Test]
        public async Task GetReviewsByHotel_ExistingHotel_ReturnsReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review
                {
                    ReviewId = 1,
                    HotelId = 1,
                    CustomerId = 1,
                    Rating = 5,
                    Comment = "Excellent!",
                    Date = DateTime.UtcNow
                },
                new Review
                {
                    ReviewId = 2,
                    HotelId = 1,
                    CustomerId = 2,
                    Rating = 4,
                    Comment = "Very good!",
                    Date = DateTime.UtcNow
                }
            };

            _reviewRepositoryMock.Setup(repo => repo.GetAll())
                .ReturnsAsync(reviews);

            // Act
            var result = await _reviewService.GetReviewsByHotel(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Excellent!", result.First().Comment);
            Assert.AreEqual("Very good!", result.Last().Comment);
        }

        [Test]
        public async Task DeleteReview_ExistingReview_ReturnsDeletedReview()
        {
            // Arrange
            var review = new Review
            {
                ReviewId = 1,
                HotelId = 1,
                CustomerId = 1,
                Rating = 5,
                Comment = "Great stay!",
                Date = DateTime.UtcNow
            };

            _reviewRepositoryMock.Setup(repo => repo.Delete(1))
                .ReturnsAsync(review);

            // Act
            var result = await _reviewService.DeleteReview(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ReviewId);
            Assert.AreEqual("Great stay!", result.Comment);
        }
    }
}
