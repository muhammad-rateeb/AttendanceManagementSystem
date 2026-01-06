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

            // Seed Sample Sessions
            await SeedSessionsAsync(context);

            // Seed Sample Sections
            await SeedSectionsAsync(context);

            // Seed Sample Courses
            await SeedCoursesAsync(context);

            // Seed Teacher-Course Assignments
            await SeedTeacherCoursesAsync(context, userManager);

            // Seed Student Enrollments
            await SeedEnrollmentsAsync(context, userManager);

            // Seed Timetable entries for Monday
            await SeedTimetableAsync(context, userManager);

            // Clear and re-seed Attendance Records (fresh start)
            // await ClearAndReseedAttendanceAsync(context); // Disabled to preserve attendance data
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
                // Get the current session (Spring 2025)
                var currentSession = await context.Sessions.FirstOrDefaultAsync(s => s.IsCurrent);
                var sessionId = currentSession?.SessionId;

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
                        IsActive = true,
                        SessionId = sessionId
                    },
                    new Course
                    {
                        CourseCode = "CSC-412",
                        CourseName = "Database Systems",
                        Description = "Advanced concepts in database management and SQL.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true,
                        SessionId = sessionId
                    },
                    new Course
                    {
                        CourseCode = "CSC-410",
                        CourseName = "Software Engineering",
                        Description = "Software development methodologies and best practices.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true,
                        SessionId = sessionId
                    },
                    new Course
                    {
                        CourseCode = "CSC-416",
                        CourseName = "Artificial Intelligence",
                        Description = "Introduction to AI concepts and machine learning.",
                        CreditHours = 3,
                        Semester = "Fall",
                        AcademicYear = 2024,
                        IsActive = true,
                        SessionId = sessionId
                    }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
            else
            {
                // Update existing courses with session if not set
                var currentSession = await context.Sessions.FirstOrDefaultAsync(s => s.IsCurrent);
                if (currentSession != null)
                {
                    var coursesWithoutSession = await context.Courses.Where(c => c.SessionId == null).ToListAsync();
                    foreach (var course in coursesWithoutSession)
                    {
                        course.SessionId = currentSession.SessionId;
                    }
                    await context.SaveChangesAsync();
                }
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
                var students = (await userManager.GetUsersInRoleAsync("Student")).ToList();
                var courses = await context.Courses.Take(3).ToListAsync();
                var sections = await context.Sections.OrderBy(s => s.SectionName).Take(3).ToListAsync();

                var enrollments = new List<Enrollment>();

                // Assign students to sections in round-robin fashion
                for (int i = 0; i < students.Count; i++)
                {
                    var student = students[i];
                    var section = sections[i % sections.Count];
                    foreach (var course in courses)
                    {
                        // Ensure student is not already enrolled in this course (any section)
                        bool alreadyEnrolled = enrollments.Any(e => e.StudentId == student.Id && e.CourseId == course.CourseId);
                        if (!alreadyEnrolled)
                        {
                            enrollments.Add(new Enrollment
                            {
                                StudentId = student.Id,
                                CourseId = course.CourseId,
                                SectionId = section.SectionId,
                                EnrollmentDate = DateTime.UtcNow.AddDays(-30),
                                IsActive = true
                            });
                        }
                    }
                }

                await context.Enrollments.AddRangeAsync(enrollments);
                await context.SaveChangesAsync();
            }
            else
            {
                // Update existing enrollments with section if not set (assign to Section A by default)
                var sectionA = await context.Sections.FirstOrDefaultAsync(s => s.SectionName == "Section A");
                if (sectionA != null)
                {
                    var enrollmentsWithoutSection = await context.Enrollments.Where(e => e.SectionId == null).ToListAsync();
                    foreach (var enrollment in enrollmentsWithoutSection)
                    {
                        enrollment.SectionId = sectionA.SectionId;
                    }
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Clear all existing attendance records for a fresh start
        /// </summary>
        private static async Task ClearAndReseedAttendanceAsync(ApplicationDbContext context)
        {
            // Clear all existing attendance records
            var existingAttendance = await context.Attendances.ToListAsync();
            if (existingAttendance.Any())
            {
                context.Attendances.RemoveRange(existingAttendance);
                await context.SaveChangesAsync();
            }
            // No pre-seeded attendance - will be marked fresh via teacher UI
        }

        /// <summary>
        /// Seed timetable entries for Monday classes
        /// </summary>
        private static async Task SeedTimetableAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!await context.Timetables.AnyAsync())
            {
                var teacher1 = await userManager.FindByEmailAsync("teacher1@ams.edu.pk");
                var teacher2 = await userManager.FindByEmailAsync("teacher2@ams.edu.pk");
                var teacher3 = await userManager.FindByEmailAsync("teacher3@ams.edu.pk");

                var teachers = new[] { teacher1, teacher2, teacher3 };
                var courses = await context.Courses.ToListAsync();
                var sections = await context.Sections.OrderBy(s => s.SectionName).Take(3).ToListAsync();
                var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
                var startTimes = new[] { new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0) };
                var roomNumbers = new[] { "Lab 1", "Room 101", "Room 102", "Room 103" };

                var timetables = new List<Timetable>();
                int timetableIndex = 0;
                foreach (var section in sections)
                {
                    for (int c = 0; c < courses.Count; c++)
                    {
                        for (int d = 0; d < days.Length; d++)
                        {
                            var teacher = teachers[(c + d + timetableIndex) % teachers.Length];
                            var startTime = startTimes[c % startTimes.Length];
                            var endTime = startTime.Add(new TimeSpan(1, 30, 0));
                            var room = roomNumbers[(c + d) % roomNumbers.Length];
                            timetables.Add(new Timetable
                            {
                                CourseId = courses[c].CourseId,
                                SectionId = section.SectionId,
                                TeacherId = teacher?.Id ?? string.Empty,
                                DayOfWeek = days[d],
                                StartTime = startTime,
                                EndTime = endTime,
                                RoomNumber = room,
                                IsActive = true
                            });
                        }
                    }
                    timetableIndex++;
                }

                await context.Timetables.AddRangeAsync(timetables);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSessionsAsync(ApplicationDbContext context)
        {
            if (!await context.Sessions.AnyAsync())
            {
                var sessions = new[]
                {
                    new Session
                    {
                        SessionName = "Fall 2024",
                        StartDate = new DateTime(2024, 9, 1),
                        EndDate = new DateTime(2025, 1, 15),
                        IsCurrent = false,
                        IsActive = true
                    },
                    new Session
                    {
                        SessionName = "Spring 2025",
                        StartDate = new DateTime(2025, 2, 1),
                        EndDate = new DateTime(2025, 6, 15),
                        IsCurrent = true,
                        IsActive = true
                    },
                    new Session
                    {
                        SessionName = "Fall 2025",
                        StartDate = new DateTime(2025, 9, 1),
                        EndDate = new DateTime(2026, 1, 15),
                        IsCurrent = false,
                        IsActive = true
                    }
                };

                await context.Sessions.AddRangeAsync(sessions);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSectionsAsync(ApplicationDbContext context)
        {
            if (!await context.Sections.AnyAsync())
            {
                var sections = new[]
                {
                    new Section
                    {
                        SectionName = "Section A",
                        Description = "Morning Section",
                        MaxCapacity = 40,
                        IsActive = true
                    },
                    new Section
                    {
                        SectionName = "Section B",
                        Description = "Afternoon Section",
                        MaxCapacity = 40,
                        IsActive = true
                    },
                    new Section
                    {
                        SectionName = "Section C",
                        Description = "Evening Section",
                        MaxCapacity = 35,
                        IsActive = true
                    }
                };

                await context.Sections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }
        }
    }
}
