namespace Portal.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedOn { get; set; }
    /// <summary>Soft delete flag (FR-11). History is never physically removed.</summary>
    public bool IsDeleted { get; set; }
}
