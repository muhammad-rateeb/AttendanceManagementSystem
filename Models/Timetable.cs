using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Timetable entity - Represents lecture schedule
    /// </summary>
    public class Timetable
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual Section Section { get; set; } = null!;
        public virtual ApplicationUser Teacher { get; set; } = null!;
    }
}
