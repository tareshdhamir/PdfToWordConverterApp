using Xunit;
using PdfToWordConverterApi.Services;
using Moq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Pdf.Operators;

namespace PdfToWordConverterApi.Tests
{
    public class PdfToWordServiceTests
    {
        private readonly PdfToWordService _service;

        public PdfToWordServiceTests()
        {
            _service = new PdfToWordService();
        }

        [Fact]
        public async Task ConvertPdfToWordAsync_TextBasedPdf_ReturnsWordFile()
        {
            // Arrange
            var pdfDocument = new Document();
            var page = pdfDocument.Pages.Add();
            page.Paragraphs.Add(new TextFragment("Test content"));

            var pdfStream = new MemoryStream();
            pdfDocument.Save(pdfStream);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(_ => _.OpenReadStream()).Returns(pdfStream);
            mockFile.Setup(_ => _.Length).Returns(pdfStream.Length); // Set the file length
            mockFile.Setup(_ => _.FileName).Returns("test2.pdf"); // Provide a mock file name
            mockFile.Setup(_ => _.ContentType).Returns("application/pdf"); // Set a content type

            // Act
            var result = await _service.ConvertPdfToWordAsync(mockFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task ConvertPdfToWordAsync_ScannedPdf_ReturnsWordFile()
        {
            // Arrange
            var pdfDocument = new Document();
            var page = pdfDocument.Pages.Add();

            // Access the image from the output directory (assuming file is copied to output directory)
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "test_image.png");
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Test image file not found.", imagePath);
            }

            using var imageStream = new MemoryStream(File.ReadAllBytes(imagePath));

            // Create an ImageStamp to add an image to the page
            var imageStamp = new ImageStamp(imageStream)
            {
                XIndent = 0,
                YIndent = 0,
                Height = page.Rect.Height,
                Width = page.Rect.Width
            };

            // Add the image to the page using ImageStamp
            page.AddStamp(imageStamp);

            // Save the PDF document to a memory stream
            var pdfStream = new MemoryStream();
            pdfDocument.Save(pdfStream);
            pdfStream.Position = 0; // Reset the stream position to 0

            // Mock the IFormFile object
            var mockFile = new Mock<IFormFile>();

            // Set up the mock to return the MemoryStream for OpenReadStream()
            mockFile.Setup(_ => _.OpenReadStream()).Returns(pdfStream);
            mockFile.Setup(_ => _.Length).Returns(pdfStream.Length); // Set the file length
            mockFile.Setup(_ => _.FileName).Returns("test.pdf"); // Provide a mock file name
            mockFile.Setup(_ => _.ContentType).Returns("application/pdf"); // Set a content type

            // Specify the correct path to the tessdata directory
            var tessdataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");

            // Check if the tessdata directory exists
            if (!Directory.Exists(tessdataPath))
            {
                throw new DirectoryNotFoundException($"Tessdata directory not found at {tessdataPath}. Please ensure the directory exists and contains the required .traineddata files.");
            }

            // Act
            var result = await _service.ConvertPdfToWordAsync(mockFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }


        [Fact]
        public async Task ConvertPdfToWordAsync_NullFile_ReturnsBadRequest()
        {
            // Arrange
            IFormFile nullFile = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConvertPdfToWordAsync(nullFile));
        }

        [Fact]
        public async Task ConvertPdfToWordAsync_NullFile_ThrowsArgumentNullException()
        {
            // Arrange
            IFormFile nullFile = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ConvertPdfToWordAsync(nullFile));
            Assert.Equal("pdfFile", exception.ParamName);
            Assert.Equal("Input PDF file cannot be null. (Parameter 'pdfFile')", exception.Message);
        }

        [Fact]
        public async Task ConvertPdfToWordAsync_EmptyFile_ThrowsArgumentException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(_ => _.Length).Returns(0);  // Simulate an empty file

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ConvertPdfToWordAsync(mockFile.Object));
            Assert.Equal("pdfFile", exception.ParamName);
            Assert.Equal("Input PDF file is empty. (Parameter 'pdfFile')", exception.Message);
        }

        [Fact]
        public async Task ConvertPdfToWordAsync_InvalidPdfFile_ThrowsInvalidDataException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var invalidPdfStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });  // Simulate corrupt/invalid PDF data
            mockFile.Setup(_ => _.OpenReadStream()).Returns(invalidPdfStream);
            mockFile.Setup(_ => _.Length).Returns(invalidPdfStream.Length);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidDataException>(() => _service.ConvertPdfToWordAsync(mockFile.Object));
        }

        [Fact]
        public void IsScannedDocument_ReturnsTrue_ForPdfWithImages()
        {
            // Arrange
            var pdfDocument = new Document();
            var page = pdfDocument.Pages.Add();

            // Load a valid image file from the Resources folder
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "test_image.png");
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Test image file not found.", imagePath);
            }

            // Create a stream from the valid image file
            using var imageStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);

            // Add the image to the page using ImageStamp
            var imageStamp = new ImageStamp(imageStream)
            {
                XIndent = 0,
                YIndent = 0,
                Height = 100,
                Width = 100
            };
            page.AddStamp(imageStamp);

            // Act
            var result = _service.IsScannedDocument(pdfDocument);

            // Assert
            Assert.True(result);  // Expect true for a scanned document
        }



        [Fact]
        public void IsScannedDocument_ReturnsFalse_ForPdfWithoutImages()
        {
            // Arrange
            var pdfDocument = new Document();
            var page = pdfDocument.Pages.Add();

            // No images added to the page, only text
            page.Paragraphs.Add(new Aspose.Pdf.Text.TextFragment("This is a text-based PDF"));

            // Act
            var result = _service.IsScannedDocument(pdfDocument);

            // Assert
            Assert.False(result);  // Expect false for a text-based document
        }

        [Fact]
        public void ConvertTextBasedPdfToWord_ReturnsEmptyWordFile_ForPdfWithNoPages()
        {
            // Arrange
            var pdfDocument = new Document();  // No pages

            // Act
            var result = _service.ConvertTextBasedPdfToWord(pdfDocument);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);  // We expect a valid empty Word document
        }

    }
}
