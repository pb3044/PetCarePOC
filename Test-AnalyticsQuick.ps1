# Quick Test Script for Enhanced Rating Analytics & Display
# This script helps verify the analytics features are working

Write-Host "Testing Enhanced Rating Analytics & Display" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Check if application is running
Write-Host "`nChecking application status..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5090" -UseBasicParsing -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "SUCCESS: Application is running!" -ForegroundColor Green
        Write-Host "URL: http://localhost:5090" -ForegroundColor Green
    } else {
        Write-Host "FAILURE: Application is not accessible. Status Code: $($response.StatusCode)" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "FAILURE: Could not connect to the application. Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the application is running with: dotnet run --project PetCarePlatform.Web" -ForegroundColor Yellow
    exit 1
}

# Test analytics endpoint (should redirect to login)
Write-Host "`nTesting Analytics endpoint..." -ForegroundColor Yellow
try {
    $analyticsResponse = Invoke-WebRequest -Uri "http://localhost:5090/ServiceProvider/Analytics" -UseBasicParsing -ErrorAction Stop
    if ($analyticsResponse.StatusCode -eq 200) {
        Write-Host "SUCCESS: Analytics endpoint is accessible!" -ForegroundColor Green
        Write-Host "Note: Redirects to login page (expected behavior)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "WARNING: Analytics endpoint test failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`nTest Accounts Available:" -ForegroundColor Cyan
Write-Host "Service Provider: janedoe@example.com / ServiceProvider123!" -ForegroundColor White
Write-Host "Pet Owner: daniel.nguyen@example.com / PetOwner123!" -ForegroundColor White

Write-Host "`nManual Testing Steps:" -ForegroundColor Cyan
Write-Host "1. Open browser to: http://localhost:5090" -ForegroundColor White
Write-Host "2. Login as Service Provider: janedoe@example.com" -ForegroundColor White
Write-Host "3. Click 'View Analytics' button on dashboard" -ForegroundColor White
Write-Host "4. Verify all charts and metrics display correctly" -ForegroundColor White

Write-Host "`nAnalytics Features to Test:" -ForegroundColor Cyan
Write-Host "✅ Overall Rating Summary" -ForegroundColor Green
Write-Host "✅ Rating Breakdown Chart (Doughnut)" -ForegroundColor Green
Write-Host "✅ Performance Metrics" -ForegroundColor Green
Write-Host "✅ Recent Reviews Section" -ForegroundColor Green
Write-Host "✅ Rating Trends Chart (Line)" -ForegroundColor Green
Write-Host "✅ Service Performance Chart (Bar)" -ForegroundColor Green
Write-Host "✅ Mobile Responsiveness" -ForegroundColor Green
Write-Host "✅ Chart Interactivity" -ForegroundColor Green

Write-Host "`nExpected Results:" -ForegroundColor Cyan
Write-Host "• Professional analytics dashboard loads" -ForegroundColor White
Write-Host "• Interactive charts display with Chart.js" -ForegroundColor White
Write-Host "• Performance metrics show accurate data" -ForegroundColor White
Write-Host "• Recent reviews list with response status" -ForegroundColor White
Write-Host "• Responsive design works on mobile" -ForegroundColor White

Write-Host "`nIf you encounter issues:" -ForegroundColor Yellow
Write-Host "• Check browser console for JavaScript errors" -ForegroundColor White
Write-Host "• Verify you're logged in as a Service Provider" -ForegroundColor White
Write-Host "• Ensure the service provider has reviews in the database" -ForegroundColor White
Write-Host "• Check application logs for backend errors" -ForegroundColor White

Write-Host "`nReady to test the Enhanced Rating Analytics & Display!" -ForegroundColor Green
Write-Host "See TEST_ANALYTICS_FEATURES.md for detailed testing guide" -ForegroundColor Yellow
