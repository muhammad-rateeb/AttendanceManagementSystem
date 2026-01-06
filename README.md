# 🎓 Attendance Management System (AMS)

## CSC-414 Enterprise Application Development - Complex Computing Problem (CCP)

### University of Engineering and Technology, New Campus, Lahore
### Department of Computer Science

---

## 📋 Project Overview

The **Attendance Management System** is a comprehensive, secure, and scalable web-based solution designed to automate attendance tracking in academic institutions. Built with **ASP.NET Core 8.0 MVC** and **Entity Framework Core**, this system provides role-based dashboards for Administrators, Teachers, and Students.

### 🎯 Problem Statement
*"Automating Attendance Tracking for Academic Institutions in Lahore"*

Traditional manual attendance recording leads to errors, misplaced records, and inefficiency. This system streamlines attendance tracking by allowing teachers to log in and mark daily attendance for each course, while enabling students to register in courses and view their attendance records in real-time.

---

## ✅ CCP Requirements Implementation

| Requirement | Status | Implementation |
|------------|--------|----------------|
| **Authentication & Authorization** | ✅ Complete | JWT + Identity, RBAC, Session Management |
| **Attendance Module** | ✅ Complete | Mark attendance, duplicate prevention, real-time view |
| **Course Registration & Enrollment** | ✅ Complete | Student registration, teacher assignment validation |
| **Reporting & Analytics** | ✅ Complete | Attendance %, filtering, Excel/PDF export |
| **Deployment** | ✅ Complete | IIS, Cloud-ready configuration |

**Grade Achieved: 50/50 (Excellent)** 🏆

---

## 🚀 Features

### 🔐 Authentication & Authorization
- ✅ JWT-based authentication with secure HttpOnly cookie storage
- ✅ Role-based access control (Admin, Teacher, Student)
- ✅ Secure login/logout with session management
- ✅ Password policies (8+ chars, uppercase, lowercase, digit, special char)
- ✅ Account lockout after 5 failed attempts
- ✅ Anti-forgery token protection (CSRF)
- ✅ User registration and profile management

### 👨‍💼 Admin Module
- ✅ Dashboard with system statistics (users, courses, enrollments)
- ✅ User management (Create, Read, Update, Delete, Activate/Deactivate)
- ✅ Course management (Full CRUD operations)
- ✅ Teacher-to-course assignment
- ✅ Section and Session management
- ✅ Timetable configuration
- ✅ System-wide attendance reports

### 👨‍🏫 Teacher Module
- ✅ Personal dashboard with course overview
- ✅ Mark attendance for assigned courses only (validation enforced)
- ✅ Attendance status options: Present, Absent, Late
- ✅ Duplicate attendance prevention
- ✅ Timetable-based attendance window validation
- ✅ View and edit attendance records
- ✅ Attendance history with date filtering
- ✅ Export reports to Excel/PDF

### 👨‍🎓 Student Module
- ✅ Personal dashboard with attendance summary
- ✅ Course registration/enrollment
- ✅ View enrolled courses
- ✅ Real-time attendance records viewing
- ✅ Attendance percentage calculation
- ✅ Download personal attendance reports

### 📊 Reporting & Analytics
- ✅ Automated attendance percentage reports (per student, per course)
- ✅ Filtering by course, date range, student, minimum percentage
- ✅ Status indicators (Good ≥75%, Warning ≥50%, Critical <50%)
- ✅ **Export to Excel** using ClosedXML
- ✅ **Export to PDF** using iText7
- ✅ Detailed student-wise and course-wise reports

---

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Framework** | ASP.NET Core MVC | 8.0 |
| **ORM** | Entity Framework Core | 8.0 |
| **Database** | SQL Server / SQLite | Latest |
| **Authentication** | ASP.NET Core Identity + JWT | Built-in |
| **Frontend** | Bootstrap | 5.3 |
| **Icons** | Bootstrap Icons | Latest |
| **JavaScript** | Vanilla JS + jQuery | 3.7 |
| **Excel Export** | ClosedXML | 0.102.2 |
| **PDF Export** | iText7 | 8.0.2 |
| **IDE** | Visual Studio 2022 / VS Code | Latest |

---

## 📁 Complete Project Structure

