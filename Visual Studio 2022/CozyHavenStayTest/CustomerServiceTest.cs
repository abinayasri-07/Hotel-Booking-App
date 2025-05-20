using AutoMapper;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Models;
using CozyHavenStay.Models.DTOs;
using CozyHavenStay.Services;
using Moq;
using NUnit.Framework;
using System.Security.Cryptography;
using System.Text;

namespace CozyHavenStay.Tests.Services
{
    [TestFixture]
    public class CustomerServiceTest
    {
        private Mock<IRepository<string, User>> _mockUserRepo;
        private Mock<IRepository<int, Customer>> _mockCustomerRepo;
        private Mock<IMapper> _mockMapper;
        private CustomerService _customerService;

        [SetUp]
        public void Setup()
        {
            _mockUserRepo = new Mock<IRepository<string, User>>();
            _mockCustomerRepo = new Mock<IRepository<int, Customer>>();
            _mockMapper = new Mock<IMapper>();
            _customerService = new CustomerService(_mockUserRepo.Object, _mockCustomerRepo.Object, _mockMapper.Object);
        }

        [Test]
        public async Task AddCustomer_ValidRequest_ReturnsResponse()
        {
            var request = new CreateCustomerRequest
            {
                Email = "test@example.com",
                Password = "password123",
                Name = "John Doe",
                Phone = "1234567890"
            };

            var user = new User { Email = request.Email, Role = "Customer", HashKey = new byte[0], Password = new byte[0] };
            var customer = new Customer { Id = 1, Name = "John Doe", Phone = "1234567890", User = user };

            _mockUserRepo.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<Customer>(request)).Returns(customer);
            _mockCustomerRepo.Setup(r => r.Add(It.IsAny<Customer>())).ReturnsAsync(customer);

            var result = await _customerService.AddCustomer(request);

            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllCustomers_ReturnsMappedDTOs()
        {
            var customers = new List<Customer> { new Customer { Id = 1, Name = "Jane", User = new User { Email = "jane@example.com" } } };
            var customerDTOs = new List<CustomerDTO> { new CustomerDTO { Id = 1, Name = "Jane", Email = "jane@example.com" } };

            _mockCustomerRepo.Setup(r => r.GetAll()).ReturnsAsync(customers);
            _mockMapper.Setup(m => m.Map<IEnumerable<CustomerDTO>>(customers)).Returns(customerDTOs);

            var result = await _customerService.GetAllCustomers();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Email, Is.EqualTo("jane@example.com"));
        }

        [Test]
        public async Task GetCustomerById_ValidId_ReturnsCustomerDTO()
        {
            var customer = new Customer { Id = 1, Name = "John", User = new User { Email = "john@example.com" } };
            var dto = new CustomerDTO { Id = 1, Name = "John", Email = "john@example.com" };

            _mockCustomerRepo.Setup(r => r.GetById(1)).ReturnsAsync(customer);
            _mockMapper.Setup(m => m.Map<CustomerDTO>(customer)).Returns(dto);

            var result = await _customerService.GetCustomerById(1);

            Assert.IsNotNull(result);
            Assert.That(result.Email, Is.EqualTo("john@example.com"));
        }

        [Test]
        public async Task GetCustomerById_InvalidId_ReturnsNull()
        {
            _mockCustomerRepo.Setup(r => r.GetById(999)).ReturnsAsync((Customer?)null);

            var result = await _customerService.GetCustomerById(999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateCustomer_ValidRequest_ReturnsUpdatedDTO()
        {
            var existingCustomer = new Customer { Id = 1, Name = "Old", User = new User { Email = "old@example.com" } };
            var updatedCustomer = new Customer { Id = 1, Name = "New", User = existingCustomer.User };
            var dto = new CustomerDTO { Id = 1, Name = "New", Email = "old@example.com" };
            var updateRequest = new UpdateCustomerRequest { Name = "New", Phone = "1111111111" };

            _mockCustomerRepo.Setup(r => r.GetById(1)).ReturnsAsync(existingCustomer);
            _mockMapper.Setup(m => m.Map(updateRequest, existingCustomer));
            _mockCustomerRepo.Setup(r => r.Update(1, existingCustomer)).ReturnsAsync(updatedCustomer);
            _mockMapper.Setup(m => m.Map<CustomerDTO>(updatedCustomer)).Returns(dto);

            var result = await _customerService.UpdateCustomer(1, updateRequest);

            Assert.IsNotNull(result);
            Assert.That(result.Name, Is.EqualTo("New"));
        }

        [Test]
        public async Task UpdateCustomer_InvalidId_ReturnsNull()
        {
            _mockCustomerRepo.Setup(r => r.GetById(999)).ReturnsAsync((Customer?)null);

            var result = await _customerService.UpdateCustomer(999, new UpdateCustomerRequest());

            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteCustomer_ValidId_ReturnsDTO()
        {
            var customer = new Customer { Id = 1, Name = "Del", User = new User { Email = "del@example.com" } };
            var dto = new CustomerDTO { Id = 1, Name = "Del", Email = "del@example.com" };

            _mockCustomerRepo.Setup(r => r.Delete(1)).ReturnsAsync(customer);
            _mockMapper.Setup(m => m.Map<CustomerDTO>(customer)).Returns(dto);

            var result = await _customerService.DeleteCustomer(1);

            Assert.IsNotNull(result);
            Assert.That(result.Email, Is.EqualTo("del@example.com"));
        }

        [Test]
        public async Task DeleteCustomer_NonExistingId_ReturnsNull()
        {
            _mockCustomerRepo.Setup(r => r.Delete(999)).ReturnsAsync((Customer?)null);

            var result = await _customerService.DeleteCustomer(999);

            Assert.IsNull(result);
        }
    }
}
