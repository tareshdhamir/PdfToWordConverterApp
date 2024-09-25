using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Tesseract;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PdfToWordConverterApi.Services;

namespace PdfToWordConverterApi.Services
{
    public class PdfToWordService : IPdfToWordService
    {
        public async Task<byte[]> ConvertPdfToWordAsync(IFormFile pdfFile)
        {
            if (pdfFile == null)
            {
                throw new ArgumentNullException(nameof(pdfFile), "Input PDF file cannot be null.");
            }

            if (pdfFile.Length == 0)
            {
                throw new ArgumentException("Input PDF file is empty.", nameof(pdfFile));
            }

            try
            {
                using var pdfStream = pdfFile.OpenReadStream();
                var pdfDocument = new Document(pdfStream);

                if (IsScannedDocument(pdfDocument))
                {
                    // OCR Conversion
                    return await ConvertPdfWithOcrAsync(pdfDocument);
                }
                else
                {
                    // Text-based PDF Conversion
                    return ConvertTextBasedPdfToWord(pdfDocument);
                }

            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Invalid or corrupt PDF file.", ex);
            }
        }

        public bool IsScannedDocument(Document pdfDocument)
        {
            foreach (var page in pdfDocument.Pages)
            {
                if (page.Resources.Images.Count > 0)
                    return true;
            }
            return false;
        }

        public byte[] ConvertTextBasedPdfToWord(Document pdfDocument)
        {
            // Check if the document contains any pages
            if (pdfDocument.Pages.Count == 0)
            {
                // Return an empty Word file if there are no pages
                using var emptyWordStream = new MemoryStream();
                var emptyWordDoc = new Aspose.Words.Document();
                emptyWordDoc.Save(emptyWordStream, Aspose.Words.SaveFormat.Docx);
                return emptyWordStream.ToArray();
            }

            // If there are pages, proceed with conversion
            using var wordStream = new MemoryStream();
            pdfDocument.Save(wordStream, SaveFormat.DocX);
            return wordStream.ToArray();
        }


        private async Task<byte[]> ConvertPdfWithOcrAsync(Document pdfDocument)
        {
            var outputDoc = new Aspose.Words.Document();
            var builder = new Aspose.Words.DocumentBuilder(outputDoc);
            var ocrEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);

            foreach (var page in pdfDocument.Pages)
            {
                using var imageStream = new MemoryStream();
                var resolution = new Resolution(300);
                var pngDevice = new PngDevice(resolution);
                pngDevice.Process(page, imageStream);

                imageStream.Position = 0;
                using var pix = Pix.LoadFromMemory(imageStream.ToArray());

                using var ocrPage = await Task.Run(() => ocrEngine.Process(pix));
                var ocrText = ocrPage.GetText();

                builder.Writeln(ocrText);
            }

            using var outputStream = new MemoryStream();
            outputDoc.Save(outputStream, Aspose.Words.SaveFormat.Docx);
            return outputStream.ToArray();
        }
    }
}