```
AttendanceManagementSystem/
│
├── 📂 Controllers/                      # MVC Controllers (Request Handling)
│   ├── AccountController.cs             # Authentication, Login, Register, Profile (312 lines)
│   ├── AdminController.cs               # Admin operations, User/Course CRUD (1286 lines)
│   ├── HomeController.cs                # Landing page, About, Privacy
│   ├── StudentController.cs             # Student dashboard, enrollment (330 lines)
│   └── TeacherController.cs             # Teacher dashboard, attendance marking (686 lines)
│
├── 📂 Data/                             # Data Access Layer
│   ├── ApplicationDbContext.cs          # EF Core DbContext, Fluent API configs (142 lines)
│   └── DbSeeder.cs                      # Database seeding - roles, users, courses (454 lines)
│
├── 📂 Migrations/                       # EF Core Migrations
│   ├── 20251221151308_InitialCreate.cs  # Initial database schema
│   ├── 20251227125640_AddSectionSessionTimetable.cs  # Added timetable support
│   └── ApplicationDbContextModelSnapshot.cs
│
├── 📂 Models/                           # Entity Models
│   ├── ApplicationUser.cs               # Extended IdentityUser (custom fields)
│   ├── Attendance.cs                    # Attendance entity (StudentId, CourseId, Date, Status)
│   ├── Course.cs                        # Course entity (Code, Name, Credits, Semester)
│   ├── Enrollment.cs                    # Student-Course relationship
│   ├── TeacherCourse.cs                 # Teacher-Course assignment
│   ├── Section.cs                       # Class sections
│   ├── Session.cs                       # Academic sessions
│   ├── Timetable.cs                     # Class schedule
│   ├── ErrorViewModel.cs                # Error handling
│   │
│   └── 📂 ViewModels/                   # Data Transfer Objects
│       ├── AccountViewModels.cs         # Login, Register, Profile models
│       ├── AdminViewModels.cs           # Admin dashboard models
│       ├── AttendanceViewModels.cs      # Attendance marking models
│       ├── CourseViewModels.cs          # Course/enrollment models
│       └── ReportViewModels.cs          # Report generation models
│
├── 📂 Services/                         # Business Logic Layer
│   ├── AttendanceService.cs             # Attendance CRUD, validation (282 lines)
│   ├── CourseService.cs                 # Course & enrollment logic (371 lines)
│   ├── ReportService.cs                 # Reports, Excel/PDF export (434 lines)
│   └── JwtTokenService.cs               # JWT token generation & validation
│
├── 📂 Views/                            # Razor Views (UI Layer)
│   ├── 📂 Account/                      # Authentication views
│   │   ├── Login.cshtml
│   │   ├── Register.cshtml
│   │   ├── Profile.cshtml
│   │   └── AccessDenied.cshtml
│   ├── 📂 Admin/                        # Admin dashboard views
│   │   ├── Index.cshtml                 # Dashboard
│   │   ├── Users.cshtml                 # User management
│   │   ├── Courses.cshtml               # Course management
│   │   ├── CreateUser.cshtml
│   │   ├── EditUser.cshtml
│   │   └── Reports.cshtml
│   ├── 📂 Teacher/                      # Teacher views
│   │   ├── Dashboard.cshtml
│   │   ├── MyCourses.cshtml
│   │   ├── MarkAttendance.cshtml
│   │   ├── AttendanceHistory.cshtml
│   │   └── Reports.cshtml
│   ├── 📂 Student/                      # Student views
│   │   ├── Dashboard.cshtml
│   │   ├── MyCourses.cshtml
│   │   ├── RegisterCourses.cshtml
│   │   ├── Attendance.cshtml
│   │   └── Reports.cshtml
│   ├── 📂 Home/                         # Public pages
│   │   ├── Index.cshtml
│   │   ├── About.cshtml
│   │   └── Privacy.cshtml
│   ├── 📂 Shared/                       # Shared layouts & partials
│   │   ├── _Layout.cshtml               # Master layout with Bootstrap
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml
│   ├── _ViewImports.cshtml              # Tag helpers & namespaces
│   └── _ViewStart.cshtml                # Default layout assignment
│
├── 📂 wwwroot/                          # Static Files
│   ├── 📂 css/
│   │   └── site.css                     # Custom styles
│   └── 📂 js/
│       └── site.js                      # Custom JavaScript (257 lines)
│
├── 📂 Properties/
│   └── launchSettings.json              # Development server config
│
├── Program.cs                           # Application entry point & DI config (149 lines)
├── appsettings.json                     # Configuration (DB, JWT settings)
├── appsettings.Development.json         # Development overrides
├── libman.json                          # Client-side library manager
├── AttendanceManagementSystem.csproj    # Project file & NuGet packages
└── README.md                            # This file
```

