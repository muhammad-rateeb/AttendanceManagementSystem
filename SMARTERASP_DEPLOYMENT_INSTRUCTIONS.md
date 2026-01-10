# SmarterASP.NET Deployment Instructions

## 🔍 Current Status
Your site is accessible at: http://muhammadrateeb-001-site1.mtempurl.com/
But functions don't work because the database is not configured.

## ✅ SOLUTION - Follow These Steps:

### Step 1: Get SQL Server Database Credentials

1. Login to your SmarterASP.NET Control Panel
2. Go to **"MS SQL"** or **"Databases"** section
3. You should see something like:
   ```
   Server: SQL6031.site4now.net (or similar)
   Database: db_a9b123_attendance (or similar)
   Username: db_a9b123_attendance_admin (or similar)
   Password: [Your Password]
   ```
4. **Copy these details** - you'll need them in Step 2

---

### Step 2: Update Connection String

I've created a template file for you. You need to update it with YOUR database details:

**File**: `publish\appsettings.json`

Replace the ConnectionStrings section with your actual details:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "UseSqlite": false,
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_FROM_STEP1;Database=YOUR_DATABASE_FROM_STEP1;User Id=YOUR_USERNAME_FROM_STEP1;Password=YOUR_PASSWORD_FROM_STEP1;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokensAttendanceSystem2024!@#$%^&*()",
    "Issuer": "AttendanceManagementSystem",
    "Audience": "AttendanceManagementSystemUsers",
    "ExpirationInHours": 24
  }
}
```

**Example** (replace with YOUR values):
```
Server=SQL6031.site4now.net;Database=db_a9b123_attendance;User Id=db_a9b123_admin;Password=MyP@ssw0rd123;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true
```

---

### Step 3: Create Database Tables

You have two options:

#### Option A: Using SQL Server Management Studio (SSMS)

1. Install SSMS if you don't have it
2. Connect to your SmarterASP.NET database using the credentials from Step 1
3. Run the migration SQL script (I'll generate this for you)

#### Option B: Using Entity Framework Migration Script

Run this command locally to generate the SQL script:

```powershell
cd "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\AttendanceManagementSystem"

dotnet ef migrations script -o ..\smarterasp-database-script.sql
```

Then:
1. Open the generated `smarterasp-database-script.sql` file
2. Login to SmarterASP.NET Control Panel → SQL Server → Execute SQL
3. Copy and paste the entire script
4. Click Execute

---

### Step 4: Upload Updated Files

1. Update `appsettings.json` with your database details (Step 2)
2. Upload the updated file to your SmarterASP.NET root directory
3. Restart the application (in SmarterASP.NET control panel)

---

### Step 5: Test Your Site

1. Go to: http://muhammadrateeb-001-site1.mtempurl.com/Account/Login
2. Try logging in with default credentials:
   - Email: `admin@ams.edu.pk`
   - Password: `Admin@123!`

---

## 🚨 Common Issues & Solutions

### Issue 1: "Cannot connect to database"
- **Solution**: Double-check your connection string in appsettings.json
- Make sure you used the EXACT server name, database name, username, and password from Step 1

### Issue 2: "Login table does not exist"
- **Solution**: You haven't run the database migration script
- Follow Step 3 to create the tables

### Issue 3: "Invalid login credentials"
- **Solution**: The database is empty
- After creating tables, you need to seed data
- Option 1: Create an admin user manually via SQL
- Option 2: I can provide you a data seeding script

---

## 📝 Quick Checklist

- [ ] Got database credentials from SmarterASP.NET control panel
- [ ] Updated appsettings.json with correct connection string
- [ ] Set `"UseSqlite": false`
- [ ] Generated and ran database migration script
- [ ] Uploaded updated appsettings.json
- [ ] Tested the login page

---

## 🆘 Need Help?

If you're stuck, provide me with:
1. The server name from your SmarterASP.NET database settings
2. The database name
3. Any error messages you see

I'll help you create the exact configuration you need!

---

**Next**: Let me generate the database script for you now...
