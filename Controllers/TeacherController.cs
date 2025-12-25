using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using AttendanceManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            ICourseService courseService,
            IAttendanceService attendanceService,
            IReportService reportService,
            ILogger<TeacherController> logger)
        {
            _userManager = userManager;
            _courseService = courseService;
            _attendanceService = attendanceService;
            _reportService = reportService;
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
    }
}
