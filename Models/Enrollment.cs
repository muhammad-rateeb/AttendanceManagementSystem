using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Enrollment entity - Links students to courses
    /// </summary>
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        [Display(Name = "Student")]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
