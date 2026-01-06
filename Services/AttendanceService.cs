using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Services
{
    public interface IAttendanceService
    {
        Task<MarkAttendanceViewModel> GetMarkAttendanceViewModelAsync(int courseId, DateTime date);
        Task<bool> MarkAttendanceAsync(SubmitAttendanceViewModel model, string teacherId);
        Task<bool> UpdateAttendanceAsync(int attendanceId, AttendanceStatus status, string? remarks);
        Task<StudentAttendanceSummaryViewModel> GetStudentAttendanceSummaryAsync(string studentId);
        Task<DailyAttendanceViewModel> GetDailyAttendanceAsync(int courseId, DateTime date);
        Task<bool> IsAttendanceMarkedAsync(int courseId, DateTime date);
        Task<List<Attendance>> GetAttendanceByDateRangeAsync(int courseId, DateTime startDate, DateTime endDate);
        Task<bool> CanTeacherMarkAttendanceAsync(string teacherId, int courseId);
    }

    /// <summary>
    /// Attendance Service - Business logic for attendance management
    /// </summary>
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MarkAttendanceViewModel> GetMarkAttendanceViewModelAsync(int courseId, DateTime date)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new ArgumentException("Course not found");

            // Get enrolled students
            var enrolledStudents = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == courseId && e.IsActive)
                .Select(e => e.Student)
                .ToListAsync();

            // Get existing attendance for the date
            var existingAttendance = await _context.Attendances
                .Where(a => a.CourseId == courseId && a.AttendanceDate.Date == date.Date)
                .ToDictionaryAsync(a => a.StudentId, a => a);

            var students = enrolledStudents.Select(s => new StudentAttendanceItem
            {
                StudentId = s.Id,
                StudentName = s.FullName,
                RegistrationNumber = s.RegistrationNumber ?? "",
                Status = existingAttendance.ContainsKey(s.Id) 
                    ? existingAttendance[s.Id].Status 
                    : AttendanceStatus.Present,
                Remarks = existingAttendance.ContainsKey(s.Id) 
                    ? existingAttendance[s.Id].Remarks 
                    : null,
                IsAlreadyMarked = existingAttendance.ContainsKey(s.Id)
            }).OrderBy(s => s.RegistrationNumber).ToList();

            return new MarkAttendanceViewModel
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                AttendanceDate = date,
                Students = students
            };
        }

        public async Task<bool> MarkAttendanceAsync(SubmitAttendanceViewModel model, string teacherId)
        {
            // Enforce timetable and time checks
            var today = model.AttendanceDate.DayOfWeek;
            var currentTime = DateTime.Now.TimeOfDay;

            // Find an active timetable for this course, teacher, and section (if available in model)
            var timetable = await _context.Timetables
                .Where(t => t.CourseId == model.CourseId
                            && t.TeacherId == teacherId
                            && t.DayOfWeek == today
                            && t.IsActive)
                .ToListAsync();

            // If section info is available in model, filter further (optional, depends on your model)
            // If you want to enforce section, add: && t.SectionId == model.SectionId

            // Check if any timetable matches the current time window
            var validTimetable = timetable.FirstOrDefault(t => currentTime >= t.StartTime && currentTime <= t.EndTime);
            if (validTimetable == null)
            {
                // Not allowed to mark attendance outside scheduled class
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check for existing attendance and update or create
                foreach (var entry in model.Entries)
                {
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a =>
                            a.StudentId == entry.StudentId &&
                            a.CourseId == model.CourseId &&
                            a.AttendanceDate.Date == model.AttendanceDate.Date);

                    if (existingAttendance != null)
                    {
                        // Update existing
                        existingAttendance.Status = entry.Status;
                        existingAttendance.Remarks = entry.Remarks;
                        existingAttendance.UpdatedAt = DateTime.UtcNow;
                        _context.Attendances.Update(existingAttendance);
                    }
                    else
                    {
                        // Create new
                        var attendance = new Attendance
                        {
                            StudentId = entry.StudentId,
                            CourseId = model.CourseId,
                            MarkedById = teacherId,
                            AttendanceDate = model.AttendanceDate.Date,
                            Status = entry.Status,
                            Remarks = entry.Remarks,
                            CreatedAt = DateTime.UtcNow,
                            TimetableId = validTimetable.TimetableId
                        };
                        await _context.Attendances.AddAsync(attendance);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateAttendanceAsync(int attendanceId, AttendanceStatus status, string? remarks)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null)
                return false;

            attendance.Status = status;
            attendance.Remarks = remarks;
            attendance.UpdatedAt = DateTime.UtcNow;

            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StudentAttendanceSummaryViewModel> GetStudentAttendanceSummaryAsync(string studentId)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId);

            if (student == null)
                throw new ArgumentException("Student not found");

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId && e.IsActive)
                .ToListAsync();

            var courseAttendances = new List<CourseAttendanceSummary>();
            var totalPresent = 0;
            var totalClasses = 0;

            foreach (var enrollment in enrollments)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.CourseId == enrollment.CourseId)
                    .OrderByDescending(a => a.AttendanceDate)
                    .ToListAsync();

                var presentCount = attendances.Count(a => a.Status == AttendanceStatus.Present);
                var absentCount = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                var lateCount = attendances.Count(a => a.Status == AttendanceStatus.Late);
                var total = attendances.Count;

                totalPresent += presentCount + lateCount; // Count late as present for overall
                totalClasses += total;

                courseAttendances.Add(new CourseAttendanceSummary
                {
                    CourseId = enrollment.CourseId,
                    CourseName = enrollment.Course.CourseName,
                    CourseCode = enrollment.Course.CourseCode,
                    TotalClasses = total,
                    PresentCount = presentCount,
                    AbsentCount = absentCount,
                    LateCount = lateCount,
                    AttendancePercentage = total > 0 ? Math.Round((double)(presentCount + lateCount) / total * 100, 2) : 0,
                    RecentAttendance = attendances.Take(5).Select(a => new AttendanceRecord
                    {
                        Date = a.AttendanceDate,
                        Status = a.Status,
                        Remarks = a.Remarks
                    }).ToList()
                });
            }

            return new StudentAttendanceSummaryViewModel
            {
                StudentId = studentId,
                StudentName = student.FullName,
                RegistrationNumber = student.RegistrationNumber ?? "",
                CourseAttendances = courseAttendances,
                OverallAttendancePercentage = totalClasses > 0 
                    ? Math.Round((double)totalPresent / totalClasses * 100, 2) 
                    : 0
            };
        }

        public async Task<DailyAttendanceViewModel> GetDailyAttendanceAsync(int courseId, DateTime date)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                throw new ArgumentException("Course not found");

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId && a.AttendanceDate.Date == date.Date)
                .ToListAsync();

            return new DailyAttendanceViewModel
            {
                Date = date,
                CourseId = courseId,
                CourseName = course.CourseName,
                TotalStudents = attendances.Count,
                PresentCount = attendances.Count(a => a.Status == AttendanceStatus.Present),
                AbsentCount = attendances.Count(a => a.Status == AttendanceStatus.Absent),
                LateCount = attendances.Count(a => a.Status == AttendanceStatus.Late),
                StudentRecords = attendances.Select(a => new StudentAttendanceRecord
                {
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    RegistrationNumber = a.Student.RegistrationNumber ?? "",
                    Status = a.Status,
                    Remarks = a.Remarks
                }).OrderBy(s => s.RegistrationNumber).ToList()
            };
        }

        public async Task<bool> IsAttendanceMarkedAsync(int courseId, DateTime date)
        {
            return await _context.Attendances
                .AnyAsync(a => a.CourseId == courseId && a.AttendanceDate.Date == date.Date);
        }

        public async Task<List<Attendance>> GetAttendanceByDateRangeAsync(int courseId, DateTime startDate, DateTime endDate)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId && 
                       a.AttendanceDate.Date >= startDate.Date && 
                       a.AttendanceDate.Date <= endDate.Date)
                .OrderBy(a => a.AttendanceDate)
                .ThenBy(a => a.Student.RegistrationNumber)
                .ToListAsync();
        }

        public async Task<bool> CanTeacherMarkAttendanceAsync(string teacherId, int courseId)
        {
            return await _context.TeacherCourses
                .AnyAsync(tc => tc.TeacherId == teacherId && tc.CourseId == courseId && tc.IsActive);
        }
    }
}
