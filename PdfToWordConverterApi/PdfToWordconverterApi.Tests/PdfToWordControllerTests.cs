using Xunit;
using Moq;
using PdfToWordConverterApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using PdfToWordConverterApi.Controllers;

namespace PdfToWordConverterApi.Tests
{
    public class PdfToWordControllerTests
    {
        private readonly PdfToWordController _controller;
        private readonly Mock<IPdfToWordService> _mockService;

        public PdfToWordControllerTests()
        {
            _mockService = new Mock<IPdfToWordService>();
            _controller = new PdfToWordController(_mockService.Object);
        }

        [Fact]
        public async Task ConvertPdfToWord_ValidPdf_ReturnsWordFile()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var pdfStream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });  // PDF header
            mockFile.Setup(_ => _.OpenReadStream()).Returns(pdfStream);
            mockFile.Setup(_ => _.Length).Returns(pdfStream.Length);
            mockFile.Setup(_ => _.FileName).Returns("test.pdf");

            var mockWordFile = new byte[] { 0x50, 0x4B, 0x03, 0x04 };  // Mock Word file (ZIP file header)
            _mockService.Setup(service => service.ConvertPdfToWordAsync(It.IsAny<IFormFile>()))
                        .ReturnsAsync(mockWordFile);

            // Act
            var result = await _controller.ConvertPdfToWord(mockFile.Object) as FileContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", result.ContentType);
            Assert.Equal("converted.docx", result.FileDownloadName);
            Assert.Equal(mockWordFile, result.FileContents);
        }


        [Fact]
        public async Task ConvertPdfToWord_NullFile_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ConvertPdfToWord(null) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid PDF file", result.Value);
        }


        [Fact]
        public async Task ConvertPdfToWord_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(_ => _.Length).Returns(0);  // Empty file

            // Act
            var result = await _controller.ConvertPdfToWord(mockFile.Object) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid PDF file", result.Value);
        }


        [Fact]
        public async Task ConvertPdfToWord_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var pdfStream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });  // PDF header
            mockFile.Setup(_ => _.OpenReadStream()).Returns(pdfStream);
            mockFile.Setup(_ => _.Length).Returns(pdfStream.Length);

            _mockService.Setup(service => service.ConvertPdfToWordAsync(It.IsAny<IFormFile>()))
                        .ThrowsAsync(new System.Exception("Conversion error"));

            // Act
            var result = await _controller.ConvertPdfToWord(mockFile.Object) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("An error occurred: Conversion error", result.Value);
        }


        [Fact]
        public async Task ConvertPdfToWord_ServiceReturnsNull_ReturnsInternalServerError()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var pdfStream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });  // PDF header
            mockFile.Setup(_ => _.OpenReadStream()).Returns(pdfStream);
            mockFile.Setup(_ => _.Length).Returns(pdfStream.Length);

            _mockService.Setup(service => service.ConvertPdfToWordAsync(It.IsAny<IFormFile>()))
                        .ReturnsAsync((byte[])null);  // Simulate conversion failure

            // Act
            var result = await _controller.ConvertPdfToWord(mockFile.Object) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Conversion failed", result.Value);
        }
    }
}
