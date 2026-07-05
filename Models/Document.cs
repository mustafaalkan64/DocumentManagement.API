using NpgsqlTypes;

namespace DocumentManagement.API.Models
{
    
    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty; // Sözleşme, Teklif, Fatura
        public string ContentSummary { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty; // Duplicate önleme için SHA-256
        public string FilePath { get; set; } = string.Empty; // Dosya yolu
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // PostgreSQL Full-Text Search için arama vektörü
        public NpgsqlTsVector SearchVector { get; set; }
    }
    
    public class UploadDto
    {
        public string Title { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string ContentSummary { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class UploadResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Document? ExistingDocument { get; set; }
    }
}

