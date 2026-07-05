using DocumentManagement.API.Data;
using DocumentManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.API.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly AppDbContext _context;

        public DocumentService(AppDbContext context)
        {
            _context = context;
        }

        // Smart Search: 400ms altı hedef
        public async Task<List<Document>> SearchDocumentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await _context.Documents.OrderByDescending(d => d.CreatedAt).Take(20).ToListAsync();

            // PostgreSQL FTS tsquery kullanımı
            return await _context.Documents
                .Where(d => d.SearchVector.Matches(EF.Functions.ToTsQuery("turkish", searchTerm)))
                .OrderByDescending(d => d.SearchVector.Rank(EF.Functions.ToTsQuery("turkish", searchTerm))) // Alakalılık skoru
                .Take(50)
                .ToListAsync();
        }

        // Upload & Deduplication Mechanism
        public async Task<UploadResult> UploadDocumentAsync(UploadDto dto, Stream fileStream)
        {
            // 1. Step: Calculate SHA-256 Hash
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(fileStream);
            var fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            // 2. Step: Check for Duplicates
            var existingDoc = await _context.Documents
                .FirstOrDefaultAsync(d => d.FileHash == fileHash);

            if (existingDoc != null)
            {
                return new UploadResult 
                { 
                    IsSuccess = false, 
                    Message = "Bu doküman sistemde zaten mevcut! Tekrar yüklemenize gerek yoktur.",
                    ExistingDocument = existingDoc 
                };
            }

            // 3. Step: Save file to disk
            var newDocId = Guid.NewGuid();
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsDir); // Ensure directory exists
            var filePath = Path.Combine(uploadsDir, newDocId.ToString());

            fileStream.Position = 0; // Reset stream position to read again
            using (var fileStreamOnDisk = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOnDisk);
            }

            // 4. Step: Save new document metadata
            var newDoc = new Document
            {
                Id = newDocId,
                Title = dto.Title,
                DocumentType = dto.DocumentType,
                ContentSummary = dto.ContentSummary,
                FileHash = fileHash,
                FilePath = filePath // Save the file path
            };
            
            var userDocument = new UserDocument()
            {
                Id = Guid.NewGuid(),
                DocumentId = newDoc.Id,
                UserId = dto.UserId,
            };

            _context.Documents.Add(newDoc);
            _context.UserDocuments.Add(userDocument);
            await _context.SaveChangesAsync();

            return new UploadResult { IsSuccess = true, Message = "Doküman başarıyla yüklendi.", ExistingDocument = newDoc };
        }

        public async Task<Document?> GetDocumentAsync(Guid id)
        {
            return await _context.Documents.FindAsync(id);
        }
    }
}