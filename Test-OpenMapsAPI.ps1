# Test Open Maps API (OpenStreetMap/Nominatim) Integration
# This script tests the location service endpoints and functionality

param(
    [string]$BaseUrl = "",
    [switch]$UseHttps = $false
)

# Auto-detect base URL if not provided
if ([string]::IsNullOrEmpty($BaseUrl)) {
    if ($UseHttps) {
        $BaseUrl = "https://localhost:7249"
    } else {
        $BaseUrl = "http://localhost:5090"
    }
}

Write-Host "Testing Open Maps API Integration" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

$testResults = @()
$totalTests = 0
$passedTests = 0
$failedTests = 0

function Test-Endpoint {
    param(
        [string]$TestName,
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )
    
    $totalTests++
    Write-Host "Testing: $TestName" -ForegroundColor Yellow -NoNewline
    Write-Host " ... " -NoNewline
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            TimeoutSec = 60
            Headers = $Headers
            UseBasicParsing = $true
            ErrorAction = "Stop"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            $params.ContentType = "application/json"
        }
        
        $response = Invoke-WebRequest @params
        $responseData = $response.Content | ConvertFrom-Json
        
        if ($response.StatusCode -eq 200) {
            Write-Host "PASS" -ForegroundColor Green
            $script:passedTests++
            return @{
                Success = $true
                StatusCode = $response.StatusCode
                Data = $responseData
                ResponseTime = $response.Headers.'X-Response-Time'
            }
        } else {
            Write-Host "FAIL (Status: $($response.StatusCode))" -ForegroundColor Red
            $script:failedTests++
            return @{
                Success = $false
                StatusCode = $response.StatusCode
                Error = "Unexpected status code"
            }
        }
    }
    catch {
        Write-Host "FAIL" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        $script:failedTests++
        return @{
            Success = $false
            Error = $_.Exception.Message
            StatusCode = $_.Exception.Response.StatusCode.value__
        }
    }
}

# Check if application is running
Write-Host "`nStep 1: Checking if application is running..." -ForegroundColor Cyan
try {
    $null = Invoke-WebRequest -Uri "$BaseUrl" -Method GET -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
    Write-Host "SUCCESS: Application is running!" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Application is not running at $BaseUrl" -ForegroundColor Red
    Write-Host "Please start the application first using: dotnet run --project PetCarePlatform.Web" -ForegroundColor Yellow
    exit 1
}

# Test 1: Comprehensive Test Endpoint
Write-Host "`nStep 2: Running comprehensive test suite..." -ForegroundColor Cyan
$testResult = Test-Endpoint `
    -TestName "Comprehensive Test Suite" `
    -Url "$BaseUrl/Location/Test"

if ($testResult.Success) {
    $testResults += $testResult
    
    Write-Host "`nTest Results Summary:" -ForegroundColor Cyan
    if ($testResult.Data.summary) {
        Write-Host "  Total Tests: $($testResult.Data.summary.totalTests)" -ForegroundColor White
        Write-Host "  Passed: $($testResult.Data.summary.passed)" -ForegroundColor Green
        Write-Host "  Failed: $($testResult.Data.summary.failed)" -ForegroundColor $(if ($testResult.Data.summary.failed -gt 0) { "Red" } else { "Green" })
        Write-Host "  Success Rate: $($testResult.Data.summary.successRate)" -ForegroundColor White
    }
    
    Write-Host "`nDetailed Test Results:" -ForegroundColor Cyan
    if ($testResult.Data.tests) {
        foreach ($test in $testResult.Data.tests) {
            $status = if ($test.result.success) { "PASS" } else { "FAIL" }
            $color = if ($test.result.success) { "Green" } else { "Red" }
            Write-Host "  [$status] $($test.testName)" -ForegroundColor $color
            
            if ($test.result.responseTimeMs) {
                Write-Host "    Response Time: $($test.result.responseTimeMs)ms" -ForegroundColor Gray
            }
            
            if (-not $test.result.success) {
                Write-Host "    Error: $($test.result.error)" -ForegroundColor Red
                if ($test.result.errorType) {
                    Write-Host "    Error Type: $($test.result.errorType)" -ForegroundColor Red
                }
            } else {
                if ($test.result.data) {
                    $data = $test.result.data
                    if ($data.latitude -and $data.longitude) {
                        Write-Host "    Coordinates: ($($data.latitude), $($data.longitude))" -ForegroundColor Gray
                    }
                    if ($data.distanceKm) {
                        Write-Host "    Distance: $($data.distanceKm) km" -ForegroundColor Gray
                    }
                    if ($data.mapUrl) {
                        Write-Host "    Map URL: $($data.mapUrl)" -ForegroundColor Gray
                    }
                }
            }
        }
    }
}

# Test 2: Individual Endpoint Tests
Write-Host "`nStep 3: Testing individual endpoints..." -ForegroundColor Cyan

# Test 2a: Geocoding - Valid Address
$testResult = Test-Endpoint `
    -TestName "Geocoding - Toronto" `
    -Url "$BaseUrl/Location/GetCoordinates?address=Toronto, ON, Canada"

if ($testResult.Success -and $testResult.Data.success) {
    Write-Host "  Coordinates: ($($testResult.Data.latitude), $($testResult.Data.longitude))" -ForegroundColor Gray
    Write-Host "  Address: $($testResult.Data.formattedAddress)" -ForegroundColor Gray
} elseif (-not $testResult.Success) {
    Write-Host "  ERROR: $($testResult.Error)" -ForegroundColor Red
}
$testResults += $testResult

