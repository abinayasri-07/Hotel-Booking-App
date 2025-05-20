using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Services;
using Moq;
using NUnit.Framework;
using System.Security.Cryptography;
using System.Text;

namespace CozyHavenStayTest
{
    [TestFixture]
    public class AuthenticationServiceTest
    {
        private Mock<IRepository<string, User>> _userRepoMock;
        private Mock<IRepository<int, Customer>> _customerRepoMock;
        private Mock<IRepository<int, Admin>> _adminRepoMock;
        private Mock<IRepository<int, HotelManager>> _hotelManagerRepoMock;
        private Mock<ITokenService> _tokenServiceMock;
        private AuthenticationService _authService;

        [SetUp]
        public void Setup()
        {
            _userRepoMock = new Mock<IRepository<string, User>>();
            _customerRepoMock = new Mock<IRepository<int, Customer>>();
            _adminRepoMock = new Mock<IRepository<int, Admin>>();
            _hotelManagerRepoMock = new Mock<IRepository<int, HotelManager>>();
            _tokenServiceMock = new Mock<ITokenService>();

            _authService = new AuthenticationService(
                _userRepoMock.Object,
                _customerRepoMock.Object,
                _adminRepoMock.Object,
                _hotelManagerRepoMock.Object,
                _tokenServiceMock.Object
            );
        }

        private (User user, string password) CreateTestUser(string role, string email)
        {
            var password = "Password123!";
            using var hmac = new HMACSHA512();
            var user = new User
            {
                Email = email,
                Role = role,
                Password = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                HashKey = hmac.Key
            };
            return (user, password);
        }

        [Test]
        public async Task Login_ValidCustomerCredentials_ReturnsLoginResponse()
        {
            // Arrange
            var (user, password) = CreateTestUser("Customer", "test@customer.com");

            _userRepoMock.Setup(r => r.GetById(user.Email)).ReturnsAsync(user);
            _customerRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Customer>
            {
                new Customer { Id = 1, Name = "Test Customer", Email = user.Email }
            });

            _tokenServiceMock.Setup(t => t.GenerateToken(1, "Test Customer", "Customer")).ReturnsAsync("valid-jwt");

            var request = new UserLoginRequest { Email = user.Email, Password = password };

            // Act
            var result = await _authService.Login(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test Customer", result.Name);
            Assert.AreEqual("Customer", result.Role);
            Assert.AreEqual("valid-jwt", result.Token);
        }

        [Test]
        public void Login_InvalidPassword_ThrowsUnauthorizedAccess()
        {
            var (user, _) = CreateTestUser("Customer", "wrong@password.com");

            _userRepoMock.Setup(r => r.GetById(user.Email)).ReturnsAsync(user);

            var request = new UserLoginRequest { Email = user.Email, Password = "WrongPassword!" };

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
            Assert.That(ex!.Message, Is.EqualTo("Invalid password"));
        }

        [Test]
        public void Login_UserNotFound_ThrowsUnauthorizedAccess()
        {
            _userRepoMock.Setup(r => r.GetById("notfound@email.com")).ReturnsAsync((User?)null);

            var request = new UserLoginRequest { Email = "notfound@email.com", Password = "any" };

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
            Assert.That(ex!.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public void Login_RoleNotRecognized_ThrowsUnauthorizedAccess()
        {
            var (user, password) = CreateTestUser("UnknownRole", "unknown@role.com");

            _userRepoMock.Setup(r => r.GetById(user.Email)).ReturnsAsync(user);

            var request = new UserLoginRequest { Email = user.Email, Password = password };

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
            Assert.That(ex!.Message, Is.EqualTo("Invalid role"));
        }

        [Test]
        public void Login_AdminNotFound_ThrowsUnauthorizedAccess()
        {
            var (user, password) = CreateTestUser("Admin", "admin@email.com");

            _userRepoMock.Setup(r => r.GetById(user.Email)).ReturnsAsync(user);
            _adminRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Admin>());

            var request = new UserLoginRequest { Email = user.Email, Password = password };

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
            Assert.That(ex!.Message, Is.EqualTo("Admin not found"));
        }

        [Test]
        public void Login_HotelManagerNotFound_ThrowsUnauthorizedAccess()
        {
            var (user, password) = CreateTestUser("HotelManager", "manager@email.com");

            _userRepoMock.Setup(r => r.GetById(user.Email)).ReturnsAsync(user);
            _hotelManagerRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<HotelManager>());

            var request = new UserLoginRequest { Email = user.Email, Password = password };

            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(request));
            Assert.That(ex!.Message, Is.EqualTo("Hotel Manager not found"));
        }
    }
}
