using DocumentManagement.API.Models;

namespace DocumentManagement.API.Services
{
    public interface IDocumentService
    {
        Task<List<Document>> SearchDocumentsAsync(string searchTerm);
        Task<UploadResult> UploadDocumentAsync(UploadDto dto, Stream fileStream);
        Task<Document?> GetDocumentAsync(Guid id);
    }
}