using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portal.Domain.Entities;
using Portal.Infrastructure.Identity;

namespace Portal.Infrastructure.Persistence;

public class PortalDbContext(DbContextOptions<PortalDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Attendance> Attendance => Set<Attendance>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<MarkEntry> MarkEntries => Set<MarkEntry>();
    public DbSet<ParticipationRecord> ParticipationRecords => Set<ParticipationRecord>();
    public DbSet<RiskConfig> RiskConfigs => Set<RiskConfig>();
    public DbSet<RiskSnapshot> RiskSnapshots => Set<RiskSnapshot>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<CounsellorAssignment> CounsellorAssignments => Set<CounsellorAssignment>();
    public DbSet<CounsellorNote> CounsellorNotes => Set<CounsellorNote>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Soft delete (FR-11): deleted rows are hidden from normal queries. Dependents carry matching
        // filters so that data belonging to a deleted student/subject/department is hidden consistently.
        b.Entity<Department>().HasQueryFilter(x => !x.IsDeleted);
        b.Entity<Student>().HasQueryFilter(x => !x.IsDeleted && !x.Department!.IsDeleted);
        b.Entity<Subject>().HasQueryFilter(x => !x.IsDeleted && !x.Department!.IsDeleted);
        b.Entity<Assessment>().HasQueryFilter(x => !x.IsDeleted && !x.Subject!.IsDeleted);
        b.Entity<Attendance>().HasQueryFilter(x => !x.Student!.IsDeleted && !x.Subject!.IsDeleted);
        b.Entity<MarkEntry>().HasQueryFilter(x => !x.Student!.IsDeleted && !x.Assessment!.IsDeleted);
        b.Entity<ParticipationRecord>().HasQueryFilter(x => !x.Student!.IsDeleted);
        b.Entity<RiskSnapshot>().HasQueryFilter(x => !x.Student!.IsDeleted);
        b.Entity<Alert>().HasQueryFilter(x => !x.Student!.IsDeleted);
        b.Entity<CounsellorAssignment>().HasQueryFilter(x => !x.IsDeleted && !x.Student!.IsDeleted);
        b.Entity<CounsellorNote>().HasQueryFilter(x => !x.Student!.IsDeleted);
        b.Entity<Intervention>().HasQueryFilter(x => !x.Student!.IsDeleted);

        b.Entity<Department>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<Student>(e =>
        {
            e.Property(x => x.EnrollmentNo).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.EnrollmentNo).IsUnique();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.GuardianContact).HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Department).WithMany(d => d.Students).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Subject>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.HasIndex(x => new { x.DepartmentId, x.Code }).IsUnique();
            e.HasOne(x => x.Department).WithMany(d => d.Subjects).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Attendance>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(10);
            e.HasIndex(x => new { x.StudentId, x.SubjectId, x.Date }).IsUnique(); // idempotent ingestion (FR-15)
            e.HasIndex(x => new { x.StudentId, x.Date });
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Assessment>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(100).IsRequired();
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MaxMarks).HasPrecision(6, 2);
            e.Property(x => x.Weight).HasPrecision(5, 2);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<MarkEntry>(e =>
        {
            e.Property(x => x.MarksObtained).HasPrecision(6, 2);
            e.HasIndex(x => new { x.AssessmentId, x.StudentId }).IsUnique();
            e.HasOne(x => x.Assessment).WithMany().HasForeignKey(x => x.AssessmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ParticipationRecord>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Score).HasPrecision(5, 2);
            e.HasIndex(x => new { x.StudentId, x.Date });
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<RiskConfig>(e =>
        {
            e.Property(x => x.WAttendance).HasPrecision(3, 2);
            e.Property(x => x.WMarks).HasPrecision(3, 2);
            e.Property(x => x.WParticipation).HasPrecision(3, 2);
            e.Property(x => x.WTrend).HasPrecision(3, 2);
            e.Property(x => x.AttendanceTarget).HasPrecision(5, 2);
            e.Property(x => x.PassMark).HasPrecision(5, 2);
        });

        b.Entity<RiskSnapshot>(e =>
        {
            e.Property(x => x.Score).HasPrecision(5, 2);
            e.Property(x => x.AttendancePct).HasPrecision(5, 2);
            e.Property(x => x.AvgMarksPct).HasPrecision(5, 2);
            e.Property(x => x.ParticipationIndex).HasPrecision(5, 2);
            e.Property(x => x.TrendDelta).HasPrecision(5, 2);
            e.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.StudentId, x.ComputedOn }).IsUnique(); // one snapshot per student per day (NFR-R1)
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Alert>(e =>
        {
            e.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.AssignedCounsellorId).HasMaxLength(450);
            e.Property(x => x.EscalatedToId).HasMaxLength(450);
            e.Property(x => x.DismissReason).HasMaxLength(300);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => new { x.StudentId, x.Status });
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Snapshot).WithMany().HasForeignKey(x => x.SnapshotId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CounsellorAssignment>(e =>
        {
            e.Property(x => x.CounsellorId).HasMaxLength(450).IsRequired();
            e.HasIndex(x => new { x.CounsellorId, x.StudentId });
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CounsellorNote>(e =>
        {
            e.Property(x => x.CounsellorId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Tags).HasMaxLength(100);
            e.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Intervention>(e =>
        {
            e.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            e.Property(x => x.Outcome).HasMaxLength(1000);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ScoreAtStart).HasPrecision(5, 2);
            e.Property(x => x.ScoreAfter4Wk).HasPrecision(5, 2);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Notification>(e =>
        {
            e.Property(x => x.RecipientUserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(200);
            e.Property(x => x.Channel).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(10);
            e.HasIndex(x => new { x.Status, x.Channel }); // outbox dispatcher scan
        });
    }
}
