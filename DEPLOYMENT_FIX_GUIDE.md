# Deployment Fix Guide

## 🔴 Issues Identified

### IIS (localhost:82) - HTTP 500 Error
**Root Causes:**
1. **Database Connection**: LocalDB doesn't work on IIS - needs SQLite or SQL Server Express
2. **Logging Disabled**: Can't see actual errors
3. **Application Pool**: May need configuration changes

### SmarterASP.NET - Functions Not Working
**Root Causes:**
1. **Database Configuration**: SQL Server connection string not configured for their hosting
2. **HTTPS Issues**: Mixed content or HTTPS redirect problems
3. **Missing Environment Variables**

---

## ✅ SOLUTION 1: Fix IIS Deployment (localhost:82)

### Step 1: Use SQLite Instead of LocalDB

The publish folder has already been updated with `"UseSqlite": true` in appsettings.json.

### Step 2: Copy SQLite Database (if exists)

If you have an existing SQLite database in your dev environment:
```powershell
Copy-Item "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\AttendanceManagementSystem\attendance.db" -Destination "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\publish\" -ErrorAction SilentlyContinue
```

### Step 3: Configure IIS Application Pool

1. Open **IIS Manager**
2. Go to **Application Pools**
3. Find your app's application pool
4. **Right-click → Advanced Settings**
5. Set:
   - **.NET CLR Version**: `No Managed Code`
   - **Enable 32-Bit Applications**: `False`
   - **Identity**: `ApplicationPoolIdentity` or your user account
   
### Step 4: Set Folder Permissions

```powershell
$publishPath = "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\publish"

# Give IIS_IUSRS full control
icacls $publishPath /grant "IIS_IUSRS:(OI)(CI)F" /T
icacls "$publishPath\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

### Step 5: Check ASP.NET Core Hosting Bundle

Verify it's installed:
```powershell
Get-ItemProperty HKLM:\SOFTWARE\Microsoft\IISExpress\* | Where-Object {$_.DisplayName -like "*ASP.NET Core*"}
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Step 6: Recycle Application Pool & Check Logs

```powershell
# Recycle the app pool (replace 'YourAppPoolName' with your actual app pool name)
Restart-WebAppPool -Name "DefaultAppPool"

# Wait a moment then try accessing http://localhost:82/

# Check error logs:
Get-Content "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\publish\logs\stdout_*.log" -Tail 50
```

---

## ✅ SOLUTION 2: Fix SmarterASP.NET Deployment

### Step 1: Update appsettings.json for SmarterASP.NET

You need to get your SQL Server connection string from SmarterASP.NET control panel.

Create a new `appsettings.Production.json`:

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
    "DefaultConnection": "Server=YOUR_SMARTERASP_SERVER;Database=YOUR_DATABASE_NAME;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokensAttendanceSystem2024!@#$%^&*()",
    "Issuer": "AttendanceManagementSystem",
    "Audience": "AttendanceManagementSystemUsers",
    "ExpirationInHours": 24
  }
}
```

### Step 2: Get SmarterASP.NET Database Details

1. Login to SmarterASP.NET control panel
2. Go to **SQL Server** section
3. Note down:
   - Server Name (e.g., `SQL6031.site4now.net`)
   - Database Name (e.g., `db_a9b123_attendance`)
   - Username
   - Password

### Step 3: Update Connection String

Replace in appsettings.Production.json:
```
Server=SQL6031.site4now.net;Database=db_a9b123_attendance;User Id=db_a9b123_attendance_admin;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### Step 4: Disable HTTPS Redirect (Temporary)

In your published web.config, the app should handle HTTP properly. SmarterASP.NET might not have SSL on free tier.

### Step 5: Re-publish with Production Settings

```powershell
cd "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\AttendanceManagementSystem"

dotnet publish -c Release -o ..\publish-smarterasp
```

Then copy `appsettings.Production.json` to the publish folder and upload to SmarterASP.NET.

---

## 🔧 Quick Fix Script for IIS

Run this PowerShell script as Administrator:

```powershell
# Navigate to publish folder
$publishPath = "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\publish"
cd $publishPath

# Set permissions
Write-Host "Setting folder permissions..." -ForegroundColor Green
icacls $publishPath /grant "IIS_IUSRS:(OI)(CI)F" /T
icacls "$publishPath\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T

# Create SQLite database if it doesn't exist
if (-not (Test-Path "attendance.db")) {
    Write-Host "SQLite database will be created on first run..." -ForegroundColor Yellow
}

# Recycle app pool
Write-Host "Recycling IIS Application Pool..." -ForegroundColor Green
Restart-WebAppPool -Name "DefaultAppPool"

Write-Host "Done! Try accessing http://localhost:82/ now" -ForegroundColor Cyan
Write-Host "If still error, check logs at: $publishPath\logs\" -ForegroundColor Yellow
```

---

## 🔍 Troubleshooting

### IIS Still Shows 500 Error?

1. **Check Event Viewer**:
   - Open Event Viewer → Windows Logs → Application
   - Look for errors from "IIS AspNetCore Module"

2. **Check stdout logs**:
   ```powershell
   Get-Content "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\publish\logs\stdout_*.log"
   ```

3. **Test database connection**:
   The SQLite database should be automatically created on first run.

### SmarterASP.NET Not Working?

1. **Check if database exists**:
   - Login to control panel
   - Verify SQL Server database is created
   - Run migrations manually if needed

2. **Check application logs**:
   - Look for any error files created
   - Check if web.config is present

3. **Test basic connectivity**:
   - Try accessing: `http://muhammadrateeb-001-site1.mtempurl.com/Account/Login`
   - If you get login page, database connection is the issue

---

## 📋 Checklist

### For IIS:
- [ ] ASP.NET Core Hosting Bundle installed
- [ ] Application Pool set to "No Managed Code"
- [ ] UseSqlite: true in appsettings.json
- [ ] Folder permissions granted to IIS_IUSRS
- [ ] Logs folder created
- [ ] Application pool recycled

### For SmarterASP.NET:
- [ ] SQL Server database created in control panel
- [ ] Connection string updated with correct credentials
- [ ] appsettings.Production.json uploaded
- [ ] All files uploaded via FTP/Control Panel
- [ ] TrustServerCertificate=True in connection string

---

## 💡 Alternative: Use SQL Server Express for IIS

If you prefer SQL Server over SQLite:

1. Install SQL Server Express
2. Update connection string:
   ```json
   "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AttendanceManagementDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
3. Set `"UseSqlite": false`
4. Grant app pool identity access to SQL Server

---

Need more help? Check the logs first, then run the fixes above step by step.
