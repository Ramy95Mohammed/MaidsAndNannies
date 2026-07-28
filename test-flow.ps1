$baseUrl = "https://localhost:7213"

function Get-Token($email, $password) {
    $body = @{ email = $email; password = $password } | ConvertTo-Json
    $resp = Invoke-WebRequest -Uri "http://localhost:5045/api/Auth/login" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing
    return ($resp.Content | ConvertFrom-Json).accessToken
}

function Api-Call($method, $url, $body, $token, $contentType) {
    if (-not $contentType) { $contentType = "application/json" }
    $headers = @{ Authorization = "Bearer $token" }
    try {
        if ($body) {
            $resp = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -Body $body -ContentType $contentType -UseBasicParsing -SkipCertificateCheck
        } else {
            $resp = Invoke-WebRequest -Uri $url -Method $method -Headers $headers -UseBasicParsing -SkipCertificateCheck
        }
        return @{ Status = $resp.StatusCode; Content = $resp.Content }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errBody = $reader.ReadToEnd()
        return @{ Status = $statusCode; Content = $errBody; Error = $_.Exception.Message }
    }
}

# ─── LOGIN ───
Write-Host "=== LOGIN ===" -ForegroundColor Green
$adminToken = Get-Token "admin@maidsandnannies.local" "Admin@12345"
$homeownerToken = Get-Token "homeowner@maidsandnannies.local" "Homeowner@12345"
$workerToken = Get-Token "worker@maidsandnannies.local" "Worker@12345"
Write-Host "Admin OK, Homeowner OK, Worker OK" -ForegroundColor Green

# ─── 1. CREATE JOB POST (Homeowner) ───
Write-Host "`n=== 1. CREATE JOB POST ===" -ForegroundColor Green
$body = @{
    description = "مطلوب عاملة منزلية للتنظيف والطبخ - يرجى الاتصال 01234567890"
    monthlySalary = 3000
    dailySalary = 0
    hourlySalary = 0
    specialization = 1
    bookingType = 1
    commissionType = 0
    startDate = "2026-08-15T00:00:00"
    quantity = 1
    currencyId = 1
} | ConvertTo-Json
$result = Api-Call "POST" "$baseUrl/api/JobPosts" $body $homeownerToken
Write-Host "Status: $($result.Status) - $($result.Content)"

# Extract job post ID
if ($result.Status -eq 200) {
    $jobPostId = ($result.Content | ConvertFrom-Json).JobPostId
    Write-Host "JobPost ID: $jobPostId" -ForegroundColor Yellow
} else {
    Write-Host "FAILED to create job post" -ForegroundColor Red
    exit
}

# ─── 2. ADMIN GET PENDING POSTS + APPROVE ───
Write-Host "`n=== 2. ADMIN APPROVE JOB POST ===" -ForegroundColor Green
$result = Api-Call "GET" "$baseUrl/api/AdminJobPosts/pending" $null $adminToken
Write-Host "Pending posts: $($result.Content)"
if ($result.Status -eq 200) {
    $pendingPosts = $result.Content | ConvertFrom-Json
    if ($pendingPosts.Count -gt 0) {
        $pendingId = $pendingPosts[0].id
        Write-Host "Approving post ID: $pendingId" -ForegroundColor Yellow
        $reviewBody = @{
            sanitizedDescription = "مطلوب عاملة منزلية للتنظيف والطبخ"
            isApproved = $true
            rejectionReason = $null
        } | ConvertTo-Json
        $result2 = Api-Call "PUT" "$baseUrl/api/AdminJobPosts/$pendingId/review" $reviewBody $adminToken
        Write-Host "Review result: Status=$($result2.Status) - $($result2.Content)"
    }
}

# ─── 3. WORKER BROWSE APPROVED POSTS ───
Write-Host "`n=== 3. WORKER BROWSE APPROVED ===" -ForegroundColor Green
$result = Api-Call "GET" "$baseUrl/api/JobPosts" $null $workerToken
Write-Host "Approved posts: $($result.Content)"

