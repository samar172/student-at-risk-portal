using Portal.Domain.Enums;

namespace Portal.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = "";
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}

public class Student : BaseEntity
{
    public string EnrollmentNo { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? GuardianContact { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public byte Semester { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
}

public class Subject : BaseEntity
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public byte Semester { get; set; }
}

public class Attendance
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
}

public class Assessment : BaseEntity
{
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public AssessmentType Type { get; set; }
    public string Title { get; set; } = "";
    public decimal MaxMarks { get; set; }
    public decimal Weight { get; set; }
    public DateOnly Date { get; set; }
}

public class MarkEntry
{
    public long Id { get; set; }
    public int AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public decimal? MarksObtained { get; set; }
}

public class ParticipationRecord
{
    public long Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public int? SubjectId { get; set; }
    public DateOnly Date { get; set; }
    public ParticipationType Type { get; set; }
    public decimal Score { get; set; }
}
