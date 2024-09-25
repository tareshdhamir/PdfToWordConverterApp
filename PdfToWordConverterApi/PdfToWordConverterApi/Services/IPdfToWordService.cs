using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PdfToWordConverterApi.Services
{
    public interface IPdfToWordService
    {
        Task<byte[]> ConvertPdfToWordAsync(IFormFile pdfFile);
    }
}
