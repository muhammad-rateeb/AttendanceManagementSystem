using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.ViewModels
{
    /// <summary>
    /// View model for student list in admin panel
    /// </summary>
    public class StudentListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public bool IsActive { get; set; }
        public int EnrolledCourses { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// View model for teacher list in admin panel
    /// </summary>
    public class TeacherListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public int AssignedCourses { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// View model for creating/editing student
    /// </summary>
    public class StudentFormViewModel
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// View model for creating/editing teacher
    /// </summary>
    public class TeacherFormViewModel
    {
        public string? Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Employee ID")]
        public string? EmployeeId { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// View model for section list
    /// </summary>
    public class SectionListViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentEnrollment { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// View model for session list
    /// </summary>
    public class SessionListViewModel
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
    }

    /// <summary>
    /// View model for timetable list
    /// </summary>
    public class TimetableListViewModel
    {
        public int TimetableId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// View model for creating/editing timetable
    /// </summary>
    public class TimetableFormViewModel
    {
        public int TimetableId { get; set; }

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Section")]
        public int SectionId { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [StringLength(100)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Dropdown lists
        public List<CourseDropdownItem> Courses { get; set; } = new();
        public List<SectionDropdownItem> Sections { get; set; } = new();
        public List<TeacherDropdownItem> Teachers { get; set; } = new();
    }

    // Note: CourseDropdownItem is defined in ReportViewModels.cs

    public class SectionDropdownItem
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
    }

    public class TeacherDropdownItem
    {
        public string TeacherId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model for student enrollment management
    /// </summary>
    public class StudentEnrollmentViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public List<EnrollmentItem> Enrollments { get; set; } = new();
        public List<CourseDropdownItem> AvailableCourses { get; set; } = new();
        public List<SectionDropdownItem> AvailableSections { get; set; } = new();
    }

    public class EnrollmentItem
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int? SectionId { get; set; }
        public string? SectionName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// View model for teacher course assignment
    /// </summary>
    public class TeacherAssignmentViewModel
    {
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string? EmployeeId { get; set; }
        public List<CourseAssignmentItem> Assignments { get; set; } = new();
        public List<CourseDropdownItem> AvailableCourses { get; set; } = new();
    }

    public class CourseAssignmentItem
    {
        public int TeacherCourseId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// View model for timetable-based attendance
    /// </summary>
    public class TimetableAttendanceViewModel
    {
        public int TimetableId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? RoomNumber { get; set; }
        public bool CanMarkAttendance { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty; // "Open", "Closed", "Upcoming"
        public int MinutesRemaining { get; set; }
    }
}
