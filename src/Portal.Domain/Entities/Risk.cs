using Portal.Domain.Enums;

namespace Portal.Domain.Entities;

/// <summary>Single active configuration row (FR-27). Weights must sum to 1.00.</summary>
public class RiskConfig : BaseEntity
{
    public decimal WAttendance { get; set; } = 0.35m;
    public decimal WMarks { get; set; } = 0.30m;
    public decimal WParticipation { get; set; } = 0.15m;
    public decimal WTrend { get; set; } = 0.20m;
    public byte MediumFrom { get; set; } = 30;
    public byte HighFrom { get; set; } = 60;
    public byte CriticalFrom { get; set; } = 80;
    public decimal AttendanceTarget { get; set; } = 75m;
    public decimal PassMark { get; set; } = 40m;
    public int TrendWindowDays { get; set; } = 28;
}

public class RiskSnapshot
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public DateOnly ComputedOn { get; set; }
    public decimal Score { get; set; }
    public RiskLevel Level { get; set; }
    public decimal? AttendancePct { get; set; }
    public decimal? AvgMarksPct { get; set; }
    public decimal? ParticipationIndex { get; set; }
    public decimal TrendDelta { get; set; }
    public string ReasonsJson { get; set; } = "[]";
}

public class Alert
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public long SnapshotId { get; set; }
    public RiskSnapshot? Snapshot { get; set; }
    public RiskLevel Level { get; set; }
    public AlertStatus Status { get; set; } = AlertStatus.New;
    public string? AssignedCounsellorId { get; set; }
    public string? EscalatedToId { get; set; }
    public string? DismissReason { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedOn { get; set; }
    public DateTime? ResolvedOn { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
