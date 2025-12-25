using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Attendance status enumeration
    /// </summary>
    public enum AttendanceStatus
    {
        [Display(Name = "Present")]
        Present = 1,
        
        [Display(Name = "Absent")]
        Absent = 2,
        
        [Display(Name = "Late")]
        Late = 3
    }

    /// <summary>
    /// Attendance entity - Records student attendance for courses
    /// </summary>
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required]
        [Display(Name = "Student")]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Marked By")]
        public string MarkedById { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AttendanceStatus Status { get; set; }

        [StringLength(500)]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("MarkedById")]
        public virtual ApplicationUser MarkedBy { get; set; } = null!;
    }
}