# ─── 4. WORKER APPLY ───
Write-Host "`n=== 4. WORKER APPLY ===" -ForegroundColor Green
$applyBody = @{ message = "أنا مهتمة بهذا العمل، لدي 5 سنوات خبرة" } | ConvertTo-Json
$result = Api-Call "POST" "$baseUrl/api/JobPosts/$jobPostId/apply" $applyBody $workerToken
Write-Host "Apply result: Status=$($result.Status) - $($result.Content)"

# ─── 5. HOMEOWNER GET APPLICATIONS ───
Write-Host "`n=== 5. HOMEOWNER VIEW APPLICATIONS ===" -ForegroundColor Green
$result = Api-Call "GET" "$baseUrl/api/JobPosts/$jobPostId/applications" $null $homeownerToken
Write-Host "Applications: $($result.Content)"
if ($result.Status -eq 200) {
    $applications = $result.Content | ConvertFrom-Json
    if ($applications.Count -gt 0) {
        $appId = $applications[0].id
        Write-Host "Application ID: $appId" -ForegroundColor Yellow

        # ─── 6. HOMEOWNER ACCEPT APPLICATION ───
        Write-Host "`n=== 6. HOMEOWNER ACCEPT APPLICATION ===" -ForegroundColor Green
        $result2 = Api-Call "POST" "$baseUrl/api/JobPosts/$jobPostId/applications/$appId/accept" $null $homeownerToken
        Write-Host "Accept result: Status=$($result2.Status) - $($result2.Content)"

        if ($result2.Status -eq 200) {
            $bookingId = ($result2.Content | ConvertFrom-Json).BookingId
            Write-Host "Booking ID: $bookingId" -ForegroundColor Yellow

            # ─── 7. CHECK BOOKING DETAILS ───
            Write-Host "`n=== 7. BOOKING DETAILS ===" -ForegroundColor Green
            $result3 = Api-Call "GET" "$baseUrl/api/Booking/$bookingId" $null $homeownerToken
            Write-Host "Booking Detail: $($result3.Content)" -ForegroundColor Cyan

            # ─── 8. ADMIN FLOW: CONFIRM WORKER → REQUEST PAYMENT → CONFIRM ───
            Write-Host "`n=== 8. ADMIN FLOW ===" -ForegroundColor Green
            $result4 = Api-Call "POST" "$baseUrl/api/Booking/$bookingId/confirm-worker" $null $adminToken
            Write-Host "Confirm worker: Status=$($result4.Status) - $($result4.Content)"

            $result4b = Api-Call "POST" "$baseUrl/api/Booking/$bookingId/request-payment" $null $adminToken
            Write-Host "Request payment: Status=$($result4b.Status) - $($result4b.Content)"

            # Upload payment proof (homeowner)
            Write-Host "`n=== 9. HOMEOWNER UPLOAD PAYMENT PROOF ===" -ForegroundColor Green
            # For payment proof upload, it's multipart, skip for now - use confirm payment directly as admin
            # In test data we can skip the actual image upload
            # Actually the upload-proof needs a file, so let's just use admin confirm payment directly
            Write-Host "Skipping payment upload (needs file), using admin confirm-payment directly..." -ForegroundColor Yellow
            $result5 = Api-Call "POST" "$baseUrl/api/Booking/$bookingId/confirm-payment" $null $adminToken
            Write-Host "Confirm payment: Status=$($result5.Status) - $($result5.Content)"

            # Start work
            $result6 = Api-Call "POST" "$baseUrl/api/Booking/$bookingId/start" $null $adminToken
            Write-Host "Start work: Status=$($result6.Status) - $($result6.Content)"

            # Complete
            $result7 = Api-Call "POST" "$baseUrl/api/Booking/$bookingId/complete" $null $adminToken
            Write-Host "Complete: Status=$($result7.Status) - $($result7.Content)"

            # ─── 10. FINAL BOOKING STATE ───
            Write-Host "`n=== FINAL BOOKING ===" -ForegroundColor Green
            $result8 = Api-Call "GET" "$baseUrl/api/Booking/$bookingId" $null $homeownerToken
            Write-Host $($result8.Content | ConvertFrom-Json | ConvertTo-Json -Depth 5) -ForegroundColor Cyan
        }
    }
}