# Wait to respect rate limiting
Start-Sleep -Seconds 2

# Test 2b: Geocoding - Another Address
$testResult = Test-Endpoint `
    -TestName "Geocoding - Vancouver" `
    -Url "$BaseUrl/Location/GetCoordinates?address=Vancouver, BC, Canada"

if ($testResult.Success -and $testResult.Data.success) {
    Write-Host "  Coordinates: ($($testResult.Data.latitude), $($testResult.Data.longitude))" -ForegroundColor Gray
    Write-Host "  Address: $($testResult.Data.formattedAddress)" -ForegroundColor Gray
} elseif (-not $testResult.Success) {
    Write-Host "  ERROR: $($testResult.Error)" -ForegroundColor Red
}
$testResults += $testResult

# Wait to respect rate limiting
Start-Sleep -Seconds 2

# Test 2c: Distance Calculation
$testResult = Test-Endpoint `
    -TestName "Distance Calculation" `
    -Url "$BaseUrl/Location/GetDistance?lat1=43.6532&lng1=-79.3832&lat2=49.2827&lng2=-123.1207"

if ($testResult.Success -and $testResult.Data.success) {
    Write-Host "  Distance: $([math]::Round($testResult.Data.distance, 2)) km" -ForegroundColor Gray
    Write-Host "  Expected: ~3364 km (Toronto to Vancouver)" -ForegroundColor Gray
} elseif (-not $testResult.Success) {
    Write-Host "  ERROR: $($testResult.Error)" -ForegroundColor Red
}
$testResults += $testResult

# Test 2d: Static Map URL
$testResult = Test-Endpoint `
    -TestName "Static Map URL Generation" `
    -Url "$BaseUrl/Location/GetStaticMapUrl?latitude=43.6532&longitude=-79.3832"

if ($testResult.Success -and $testResult.Data.success) {
    Write-Host "  Map URL: $($testResult.Data.mapUrl)" -ForegroundColor Gray
} elseif (-not $testResult.Success) {
    Write-Host "  ERROR: $($testResult.Error)" -ForegroundColor Red
}
$testResults += $testResult

# Test 2e: Directions URL
$testResult = Test-Endpoint `
    -TestName "Directions URL Generation" `
    -Url "$BaseUrl/Location/GetDirectionsUrl?originAddress=Toronto, ON&destinationAddress=Vancouver, BC"

if ($testResult.Success -and $testResult.Data.success) {
    Write-Host "  Directions URL: $($testResult.Data.directionsUrl)" -ForegroundColor Gray
} elseif (-not $testResult.Success) {
    Write-Host "  ERROR: $($testResult.Error)" -ForegroundColor Red
}
$testResults += $testResult

# Test 3: Rate Limiting Test
Write-Host "`nStep 4: Testing rate limiting (3 rapid requests)..." -ForegroundColor Cyan
Write-Host "  Making 3 rapid requests to test rate limiting behavior..." -ForegroundColor Yellow

$rapidResults = @()
for ($i = 1; $i -le 3; $i++) {
    Write-Host "  Request $i..." -NoNewline
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl/Location/GetCoordinates?address=Montreal, QC, Canada" -Method GET -TimeoutSec 60 -UseBasicParsing -ErrorAction Stop
        Write-Host " SUCCESS" -ForegroundColor Green
        $rapidResults += @{ Success = $true; RequestNumber = $i }
    }
    catch {
        Write-Host " FAILED" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
        $rapidResults += @{ Success = $false; RequestNumber = $i; Error = $_.Exception.Message }
    }
    # No delay between requests to test rate limiting
}

$rapidSuccess = ($rapidResults | Where-Object { $_.Success }).Count
Write-Host "  Rate Limiting Test Results: $rapidSuccess/3 requests succeeded" -ForegroundColor $(if ($rapidSuccess -eq 3) { "Green" } else { "Yellow" })
if ($rapidSuccess -lt 3) {
    Write-Host "  Note: Some requests failed, which may indicate rate limiting is working" -ForegroundColor Yellow
}

# Final Summary
Write-Host "`n" -NoNewline
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Cyan
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor $(if ($failedTests -gt 0) { "Red" } else { "Green" })

if ($failedTests -eq 0) {
    Write-Host "`nSUCCESS: All tests passed!" -ForegroundColor Green
} else {
    Write-Host "`nWARNING: Some tests failed. Review the errors above." -ForegroundColor Yellow
    Write-Host "`nCommon Issues:" -ForegroundColor Yellow
    Write-Host "  - Rate limiting: Nominatim allows 1 request per second" -ForegroundColor White
    Write-Host "  - Network connectivity: Check internet connection" -ForegroundColor White
    Write-Host "  - User-Agent header: Must be set (configured in Program.cs)" -ForegroundColor White
    Write-Host "  - Timeout: Requests may be timing out (check HttpClient timeout)" -ForegroundColor White
}

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "  1. Review the detailed test results above" -ForegroundColor White
Write-Host "  2. Check application logs for additional error details" -ForegroundColor White
Write-Host "  3. Fix any identified issues" -ForegroundColor White
Write-Host "  4. Re-run this test script to verify fixes" -ForegroundColor White

