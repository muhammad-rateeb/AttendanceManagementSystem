using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using AttendanceManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Controllers
{
    /// <summary>
    /// Student Controller - Student-specific functionality
    /// </summary>
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseService _courseService;
        private readonly IAttendanceService _attendanceService;
        private readonly IReportService _reportService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            UserManager<ApplicationUser> userManager,
            ICourseService courseService,
            IAttendanceService attendanceService,
            IReportService reportService,
            ILogger<StudentController> logger)
        {
            _userManager = userManager;
            _courseService = courseService;
            _attendanceService = attendanceService;
            _reportService = reportService;
            _logger = logger;
        }

        // GET: /Student/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboard = await _courseService.GetStudentDashboardAsync(user.Id);
            return View(dashboard);
        }

        // GET: /Student/MyCourses
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var courses = await _courseService.GetAvailableCoursesForStudentAsync(user.Id);
            return View(courses);
        }

        // GET: /Student/RegisterCourses
        [HttpGet]
        public async Task<IActionResult> RegisterCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var courses = await _courseService.GetAvailableCoursesForStudentAsync(user.Id);
            return View(courses);
        }

        // POST: /Student/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _courseService.EnrollStudentAsync(user.Id, courseId);

            if (result)
            {
                TempData["SuccessMessage"] = "Successfully enrolled in the course!";
                _logger.LogInformation("Student {StudentId} enrolled in course {CourseId}", user.Id, courseId);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to enroll in the course. You may already be enrolled.";
            }

            return RedirectToAction("RegisterCourses");
        }

        // POST: /Student/Unenroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _courseService.UnenrollStudentAsync(user.Id, courseId);

            if (result)
            {
                TempData["SuccessMessage"] = "Successfully unenrolled from the course.";
                _logger.LogInformation("Student {StudentId} unenrolled from course {CourseId}", user.Id, courseId);
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to unenroll from the course.";
            }

            return RedirectToAction("MyCourses");
        }

        // GET: /Student/Attendance
        [HttpGet]
        public async Task<IActionResult> Attendance()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var summary = await _attendanceService.GetStudentAttendanceSummaryAsync(user.Id);
            return View(summary);
        }

        // GET: /Student/AttendanceDetails/{courseId}
        [HttpGet]
        public async Task<IActionResult> AttendanceDetails(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var report = await _reportService.GetDetailedStudentReportAsync(user.Id);
            var courseReport = report.CourseReports.FirstOrDefault(c => c.CourseId == courseId);

            if (courseReport == null)
            {
                TempData["ErrorMessage"] = "Course not found or you are not enrolled.";
                return RedirectToAction("Attendance");
            }

            return View(courseReport);
        }

        // GET: /Student/Report
        [HttpGet]
        public async Task<IActionResult> Report()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var report = await _reportService.GetDetailedStudentReportAsync(user.Id);
            return View(report);
        }

        // GET: /Student/ExportReport
        [HttpGet]
        public async Task<IActionResult> ExportReport(string format)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var filter = new ReportFilterViewModel
            {
                StudentId = user.Id
            };

            var report = await _reportService.GenerateAttendanceReportAsync(filter);

            if (format.ToLower() == "excel")
            {
                var excelBytes = await _reportService.ExportToExcelAsync(report);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"MyAttendanceReport_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            else if (format.ToLower() == "pdf")
            {
                var pdfBytes = await _reportService.ExportToPdfAsync(report);
                return File(pdfBytes, "application/pdf", $"MyAttendanceReport_{DateTime.Now:yyyyMMdd}.pdf");
            }

            return BadRequest("Invalid format specified");
        }
    }
}
