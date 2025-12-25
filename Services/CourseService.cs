using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Services
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course> CreateCourseAsync(Course course);
        Task<bool> UpdateCourseAsync(Course course);
        Task<bool> DeleteCourseAsync(int id);
        Task<AvailableCoursesViewModel> GetAvailableCoursesForStudentAsync(string studentId);
        Task<bool> EnrollStudentAsync(string studentId, int courseId);
        Task<bool> UnenrollStudentAsync(string studentId, int courseId);
        Task<List<TeacherCourseAssignmentViewModel>> GetTeacherCoursesAsync(string teacherId);
        Task<bool> AssignTeacherToCourseAsync(string teacherId, int courseId);
        Task<bool> RemoveTeacherFromCourseAsync(string teacherId, int courseId);
        Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId);
        Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(string teacherId);
    }

    /// <summary>
    /// Course Service - Business logic for course management
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAttendanceService _attendanceService;

        public CourseService(ApplicationDbContext context, IAttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Where(c => c.IsActive)
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                .Include(c => c.TeacherCourses)
                .ThenInclude(tc => tc.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            var existingCourse = await _context.Courses.FindAsync(course.CourseId);
            if (existingCourse == null)
                return false;

            existingCourse.CourseCode = course.CourseCode;
            existingCourse.CourseName = course.CourseName;
            existingCourse.Description = course.Description;
            existingCourse.CreditHours = course.CreditHours;
            existingCourse.Semester = course.Semester;
            existingCourse.AcademicYear = course.AcademicYear;
            existingCourse.IsActive = course.IsActive;
            existingCourse.UpdatedAt = DateTime.UtcNow;

            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return false;

            // Soft delete
            course.IsActive = false;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AvailableCoursesViewModel> GetAvailableCoursesForStudentAsync(string studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
                throw new ArgumentException("Student not found");

            var allCourses = await _context.Courses
                .Where(c => c.IsActive)
                .Include(c => c.TeacherCourses)
                .ThenInclude(tc => tc.Teacher)
                .ToListAsync();

            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId && e.IsActive)
                .Select(e => e.CourseId)
                .ToListAsync();

            var availableCourses = new List<EnrollmentViewModel>();
            var enrolledCourses = new List<EnrollmentViewModel>();

            foreach (var course in allCourses)
            {
                var teacher = course.TeacherCourses.FirstOrDefault(tc => tc.IsActive)?.Teacher;
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == course.CourseId);

                var viewModel = new EnrollmentViewModel
                {
                    CourseId = course.CourseId,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    CreditHours = course.CreditHours,
                    Semester = course.Semester,
                    TeacherName = teacher?.FullName ?? "Not Assigned",
                    IsEnrolled = enrolledCourseIds.Contains(course.CourseId),
                    EnrollmentDate = enrollment?.EnrollmentDate
                };

                if (viewModel.IsEnrolled)
                    enrolledCourses.Add(viewModel);
                else
                    availableCourses.Add(viewModel);
            }

            return new AvailableCoursesViewModel
            {
                StudentId = studentId,
                StudentName = student.FullName,
                AvailableCourses = availableCourses,
                EnrolledCourses = enrolledCourses
            };
        }

        public async Task<bool> EnrollStudentAsync(string studentId, int courseId)
        {
            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsActive)
                    return false; // Already enrolled

                // Reactivate enrollment
                existingEnrollment.IsActive = true;
                existingEnrollment.EnrollmentDate = DateTime.UtcNow;
                _context.Enrollments.Update(existingEnrollment);
            }
            else
            {
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Enrollments.AddAsync(enrollment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnenrollStudentAsync(string studentId, int courseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.IsActive);

            if (enrollment == null)
                return false;

            enrollment.IsActive = false;
            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TeacherCourseAssignmentViewModel>> GetTeacherCoursesAsync(string teacherId)
        {
            var teacherCourses = await _context.TeacherCourses
                .Include(tc => tc.Course)
                .ThenInclude(c => c.Enrollments)
                .Where(tc => tc.TeacherId == teacherId && tc.IsActive)
                .ToListAsync();

            return teacherCourses.Select(tc => new TeacherCourseAssignmentViewModel
            {
                CourseId = tc.CourseId,
                CourseCode = tc.Course.CourseCode,
                CourseName = tc.Course.CourseName,
                EnrolledStudentsCount = tc.Course.Enrollments.Count(e => e.IsActive),
                IsAssigned = true
            }).ToList();
        }

        public async Task<bool> AssignTeacherToCourseAsync(string teacherId, int courseId)
        {
            var existingAssignment = await _context.TeacherCourses
                .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.CourseId == courseId);

            if (existingAssignment != null)
            {
                if (existingAssignment.IsActive)
                    return false;

                existingAssignment.IsActive = true;
                existingAssignment.AssignmentDate = DateTime.UtcNow;
                _context.TeacherCourses.Update(existingAssignment);
            }
            else
            {
                var assignment = new TeacherCourse
                {
                    TeacherId = teacherId,
                    CourseId = courseId,
                    AssignmentDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.TeacherCourses.AddAsync(assignment);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTeacherFromCourseAsync(string teacherId, int courseId)
        {
            var assignment = await _context.TeacherCourses
                .FirstOrDefaultAsync(tc => tc.TeacherId == teacherId && tc.CourseId == courseId && tc.IsActive);

            if (assignment == null)
                return false;

            assignment.IsActive = false;
            _context.TeacherCourses.Update(assignment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StudentDashboardViewModel> GetStudentDashboardAsync(string studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
                throw new ArgumentException("Student not found");

            var summary = await _attendanceService.GetStudentAttendanceSummaryAsync(studentId);

            // Get today's attendance status
            var todaysAttendance = await _context.Attendances
                .Where(a => a.StudentId == studentId && a.AttendanceDate.Date == DateTime.Today)
                .FirstOrDefaultAsync();

            return new StudentDashboardViewModel
            {
                StudentId = studentId,
                StudentName = student.FullName,
                RegistrationNumber = student.RegistrationNumber ?? "",
                EnrolledCourses = summary.CourseAttendances,
                OverallAttendancePercentage = summary.OverallAttendancePercentage,
                TodaysStatus = todaysAttendance?.Status
            };
        }

        public async Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(string teacherId)
        {
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null)
                throw new ArgumentException("Teacher not found");

            var assignedCourses = await GetTeacherCoursesAsync(teacherId);

            var totalStudents = assignedCourses.Sum(c => c.EnrolledStudentsCount);

            // Count classes with attendance marked today
            var todaysClasses = 0;
            foreach (var course in assignedCourses)
            {
                if (await _attendanceService.IsAttendanceMarkedAsync(course.CourseId, DateTime.Today))
                    todaysClasses++;
            }

            return new TeacherDashboardViewModel
            {
                TeacherId = teacherId,
                TeacherName = teacher.FullName,
                AssignedCourses = assignedCourses,
                TotalStudents = totalStudents,
                TodaysClassesCount = todaysClasses
            };
        }
    }
}
