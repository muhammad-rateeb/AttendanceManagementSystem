using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Data
{
    /// <summary>
    /// Application Database Context - Entity Framework Core
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for entities
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<TeacherCourse> TeacherCourses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Timetable> Timetables { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Course Configuration
            builder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.HasIndex(e => e.CourseCode).IsUnique();
                entity.Property(e => e.CourseCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CourseName).IsRequired().HasMaxLength(200);
            });

            // Enrollment Configuration - Student-Course relationship
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId);
                
                // Unique constraint: A student can only enroll once in a course
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TeacherCourse Configuration - Teacher-Course relationship
            builder.Entity<TeacherCourse>(entity =>
            {
                entity.HasKey(e => e.TeacherCourseId);
                
                // Unique constraint: A teacher can only be assigned once to a course
                entity.HasIndex(e => new { e.TeacherId, e.CourseId }).IsUnique();

                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.TeacherCourses)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.TeacherCourses)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Attendance Configuration
            builder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
                
                // Unique constraint: Prevent duplicate attendance for same student, course, date
                entity.HasIndex(e => new { e.StudentId, e.CourseId, e.AttendanceDate }).IsUnique();

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Attendances)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Attendances)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                // MarkedBy doesn't need navigation from ApplicationUser
                entity.HasOne(e => e.MarkedBy)
                    .WithMany()
                    .HasForeignKey(e => e.MarkedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Status)
                    .HasConversion<int>();
            });

            // Section Configuration
            builder.Entity<Section>(entity =>
            {
                entity.HasKey(e => e.SectionId);
                entity.Property(e => e.SectionName).IsRequired().HasMaxLength(50);
            });

            // Session Configuration
            builder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.SessionName).IsRequired().HasMaxLength(100);
            });

            // Timetable Configuration
            builder.Entity<Timetable>(entity =>
            {
                entity.HasKey(e => e.TimetableId);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Timetables)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Section)
                    .WithMany(s => s.Timetables)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Teacher)
                    .WithMany(t => t.Timetables)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
