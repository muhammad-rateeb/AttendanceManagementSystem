using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using AttendanceManagementSystem.Services;
using AttendanceManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Controllers
{
    /// <summary>
    /// Teacher Controller - Teacher-specific functionality
    /// </summary>
    [Authorize(Roles = "Teacher,Admin")]
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseService _courseService;
        private readonly IAttendanceService _attendanceService;
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeacherController> _logger;

        // Time window for marking attendance (in minutes from lecture start)
        private const int ATTENDANCE_WINDOW_MINUTES = 10;

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            ICourseService courseService,
            IAttendanceService attendanceService,
            IReportService reportService,
            ApplicationDbContext context,
            ILogger<TeacherController> logger)
        {
            _userManager = userManager;
            _courseService = courseService;
            _attendanceService = attendanceService;
            _reportService = reportService;
            _context = context;
            _logger = logger;
        }

        // GET: /Teacher/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboard = await _courseService.GetTeacherDashboardAsync(user.Id);
            return View(dashboard);
        }

        // GET: /Teacher/MyCourses
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var courses = await _courseService.GetTeacherCoursesAsync(user.Id);
            return View(courses);
        }

        // GET: /Teacher/MarkAttendance/{courseId}
        [HttpGet]
        public async Task<IActionResult> MarkAttendance(int courseId, DateTime? date = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate teacher has access to this course
            if (!await _attendanceService.CanTeacherMarkAttendanceAsync(user.Id, courseId))
            {
                TempData["ErrorMessage"] = "You are not authorized to mark attendance for this course.";
                return RedirectToAction("MyCourses");
            }

            var attendanceDate = date ?? DateTime.Today;
            var model = await _attendanceService.GetMarkAttendanceViewModelAsync(courseId, attendanceDate);

            return View(model);
        }

        // POST: /Teacher/MarkAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(SubmitAttendanceViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate teacher has access to this course
            if (!await _attendanceService.CanTeacherMarkAttendanceAsync(user.Id, model.CourseId))
            {
                TempData["ErrorMessage"] = "You are not authorized to mark attendance for this course.";
                return RedirectToAction("MyCourses");
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await _attendanceService.GetMarkAttendanceViewModelAsync(model.CourseId, model.AttendanceDate);
                return View(viewModel);
            }

            var result = await _attendanceService.MarkAttendanceAsync(model, user.Id);

            if (result)
            {
                TempData["SuccessMessage"] = "Attendance marked successfully!";
                _logger.LogInformation("Teacher {TeacherId} marked attendance for course {CourseId} on {Date}", 
                    user.Id, model.CourseId, model.AttendanceDate);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to mark attendance. Please try again.";
            }

            return RedirectToAction("ViewAttendance", new { courseId = model.CourseId, date = model.AttendanceDate.ToString("yyyy-MM-dd") });
        }

        // GET: /Teacher/ViewAttendance/{courseId}
        [HttpGet]
        public async Task<IActionResult> ViewAttendance(int courseId, DateTime? date = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!await _attendanceService.CanTeacherMarkAttendanceAsync(user.Id, courseId))
            {
                TempData["ErrorMessage"] = "You are not authorized to view attendance for this course.";
                return RedirectToAction("MyCourses");
            }

            var attendanceDate = date ?? DateTime.Today;
            var model = await _attendanceService.GetDailyAttendanceAsync(courseId, attendanceDate);

            return View(model);
        }

        // GET: /Teacher/AttendanceHistory/{courseId}
        [HttpGet]
        public async Task<IActionResult> AttendanceHistory(int courseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!await _attendanceService.CanTeacherMarkAttendanceAsync(user.Id, courseId))
            {
                TempData["ErrorMessage"] = "You are not authorized to view attendance for this course.";
                return RedirectToAction("MyCourses");
            }

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var attendances = await _attendanceService.GetAttendanceByDateRangeAsync(courseId, start, end);
            var course = await _courseService.GetCourseByIdAsync(courseId);

            ViewData["Course"] = course;
            ViewData["StartDate"] = start;
            ViewData["EndDate"] = end;

            return View(attendances);
        }

        // GET: /Teacher/Reports
        [HttpGet]
        public async Task<IActionResult> Reports(ReportFilterViewModel? filter = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            filter ??= new ReportFilterViewModel
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today
            };

            var report = await _reportService.GenerateAttendanceReportAsync(filter, user.Id);
            return View(report);
        }

        // GET: /Teacher/ExportReport
        [HttpGet]
        public async Task<IActionResult> ExportReport(string format, int? courseId = null, string? studentId = null, 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var filter = new ReportFilterViewModel
            {
                CourseId = courseId,
                StudentId = studentId,
                StartDate = startDate ?? DateTime.Today.AddDays(-30),
                EndDate = endDate ?? DateTime.Today
            };

            var report = await _reportService.GenerateAttendanceReportAsync(filter, user.Id);

            if (format.ToLower() == "excel")
            {
                var excelBytes = await _reportService.ExportToExcelAsync(report);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"AttendanceReport_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            else if (format.ToLower() == "pdf")
            {
                var pdfBytes = await _reportService.ExportToPdfAsync(report);
                return File(pdfBytes, "application/pdf", $"AttendanceReport_{DateTime.Now:yyyyMMdd}.pdf");
            }

            return BadRequest("Invalid format specified");
        }

        // GET: /Teacher/StudentDetails/{studentId}
        [HttpGet]
        public async Task<IActionResult> StudentDetails(string studentId)
        {
            var report = await _reportService.GetDetailedStudentReportAsync(studentId);
            return View(report);
        }

        #region Timetable-Based Attendance

        // GET: /Teacher/TodaysSchedule
        [HttpGet]
        public async Task<IActionResult> TodaysSchedule()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var today = DateTime.Today.DayOfWeek;
            var currentTime = DateTime.Now.TimeOfDay;

            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Section)
                .Where(t => t.TeacherId == user.Id && t.DayOfWeek == today && t.IsActive)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            var scheduleItems = timetables.Select(t => 
            {
                var (canMark, status, minutesRemaining) = GetAttendanceStatus(t.StartTime, t.EndTime, currentTime);
                return new TimetableAttendanceViewModel
                {
                    TimetableId = t.TimetableId,
                    CourseId = t.CourseId,
                    CourseCode = t.Course.CourseCode,
                    CourseName = t.Course.CourseName,
                    SectionName = t.Section.SectionName,
                    DayOfWeek = t.DayOfWeek,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    RoomNumber = t.RoomNumber,
                    CanMarkAttendance = canMark,
                    AttendanceStatus = status,
                    MinutesRemaining = minutesRemaining
                };
            }).ToList();

            return View(scheduleItems);
        }

        // GET: /Teacher/MarkTimetableAttendance/{timetableId}
        [HttpGet]
        public async Task<IActionResult> MarkTimetableAttendance(int timetableId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var timetable = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Section)
                .FirstOrDefaultAsync(t => t.TimetableId == timetableId && t.TeacherId == user.Id);

            if (timetable == null)
            {
                TempData["ErrorMessage"] = "Timetable entry not found or you are not authorized.";
                return RedirectToAction("TodaysSchedule");
            }

            // Check if today is the correct day
            if (DateTime.Today.DayOfWeek != timetable.DayOfWeek)
            {
                TempData["ErrorMessage"] = $"This class is scheduled for {timetable.DayOfWeek}. You can only mark attendance on the scheduled day.";
                return RedirectToAction("TodaysSchedule");
            }

            // Check time window
            var currentTime = DateTime.Now.TimeOfDay;
            var (canMark, status, minutesRemaining) = GetAttendanceStatus(timetable.StartTime, timetable.EndTime, currentTime);

            if (!canMark)
            {
                if (status == "Upcoming")
                {
                    TempData["ErrorMessage"] = $"Attendance marking will open at {timetable.StartTime:hh\\:mm}. Please wait.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Attendance marking window has closed. You can only mark attendance within the first {ATTENDANCE_WINDOW_MINUTES} minutes of the lecture.";
                }
                return RedirectToAction("TodaysSchedule");
            }

            // Get enrolled students in this section for this course
            var enrolledStudents = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == timetable.CourseId && 
                           e.SectionId == timetable.SectionId && 
                           e.IsActive)
                .Select(e => e.Student)
                .ToListAsync();

            // If no section-specific enrollments, get all course enrollments
            if (!enrolledStudents.Any())
            {
                enrolledStudents = await _context.Enrollments
                    .Include(e => e.Student)
                    .Where(e => e.CourseId == timetable.CourseId && e.IsActive)
                    .Select(e => e.Student)
                    .ToListAsync();
            }

            // Get existing attendance for today
            var existingAttendance = await _context.Attendances
                .Where(a => a.CourseId == timetable.CourseId && 
                           a.AttendanceDate.Date == DateTime.Today &&
                           a.TimetableId == timetableId)
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

            var model = new MarkAttendanceViewModel
            {
                CourseId = timetable.CourseId,
                CourseName = timetable.Course.CourseName,
                CourseCode = timetable.Course.CourseCode,
                AttendanceDate = DateTime.Today,
                Students = students
            };

            ViewData["TimetableId"] = timetableId;
            ViewData["SectionName"] = timetable.Section.SectionName;
            ViewData["StartTime"] = timetable.StartTime;
            ViewData["EndTime"] = timetable.EndTime;
            ViewData["RoomNumber"] = timetable.RoomNumber;
            ViewData["MinutesRemaining"] = minutesRemaining;

            return View(model);
        }

        // POST: /Teacher/MarkTimetableAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkTimetableAttendance(SubmitAttendanceViewModel model, int timetableId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var timetable = await _context.Timetables
                .FirstOrDefaultAsync(t => t.TimetableId == timetableId && t.TeacherId == user.Id);

            if (timetable == null)
            {
                TempData["ErrorMessage"] = "Timetable entry not found or you are not authorized.";
                return RedirectToAction("TodaysSchedule");
            }

            // Check time window again
            var currentTime = DateTime.Now.TimeOfDay;
            var (canMark, _, _) = GetAttendanceStatus(timetable.StartTime, timetable.EndTime, currentTime);

            if (!canMark)
            {
                TempData["ErrorMessage"] = $"Attendance marking window has closed. You can only mark attendance within the first {ATTENDANCE_WINDOW_MINUTES} minutes of the lecture.";
                return RedirectToAction("TodaysSchedule");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                foreach (var entry in model.Entries)
                {
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => 
                            a.StudentId == entry.StudentId && 
                            a.CourseId == model.CourseId && 
                            a.AttendanceDate.Date == DateTime.Today &&
                            a.TimetableId == timetableId);

                    if (existingAttendance != null)
                    {
                        existingAttendance.Status = entry.Status;
                        existingAttendance.Remarks = entry.Remarks;
                        existingAttendance.UpdatedAt = DateTime.UtcNow;
                        _context.Attendances.Update(existingAttendance);
                    }
                    else
                    {
                        var attendance = new Attendance
                        {
                            StudentId = entry.StudentId,
                            CourseId = model.CourseId,
                            MarkedById = user.Id,
                            TimetableId = timetableId,
                            AttendanceDate = DateTime.Today,
                            Status = entry.Status,
                            Remarks = entry.Remarks,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _context.Attendances.AddAsync(attendance);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Attendance marked successfully!";
                _logger.LogInformation("Teacher {TeacherId} marked timetable attendance for timetable {TimetableId}", 
                    user.Id, timetableId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error marking timetable attendance");
                TempData["ErrorMessage"] = "Failed to mark attendance. Please try again.";
            }

            return RedirectToAction("TodaysSchedule");
        }

        /// <summary>
        /// Determines if attendance can be marked based on current time and lecture schedule
        /// </summary>
        private (bool CanMark, string Status, int MinutesRemaining) GetAttendanceStatus(TimeSpan startTime, TimeSpan endTime, TimeSpan currentTime)
        {
            var attendanceWindowEnd = startTime.Add(TimeSpan.FromMinutes(ATTENDANCE_WINDOW_MINUTES));

            if (currentTime < startTime)
            {
                // Before class starts
                var minutesUntilStart = (int)(startTime - currentTime).TotalMinutes;
                return (false, "Upcoming", minutesUntilStart);
            }
            else if (currentTime >= startTime && currentTime <= attendanceWindowEnd)
            {
                // Within the 10-minute window
                var minutesRemaining = (int)(attendanceWindowEnd - currentTime).TotalMinutes;
                return (true, "Open", minutesRemaining);
            }
            else
            {
                // After the 10-minute window
                return (false, "Closed", 0);
            }
        }

        #endregion

        #region Profile & Sections

        // GET: /Teacher/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherCourses = await _context.TeacherCourses
                .Include(tc => tc.Course)
                    .ThenInclude(c => c.Session)
                .Include(tc => tc.Course.Enrollments)
                .Where(tc => tc.TeacherId == user.Id && tc.IsActive)
                .ToListAsync();

            var courses = teacherCourses.Select(tc => new TeacherCourseInfo
            {
                CourseId = tc.CourseId,
                CourseCode = tc.Course.CourseCode,
                CourseName = tc.Course.CourseName,
                EnrolledStudents = tc.Course.Enrollments.Count(e => e.IsActive),
                SessionName = tc.Course.Session?.SessionName
            }).ToList();

            var totalStudents = courses.Sum(c => c.EnrolledStudents);

            var model = new TeacherProfileViewModel
            {
                TeacherId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                CreatedAt = user.CreatedAt,
                AssignedCoursesCount = courses.Count,
                TotalStudents = totalStudents,
                Courses = courses
            };

            return View(model);
        }

        // GET: /Teacher/MySections
        [HttpGet]
        public async Task<IActionResult> MySections()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all sections where this teacher has timetable entries
            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Enrollments.Where(e => e.IsActive))
                .Where(t => t.TeacherId == user.Id && t.IsActive)
                .ToListAsync();

            var sectionGroups = timetables
                .GroupBy(t => t.SectionId)
                .Select(g => new TeacherSectionInfo
                {
                    SectionId = g.Key,
                    SectionName = g.First().Section.SectionName,
                    Description = g.First().Section.Description,
                    TotalStudents = g.First().Section.Enrollments.Count(),
                    Courses = g.Select(t => new SectionCourseInfo
                    {
                        CourseId = t.CourseId,
                        CourseCode = t.Course.CourseCode,
                        CourseName = t.Course.CourseName,
                        StudentCount = t.Section.Enrollments.Count(e => e.CourseId == t.CourseId)
                    }).DistinctBy(c => c.CourseId).ToList()
                })
                .ToList();

            var model = new TeacherSectionsViewModel
            {
                TeacherName = user.FullName,
                Sections = sectionGroups
            };

            return View(model);
        }

        // GET: /Teacher/MyTimetable
        [HttpGet]
        public async Task<IActionResult> MyTimetable()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Enrollments.Where(e => e.IsActive))
                .Where(t => t.TeacherId == user.Id && t.IsActive)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            var items = timetables.Select(t => new TeacherTimetableItem
            {
                TimetableId = t.TimetableId,
                CourseCode = t.Course.CourseCode,
                CourseName = t.Course.CourseName,
                SectionName = t.Section.SectionName,
                DayOfWeek = t.DayOfWeek,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                RoomNumber = t.RoomNumber,
                StudentCount = t.Section.Enrollments.Count()
            }).ToList();

            var model = new TeacherTimetableViewModel
            {
                TeacherName = user.FullName,
                Monday = items.Where(i => i.DayOfWeek == DayOfWeek.Monday).ToList(),
                Tuesday = items.Where(i => i.DayOfWeek == DayOfWeek.Tuesday).ToList(),
                Wednesday = items.Where(i => i.DayOfWeek == DayOfWeek.Wednesday).ToList(),
                Thursday = items.Where(i => i.DayOfWeek == DayOfWeek.Thursday).ToList(),
                Friday = items.Where(i => i.DayOfWeek == DayOfWeek.Friday).ToList(),
                Saturday = items.Where(i => i.DayOfWeek == DayOfWeek.Saturday).ToList()
            };

            return View(model);
        }

        // GET: /Teacher/SectionStudents/{sectionId}
        [HttpGet]
        public async Task<IActionResult> SectionStudents(int sectionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Verify teacher has access to this section
            var hasAccess = await _context.Timetables
                .AnyAsync(t => t.TeacherId == user.Id && t.SectionId == sectionId && t.IsActive);

            if (!hasAccess)
            {
                TempData["ErrorMessage"] = "You don't have access to this section.";
                return RedirectToAction("MySections");
            }

            var section = await _context.Sections
                .Include(s => s.Enrollments.Where(e => e.IsActive))
                    .ThenInclude(e => e.Student)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("MySections");
            }

            ViewData["SectionName"] = section.SectionName;
            ViewData["SectionId"] = sectionId;

            return View(section.Enrollments.Where(e => e.IsActive).ToList());
        }

        #endregion
    }
}
