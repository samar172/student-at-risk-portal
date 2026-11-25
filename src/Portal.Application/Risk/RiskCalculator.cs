using System.Globalization;
using Portal.Domain.Enums;

namespace Portal.Application.Risk;

/// <summary>
/// Explainable, rule-based risk score from 0 to 100 (SRS Section 4.5, FR-23..FR-28).
/// Score = wA*RA + wM*RM + wP*RP + wT*RT. Missing components are excluded and the
/// remaining weights are re-normalised.
/// </summary>
public sealed class RiskCalculator
{
    // Constants from SRS 4.5.
    private const decimal AttendanceZeroRiskPct = 85m, AttendanceSpan = 35m;
    private const decimal MarksZeroRiskPct = 70m, MarksSpan = 35m;
    private const decimal ParticipationFullDrop = 0.5m;
    private const decimal TrendFullDeclinePoints = 20m;
    private const int ConsecutiveAbsenceTrigger = 3;

    private readonly RiskSettings _s;

    public RiskCalculator(RiskSettings? settings = null) => _s = settings ?? new RiskSettings();

    public RiskResult Calculate(RiskInput i)
    {
        if (i.DaysOfData < _s.MinDaysOfData)
            return new RiskResult(0m, RiskLevel.InsufficientData, null, null, null, null,
                ["Insufficient data: less than two weeks of records."]);

        var reasons = new List<string>();
        decimal? ra = null, rm = null, rp = null, rt = null;

        if (i.AttendancePct is { } a)
        {
            ra = Clamp((AttendanceZeroRiskPct - a) / AttendanceSpan * 100m);
            if (i.ConsecutiveAbsences >= ConsecutiveAbsenceTrigger) ra = Math.Min(100m, ra.Value + 10m);
            if (a < _s.AttendanceTarget)
            {
                var extra = i.ConsecutiveAbsences >= ConsecutiveAbsenceTrigger
                    ? $" with {i.ConsecutiveAbsences} consecutive absences" : "";
                reasons.Add($"Attendance {F(a)}% (below {F(_s.AttendanceTarget)}%){extra}");
            }
        }

        if (i.AvgMarksPct is { } m)
        {
            rm = Clamp((MarksZeroRiskPct - m) / MarksSpan * 100m);
            if (i.SubjectsBelowPassMark > 0)
            {
                rm = Math.Min(100m, rm.Value + Math.Min(30m, 10m * i.SubjectsBelowPassMark));
                reasons.Add($"{i.SubjectsBelowPassMark} subject(s) below the pass mark of {F(_s.PassMark)}%");
            }
            if (m < MarksZeroRiskPct && i.EarlierMarksPct is { } e && e > m)
                reasons.Add($"Average marks {F(m)}%, down {F(e - m)} points");
            else if (m < _s.PassMark + 10m)
                reasons.Add($"Average marks {F(m)}%");
        }

        if (i.ParticipationCurrent is { } p && i.ParticipationBaseline is { } b && b > 0)
        {
            var drop = (b - p) / b;
            rp = drop > 0 ? Clamp(drop / ParticipationFullDrop * 100m) : 0m;
            if (drop > 0.2m) reasons.Add($"Participation {F(drop * 100m)}% below the student's baseline");
        }

        var dA = i is { AttendancePct: { } ca, AttendancePriorPct: { } pa } ? pa - ca : (decimal?)null;
        var dM = i is { AvgMarksPct: { } cm, EarlierMarksPct: { } em } ? em - cm : (decimal?)null;
        if (dA is not null || dM is not null)
        {
            var sum = Math.Max(dA ?? 0m, 0m) + Math.Max(dM ?? 0m, 0m);
            rt = Clamp(sum / 2m / TrendFullDeclinePoints * 100m);
            if (rt > 0 && dA is > 0) reasons.Add($"Attendance fell {F(dA.Value)} points versus the previous 4 weeks");
        }

        var parts = new (decimal? risk, decimal weight)[]
        {
            (ra, _s.WAttendance), (rm, _s.WMarks), (rp, _s.WParticipation), (rt, _s.WTrend),
        };
        var totalWeight = parts.Where(x => x.risk is not null).Sum(x => x.weight);
        if (totalWeight == 0m)
            return new RiskResult(0m, RiskLevel.InsufficientData, ra, rm, rp, rt, ["No usable data for scoring."]);

        var score = parts.Where(x => x.risk is not null).Sum(x => x.risk!.Value * x.weight) / totalWeight;
        score = Math.Round(Clamp(score), 1, MidpointRounding.AwayFromZero);

        if (parts.Any(x => x.risk is null))
            reasons.Add("Some data was missing; remaining components were re-weighted.");

        return new RiskResult(score, LevelFor(score), ra, rm, rp, rt, reasons);
    }

    public RiskLevel LevelFor(decimal score) =>
        score >= _s.CriticalFrom ? RiskLevel.Critical :
        score >= _s.HighFrom ? RiskLevel.High :
        score >= _s.MediumFrom ? RiskLevel.Medium : RiskLevel.Low;

    private static decimal Clamp(decimal v) => Math.Min(100m, Math.Max(0m, v));
    private static string F(decimal v) => v.ToString("0.#", CultureInfo.InvariantCulture);
}
