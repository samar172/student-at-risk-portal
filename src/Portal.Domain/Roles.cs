namespace Portal.Domain;

/// <summary>Role names used by ASP.NET Core Identity (SRS FR-02).</summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Counsellor = "Counsellor";
    public const string Faculty = "Faculty";
    public const string Hod = "HOD";

    public static readonly string[] All = [Administrator, Counsellor, Faculty, Hod];
}
