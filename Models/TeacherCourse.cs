using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// TeacherCourse entity - Links teachers to courses they teach
    /// </summary>
    public class TeacherCourse
    {
        [Key]
        public int TeacherCourseId { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public string TeacherId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Assignment Date")]
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("TeacherId")]
        public virtual ApplicationUser Teacher { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}
