using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Course entity representing a course in the institution
    /// </summary>
    public class Course
    {
        [Key]
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

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; } = new List<TeacherCourse>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
