using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using AttendanceManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Controllers
{
    /// <summary>
    /// Admin Controller - Administrative functionality
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ICourseService _courseService;
        private readonly IReportService _reportService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ICourseService courseService,
            IReportService reportService,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _courseService = courseService;
            _reportService = reportService;
            _logger = logger;
        }

        // GET: /Admin/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var courses = await _courseService.GetAllCoursesAsync();

            ViewData["StudentCount"] = students.Count;
            ViewData["TeacherCount"] = teachers.Count;
            ViewData["CourseCount"] = courses.Count;
            ViewData["TotalEnrollments"] = await _context.Enrollments.CountAsync(e => e.IsActive);

            return View();
        }

        #region User Management

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<dynamic>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.RegistrationNumber,
                    user.EmployeeId,
                    user.IsActive,
                    Role = roles.FirstOrDefault() ?? "None"
                });
            }

            return View(userViewModels);
        }

        // GET: /Admin/CreateUser
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                RegistrationNumber = model.RegistrationNumber,
                EmployeeId = model.EmployeeId,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Users");
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.";
            return RedirectToAction("Users");
        }

        #endregion

        #region Course Management

        // GET: /Admin/Courses
        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                .Include(c => c.TeacherCourses)
                .ThenInclude(tc => tc.Teacher)
                .ToListAsync();

            var courseViewModels = courses.Select(c => new CourseViewModel
            {
                CourseId = c.CourseId,
                CourseCode = c.CourseCode,
                CourseName = c.CourseName,
                Description = c.Description,
                CreditHours = c.CreditHours,
                Semester = c.Semester,
                AcademicYear = c.AcademicYear,
                IsActive = c.IsActive,
                EnrolledStudentsCount = c.Enrollments.Count(e => e.IsActive),
                TeachersCount = c.TeacherCourses.Count(tc => tc.IsActive)
            }).ToList();

            return View(courseViewModels);
        }

        // GET: /Admin/CreateCourse
        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View(new Course { AcademicYear = DateTime.Now.Year });
        }

        // POST: /Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            // Check if course code already exists
            if (await _context.Courses.AnyAsync(c => c.CourseCode == course.CourseCode))
            {
                ModelState.AddModelError("CourseCode", "Course code already exists.");
                return View(course);
            }

            await _courseService.CreateCourseAsync(course);
            TempData["SuccessMessage"] = "Course created successfully!";
            return RedirectToAction("Courses");
        }

        // GET: /Admin/EditCourse/{id}
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction("Courses");
            }

            return View(course);
        }

        // POST: /Admin/EditCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(Course course)
        {
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            await _courseService.UpdateCourseAsync(course);
            TempData["SuccessMessage"] = "Course updated successfully!";
            return RedirectToAction("Courses");
        }

        // POST: /Admin/DeleteCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            TempData["SuccessMessage"] = "Course deleted successfully!";
            return RedirectToAction("Courses");
        }

        #endregion

        #region Teacher-Course Assignment

        // GET: /Admin/AssignTeacher/{courseId}
        [HttpGet]
        public async Task<IActionResult> AssignTeacher(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction("Courses");
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var assignedTeacherIds = course.TeacherCourses
                .Where(tc => tc.IsActive)
                .Select(tc => tc.TeacherId)
                .ToList();

            ViewData["Course"] = course;
            ViewData["AssignedTeacherIds"] = assignedTeacherIds;

            return View(teachers);
        }

        // POST: /Admin/AssignTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher(int courseId, string teacherId)
        {
            var result = await _courseService.AssignTeacherToCourseAsync(teacherId, courseId);

            if (result)
            {
                TempData["SuccessMessage"] = "Teacher assigned successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to assign teacher.";
            }

            return RedirectToAction("AssignTeacher", new { courseId });
        }

        // POST: /Admin/RemoveTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTeacher(int courseId, string teacherId)
        {
            var result = await _courseService.RemoveTeacherFromCourseAsync(teacherId, courseId);

            if (result)
            {
                TempData["SuccessMessage"] = "Teacher removed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove teacher.";
            }

            return RedirectToAction("AssignTeacher", new { courseId });
        }

        #endregion

        #region Reports

        // GET: /Admin/Reports
        [HttpGet]
        public async Task<IActionResult> Reports(ReportFilterViewModel? filter = null)
        {
            filter ??= new ReportFilterViewModel
            {
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today
            };

            var report = await _reportService.GenerateAttendanceReportAsync(filter);
            return View(report);
        }

        // GET: /Admin/ExportReport
        [HttpGet]
        public async Task<IActionResult> ExportReport(string format, int? courseId = null, string? studentId = null,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var filter = new ReportFilterViewModel
            {
                CourseId = courseId,
                StudentId = studentId,
                StartDate = startDate ?? DateTime.Today.AddDays(-30),
                EndDate = endDate ?? DateTime.Today
            };

            var report = await _reportService.GenerateAttendanceReportAsync(filter);

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

        #endregion
    }
}
