using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Session entity - Represents academic sessions (e.g., Fall 2024, Spring 2025)
    /// </summary>
    public class Session
    {
        public int SessionId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Session Name")]
        public string SessionName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Current Session")]
        public bool IsCurrent { get; set; } = false;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
