using AttendanceManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Data
{
    /// <summary>
    /// Database Seeder - Seeds initial data including roles, admin user, and sample data
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager);

            // Seed Sample Teachers
            await SeedTeachersAsync(userManager);

            // Seed Sample Students
            await SeedStudentsAsync(userManager);

            // Seed Sample Courses
            await SeedCoursesAsync(context);

            // Seed Teacher-Course Assignments
            await SeedTeacherCoursesAsync(context, userManager);

            // Seed Student Enrollments
            await SeedEnrollmentsAsync(context, userManager);

            // Seed Sample Attendance Records
            await SeedAttendanceAsync(context, userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Teacher", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@ams.edu.pk";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmployeeId = "ADMIN001",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedTeachersAsync(UserManager<ApplicationUser> userManager)
        {
            var teachers = new[]
            {
                new { Email = "teacher1@ams.edu.pk", FirstName = "Dr. Ahmed", LastName = "Khan", EmployeeId = "TCH001" },
                new { Email = "teacher2@ams.edu.pk", FirstName = "Dr. Fatima", LastName = "Ali", EmployeeId = "TCH002" },
                new { Email = "teacher3@ams.edu.pk", FirstName = "Prof. Muhammad", LastName = "Hassan", EmployeeId = "TCH003" }
            };

            foreach (var teacher in teachers)
            {
                if (await userManager.FindByEmailAsync(teacher.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = teacher.Email,
                        Email = teacher.Email,
                        FirstName = teacher.FirstName,
                        LastName = teacher.LastName,
                        EmployeeId = teacher.EmployeeId,
                        EmailConfirmed = true,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(user, "Teacher@123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Teacher");
                    }
                }
            }
        }

        private static async Task SeedStudentsAsync(UserManager<ApplicationUser> userManager)
        {
            var students = new[]
            {
                new { Email = "student1@ams.edu.pk", FirstName = "Ali", LastName = "Raza", RegNo = "2021-CS-001" },
                new { Email = "student2@ams.edu.pk", FirstName = "Sara", LastName = "Ahmad", RegNo = "2021-CS-002" },
                new { Email = "student3@ams.edu.pk", FirstName = "Usman", LastName = "Malik", RegNo = "2021-CS-003" },
                new { Email = "student4@ams.edu.pk", FirstName = "Ayesha", LastName = "Khan", RegNo = "2021-CS-004" },
                new { Email = "student5@ams.edu.pk", FirstName = "Bilal", LastName = "Hussain", RegNo = "2021-CS-005" }
            };

            foreach (var student in students)
            {
                if (await userManager.FindByEmailAsync(student.Email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = student.Email,
                        Email = student.Email,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        RegistrationNumber = student.RegNo,
                        EmailConfirmed = true,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(user, "Student@123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Student");
                    }
                }
            }
        }

        private static async Task SeedCoursesAsync(ApplicationDbContext context)
        {
            if (!await context.Courses.AnyAsync())
            {
                var courses = new[]
                {
                    new Course
                    {
                        CourseCode = "CSC-414",
                        CourseName = "Enterprise Application Development",
                        Description = "Learn to develop scalable enterprise applications using modern frameworks.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true
                    },
                    new Course
                    {
                        CourseCode = "CSC-412",
                        CourseName = "Database Systems",
                        Description = "Advanced concepts in database management and SQL.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true
                    },
                    new Course
                    {
                        CourseCode = "CSC-410",
                        CourseName = "Software Engineering",
                        Description = "Software development methodologies and best practices.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true
                    },
                    new Course
                    {
                        CourseCode = "CSC-416",
                        CourseName = "Artificial Intelligence",
                        Description = "Introduction to AI concepts and machine learning.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true
                    }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTeacherCoursesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!await context.TeacherCourses.AnyAsync())
            {
                var teacher1 = await userManager.FindByEmailAsync("teacher1@ams.edu.pk");
                var teacher2 = await userManager.FindByEmailAsync("teacher2@ams.edu.pk");
                var teacher3 = await userManager.FindByEmailAsync("teacher3@ams.edu.pk");

                var courses = await context.Courses.ToListAsync();

                if (teacher1 != null && teacher2 != null && teacher3 != null && courses.Any())
                {
                    var assignments = new List<TeacherCourse>
                    {
                        new TeacherCourse { TeacherId = teacher1.Id, CourseId = courses[0].CourseId, IsActive = true },
                        new TeacherCourse { TeacherId = teacher1.Id, CourseId = courses[1].CourseId, IsActive = true },
                        new TeacherCourse { TeacherId = teacher2.Id, CourseId = courses[2].CourseId, IsActive = true },
                        new TeacherCourse { TeacherId = teacher3.Id, CourseId = courses[3].CourseId, IsActive = true }
                    };

                    await context.TeacherCourses.AddRangeAsync(assignments);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedEnrollmentsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!await context.Enrollments.AnyAsync())
            {
                var students = await userManager.GetUsersInRoleAsync("Student");
                var courses = await context.Courses.Take(3).ToListAsync();

                var enrollments = new List<Enrollment>();

                foreach (var student in students)
                {
                    foreach (var course in courses)
                    {
                        enrollments.Add(new Enrollment
                        {
                            StudentId = student.Id,
                            CourseId = course.CourseId,
                            EnrollmentDate = DateTime.UtcNow.AddDays(-30),
                            IsActive = true
                        });
                    }
                }

                await context.Enrollments.AddRangeAsync(enrollments);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedAttendanceAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!await context.Attendances.AnyAsync())
            {
                var students = await userManager.GetUsersInRoleAsync("Student");
                var teacherCourses = await context.TeacherCourses
                    .Include(tc => tc.Course)
                    .ToListAsync();

                var random = new Random();
                var attendances = new List<Attendance>();

                // Generate attendance for last 10 days
                for (int day = 10; day >= 1; day--)
                {
                    var date = DateTime.Today.AddDays(-day);
                    
                    // Skip weekends
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    foreach (var tc in teacherCourses)
                    {
                        var enrolledStudents = await context.Enrollments
                            .Where(e => e.CourseId == tc.CourseId && e.IsActive)
                            .Select(e => e.StudentId)
                            .ToListAsync();

                        foreach (var studentId in enrolledStudents)
                        {
                            var statusValue = random.Next(1, 11); // 1-10
                            AttendanceStatus status;
                            
                            if (statusValue <= 7) // 70% Present
                                status = AttendanceStatus.Present;
                            else if (statusValue <= 9) // 20% Absent
                                status = AttendanceStatus.Absent;
                            else // 10% Late
                                status = AttendanceStatus.Late;

                            attendances.Add(new Attendance
                            {
                                StudentId = studentId,
                                CourseId = tc.CourseId,
                                MarkedById = tc.TeacherId,
                                AttendanceDate = date,
                                Status = status,
                                CreatedAt = date
                            });
                        }
                    }
                }

                await context.Attendances.AddRangeAsync(attendances);
                await context.SaveChangesAsync();
            }
        }
    }
}
