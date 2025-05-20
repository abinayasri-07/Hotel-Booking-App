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
    public class RefundServiceTest
    {
        private Mock<IRepository<int, Refund>> _refundRepoMock;
        private Mock<IRepository<int, Payment>> _paymentRepoMock;
        private Mock<IMapper> _mapperMock;
        private RefundService _refundService;

        [SetUp]
        public void Setup()
        {
            _refundRepoMock = new Mock<IRepository<int, Refund>>();
            _paymentRepoMock = new Mock<IRepository<int, Payment>>();
            _mapperMock = new Mock<IMapper>();

            _refundService = new RefundService(
                _refundRepoMock.Object,
                _paymentRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task CreateRefund_ValidPayment_ReturnsCreateRefundResponse()
        {
            // Arrange
            var payment = new Payment
            {
                PaymentId = 1,
                Status = "Completed"
            };

            var request = new CreateRefundRequest
            {
                PaymentId = 1,
                AmountRefunded = 100
            };

            var refund = new Refund
            {
                RefundId = 1,
                PaymentId = 1,
                AmountRefunded = 100,
                Status = "Refunded",
                RefundDate = DateTime.UtcNow
            };

            var response = new CreateRefundResponse
            {
                RefundId = 1
            };

            _paymentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(payment);
            _mapperMock.Setup(m => m.Map<Refund>(request)).Returns(refund);
            _refundRepoMock.Setup(r => r.Add(refund)).ReturnsAsync(refund);
            _mapperMock.Setup(m => m.Map<CreateRefundResponse>(refund)).Returns(response);
            _paymentRepoMock.Setup(r => r.Update(payment.PaymentId, payment)).ReturnsAsync(payment);

            // Act
            var result = await _refundService.CreateRefund(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.RefundId);
            Assert.AreEqual("Refund issued successfully", result.Message);
        }


        [Test]
        public void CreateRefund_AlreadyRefunded_ThrowsException()
        {
            // Arrange
            var payment = new Payment
            {
                PaymentId = 1,
                Status = "Refunded"
            };

            var request = new CreateRefundRequest
            {
                PaymentId = 1,
                AmountRefunded = 50
            };

            _paymentRepoMock.Setup(r => r.GetById(1)).ReturnsAsync(payment);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _refundService.CreateRefund(request));
            Assert.That(ex!.Message, Is.EqualTo("Invalid or already refunded payment."));
        }

        [Test]
        public async Task GetRefundsByPayment_ValidPaymentId_ReturnsRefunds()
        {
            // Arrange
            var refunds = new List<Refund>
            {
                new Refund { RefundId = 1, PaymentId = 1, AmountRefunded = 50 },
                new Refund { RefundId = 2, PaymentId = 2, AmountRefunded = 30 },
                new Refund { RefundId = 3, PaymentId = 1, AmountRefunded = 20 }
            };

            _refundRepoMock.Setup(r => r.GetAll()).ReturnsAsync(refunds);

            _mapperMock.Setup(m => m.Map<IEnumerable<RefundResponse>>(It.IsAny<IEnumerable<Refund>>()))
                .Returns((IEnumerable<Refund> source) =>
                    source.Select(r => new RefundResponse
                    {
                        RefundId = r.RefundId,
                        PaymentId = r.PaymentId,
                        AmountRefunded = r.AmountRefunded
                    }));

            // Act
            var result = await _refundService.GetRefundsByPayment(1);

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.True(result.All(r => r.PaymentId == 1));
        }
    }
}
