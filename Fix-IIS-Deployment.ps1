# IIS Deployment Fix Script
# Run this as Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IIS Deployment Fix for AMS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$publishPath = "C:\Users\Administrator\Desktop\5th semester\5th_semester_projects\ead\publish"

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Please run this script as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit
}

Write-Host "✓ Running as Administrator" -ForegroundColor Green
Write-Host ""

# Step 1: Set Folder Permissions
Write-Host "[1/6] Setting folder permissions..." -ForegroundColor Yellow
try {
    icacls $publishPath /grant "IIS_IUSRS:(OI)(CI)F" /T > $null
    icacls "$publishPath\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T > $null
    icacls "$publishPath" /grant "IUSR:(OI)(CI)F" /T > $null
    Write-Host "✓ Folder permissions set successfully" -ForegroundColor Green
} catch {
    Write-Host "⚠ Warning: Could not set all permissions" -ForegroundColor Yellow
}
Write-Host ""

# Step 2: Check ASP.NET Core Hosting Bundle
Write-Host "[2/6] Checking ASP.NET Core Hosting Bundle..." -ForegroundColor Yellow
$hostingBundle = Get-ChildItem "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall" -ErrorAction SilentlyContinue | 
    Get-ItemProperty | 
    Where-Object { $_.DisplayName -like "*ASP.NET Core*Runtime*" }

if ($hostingBundle) {
    Write-Host "✓ ASP.NET Core Hosting Bundle is installed" -ForegroundColor Green
    Write-Host "  Version: $($hostingBundle.DisplayVersion)" -ForegroundColor Gray
} else {
    Write-Host "⚠ ASP.NET Core Hosting Bundle NOT found!" -ForegroundColor Red
    Write-Host "  Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Write-Host "  Look for 'Hosting Bundle' under .NET 8.0 Runtime" -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Check if IIS is installed and get app pool
Write-Host "[3/6] Checking IIS configuration..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    
    # Try to find the app pool for port 82
    $site = Get-Website | Where-Object { $_.Bindings.Collection.bindingInformation -like "*:82:*" }
    
    if ($site) {
        $appPoolName = $site.applicationPool
        Write-Host "✓ Found website on port 82: $($site.name)" -ForegroundColor Green
        Write-Host "  App Pool: $appPoolName" -ForegroundColor Gray
        
        # Check app pool settings
        $appPool = Get-Item "IIS:\AppPools\$appPoolName"
        $clrVersion = $appPool.managedRuntimeVersion
        
        if ($clrVersion -eq "" -or $clrVersion -eq "No Managed Code") {
            Write-Host "✓ App Pool CLR Version: No Managed Code (Correct for .NET Core)" -ForegroundColor Green
        } else {
            Write-Host "⚠ App Pool CLR Version: $clrVersion" -ForegroundColor Yellow
            Write-Host "  Should be 'No Managed Code' for ASP.NET Core" -ForegroundColor Yellow
            Write-Host "  Fixing..." -ForegroundColor Yellow
            Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name managedRuntimeVersion -Value ""
            Write-Host "✓ Fixed!" -ForegroundColor Green
        }
    } else {
        Write-Host "⚠ Could not find website on port 82" -ForegroundColor Yellow
        Write-Host "  You may need to configure IIS manually" -ForegroundColor Yellow
        $appPoolName = "DefaultAppPool"
    }
} catch {
    Write-Host "⚠ Could not access IIS configuration" -ForegroundColor Yellow
    Write-Host "  Make sure IIS is installed and this script runs as Admin" -ForegroundColor Yellow
    $appPoolName = "DefaultAppPool"
}
Write-Host ""

# Step 4: Verify SQLite DLLs
Write-Host "[4/6] Checking SQLite support..." -ForegroundColor Yellow
$sqliteDll = Test-Path "$publishPath\Microsoft.Data.Sqlite.dll"
$sqliteProvider = Test-Path "$publishPath\SQLitePCLRaw.provider.e_sqlite3.dll"

if ($sqliteDll -and $sqliteProvider) {
    Write-Host "✓ SQLite DLLs found" -ForegroundColor Green
} else {
    Write-Host "⚠ SQLite DLLs might be missing" -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Check appsettings.json
Write-Host "[5/6] Checking configuration files..." -ForegroundColor Yellow
if (Test-Path "$publishPath\appsettings.json") {
    $config = Get-Content "$publishPath\appsettings.json" | ConvertFrom-Json
    if ($config.UseSqlite -eq $true) {
        Write-Host "✓ UseSqlite is set to true" -ForegroundColor Green
    } else {
        Write-Host "⚠ UseSqlite not set or false" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ appsettings.json not found!" -ForegroundColor Red
}

if (Test-Path "$publishPath\web.config") {
    Write-Host "✓ web.config exists" -ForegroundColor Green
} else {
    Write-Host "⚠ web.config not found!" -ForegroundColor Red
}
Write-Host ""

# Step 6: Recycle App Pool
Write-Host "[6/6] Recycling application pool..." -ForegroundColor Yellow
try {
    Restart-WebAppPool -Name $appPoolName -ErrorAction Stop
    Write-Host "✓ App pool '$appPoolName' recycled" -ForegroundColor Green
} catch {
    Write-Host "⚠ Could not recycle app pool" -ForegroundColor Yellow
    Write-Host "  Try manually in IIS Manager" -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Fix Applied!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Try accessing: http://localhost:82/" -ForegroundColor White
Write-Host "2. If error persists, check logs at:" -ForegroundColor White
Write-Host "   $publishPath\logs\" -ForegroundColor Gray
Write-Host ""
Write-Host "3. To view recent logs, run:" -ForegroundColor White
Write-Host '   Get-ChildItem "' + $publishPath + '\logs\stdout_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content' -ForegroundColor Gray
Write-Host ""

# Check if logs exist
$logFiles = Get-ChildItem "$publishPath\logs\stdout_*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
if ($logFiles) {
    Write-Host "Recent log file found. Last 20 lines:" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Gray
    Get-Content $logFiles[0].FullName -Tail 20
    Write-Host "========================================" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
