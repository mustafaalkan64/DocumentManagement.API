namespace DocumentManagement.API.Models;

public class UserDocument
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid DocumentId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string PermissionType { get; set; } = "Owner"; // Owner, Reader, Writer
}