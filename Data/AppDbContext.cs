using DocumentManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<UserDocument> UserDocuments { get; set; } = null!; // Yeni Tablo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Document FTS Tanımlamaları...
            modelBuilder.Entity<Document>()
                .HasGeneratedTsVectorColumn(p => p.SearchVector, "turkish", p => new { p.Title, p.ContentSummary })
                .HasIndex(p => p.SearchVector).HasMethod("GIN");

            // UserDocument için Performans Indexleri
            // Bir kullanıcının sahip olduğu tüm dokümanları hızlı getirmek için:
            modelBuilder.Entity<UserDocument>()
                .HasIndex(p => p.UserId);

            // Bir dokümana erişimi olan tüm kullanıcıları hızlı getirmek için:
            modelBuilder.Entity<UserDocument>()
                .HasIndex(p => p.DocumentId);
                
            // Mükerrer yetkilendirmeyi önlemek için (Aynı kullanıcıya aynı doküman 2 kere atanmasın)
            modelBuilder.Entity<UserDocument>()
                .HasIndex(p => new { p.UserId, p.DocumentId })
                .IsUnique();
        }
    }
}