---

## 📊 Database Schema

### Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│  AspNetUsers    │       │    Courses      │       │   Sections      │
│  (Identity)     │       │                 │       │                 │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ CourseId (PK)   │       │ SectionId (PK)  │
│ FirstName       │       │ CourseCode (UK) │       │ SectionName     │
│ LastName        │       │ CourseName      │       │ SessionId (FK)  │
│ Email           │       │ CreditHours     │       └─────────────────┘
│ RegistrationNo  │       │ Semester        │
│ EmployeeId      │       │ AcademicYear    │
│ IsActive        │       │ IsActive        │
└────────┬────────┘       └────────┬────────┘
         │                         │
         │    ┌────────────────────┼────────────────────┐
         │    │                    │                    │
         ▼    ▼                    ▼                    ▼
┌─────────────────┐       ┌─────────────────┐   ┌─────────────────┐
│   Enrollments   │       │ TeacherCourses  │   │   Attendances   │
├─────────────────┤       ├─────────────────┤   ├─────────────────┤
│ EnrollmentId(PK)│       │ Id (PK)         │   │ AttendanceId(PK)│
│ StudentId (FK)  │       │ TeacherId (FK)  │   │ StudentId (FK)  │
│ CourseId (FK)   │       │ CourseId (FK)   │   │ CourseId (FK)   │
│ EnrollmentDate  │       │ AssignedDate    │   │ AttendanceDate  │
│ IsActive        │       │ IsActive        │   │ Status (Enum)   │
└─────────────────┘       └─────────────────┘   │ MarkedBy (FK)   │
    UK: (StudentId,           UK: (TeacherId,   │ Remarks         │
         CourseId)                 CourseId)    └─────────────────┘
                                                    UK: (StudentId,
                                                         CourseId,
                                                         Date)
```

### Tables Summary

| Table | Description | Key Constraints |
|-------|-------------|-----------------|
| **AspNetUsers** | Extended Identity users | Email unique |
| **AspNetRoles** | Roles (Admin, Teacher, Student) | Name unique |
| **Courses** | Course information | CourseCode unique |
| **Enrollments** | Student-Course mapping | (StudentId, CourseId) unique |
| **TeacherCourses** | Teacher-Course assignment | (TeacherId, CourseId) unique |
| **Attendances** | Daily attendance records | (StudentId, CourseId, Date) unique |
| **Sections** | Class sections | - |
| **Sessions** | Academic sessions | - |
| **Timetables** | Class schedules | - |

---

## 🔐 Security Implementation

| Feature | Implementation | File Location |
|---------|---------------|---------------|
| **JWT Authentication** | Token-based auth with configurable expiry | `JwtTokenService.cs`, `Program.cs` |
| **HttpOnly Cookies** | Tokens stored securely, not accessible via JS | `AccountController.cs` |
| **Password Hashing** | BCrypt via ASP.NET Core Identity | Built-in Identity |
| **CSRF Protection** | `[ValidateAntiForgeryToken]` on all POST | All Controllers |
| **Role-Based Access** | `[Authorize(Roles = "...")]` attributes | All Controllers |
| **Account Lockout** | 5 failed attempts = 5 min lockout | `Program.cs` |
| **Input Validation** | DataAnnotations + Server-side | ViewModels |
| **SQL Injection Prevention** | Entity Framework parameterized queries | All Data Access |

---

## 🔧 Setup Instructions

### Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - LocalDB, Express, or Full version
- **Visual Studio 2022** or **VS Code** with C# Dev Kit

### Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/muhammad-rateeb/AttendanceManagementSystem.git
cd AttendanceManagementSystem

# 2. Restore packages
dotnet restore

# 3. Apply database migrations
dotnet ef database update

# 4. Run the application
dotnet run

# 5. Open in browser
# https://localhost:54030
```

### Configuration

Edit `appsettings.json` for custom settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AttendanceManagementDB;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKey",
    "Issuer": "AttendanceManagementSystem",
    "Audience": "AttendanceManagementSystemUsers",
    "ExpirationInHours": 24
  }
}
```

---

## 👤 Default Login Credentials

| Role | Email | Password |
|------|-------|----------|
| **Admin** | admin@ams.edu.pk | Admin@123! |
| **Teacher** | teacher1@ams.edu.pk | Teacher@123! |
| **Teacher** | teacher2@ams.edu.pk | Teacher@123! |
| **Student** | student1@ams.edu.pk | Student@123! |
| **Student** | student2@ams.edu.pk | Student@123! |

---

## 📈 Deployment Options

### Option 1: IIS (Windows Server)

```bash
# Publish
dotnet publish -c Release -o ./publish

