# Quick Application Test
Write-Host "Testing Enhanced Rating & Review System" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Test application accessibility
try {
    Write-Host "`nChecking application status..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "http://localhost:5090" -Method GET -TimeoutSec 10
    
    if ($response.StatusCode -eq 200) {
        Write-Host "SUCCESS: Application is running!" -ForegroundColor Green
        Write-Host "URL: http://localhost:5090" -ForegroundColor White
    } else {
        Write-Host "ERROR: Application returned status: $($response.StatusCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "ERROR: Application is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "TIP: Make sure the application is running with: dotnet run --project PetCarePlatform.Web" -ForegroundColor Yellow
}

Write-Host "`nTest Accounts Available:" -ForegroundColor Cyan
Write-Host "Pet Owner: daniel.nguyen@example.com / PetOwner123!" -ForegroundColor White
Write-Host "Service Provider: janedoe@example.com / ServiceProvider123!" -ForegroundColor White

Write-Host "`nNew Features to Test:" -ForegroundColor Cyan
Write-Host "1. Photo Upload in Reviews" -ForegroundColor White
Write-Host "2. Provider Responses to Reviews" -ForegroundColor White
Write-Host "3. Enhanced Review Display" -ForegroundColor White

Write-Host "`nSee TESTING_GUIDE.md for detailed instructions" -ForegroundColor Yellow
Write-Host "`nReady to test the enhanced rating and review system!" -ForegroundColor Green