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

        #region Student Management

        // GET: /Admin/Students
        [HttpGet]
        public async Task<IActionResult> Students()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var studentList = new List<StudentListViewModel>();

            foreach (var student in students)
            {
                var enrollmentCount = await _context.Enrollments.CountAsync(e => e.StudentId == student.Id && e.IsActive);
                studentList.Add(new StudentListViewModel
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Email = student.Email ?? "",
                    RegistrationNumber = student.RegistrationNumber,
                    IsActive = student.IsActive,
                    EnrolledCourses = enrollmentCount,
                    CreatedAt = student.CreatedAt
                });
            }

            return View(studentList.OrderBy(s => s.RegistrationNumber).ToList());
        }

        // GET: /Admin/CreateStudent
        [HttpGet]
        public IActionResult CreateStudent()
        {
            return View(new StudentFormViewModel());
        }

        // POST: /Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentFormViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new student.");
            }

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
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");
                TempData["SuccessMessage"] = "Student created successfully!";
                return RedirectToAction("Students");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Admin/EditStudent/{id}
        [HttpGet]
        public async Task<IActionResult> EditStudent(string id)
        {
            var student = await _userManager.FindByIdAsync(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Students");
            }

            var model = new StudentFormViewModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email ?? "",
                RegistrationNumber = student.RegistrationNumber,
                IsActive = student.IsActive
            };

            return View(model);
        }

        // POST: /Admin/EditStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(StudentFormViewModel model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = await _userManager.FindByIdAsync(model.Id!);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Students");
            }

            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Email = model.Email;
            student.UserName = model.Email;
            student.RegistrationNumber = model.RegistrationNumber;
            student.IsActive = model.IsActive;
            student.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(student);

            if (result.Succeeded)
            {
                // Update password if provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(student);
                    await _userManager.ResetPasswordAsync(student, token, model.Password);
                }

                TempData["SuccessMessage"] = "Student updated successfully!";
                return RedirectToAction("Students");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Admin/DeleteStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var student = await _userManager.FindByIdAsync(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Students");
            }

            // Soft delete - just deactivate
            student.IsActive = false;
            student.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(student);

            TempData["SuccessMessage"] = "Student deleted successfully!";
            return RedirectToAction("Students");
        }

        // GET: /Admin/ManageStudentEnrollments/{id}
        [HttpGet]
        public async Task<IActionResult> ManageStudentEnrollments(string id)
        {
            var student = await _userManager.FindByIdAsync(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Students");
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Section)
                .Where(e => e.StudentId == id)
                .ToListAsync();

            var enrolledCourseIds = enrollments.Where(e => e.IsActive).Select(e => e.CourseId).ToList();
            var availableCourses = await _context.Courses
                .Where(c => c.IsActive && !enrolledCourseIds.Contains(c.CourseId))
                .Select(c => new CourseDropdownItem { CourseId = c.CourseId, DisplayName = $"{c.CourseCode} - {c.CourseName}" })
                .ToListAsync();

            var sections = await _context.Sections
                .Where(s => s.IsActive)
                .Select(s => new SectionDropdownItem { SectionId = s.SectionId, SectionName = s.SectionName })
                .ToListAsync();

            var model = new StudentEnrollmentViewModel
            {
                StudentId = id,
                StudentName = student.FullName,
                RegistrationNumber = student.RegistrationNumber,
                Enrollments = enrollments.Select(e => new EnrollmentItem
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseCode = e.Course.CourseCode,
                    CourseName = e.Course.CourseName,
                    SectionId = e.SectionId,
                    SectionName = e.Section?.SectionName,
                    EnrollmentDate = e.EnrollmentDate,
                    IsActive = e.IsActive
                }).ToList(),
                AvailableCourses = availableCourses,
                AvailableSections = sections
            };

            return View(model);
        }

        // POST: /Admin/EnrollStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(string studentId, int courseId, int? sectionId)
        {
            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.SectionId = sectionId;
                existing.EnrollmentDate = DateTime.UtcNow;
                _context.Enrollments.Update(existing);
            }
            else
            {
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    SectionId = sectionId,
                    EnrollmentDate = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Enrollments.AddAsync(enrollment);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Student enrolled successfully!";
            return RedirectToAction("ManageStudentEnrollments", new { id = studentId });
        }

        // POST: /Admin/UnenrollStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollStudent(string studentId, int enrollmentId)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment != null)
            {
                enrollment.IsActive = false;
                _context.Enrollments.Update(enrollment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student unenrolled successfully!";
            }
            return RedirectToAction("ManageStudentEnrollments", new { id = studentId });
        }

        #endregion

        #region Teacher Management

        // GET: /Admin/Teachers
        [HttpGet]
        public async Task<IActionResult> Teachers()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var teacherList = new List<TeacherListViewModel>();

            foreach (var teacher in teachers)
            {
                var assignmentCount = await _context.TeacherCourses.CountAsync(tc => tc.TeacherId == teacher.Id && tc.IsActive);
                teacherList.Add(new TeacherListViewModel
                {
                    Id = teacher.Id,
                    FullName = teacher.FullName,
                    Email = teacher.Email ?? "",
                    EmployeeId = teacher.EmployeeId,
                    IsActive = teacher.IsActive,
                    AssignedCourses = assignmentCount,
                    CreatedAt = teacher.CreatedAt
                });
            }

            return View(teacherList.OrderBy(t => t.EmployeeId).ToList());
        }

        // GET: /Admin/CreateTeacher
        [HttpGet]
        public IActionResult CreateTeacher()
        {
            return View(new TeacherFormViewModel());
        }

        // POST: /Admin/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(TeacherFormViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new teacher.");
            }

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
                EmployeeId = model.EmployeeId,
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Teacher");
                TempData["SuccessMessage"] = "Teacher created successfully!";
                return RedirectToAction("Teachers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Admin/EditTeacher/{id}
        [HttpGet]
        public async Task<IActionResult> EditTeacher(string id)
        {
            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("Teachers");
            }

            var model = new TeacherFormViewModel
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email ?? "",
                EmployeeId = teacher.EmployeeId,
                IsActive = teacher.IsActive
            };

            return View(model);
        }

        // POST: /Admin/EditTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(TeacherFormViewModel model)
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var teacher = await _userManager.FindByIdAsync(model.Id!);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("Teachers");
            }

            teacher.FirstName = model.FirstName;
            teacher.LastName = model.LastName;
            teacher.Email = model.Email;
            teacher.UserName = model.Email;
            teacher.EmployeeId = model.EmployeeId;
            teacher.IsActive = model.IsActive;
            teacher.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(teacher);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(teacher);
                    await _userManager.ResetPasswordAsync(teacher, token, model.Password);
                }

                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction("Teachers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Admin/DeleteTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("Teachers");
            }

            teacher.IsActive = false;
            teacher.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(teacher);

            TempData["SuccessMessage"] = "Teacher deleted successfully!";
            return RedirectToAction("Teachers");
        }

        // GET: /Admin/ManageTeacherCourses/{id}
        [HttpGet]
        public async Task<IActionResult> ManageTeacherCourses(string id)
        {
            var teacher = await _userManager.FindByIdAsync(id);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction("Teachers");
            }

            var assignments = await _context.TeacherCourses
                .Include(tc => tc.Course)
                .Where(tc => tc.TeacherId == id)
                .ToListAsync();

            var assignedCourseIds = assignments.Where(a => a.IsActive).Select(a => a.CourseId).ToList();
            var availableCourses = await _context.Courses
                .Where(c => c.IsActive && !assignedCourseIds.Contains(c.CourseId))
                .Select(c => new CourseDropdownItem { CourseId = c.CourseId, DisplayName = $"{c.CourseCode} - {c.CourseName}" })
                .ToListAsync();

            var model = new TeacherAssignmentViewModel
            {
                TeacherId = id,
                TeacherName = teacher.FullName,
                EmployeeId = teacher.EmployeeId,
                Assignments = assignments.Select(a => new CourseAssignmentItem
                {
                    TeacherCourseId = a.TeacherCourseId,
                    CourseId = a.CourseId,
                    CourseCode = a.Course.CourseCode,
                    CourseName = a.Course.CourseName,
                    AssignmentDate = a.AssignmentDate,
                    IsActive = a.IsActive
                }).ToList(),
                AvailableCourses = availableCourses
            };

            return View(model);
        }

        // POST: /Admin/AssignCourseToTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCourseToTeacher(string teacherId, int courseId)
        {
            var result = await _courseService.AssignTeacherToCourseAsync(teacherId, courseId);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Course assigned successfully!" : "Failed to assign course.";
            return RedirectToAction("ManageTeacherCourses", new { id = teacherId });
        }

        // POST: /Admin/RemoveCourseFromTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCourseFromTeacher(string teacherId, int courseId)
        {
            var result = await _courseService.RemoveTeacherFromCourseAsync(teacherId, courseId);
            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result ? "Course removed successfully!" : "Failed to remove course.";
            return RedirectToAction("ManageTeacherCourses", new { id = teacherId });
        }

        #endregion

        #region Section Management

        // GET: /Admin/Sections
        [HttpGet]
        public async Task<IActionResult> Sections()
        {
            var sections = await _context.Sections
                .Select(s => new SectionListViewModel
                {
                    SectionId = s.SectionId,
                    SectionName = s.SectionName,
                    Description = s.Description,
                    MaxCapacity = s.MaxCapacity,
                    CurrentEnrollment = s.Enrollments.Count(e => e.IsActive),
                    IsActive = s.IsActive
                })
                .OrderBy(s => s.SectionName)
                .ToListAsync();

            return View(sections);
        }

        // GET: /Admin/CreateSection
        [HttpGet]
        public IActionResult CreateSection()
        {
            return View(new Section());
        }

        // POST: /Admin/CreateSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSection(Section section)
        {
            if (!ModelState.IsValid)
            {
                return View(section);
            }

            section.CreatedAt = DateTime.UtcNow;
            await _context.Sections.AddAsync(section);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Section created successfully!";
            return RedirectToAction("Sections");
        }

        // GET: /Admin/EditSection/{id}
        [HttpGet]
        public async Task<IActionResult> EditSection(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("Sections");
            }
            return View(section);
        }

        // POST: /Admin/EditSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSection(Section section)
        {
            if (!ModelState.IsValid)
            {
                return View(section);
            }

            var existing = await _context.Sections.FindAsync(section.SectionId);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("Sections");
            }

            existing.SectionName = section.SectionName;
            existing.Description = section.Description;
            existing.MaxCapacity = section.MaxCapacity;
            existing.IsActive = section.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Sections.Update(existing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Section updated successfully!";
            return RedirectToAction("Sections");
        }

        // POST: /Admin/DeleteSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section != null)
            {
                section.IsActive = false;
                section.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Section deleted successfully!";
            }
            return RedirectToAction("Sections");
        }

        #endregion

        #region Session Management

        // GET: /Admin/Sessions
        [HttpGet]
        public async Task<IActionResult> Sessions()
        {
            var sessions = await _context.Sessions
                .Select(s => new SessionListViewModel
                {
                    SessionId = s.SessionId,
                    SessionName = s.SessionName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsCurrent = s.IsCurrent,
                    IsActive = s.IsActive,
                    CourseCount = s.Courses.Count
                })
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return View(sessions);
        }

        // GET: /Admin/CreateSession
        [HttpGet]
        public IActionResult CreateSession()
        {
            return View(new Session { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(4) });
        }

        // POST: /Admin/CreateSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSession(Session session)
        {
            if (!ModelState.IsValid)
            {
                return View(session);
            }

            if (session.IsCurrent)
            {
                // Unset other current sessions
                var currentSessions = await _context.Sessions.Where(s => s.IsCurrent).ToListAsync();
                foreach (var s in currentSessions)
                {
                    s.IsCurrent = false;
                }
            }

            session.CreatedAt = DateTime.UtcNow;
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Session created successfully!";
            return RedirectToAction("Sessions");
        }

        // GET: /Admin/EditSession/{id}
        [HttpGet]
        public async Task<IActionResult> EditSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                TempData["ErrorMessage"] = "Session not found.";
                return RedirectToAction("Sessions");
            }
            return View(session);
        }

        // POST: /Admin/EditSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(Session session)
        {
            if (!ModelState.IsValid)
            {
                return View(session);
            }

            var existing = await _context.Sessions.FindAsync(session.SessionId);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Session not found.";
                return RedirectToAction("Sessions");
            }

            if (session.IsCurrent && !existing.IsCurrent)
            {
                var currentSessions = await _context.Sessions.Where(s => s.IsCurrent && s.SessionId != session.SessionId).ToListAsync();
                foreach (var s in currentSessions)
                {
                    s.IsCurrent = false;
                }
            }

            existing.SessionName = session.SessionName;
            existing.StartDate = session.StartDate;
            existing.EndDate = session.EndDate;
            existing.IsCurrent = session.IsCurrent;
            existing.IsActive = session.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Sessions.Update(existing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Session updated successfully!";
            return RedirectToAction("Sessions");
        }

        // POST: /Admin/DeleteSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                session.IsActive = false;
                session.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Session deleted successfully!";
            }
            return RedirectToAction("Sessions");
        }

        #endregion

        #region Timetable Management

        // GET: /Admin/Timetables
        [HttpGet]
        public async Task<IActionResult> Timetables()
        {
            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Section)
                .Include(t => t.Teacher)
                .Select(t => new TimetableListViewModel
                {
                    TimetableId = t.TimetableId,
                    CourseCode = t.Course.CourseCode,
                    CourseName = t.Course.CourseName,
                    SectionName = t.Section.SectionName,
                    TeacherName = t.Teacher.FirstName + " " + t.Teacher.LastName,
                    DayOfWeek = t.DayOfWeek,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    RoomNumber = t.RoomNumber,
                    IsActive = t.IsActive
                })
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            return View(timetables);
        }

        // GET: /Admin/CreateTimetable
        [HttpGet]
        public async Task<IActionResult> CreateTimetable()
        {
            var model = new TimetableFormViewModel
            {
                Courses = await _context.Courses
                    .Where(c => c.IsActive)
                    .Select(c => new CourseDropdownItem { CourseId = c.CourseId, DisplayName = $"{c.CourseCode} - {c.CourseName}" })
                    .ToListAsync(),
                Sections = await _context.Sections
                    .Where(s => s.IsActive)
                    .Select(s => new SectionDropdownItem { SectionId = s.SectionId, SectionName = s.SectionName })
                    .ToListAsync(),
                Teachers = await GetTeacherDropdownAsync()
            };
            return View(model);
        }

        // POST: /Admin/CreateTimetable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTimetable(TimetableFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _context.Courses.Where(c => c.IsActive).Select(c => new CourseDropdownItem { CourseId = c.CourseId, DisplayName = $"{c.CourseCode} - {c.CourseName}" }).ToListAsync();
                model.Sections = await _context.Sections.Where(s => s.IsActive).Select(s => new SectionDropdownItem { SectionId = s.SectionId, SectionName = s.SectionName }).ToListAsync();
                model.Teachers = await GetTeacherDropdownAsync();
                return View(model);
            }

            var timetable = new Timetable
            {
                CourseId = model.CourseId,
                SectionId = model.SectionId,
                TeacherId = model.TeacherId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                RoomNumber = model.RoomNumber,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Timetables.AddAsync(timetable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Timetable entry created successfully!";
            return RedirectToAction("Timetables");
        }

        // GET: /Admin/EditTimetable/{id}
        [HttpGet]
        public async Task<IActionResult> EditTimetable(int id)
        {
            var timetable = await _context.Timetables.FindAsync(id);
            if (timetable == null)
            {
                TempData["ErrorMessage"] = "Timetable entry not found.";
                return RedirectToAction("Timetables");
            }

            var model = new TimetableFormViewModel
            {
                TimetableId = timetable.TimetableId,
                CourseId = timetable.CourseId,
                SectionId = timetable.SectionId,
                TeacherId = timetable.TeacherId,
                DayOfWeek = timetable.DayOfWeek,
                StartTime = timetable.StartTime,
                EndTime = timetable.EndTime,
                RoomNumber = timetable.RoomNumber,
                IsActive = timetable.IsActive,
                Courses = await _context.Courses.Where(c => c.IsActive).Select(c => new CourseDropdownItem { CourseId = c.CourseId, DisplayName = $"{c.CourseCode} - {c.CourseName}" }).ToListAsync(),
                Sections = await _context.Sections.Where(s => s.IsActive).Select(s => new SectionDropdownItem { SectionId = s.SectionId, SectionName = s.SectionName }).ToListAsync(),
                Teachers = await GetTeacherDropdownAsync()
            };

            return View(model);
        }

        // POST: /Admin/EditTimetable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTimetable(TimetableFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Courses = await _context.Courses.Where(c => c.IsActive).Select(c => new CourseDropdownItem { CourseId = c.CourseId, DisplayName = $"{c.CourseCode} - {c.CourseName}" }).ToListAsync();
                model.Sections = await _context.Sections.Where(s => s.IsActive).Select(s => new SectionDropdownItem { SectionId = s.SectionId, SectionName = s.SectionName }).ToListAsync();
                model.Teachers = await GetTeacherDropdownAsync();
                return View(model);
            }

            var timetable = await _context.Timetables.FindAsync(model.TimetableId);
            if (timetable == null)
            {
                TempData["ErrorMessage"] = "Timetable entry not found.";
                return RedirectToAction("Timetables");
            }

            timetable.CourseId = model.CourseId;
            timetable.SectionId = model.SectionId;
            timetable.TeacherId = model.TeacherId;
            timetable.DayOfWeek = model.DayOfWeek;
            timetable.StartTime = model.StartTime;
            timetable.EndTime = model.EndTime;
            timetable.RoomNumber = model.RoomNumber;
            timetable.IsActive = model.IsActive;
            timetable.UpdatedAt = DateTime.UtcNow;

            _context.Timetables.Update(timetable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Timetable entry updated successfully!";
            return RedirectToAction("Timetables");
        }

        // POST: /Admin/DeleteTimetable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimetable(int id)
        {
            var timetable = await _context.Timetables.FindAsync(id);
            if (timetable != null)
            {
                timetable.IsActive = false;
                timetable.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Timetable entry deleted successfully!";
            }
            return RedirectToAction("Timetables");
        }

        private async Task<List<TeacherDropdownItem>> GetTeacherDropdownAsync()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            return teachers.Where(t => t.IsActive).Select(t => new TeacherDropdownItem { TeacherId = t.Id, FullName = t.FullName }).ToList();
        }

        #endregion
    }
}
