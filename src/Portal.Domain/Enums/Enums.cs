namespace Portal.Domain.Enums;

public enum StudentStatus { Active, Graduated, Withdrawn, OnLeave }
public enum AttendanceStatus { Present, Absent, Late, Excused }
public enum AssessmentType { Quiz, Assignment, Midterm, Final, Practical }
public enum ParticipationType { ClassActivity, LmsLogin, AssignmentSubmit, Club, Event }
public enum RiskLevel { Low, Medium, High, Critical, InsufficientData }
public enum AlertStatus { New, Acknowledged, Resolved, Dismissed }
public enum NoteVisibility { Private, Shared }
public enum InterventionType { Meeting, RemedialClass, ParentCall, Referral, Other }
public enum InterventionStatus { Planned, InProgress, Done, Cancelled }
public enum NotificationChannel { InApp, Email, Sms }
public enum NotificationStatus { Pending, Sent, Failed, Read }
