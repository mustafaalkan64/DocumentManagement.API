using DocumentManagement.API.Models;
using DocumentManagement.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? query)
        {
            var results = await _documentService.SearchDocumentsAsync(query ?? string.Empty);
            return Ok(results);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadDto dto, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Geçersiz dosya.");

            using var stream = file.OpenReadStream();
            var result = await _documentService.UploadDocumentAsync(dto, stream);

            if (!result.IsSuccess)
                return Conflict(result); // HTTP 409 Mükerrer hatası ve mevcut dosya bilgisi döner

            return Ok(result);
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var document = await _documentService.GetDocumentAsync(id);

            if (document == null || string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
                return NotFound("Dosya bulunamadı.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(document.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Dosya türüne göre content type belirle
            var contentType = "application/octet-stream";
            if (!string.IsNullOrEmpty(document.DocumentType))
            {
                contentType = GetMimeType(document.DocumentType);
            }

            return File(memory, contentType, document.Title);
        }

        private string GetMimeType(string fileExtension)
        {
            // Basit bir MIME type çıkarımı
            return fileExtension.ToLower() switch
            {
                "pdf" => "application/pdf",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "png" => "image/png",
                "jpg" => "image/jpeg",
                "jpeg" => "image/jpeg",
                "gif" => "image/gif",
                "csv" => "text/csv",
                _ => "application/octet-stream",
            };
        }
    }
}