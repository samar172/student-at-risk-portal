using Portal.Application.Risk;
using Portal.Domain.Enums;

namespace Portal.UnitTests.Risk;

public class RiskCalculatorTests
{
    private readonly RiskCalculator _calc = new();

    // SRS Appendix A.3 worked example: expected score 77.0 (High).
    [Fact]
    public void WorkedExample_ScoresSeventySevenHigh()
    {
        var r = _calc.Calculate(new RiskInput
        {
            DaysOfData = 60,
            AttendancePct = 58m, AttendancePriorPct = 66m, ConsecutiveAbsences = 3,
            AvgMarksPct = 42m, EarlierMarksPct = 52m, SubjectsBelowPassMark = 0,
            ParticipationCurrent = 33m, ParticipationBaseline = 60m,
        });

        Assert.Equal(77.0m, r.Score);
        Assert.Equal(RiskLevel.High, r.Level);
        Assert.Equal(87.1m, Math.Round(r.AttendanceRisk!.Value, 1));
        Assert.Equal(80.0m, r.MarksRisk);
        Assert.Equal(90.0m, r.ParticipationRisk);
        Assert.Equal(45.0m, r.TrendPenalty);
        Assert.Contains(r.Reasons, x => x.StartsWith("Attendance 58%"));
    }

    // TC-10: attendance exactly 75%, 85% and 50%.
    [Theory]
    [InlineData(75, 28.6)]
    [InlineData(85, 0.0)]
    [InlineData(50, 100.0)]
    [InlineData(40, 100.0)] // clamped
    public void AttendanceOnly_MatchesFormula(double attendance, double expected)
    {
        var r = _calc.Calculate(new RiskInput { DaysOfData = 30, AttendancePct = (decimal)attendance });
        Assert.Equal((decimal)expected, r.Score);
    }

    // TC-09: level boundaries.
    [Theory]
    [InlineData(29.9, RiskLevel.Low)]
    [InlineData(30.0, RiskLevel.Medium)]
    [InlineData(59.9, RiskLevel.Medium)]
    [InlineData(60.0, RiskLevel.High)]
    [InlineData(79.9, RiskLevel.High)]
    [InlineData(80.0, RiskLevel.Critical)]
    public void LevelBoundaries(double score, RiskLevel expected) =>
        Assert.Equal(expected, _calc.LevelFor((decimal)score));

    // TC-11 / FR-28: fewer than two weeks of data.
    [Fact]
    public void LessThanTwoWeeks_IsInsufficientData()
    {
        var r = _calc.Calculate(new RiskInput { DaysOfData = 10, AttendancePct = 20m });
        Assert.Equal(RiskLevel.InsufficientData, r.Level);
        Assert.Equal(0m, r.Score);
    }

    [Fact]
    public void NoUsableData_IsInsufficientData()
    {
        var r = _calc.Calculate(new RiskInput { DaysOfData = 30 });
        Assert.Equal(RiskLevel.InsufficientData, r.Level);
    }

    [Fact]
    public void MissingComponents_AreRenormalisedAndExplained()
    {
        // Only marks available: score equals marks risk, with a "re-weighted" note.
        var r = _calc.Calculate(new RiskInput { DaysOfData = 30, AvgMarksPct = 52.5m });
        Assert.Equal(50.0m, r.Score);
        Assert.Contains(r.Reasons, x => x.Contains("re-weighted"));
    }

    [Fact]
    public void ConsecutiveAbsences_AddTenPointsCappedAtHundred()
    {
        var r = _calc.Calculate(new RiskInput { DaysOfData = 30, AttendancePct = 52m, ConsecutiveAbsences = 4 });
        Assert.Equal(100.0m, r.Score);
    }

    [Fact]
    public void FailedSubjects_AddTenEachCappedAtThirty()
    {
        // M=60 -> base 28.6; 5 failed subjects add 30 (cap) -> 58.6
        var r = _calc.Calculate(new RiskInput { DaysOfData = 30, AvgMarksPct = 60m, SubjectsBelowPassMark = 5 });
        Assert.Equal(58.6m, r.Score);
    }

    [Fact]
    public void ImprovingStudent_HasNoTrendPenalty()
    {
        var r = _calc.Calculate(new RiskInput
        {
            DaysOfData = 30, AttendancePct = 90m, AttendancePriorPct = 80m,
            AvgMarksPct = 75m, EarlierMarksPct = 60m,
        });
        Assert.Equal(0m, r.TrendPenalty);
        Assert.Equal(RiskLevel.Low, r.Level);
    }

    [Fact]
    public void Weights_MustSumToOne()
    {
        Assert.True(new RiskSettings().WeightsAreValid());
        Assert.False((new RiskSettings { WTrend = 0.30m }).WeightsAreValid());
    }
}
