using AttendanceManagementSystem.Data;
using AttendanceManagementSystem.Models;
using AttendanceManagementSystem.Models.ViewModels;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Services
{
    public interface IReportService
    {
        Task<AttendanceReportViewModel> GenerateAttendanceReportAsync(ReportFilterViewModel filter, string? teacherId = null);
        Task<DetailedStudentReportViewModel> GetDetailedStudentReportAsync(string studentId);
        Task<byte[]> ExportToExcelAsync(AttendanceReportViewModel report);
        Task<byte[]> ExportToPdfAsync(AttendanceReportViewModel report);
        Task<List<CourseDropdownItem>> GetCourseDropdownItemsAsync(string? teacherId = null);
        Task<List<StudentDropdownItem>> GetStudentDropdownItemsAsync(int? courseId = null);
    }

    /// <summary>
    /// Report Service - Business logic for generating reports and analytics
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AttendanceReportViewModel> GenerateAttendanceReportAsync(ReportFilterViewModel filter, string? teacherId = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .AsQueryable();

            // Apply filters
            if (filter.CourseId.HasValue)
                query = query.Where(a => a.CourseId == filter.CourseId.Value);

            if (!string.IsNullOrEmpty(filter.StudentId))
                query = query.Where(a => a.StudentId == filter.StudentId);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.AttendanceDate.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.AttendanceDate.Date <= filter.EndDate.Value.Date);

            // If teacher, only show their courses
            if (!string.IsNullOrEmpty(teacherId))
            {
                var teacherCourseIds = await _context.TeacherCourses
                    .Where(tc => tc.TeacherId == teacherId && tc.IsActive)
                    .Select(tc => tc.CourseId)
                    .ToListAsync();
                query = query.Where(a => teacherCourseIds.Contains(a.CourseId));
            }

            var attendances = await query.ToListAsync();

            // Generate student reports
            var studentReports = attendances
                .GroupBy(a => new { a.StudentId, a.CourseId })
                .Select(g =>
                {
                    var first = g.First();
                    var total = g.Count();
                    var present = g.Count(a => a.Status == AttendanceStatus.Present);
                    var absent = g.Count(a => a.Status == AttendanceStatus.Absent);
                    var late = g.Count(a => a.Status == AttendanceStatus.Late);
                    var percentage = total > 0 ? Math.Round((double)(present + late) / total * 100, 2) : 0;

                    return new StudentReportItem
                    {
                        StudentId = first.StudentId,
                        StudentName = first.Student.FullName,
                        RegistrationNumber = first.Student.RegistrationNumber ?? "",
                        CourseName = first.Course.CourseName,
                        CourseCode = first.Course.CourseCode,
                        TotalClasses = total,
                        PresentCount = present,
                        AbsentCount = absent,
                        LateCount = late,
                        AttendancePercentage = percentage,
                        AttendanceStatus = percentage >= 75 ? "Good" : percentage >= 50 ? "Warning" : "Critical"
                    };
                })
                .OrderBy(r => r.CourseCode)
                .ThenBy(r => r.RegistrationNumber)
                .ToList();

            // Apply percentage filters
            if (filter.MinAttendancePercentage.HasValue)
                studentReports = studentReports.Where(r => r.AttendancePercentage >= filter.MinAttendancePercentage.Value).ToList();

            if (filter.MaxAttendancePercentage.HasValue)
                studentReports = studentReports.Where(r => r.AttendancePercentage <= filter.MaxAttendancePercentage.Value).ToList();

            // Generate course reports
            var courseReports = attendances
                .GroupBy(a => a.CourseId)
                .Select(g =>
                {
                    var first = g.First();
                    var studentGroups = g.GroupBy(a => a.StudentId);
                    var totalStudents = studentGroups.Count();
                    var totalClasses = g.Select(a => a.AttendanceDate.Date).Distinct().Count();

                    var studentPercentages = studentGroups.Select(sg =>
                    {
                        var total = sg.Count();
                        var present = sg.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
                        return total > 0 ? (double)present / total * 100 : 0;
                    }).ToList();

                    return new CourseReportItem
                    {
                        CourseId = first.CourseId,
                        CourseName = first.Course.CourseName,
                        CourseCode = first.Course.CourseCode,
                        TotalStudents = totalStudents,
                        TotalClasses = totalClasses,
                        AverageAttendance = studentPercentages.Any() ? Math.Round(studentPercentages.Average(), 2) : 0,
                        StudentsAbove75Percent = studentPercentages.Count(p => p >= 75),
                        StudentsBelow75Percent = studentPercentages.Count(p => p < 75)
                    };
                })
                .OrderBy(r => r.CourseCode)
                .ToList();

            // Generate summary
            var summary = new ReportSummary
            {
                TotalStudents = studentReports.Select(r => r.StudentId).Distinct().Count(),
                TotalCourses = courseReports.Count,
                TotalClasses = courseReports.Sum(r => r.TotalClasses),
                OverallAverageAttendance = studentReports.Any() ? Math.Round(studentReports.Average(r => r.AttendancePercentage), 2) : 0,
                StudentsWithGoodAttendance = studentReports.Count(r => r.AttendancePercentage >= 75),
                StudentsWithPoorAttendance = studentReports.Count(r => r.AttendancePercentage < 75),
                ReportGeneratedAt = DateTime.Now
            };

            return new AttendanceReportViewModel
            {
                Filter = filter,
                StudentReports = studentReports,
                CourseReports = courseReports,
                Summary = summary,
                AvailableCourses = await GetCourseDropdownItemsAsync(teacherId),
                AvailableStudents = await GetStudentDropdownItemsAsync(filter.CourseId)
            };
        }

        public async Task<DetailedStudentReportViewModel> GetDetailedStudentReportAsync(string studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null)
                throw new ArgumentException("Student not found");

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.TeacherCourses)
                .ThenInclude(tc => tc.Teacher)
                .Where(e => e.StudentId == studentId && e.IsActive)
                .ToListAsync();

            var courseReports = new List<CourseDetailedReport>();
            double totalPercentage = 0;

            foreach (var enrollment in enrollments)
            {
                var attendances = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.CourseId == enrollment.CourseId)
                    .OrderBy(a => a.AttendanceDate)
                    .ToListAsync();

                var total = attendances.Count;
                var present = attendances.Count(a => a.Status == AttendanceStatus.Present);
                var absent = attendances.Count(a => a.Status == AttendanceStatus.Absent);
                var late = attendances.Count(a => a.Status == AttendanceStatus.Late);
                var percentage = total > 0 ? Math.Round((double)(present + late) / total * 100, 2) : 0;

                totalPercentage += percentage;

                var teacher = enrollment.Course.TeacherCourses.FirstOrDefault(tc => tc.IsActive)?.Teacher;

                courseReports.Add(new CourseDetailedReport
                {
                    CourseId = enrollment.CourseId,
                    CourseName = enrollment.Course.CourseName,
                    CourseCode = enrollment.Course.CourseCode,
                    TeacherName = teacher?.FullName ?? "Not Assigned",
                    TotalClasses = total,
                    PresentCount = present,
                    AbsentCount = absent,
                    LateCount = late,
                    AttendancePercentage = percentage,
                    DailyAttendance = attendances.Select(a => new DailyAttendanceItem
                    {
                        Date = a.AttendanceDate,
                        Status = a.Status,
                        Remarks = a.Remarks
                    }).ToList()
                });
            }

            return new DetailedStudentReportViewModel
            {
                StudentId = studentId,
                StudentName = student.FullName,
                RegistrationNumber = student.RegistrationNumber ?? "",
                Email = student.Email ?? "",
                CourseReports = courseReports,
                OverallAttendancePercentage = courseReports.Any() ? Math.Round(totalPercentage / courseReports.Count, 2) : 0
            };
        }

        public async Task<byte[]> ExportToExcelAsync(AttendanceReportViewModel report)
        {
            using var workbook = new XLWorkbook();

            // Student Reports Sheet
            var studentSheet = workbook.Worksheets.Add("Student Attendance");
            studentSheet.Cell(1, 1).Value = "Registration No";
            studentSheet.Cell(1, 2).Value = "Student Name";
            studentSheet.Cell(1, 3).Value = "Course Code";
            studentSheet.Cell(1, 4).Value = "Course Name";
            studentSheet.Cell(1, 5).Value = "Total Classes";
            studentSheet.Cell(1, 6).Value = "Present";
            studentSheet.Cell(1, 7).Value = "Absent";
            studentSheet.Cell(1, 8).Value = "Late";
            studentSheet.Cell(1, 9).Value = "Attendance %";
            studentSheet.Cell(1, 10).Value = "Status";

            var headerRange = studentSheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            int row = 2;
            foreach (var item in report.StudentReports)
            {
                studentSheet.Cell(row, 1).Value = item.RegistrationNumber;
                studentSheet.Cell(row, 2).Value = item.StudentName;
                studentSheet.Cell(row, 3).Value = item.CourseCode;
                studentSheet.Cell(row, 4).Value = item.CourseName;
                studentSheet.Cell(row, 5).Value = item.TotalClasses;
                studentSheet.Cell(row, 6).Value = item.PresentCount;
                studentSheet.Cell(row, 7).Value = item.AbsentCount;
                studentSheet.Cell(row, 8).Value = item.LateCount;
                studentSheet.Cell(row, 9).Value = item.AttendancePercentage;
                studentSheet.Cell(row, 10).Value = item.AttendanceStatus;

                // Color code based on attendance status
                if (item.AttendanceStatus == "Critical")
                    studentSheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightCoral;
                else if (item.AttendanceStatus == "Warning")
                    studentSheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightYellow;

                row++;
            }

            studentSheet.Columns().AdjustToContents();

            // Course Summary Sheet
            var courseSheet = workbook.Worksheets.Add("Course Summary");
            courseSheet.Cell(1, 1).Value = "Course Code";
            courseSheet.Cell(1, 2).Value = "Course Name";
            courseSheet.Cell(1, 3).Value = "Total Students";
            courseSheet.Cell(1, 4).Value = "Total Classes";
            courseSheet.Cell(1, 5).Value = "Average Attendance %";
            courseSheet.Cell(1, 6).Value = "Above 75%";
            courseSheet.Cell(1, 7).Value = "Below 75%";

            var courseHeaderRange = courseSheet.Range(1, 1, 1, 7);
            courseHeaderRange.Style.Font.Bold = true;
            courseHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

            row = 2;
            foreach (var item in report.CourseReports)
            {
                courseSheet.Cell(row, 1).Value = item.CourseCode;
                courseSheet.Cell(row, 2).Value = item.CourseName;
                courseSheet.Cell(row, 3).Value = item.TotalStudents;
                courseSheet.Cell(row, 4).Value = item.TotalClasses;
                courseSheet.Cell(row, 5).Value = item.AverageAttendance;
                courseSheet.Cell(row, 6).Value = item.StudentsAbove75Percent;
                courseSheet.Cell(row, 7).Value = item.StudentsBelow75Percent;
                row++;
            }

            courseSheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportToPdfAsync(AttendanceReportViewModel report)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Title
            document.Add(new Paragraph("Attendance Management System")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetBold());

            document.Add(new Paragraph("Attendance Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16));

            document.Add(new Paragraph($"Generated on: {report.Summary.ReportGeneratedAt:yyyy-MM-dd HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10));

            document.Add(new Paragraph("\n"));

            // Summary Section
            document.Add(new Paragraph("Summary")
                .SetFontSize(14)
                .SetBold());

            var summaryTable = new Table(2).UseAllAvailableWidth();
            summaryTable.AddCell("Total Students:");
            summaryTable.AddCell(report.Summary.TotalStudents.ToString());
            summaryTable.AddCell("Total Courses:");
            summaryTable.AddCell(report.Summary.TotalCourses.ToString());
            summaryTable.AddCell("Overall Average Attendance:");
            summaryTable.AddCell($"{report.Summary.OverallAverageAttendance}%");
            summaryTable.AddCell("Students with Good Attendance (≥75%):");
            summaryTable.AddCell(report.Summary.StudentsWithGoodAttendance.ToString());
            summaryTable.AddCell("Students with Poor Attendance (<75%):");
            summaryTable.AddCell(report.Summary.StudentsWithPoorAttendance.ToString());
            document.Add(summaryTable);

            document.Add(new Paragraph("\n"));

            // Student Attendance Table
            document.Add(new Paragraph("Student Attendance Details")
                .SetFontSize(14)
                .SetBold());

            var studentTable = new Table(new float[] { 2, 3, 2, 1, 1, 1, 1, 2 }).UseAllAvailableWidth();
            studentTable.AddHeaderCell("Reg No");
            studentTable.AddHeaderCell("Name");
            studentTable.AddHeaderCell("Course");
            studentTable.AddHeaderCell("Total");
            studentTable.AddHeaderCell("Present");
            studentTable.AddHeaderCell("Absent");
            studentTable.AddHeaderCell("Late");
            studentTable.AddHeaderCell("Attendance %");

            foreach (var item in report.StudentReports.Take(50)) // Limit for PDF
            {
                studentTable.AddCell(item.RegistrationNumber);
                studentTable.AddCell(item.StudentName);
                studentTable.AddCell(item.CourseCode);
                studentTable.AddCell(item.TotalClasses.ToString());
                studentTable.AddCell(item.PresentCount.ToString());
                studentTable.AddCell(item.AbsentCount.ToString());
                studentTable.AddCell(item.LateCount.ToString());
                studentTable.AddCell($"{item.AttendancePercentage}%");
            }

            document.Add(studentTable);

            document.Close();
            return stream.ToArray();
        }

        public async Task<List<CourseDropdownItem>> GetCourseDropdownItemsAsync(string? teacherId = null)
        {
            var query = _context.Courses.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(teacherId))
            {
                var teacherCourseIds = await _context.TeacherCourses
                    .Where(tc => tc.TeacherId == teacherId && tc.IsActive)
                    .Select(tc => tc.CourseId)
                    .ToListAsync();

                query = query.Where(c => teacherCourseIds.Contains(c.CourseId));
            }

            var courses = await query.ToListAsync();
            
            return courses
                .Select(c => new CourseDropdownItem
                {
                    CourseId = c.CourseId,
                    DisplayName = $"{c.CourseCode} - {c.CourseName}"
                })
                .OrderBy(c => c.DisplayName)
                .ToList();
        }

        public async Task<List<StudentDropdownItem>> GetStudentDropdownItemsAsync(int? courseId = null)
        {
            var query = _context.Users.AsQueryable();

            if (courseId.HasValue)
            {
                var enrolledStudentIds = await _context.Enrollments
                    .Where(e => e.CourseId == courseId.Value && e.IsActive)
                    .Select(e => e.StudentId)
                    .ToListAsync();

                query = query.Where(u => enrolledStudentIds.Contains(u.Id));
            }

            var students = await query.ToListAsync();

            return students
                .Where(u => !string.IsNullOrEmpty(u.RegistrationNumber)) // Only students have registration numbers
                .Select(u => new StudentDropdownItem
                {
                    StudentId = u.Id,
                    DisplayName = $"{u.RegistrationNumber} - {u.FullName}"
                })
                .OrderBy(s => s.DisplayName)
                .ToList();
        }
    }
}
