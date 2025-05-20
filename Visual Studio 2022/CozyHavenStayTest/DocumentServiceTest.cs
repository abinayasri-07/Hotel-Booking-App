using Moq;
using NUnit.Framework;
using CozyHavenStay.Services;
using CozyHavenStay.Models;
using CozyHavenStay.Interfaces;
using CozyHavenStay.Repositories;
using CozyHavenStay.Models.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CozyHavenStay.Tests.Services
{
    [TestFixture]
    public class DocumentServiceTest
    {
        private Mock<IRepository<int, Document>> _mockDocumentRepository;
        private Mock<ILogger<DocumentService>> _mockLogger;
        private DocumentService _documentService;

        [SetUp]
        public void SetUp()
        {
            _mockDocumentRepository = new Mock<IRepository<int, Document>>();
            _mockLogger = new Mock<ILogger<DocumentService>>();
            _documentService = new DocumentService(_mockDocumentRepository.Object, _mockLogger.Object);
        }

        [Test]
        public async Task SaveIdProofDocumentsAsync_ValidFile_SavesDocument()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileContent = "Test content";
            var fileName = "idproof.pdf";
            var customerId = 1;

            var fileStream = new MemoryStream();
            var writer = new StreamWriter(fileStream);
            writer.Write(fileContent);
            writer.Flush();
            fileStream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(fileStream.Length);

            var request = new IDProofUploadRequest
            {
                IDProof = fileMock.Object
            };

            // Act
            await _documentService.SaveIdProofDocumentsAsync(customerId, request);

            // Assert
            _mockDocumentRepository.Verify(repo => repo.Add(It.IsAny<Document>()), Times.Once);
        }

        [Test]
        public async Task SaveIdProofDocumentsAsync_InvalidFileExtension_ThrowsException()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var invalidFileName = "idproof.txt";
            var customerId = 1;

            var fileStream = new MemoryStream();
            var writer = new StreamWriter(fileStream);
            writer.Write("Invalid file");
            writer.Flush();
            fileStream.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(f => f.FileName).Returns(invalidFileName);
            fileMock.Setup(f => f.Length).Returns(fileStream.Length);

            var request = new IDProofUploadRequest
            {
                IDProof = fileMock.Object
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _documentService.SaveIdProofDocumentsAsync(customerId, request));
            Assert.That(ex.Message, Does.Contain("Invalid file type"));
        }

        [Test]
        public async Task DownloadIdProofDocumentAsync_DocumentExists_ReturnsDocument()
        {
            // Arrange
            var customerId = 1;
            var document = new Document
            {
                DocumentId = 1,
                CustomerId = customerId,
                FileName = "idproof.pdf",
                FileType = "ID Proof",
                Data = new byte[] { 1, 2, 3, 4 }
            };

            _mockDocumentRepository.Setup(repo => repo.GetAll()).ReturnsAsync(new List<Document> { document });

            // Act
            var result = await _documentService.DownloadIdProofDocumentAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(document.FileName, result?.FileName);
            Assert.AreEqual(document.Data, result?.FileData);
        }

        [Test]
        public async Task DownloadIdProofDocumentAsync_DocumentNotFound_ReturnsNull()
        {
            // Arrange
            var customerId = 1;
            _mockDocumentRepository.Setup(repo => repo.GetAll()).ReturnsAsync(new List<Document>());

            // Act
            var result = await _documentService.DownloadIdProofDocumentAsync(customerId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task SaveIdProofDocumentsAsync_FileExceedsMaxSize_ThrowsException()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var fileContent = new byte[6 * 1024 * 1024]; // 6 MB file
            var fileName = "idproof.pdf";
            var customerId = 1;

            var fileStream = new MemoryStream(fileContent);

            fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(fileStream.Length);

            var request = new IDProofUploadRequest
            {
                IDProof = fileMock.Object
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _documentService.SaveIdProofDocumentsAsync(customerId, request));
            Assert.That(ex.Message, Does.Contain("exceeds 5MB size limit"));
        }

        // Add more tests as necessary for different scenarios
    }
}
