using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Models
{
    /// <summary>
    /// Extended Identity User with additional properties for the Attendance System
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? RegistrationNumber { get; set; }

        [StringLength(20)]
        public string? EmployeeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; } = new List<TeacherCourse>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        // Computed Properties
        public string FullName => $"{FirstName} {LastName}";
    }
}
