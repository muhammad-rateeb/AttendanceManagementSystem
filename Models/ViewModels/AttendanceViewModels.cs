using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Mark Attendance View Model - For teacher to mark attendance
    /// </summary>
    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public List<StudentAttendanceItem> Students { get; set; } = new();
    }

    /// <summary>
    /// Individual student attendance item
    /// </summary>
    public class StudentAttendanceItem
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Remarks { get; set; }
        public bool IsAlreadyMarked { get; set; }
    }

    /// <summary>
    /// Submit Attendance View Model
    /// </summary>
    public class SubmitAttendanceViewModel
    {
        public int CourseId { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }
        
        public List<AttendanceEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Single attendance entry
    /// </summary>
    public class AttendanceEntry
    {
        public string StudentId { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Student Attendance Summary View Model
    /// </summary>
    public class StudentAttendanceSummaryViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public List<CourseAttendanceSummary> CourseAttendances { get; set; } = new();
        public double OverallAttendancePercentage { get; set; }
    }

    /// <summary>
    /// Course Attendance Summary
    /// </summary>
    public class CourseAttendanceSummary
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
        public List<AttendanceRecord> RecentAttendance { get; set; } = new();
    }

    /// <summary>
    /// Individual attendance record
    /// </summary>
    public class AttendanceRecord
    {
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Daily Attendance View Model
    /// </summary>
    public class DailyAttendanceViewModel
    {
        public DateTime Date { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public List<StudentAttendanceRecord> StudentRecords { get; set; } = new();
    }

    /// <summary>
    /// Student Attendance Record
    /// </summary>
    public class StudentAttendanceRecord
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }
}
