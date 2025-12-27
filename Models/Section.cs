using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Section entity - Represents class sections (e.g., Section A, Section B)
    /// </summary>
    public class Section
    {
        public int SectionId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Section Name")]
        public string SectionName { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Max Capacity")]
        [Range(1, 500)]
        public int MaxCapacity { get; set; } = 50;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
    }
}
