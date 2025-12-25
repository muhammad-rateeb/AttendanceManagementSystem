using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Report Filter View Model
    /// </summary>
    public class ReportFilterViewModel
    {
        [Display(Name = "Course")]
        public int? CourseId { get; set; }

        [Display(Name = "Student")]
        public string? StudentId { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Minimum Attendance %")]
        [Range(0, 100)]
        public int? MinAttendancePercentage { get; set; }

        [Display(Name = "Maximum Attendance %")]
        [Range(0, 100)]
        public int? MaxAttendancePercentage { get; set; }
    }

    /// <summary>
    /// Attendance Report View Model
    /// </summary>
    public class AttendanceReportViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public List<StudentReportItem> StudentReports { get; set; } = new();
        public List<CourseReportItem> CourseReports { get; set; } = new();
        public ReportSummary Summary { get; set; } = new();
        
        // For dropdowns
        public List<CourseDropdownItem> AvailableCourses { get; set; } = new();
        public List<StudentDropdownItem> AvailableStudents { get; set; } = new();
    }

    /// <summary>
    /// Student Report Item
    /// </summary>
    public class StudentReportItem
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty; // Good, Warning, Critical
    }

    /// <summary>
    /// Course Report Item
    /// </summary>
    public class CourseReportItem
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public double AverageAttendance { get; set; }
        public int StudentsAbove75Percent { get; set; }
        public int StudentsBelow75Percent { get; set; }
    }

    /// <summary>
    /// Report Summary
    /// </summary>
    public class ReportSummary
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalClasses { get; set; }
        public double OverallAverageAttendance { get; set; }
        public int StudentsWithGoodAttendance { get; set; }
        public int StudentsWithPoorAttendance { get; set; }
        public DateTime ReportGeneratedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Course Dropdown Item
    /// </summary>
    public class CourseDropdownItem
    {
        public int CourseId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Student Dropdown Item
    /// </summary>
    public class StudentDropdownItem
    {
        public string StudentId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Detailed Student Report
    /// </summary>
    public class DetailedStudentReportViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<CourseDetailedReport> CourseReports { get; set; } = new();
        public double OverallAttendancePercentage { get; set; }
    }

    /// <summary>
    /// Course Detailed Report
    /// </summary>
    public class CourseDetailedReport
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public double AttendancePercentage { get; set; }
        public List<DailyAttendanceItem> DailyAttendance { get; set; } = new();
    }

    /// <summary>
    /// Daily Attendance Item
    /// </summary>
    public class DailyAttendanceItem
    {
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string StatusDisplay => Status.ToString();
        public string? Remarks { get; set; }
    }
}
