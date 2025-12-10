# PowerShell Script to Test New Features
# Run: .\test-new-features.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  CreditoAPI - Feature Tests" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

# Test 1: Health Check
Write-Host "[1/8] Testing Health Check..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/v1/self" -Method Get
    Write-Host "✓ Health Check OK: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "✗ Health Check Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Login (Admin)
Write-Host "[2/8] Testing JWT Authentication (Admin)..." -ForegroundColor Yellow
try {
    $loginBody = @{
        username = "admin"
        password = "admin123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Login Successful" -ForegroundColor Green
    Write-Host "  Token: $($token.Substring(0, 30))..." -ForegroundColor Gray
    Write-Host "  Expires: $($loginResponse.expiresAt)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit
}
Write-Host ""

# Test 3: Login (User)
Write-Host "[3/8] Testing JWT Authentication (User)..." -ForegroundColor Yellow
try {
    $loginBody = @{
        username = "user"
        password = "user123"
    } | ConvertTo-Json

    $userLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/v1/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    Write-Host "✓ User Login Successful" -ForegroundColor Green
} catch {
    Write-Host "✗ User Login Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: API Versioning
Write-Host "[4/8] Testing API Versioning..." -ForegroundColor Yellow
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/v1/ready" -Method Get -Headers $headers
    Write-Host "✓ API Version v1 Working" -ForegroundColor Green
} catch {
    Write-Host "✗ API Versioning Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 5: Pagination
Write-Host "[5/8] Testing Pagination..." -ForegroundColor Yellow
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    $response = Invoke-RestMethod -Uri "$baseUrl/api/v1/creditos/123456?pageNumber=1&pageSize=5" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    
    if ($response.pageNumber) {
        Write-Host "✓ Pagination Working" -ForegroundColor Green
        Write-Host "  Page: $($response.pageNumber)/$($response.totalPages)" -ForegroundColor Gray
        Write-Host "  Items: $($response.items.Count)/$($response.totalCount)" -ForegroundColor Gray
    } else {
        Write-Host "⚠ No data found (404 expected if no credits exist)" -ForegroundColor Yellow
    }
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "⚠ No credits found (404 - Expected for empty database)" -ForegroundColor Yellow
    } else {
        Write-Host "✗ Pagination Test Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 6: Cache (Redis)
Write-Host "[6/8] Testing Redis Cache..." -ForegroundColor Yellow
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    
    Write-Host "  First request (cache miss)..." -ForegroundColor Gray
    $start1 = Get-Date
    try {
        $response1 = Invoke-RestMethod -Uri "$baseUrl/api/v1/creditos/credito/TEST-001" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    } catch {}
    $time1 = (Get-Date) - $start1
    
    Write-Host "  Second request (cache hit)..." -ForegroundColor Gray
    $start2 = Get-Date
    try {
        $response2 = Invoke-RestMethod -Uri "$baseUrl/api/v1/creditos/credito/TEST-001" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    } catch {}
    $time2 = (Get-Date) - $start2
    
    Write-Host "✓ Cache Test Complete" -ForegroundColor Green
    Write-Host "  First request: $($time1.TotalMilliseconds)ms" -ForegroundColor Gray
    Write-Host "  Second request: $($time2.TotalMilliseconds)ms" -ForegroundColor Gray
    
    if ($time2.TotalMilliseconds -lt $time1.TotalMilliseconds) {
        Write-Host "  ✓ Cache is faster!" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Cache Test Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 7: Rate Limiting
Write-Host "[7/8] Testing Rate Limiting..." -ForegroundColor Yellow
try {
    $headers = @{
        Authorization = "Bearer $token"
    }
    
    $rateLimitHit = $false
    Write-Host "  Sending multiple requests..." -ForegroundColor Gray
    
    for ($i = 1; $i -le 65; $i++) {
        try {
            $response = Invoke-RestMethod -Uri "$baseUrl/api/v1/creditos/rate-test-$i" -Method Get -Headers $headers -ErrorAction Stop
        } catch {
            if ($_.Exception.Response.StatusCode -eq 429) {
                Write-Host "✓ Rate Limit Working (429 after $i requests)" -ForegroundColor Green
                $rateLimitHit = $true
                break
            }
        }
        Start-Sleep -Milliseconds 50
    }
    
    if (-not $rateLimitHit) {
        Write-Host "⚠ Rate limit not reached (may need more requests)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Rate Limiting Test Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 8: Prometheus Metrics
Write-Host "[8/8] Testing Prometheus Metrics..." -ForegroundColor Yellow
try {
    $metrics = Invoke-RestMethod -Uri "$baseUrl/metrics" -Method Get
    if ($metrics -match "http_requests_received_total") {
        Write-Host "✓ Prometheus Metrics Available" -ForegroundColor Green
        
        $lines = $metrics -split "`n" | Where-Object { $_ -match "http_requests_received_total" } | Select-Object -First 3
        Write-Host "  Sample metrics:" -ForegroundColor Gray
        foreach ($line in $lines) {
            Write-Host "    $line" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠ Metrics endpoint available but no HTTP metrics found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Metrics Test Failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Summary
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services to check:" -ForegroundColor White
Write-Host "  • Swagger UI: $baseUrl/swagger" -ForegroundColor Gray
Write-Host "  • Prometheus: http://localhost:9090" -ForegroundColor Gray
Write-Host "  • Grafana: http://localhost:3000 (admin/admin)" -ForegroundColor Gray
Write-Host "  • Metrics: $baseUrl/metrics" -ForegroundColor Gray
Write-Host ""
Write-Host "Test Credentials:" -ForegroundColor White
Write-Host "  • Admin: admin / admin123" -ForegroundColor Gray
Write-Host "  • User: user / user123" -ForegroundColor Gray
Write-Host ""
Write-Host "✓ All tests completed!" -ForegroundColor Green
Write-Host ""
