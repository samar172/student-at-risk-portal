using Portal.Domain.Enums;

namespace Portal.Application.Risk;

/// <summary>Tunable settings (SRS 4.5, FR-27). Defaults match the SRS.</summary>
public sealed record RiskSettings
{
    public decimal WAttendance { get; init; } = 0.35m;
    public decimal WMarks { get; init; } = 0.30m;
    public decimal WParticipation { get; init; } = 0.15m;
    public decimal WTrend { get; init; } = 0.20m;
    public decimal MediumFrom { get; init; } = 30m;
    public decimal HighFrom { get; init; } = 60m;
    public decimal CriticalFrom { get; init; } = 80m;
    public decimal AttendanceTarget { get; init; } = 75m;
    public decimal PassMark { get; init; } = 40m;
    public int MinDaysOfData { get; init; } = 14;

    public bool WeightsAreValid() =>
        WAttendance + WMarks + WParticipation + WTrend == 1.00m &&
        WAttendance >= 0 && WMarks >= 0 && WParticipation >= 0 && WTrend >= 0;
}

/// <summary>Per-student inputs for one evaluation date. Null means "data not available".</summary>
public sealed record RiskInput
{
    public int DaysOfData { get; init; }

    public decimal? AttendancePct { get; init; }
    public decimal? AttendancePriorPct { get; init; }
    public int ConsecutiveAbsences { get; init; }

    public decimal? AvgMarksPct { get; init; }
    public decimal? EarlierMarksPct { get; init; }
    public int SubjectsBelowPassMark { get; init; }

    public decimal? ParticipationCurrent { get; init; }
    /// <summary>Student's own baseline, or the cohort median when no baseline exists.</summary>
    public decimal? ParticipationBaseline { get; init; }
}

public sealed record RiskResult(
    decimal Score,
    RiskLevel Level,
    decimal? AttendanceRisk,
    decimal? MarksRisk,
    decimal? ParticipationRisk,
    decimal? TrendPenalty,
    IReadOnlyList<string> Reasons);
