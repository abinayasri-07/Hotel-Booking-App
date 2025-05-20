using AutoMapper;
using Moq;
using NUnit.Framework;
using CozyHavenStay.Services;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using System;
using System.Threading.Tasks;
using CozyHavenStay.Misc;

namespace CozyHavenStayTest
{
    public class PaymentServiceTest
    {
        private IMapper _mapper;
        private IPaymentService _paymentService;
        private Mock<IRepository<int, Payment>> _paymentRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            // Initialize AutoMapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentMapper>();  // Add PaymentMapper profile
            });

            _mapper = config.CreateMapper();

            // Mock the PaymentRepository
            _paymentRepositoryMock = new Mock<IRepository<int, Payment>>();

            // Initialize PaymentService with the mock repository and mapper
            _paymentService = new PaymentService(_paymentRepositoryMock.Object, _mapper);
        }

        [Test]
        public async Task ProcessPayment_ValidRequest_ReturnsPaymentResponse()
        {
            // Arrange: Set up request and mock behavior
            var createPaymentRequest = new CreatePaymentRequest
            {
                PaymentMethod = "CreditCard",
                AmountPaid = 100.00m,
                BookingId = 1
            };

            var payment = new Payment
            {
                PaymentId = 1,
                PaymentMethod = createPaymentRequest.PaymentMethod,
                AmountPaid = createPaymentRequest.AmountPaid,
                BookingId = createPaymentRequest.BookingId,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            };

            _paymentRepositoryMock.Setup(repo => repo.Add(It.IsAny<Payment>()))
                                  .ReturnsAsync(payment);

            // Act: Call the method
            var result = await _paymentService.ProcessPayment(createPaymentRequest);

            // Assert: Verify result
            Assert.NotNull(result);
            Assert.AreEqual("Payment processed successfully", result.Message);
            Assert.AreEqual(1, result.PaymentId);
        }

        [Test]
        public async Task GetPaymentById_ExistingPayment_ReturnsPaymentResponse()
        {
            // Arrange: Set up a mock payment and repository behavior
            var paymentId = 1;
            var payment = new Payment
            {
                PaymentId = paymentId,
                PaymentMethod = "CreditCard",
                AmountPaid = 100.00m,
                BookingId = 1,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            };

            _paymentRepositoryMock.Setup(repo => repo.GetById(paymentId))
                                  .ReturnsAsync(payment);

            // Act: Call the method
            var result = await _paymentService.GetPaymentById(paymentId);

            // Assert: Verify result
            Assert.NotNull(result);
            Assert.AreEqual(payment.PaymentId, result.PaymentId);
            Assert.AreEqual(payment.PaymentMethod, result.PaymentMethod);
            Assert.AreEqual(payment.AmountPaid, result.AmountPaid);
        }

        [Test]
        public async Task GetPaymentsByBooking_ExistingBooking_ReturnsPaymentResponseList()
        {
            // Arrange: Set up request and mock behavior
            var bookingId = 1;
            var payments = new[]
            {
        new Payment
        {
            PaymentId = 1,
            PaymentMethod = "CreditCard",
            AmountPaid = 100.00m,
            BookingId = bookingId,
            PaymentDate = DateTime.UtcNow,
            Status = "Success"
        },
        new Payment
        {
            PaymentId = 2,
            PaymentMethod = "PayPal",
            AmountPaid = 50.00m,
            BookingId = bookingId,
            PaymentDate = DateTime.UtcNow,
            Status = "Success"
        }
    };

            _paymentRepositoryMock.Setup(repo => repo.GetAll())
                                  .ReturnsAsync(payments);

            // Act: Call the method
            var result = await _paymentService.GetPaymentsByBooking(bookingId);

            // Cast the result to a List<PaymentResponse>
            var paymentList = result.ToList();

            // Assert: Verify result
            Assert.NotNull(paymentList);
            Assert.AreEqual(2, paymentList.Count);

            // Assert for the first payment
            var firstPayment = paymentList[0];
            Assert.AreEqual(100.00m, firstPayment.AmountPaid);
            Assert.AreEqual("CreditCard", firstPayment.PaymentMethod);
            Assert.AreEqual("Success", firstPayment.Status);

            // Assert for the second payment
            var secondPayment = paymentList[1];
            Assert.AreEqual(50.00m, secondPayment.AmountPaid);
            Assert.AreEqual("PayPal", secondPayment.PaymentMethod);
            Assert.AreEqual("Success", secondPayment.Status);
        }

        [Test]
        public async Task GetAllPayments_ReturnsPaymentResponseList()
        {
            // Arrange: Set up the mock repository and AutoMapper
            var payments = new[]
            {
        new Payment
        {
            PaymentId = 1,
            BookingId = 1,
            AmountPaid = 100.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "CreditCard",
            Status = "Success"
        },
        new Payment
        {
            PaymentId = 2,
            BookingId = 2,
            AmountPaid = 50.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "PayPal",
            Status = "Success"
        }
    };

            _paymentRepositoryMock.Setup(repo => repo.GetAll())
                                  .ReturnsAsync(payments);

            // Act: Call the GetAllPayments method
            var result = await _paymentService.GetAllPayments();

            // Assert: Verify the result
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count()); // Ensure we have 2 payments

            // Verify the first payment response
            var firstPayment = result.ElementAt(0);
            Assert.AreEqual(1, firstPayment.PaymentId);
            Assert.AreEqual(100.00m, firstPayment.AmountPaid);
            Assert.AreEqual("CreditCard", firstPayment.PaymentMethod);
            Assert.AreEqual("Success", firstPayment.Status);

            // Verify the second payment response
            var secondPayment = result.ElementAt(1);
            Assert.AreEqual(2, secondPayment.PaymentId);
            Assert.AreEqual(50.00m, secondPayment.AmountPaid);
            Assert.AreEqual("PayPal", secondPayment.PaymentMethod);
            Assert.AreEqual("Success", secondPayment.Status);
        }



        [Test]
        public async Task GetPaymentStats_ReturnsPaymentStatsResponse()
        {
            // Arrange: Set up mock payments
            var payments = new[]
            {
                new Payment
                {
                    PaymentId = 1,
                    PaymentMethod = "CreditCard",
                    AmountPaid = 100.00m,
                    Status = "Success",
                    PaymentDate = DateTime.UtcNow
                },
                new Payment
                {
                    PaymentId = 2,
                    PaymentMethod = "PayPal",
                    AmountPaid = 50.00m,
                    Status = "Success",
                    PaymentDate = DateTime.UtcNow
                }
            };

            _paymentRepositoryMock.Setup(repo => repo.GetAll())
                                  .ReturnsAsync(payments);

            // Act: Call the method
            var result = await _paymentService.GetPaymentStats();

            // Assert: Verify result
            Assert.NotNull(result);
            Assert.AreEqual(150.00m, result.TotalRevenue);
            Assert.AreEqual(150.00m, result.TodayRevenue);
            Assert.AreEqual(2, result.PaymentMethodCount.Count);
        }
    }
}
