@echo off
setlocal enabledelayedexpansion

REM Get tokens using PowerShell
for /f "usebackq tokens=*" %%a in (`powershell -Command "$r=curl.exe -s -k -X POST -H 'Content-Type: application/json' -d '{\"email\":\"admin@maidsandnannies.local\",\"password\":\"Admin@12345\"}' https://localhost:7213/api/Auth/login 2^>$null; ($r^|ConvertFrom-Json).accessToken"`) do set ADMIN_TOKEN=%%a
echo Admin token obtained

for /f "usebackq tokens=*" %%a in (`powershell -Command "$r=curl.exe -s -k -X POST -H 'Content-Type: application/json' -d '{\"email\":\"homeowner@maidsandnannies.local\",\"password\":\"Homeowner@12345\"}' https://localhost:7213/api/Auth/login 2^>$null; ($r^|ConvertFrom-Json).accessToken"`) do set HOMEOWNER_TOKEN=%%a
echo Homeowner token obtained

for /f "usebackq tokens=*" %%a in (`powershell -Command "$r=curl.exe -s -k -X POST -H 'Content-Type: application/json' -d '{\"email\":\"worker@maidsandnannies.local\",\"password\":\"Worker@12345\"}' https://localhost:7213/api/Auth/login 2^>$null; ($r^|ConvertFrom-Json).accessToken"`) do set WORKER_TOKEN=%%a
echo Worker token obtained

echo.
echo === 1. CREATE JOB POST ===
curl.exe -s -k -X POST -H "Authorization: Bearer %HOMEOWNER_TOKEN%" -H "Content-Type: application/json" -d "{ \"description\": \"مطلوب عاملة منزلية للتنظيف والطبخ - للاتصال 01234567890\", \"monthlySalary\": 3000, \"dailySalary\": 0, \"hourlySalary\": 0, \"specialization\": 1, \"bookingType\": 1, \"commissionType\": 0, \"startDate\": \"2026-08-15T00:00:00\", \"quantity\": 1, \"currencyId\": 1 }" https://localhost:7213/api/JobPosts 2>&1