# IIS Configuration
- Application Pool: .NET CLR = No Managed Code
- Install ASP.NET Core Hosting Bundle
```

### Option 2: Cloud Hosting (SmarterASP.NET / MonsterASP.NET)

1. Update connection string with cloud SQL Server
2. Run database migration script
3. Upload publish folder via FTP

### Option 3: Azure App Service

```bash
# Using Azure CLI
az webapp up --name AttendanceManagementSystem --resource-group MyRG
```

---

## 📝 API Endpoints Reference

### Authentication
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Account/Login` | GET/POST | User login |
| `/Account/Register` | GET/POST | User registration |
| `/Account/Logout` | POST | User logout |
| `/Account/Profile` | GET | View profile |

### Admin Operations
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Admin/Index` | GET | Admin dashboard |
| `/Admin/Users` | GET | List all users |
| `/Admin/CreateUser` | GET/POST | Create user |
| `/Admin/Courses` | GET | List courses |
| `/Admin/CreateCourse` | GET/POST | Create course |

### Teacher Operations
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Teacher/Dashboard` | GET | Teacher dashboard |
| `/Teacher/MyCourses` | GET | Assigned courses |
| `/Teacher/MarkAttendance/{id}` | GET/POST | Mark attendance |
| `/Teacher/ExportReport` | GET | Export report |

### Student Operations
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Student/Dashboard` | GET | Student dashboard |
| `/Student/RegisterCourses` | GET | Available courses |
| `/Student/Enroll` | POST | Enroll in course |
| `/Student/Attendance` | GET | View attendance |

---

## 🎯 CCP Attributes Alignment

| Attribute | Description | Implementation Evidence |
|-----------|-------------|------------------------|
| **A2: Depth of Analysis** | No obvious solution, requires innovative analysis | Complex attendance validation, duplicate prevention, timetable-based access |
| **A3: Depth of Knowledge** | In-depth computing knowledge required | ASP.NET Core, EF Core, JWT, Identity, MVC pattern, DI |
| **A5: Level of Problem** | Beyond standard practice | Enterprise-level security, scalability, multi-role system |
| **A8: Interdependence** | High-level with many submodules | Auth ↔ Enrollment ↔ Attendance ↔ Reporting ↔ Deployment |

---

## 📸 Screenshots

### Login Page
- Clean Bootstrap 5 design
- Email/Password validation
- Remember me functionality

### Admin Dashboard
- User statistics cards
- Quick action buttons
- System overview

### Teacher Attendance Marking
- Student list with radio buttons
- Present/Absent/Late options
- Remarks field
- Duplicate prevention

### Student Dashboard
- Attendance percentage display
- Enrolled courses list
- Quick access to reports

---

## 🧪 Testing Checklist

- [x] User Registration (Admin, Teacher, Student)
- [x] Login/Logout with session management
- [x] Role-based dashboard redirection
- [x] Course CRUD operations
- [x] Teacher-Course assignment
- [x] Student enrollment
- [x] Attendance marking with validation
- [x] Duplicate attendance prevention
- [x] Report generation
- [x] Excel export
- [x] PDF export
- [x] Password change
- [x] Profile update

---

## 👨‍💻 Developer Information

| Field | Value |
|-------|-------|
| **Developer** | Muhammad Rateeb |
| **Course** | CSC-414 Enterprise Application Development |
| **Project Type** | Complex Computing Problem (CCP) |
| **University** | UET Lahore, New Campus |
| **Semester** | 5th Semester |
| **Submission Date** | December 31, 2025 |
| **Viva Date** | January 6, 2026 |

---

## 📄 License

This project is developed for educational purposes as part of the CSC-414 course requirements at UET Lahore.

---

## 🙏 Acknowledgments

- Course Instructor for guidance
- University of Engineering and Technology, Lahore
- Microsoft Documentation for ASP.NET Core

---

*Last Updated: January 6, 2026*

**Repository:** [github.com/muhammad-rateeb/AttendanceManagementSystem](https://github.com/muhammad-rateeb/AttendanceManagementSystem)
