using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Course View Model for display and management
    /// </summary>
    public class CourseViewModel
    {
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course code is required")]
        [StringLength(20, ErrorMessage = "Course code cannot exceed 20 characters")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Range(1, 6, ErrorMessage = "Credit hours must be between 1 and 6")]
        [Display(Name = "Credit Hours")]
        public int CreditHours { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Semester")]
        public string Semester { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Academic Year")]
        public int AcademicYear { get; set; }

        public bool IsActive { get; set; } = true;

        public int EnrolledStudentsCount { get; set; }
        public int TeachersCount { get; set; }
    }

    /// <summary>
    /// Student Enrollment View Model
    /// </summary>
    public class EnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public string Semester { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public bool IsEnrolled { get; set; }
        public DateTime? EnrollmentDate { get; set; }
    }

    /// <summary>
    /// Available Courses View Model
    /// </summary>
    public class AvailableCoursesViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public List<EnrollmentViewModel> AvailableCourses { get; set; } = new();
        public List<EnrollmentViewModel> EnrolledCourses { get; set; } = new();
    }

    /// <summary>
    /// Teacher Course Assignment View Model
    /// </summary>
    public class TeacherCourseAssignmentViewModel
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int EnrolledStudentsCount { get; set; }
        public bool IsAssigned { get; set; }
    }

    /// <summary>
    /// Teacher Dashboard View Model
    /// </summary>
    public class TeacherDashboardViewModel
    {
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public List<TeacherCourseAssignmentViewModel> AssignedCourses { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TodaysClassesCount { get; set; }
    }

    /// <summary>
    /// Student Dashboard View Model
    /// </summary>
    public class StudentDashboardViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public List<CourseAttendanceSummary> EnrolledCourses { get; set; } = new();
        public double OverallAttendancePercentage { get; set; }
        public AttendanceStatus? TodaysStatus { get; set; }
    }
}
