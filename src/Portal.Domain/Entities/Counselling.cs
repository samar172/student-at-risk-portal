using Portal.Domain.Enums;

namespace Portal.Domain.Entities;

public class CounsellorAssignment : BaseEntity
{
    public string CounsellorId { get; set; } = "";
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public DateOnly From { get; set; }
    public DateOnly? To { get; set; }
}

public class CounsellorNote
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public string CounsellorId { get; set; } = "";
    public long? AlertId { get; set; }
    public string Text { get; set; } = "";
    public string? Tags { get; set; }
    public NoteVisibility Visibility { get; set; } = NoteVisibility.Private;
    public int Version { get; set; } = 1;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
}

public class Intervention
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public long? AlertId { get; set; }
    public InterventionType Type { get; set; }
    public string Description { get; set; } = "";
    public DateOnly PlannedDate { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public InterventionStatus Status { get; set; } = InterventionStatus.Planned;
    public string? Outcome { get; set; }
    public decimal? ScoreAtStart { get; set; }
    public decimal? ScoreAfter4Wk { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class Notification
{
    public long Id { get; set; }
    public string RecipientUserId { get; set; } = "";
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public byte Attempts { get; set; }
    public long? RelatedAlertId { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? SentOn { get; set; }
}
