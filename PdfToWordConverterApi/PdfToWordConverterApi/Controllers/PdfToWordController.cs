using Microsoft.AspNetCore.Mvc;
using PdfToWordConverterApi.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PdfToWordConverterApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfToWordController : ControllerBase
    {
        private readonly IPdfToWordService _pdfToWordService;

        public PdfToWordController(IPdfToWordService pdfToWordService)
        {
            _pdfToWordService = pdfToWordService;
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertPdfToWord([FromForm] IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return BadRequest("Invalid PDF file");

            try
            {
                var wordFile = await _pdfToWordService.ConvertPdfToWordAsync(pdfFile);
            if (wordFile == null)
                return StatusCode(500, "Conversion failed");

            return File(wordFile, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "converted.docx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
