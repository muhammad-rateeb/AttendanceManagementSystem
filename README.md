# Attendance Management System (AMS)

## CSC-414 Enterprise Application Development - CCP Project

### University of Engineering and Technology, Lahore

---

## 📋 Project Overview

The Attendance Management System is a comprehensive web-based solution designed to streamline attendance tracking, reporting, and course management in academic institutions. Built with ASP.NET Core MVC and Entity Framework Core, this system provides role-based access for Administrators, Teachers, and Students.

---

## 🚀 Features

### Authentication & Authorization
- ✅ JWT-based authentication with secure cookie storage
- ✅ Role-based access control (Admin, Teacher, Student)
- ✅ User registration and profile management
- ✅ Password change functionality

### Admin Module
- ✅ Dashboard with system statistics
- ✅ User management (Create, Edit, Delete, Activate/Deactivate)
- ✅ Course management (CRUD operations)
- ✅ Teacher assignment to courses
- ✅ System-wide attendance reports

### Teacher Module
- ✅ Personal dashboard with course overview
- ✅ Mark attendance for assigned courses
- ✅ View and edit attendance records
- ✅ Attendance history and filtering
- ✅ Export reports (Excel/PDF)

### Student Module
- ✅ Personal dashboard with attendance summary
- ✅ Course registration
- ✅ View personal attendance records
- ✅ Download attendance reports

### Reporting
- ✅ Comprehensive attendance reports
- ✅ Export to Excel (ClosedXML)
- ✅ Export to PDF (iText7)
- ✅ Filter by date range, course, and student

---

## 🛠️ Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | ASP.NET Core 8.0 MVC |
| ORM | Entity Framework Core 8.0 |
| Database | SQL Server |
| Authentication | ASP.NET Core Identity + JWT |
| Frontend | Bootstrap 5.3, Bootstrap Icons |
| Client-side | jQuery 3.7, JavaScript |
| Excel Export | ClosedXML 0.102.2 |
| PDF Export | iText7 8.0.2 |

---

## 📁 Project Structure

```
AttendanceManagementSystem/
├── Controllers/                 # MVC Controllers
│   ├── AccountController.cs     # Authentication & Profile
│   ├── AdminController.cs       # Admin operations
│   ├── HomeController.cs        # Landing page
│   ├── StudentController.cs     # Student operations
│   └── TeacherController.cs     # Teacher operations
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── DbSeeder.cs              # Initial data seeder
├── Models/
│   ├── ApplicationUser.cs       # Custom Identity User
│   ├── Attendance.cs            # Attendance entity
│   ├── Course.cs                # Course entity
│   ├── Enrollment.cs            # Student-Course relation
│   ├── TeacherCourse.cs         # Teacher-Course relation
│   └── ViewModels/              # View Models
├── Services/                    # Business logic layer
│   ├── Interfaces/              # Service interfaces
│   ├── AttendanceService.cs
│   ├── CourseService.cs
│   ├── JwtTokenService.cs
│   └── ReportService.cs
├── Views/                       # Razor Views
│   ├── Account/
│   ├── Admin/
│   ├── Home/
│   ├── Shared/
│   ├── Student/
│   └── Teacher/
└── wwwroot/                     # Static files
    ├── css/site.css
    └── js/site.js
```

---

## 🔧 Setup Instructions

### Prerequisites

1. **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **SQL Server** - LocalDB, Express, or Full version
3. **Visual Studio 2022** or **VS Code** with C# extension

### Installation Steps

1. **Clone/Extract the project**
   ```bash
   cd c:\path\to\AttendanceManagementSystem
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   
   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AttendanceManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   - URL: `https://localhost:5001` or `http://localhost:5000`

---

## 👤 Default Login Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@ams.edu.pk | Admin@123! |
| Teacher | teacher1@ams.edu.pk | Teacher@123! |
| Student | student1@ams.edu.pk | Student@123! |

---

## 📊 Database Schema

### Tables

1. **AspNetUsers** (Extended with ApplicationUser)
   - FirstName, LastName
   - RegistrationNumber (Students)
   - EmployeeId (Teachers/Admin)

2. **Courses**
   - CourseId, CourseCode, CourseName
   - CreditHours, Semester, AcademicYear

3. **Enrollments**
   - Links Students to Courses
   - Unique constraint: (StudentId, CourseId)

4. **TeacherCourses**
   - Links Teachers to Courses they teach
   - Unique constraint: (TeacherId, CourseId)

5. **Attendances**
   - StudentId, CourseId, AttendanceDate
   - Status: Present, Absent, Late
   - Unique constraint: (StudentId, CourseId, AttendanceDate)

---

## 🔐 Security Features

1. **JWT Authentication** - Secure token-based authentication
2. **HttpOnly Cookies** - Tokens stored in secure cookies
3. **Anti-Forgery Tokens** - CSRF protection on all forms
4. **Role-Based Authorization** - Attribute-based access control
5. **Password Hashing** - ASP.NET Core Identity password hashing
6. **Input Validation** - Both client-side and server-side validation

---

## 📈 Deployment

### IIS Deployment

1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Configure IIS:
   - Create a new website pointing to the publish folder
   - Set Application Pool to "No Managed Code"
   - Install ASP.NET Core Hosting Bundle

### Azure Deployment

1. Create Azure App Service
2. Configure connection string in Application Settings
3. Deploy using Visual Studio or Azure CLI

---

## 📝 API Endpoints

| Endpoint | Method | Description | Role |
|----------|--------|-------------|------|
| /Account/Login | POST | User login | Public |
| /Account/Register | POST | User registration | Public |
| /Admin/Users | GET | List all users | Admin |
| /Admin/Courses | GET | List all courses | Admin |
| /Teacher/MarkAttendance | POST | Mark attendance | Teacher |
| /Student/Enroll | POST | Enroll in course | Student |
| /*/ExportReport | GET | Export reports | All |

---

## 🧪 Testing

### Sample Test Scenarios

1. **Login Test** - Verify JWT token generation
2. **Attendance Marking** - Mark and verify attendance
3. **Report Export** - Generate Excel/PDF reports
4. **Course Enrollment** - Student course registration

---

## 👥 Contributors

- **Project Type**: Complex Computing Problem (CCP)
- **Course**: CSC-414 Enterprise Application Development
- **University**: UET Lahore
- **Deadline**: December 31, 2025

---

## 📄 License

This project is developed for educational purposes as part of the CSC-414 course requirements.

---

## 📞 Support

For any issues or questions, please contact the course instructor or teaching assistant.

---

*Last Updated: December 2024*
