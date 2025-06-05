@echo off
echo Entity Framework Setup Fix for PetCarePOC
echo ==========================================
echo.

REM Check if we're in the right directory
if not exist "PetCarePlatform.sln" (
    echo Error: PetCarePlatform.sln not found. Please run this script from the solution root directory.
    pause
    exit /b 1
)

echo Step 1: Creating backup of project files...
if not exist ef-backup mkdir ef-backup
copy "PetCarePlatform.Infrastructure\PetCarePlatform.Infrastructure.csproj" "ef-backup\" >nul
copy "PetCarePlatform.API\PetCarePlatform.API.csproj" "ef-backup\" >nul
copy "PetCarePlatform.Core\PetCarePlatform.Core.csproj" "ef-backup\" >nul

echo Step 2: Fixing circular dependency...
powershell -Command "(Get-Content 'PetCarePlatform.Core\PetCarePlatform.Core.csproj') | Where-Object { $_ -notmatch 'PetCarePlatform.Infrastructure' } | Set-Content 'PetCarePlatform.Core\PetCarePlatform.Core.csproj'"

echo Step 3: Updating Infrastructure project packages...
powershell -Command "$content = Get-Content 'PetCarePlatform.Infrastructure\PetCarePlatform.Infrastructure.csproj' -Raw; $content = $content -replace 'Version=\"9\.0\.5\"', 'Version=\"8.0.16\"'; $content = $content -replace 'Version=\"8\.0\.0\"', 'Version=\"8.0.16\"'; Set-Content 'PetCarePlatform.Infrastructure\PetCarePlatform.Infrastructure.csproj' -Value $content"

echo Step 4: Adding Infrastructure reference to API project...
findstr /C:"PetCarePlatform.Infrastructure" "PetCarePlatform.API\PetCarePlatform.API.csproj" >nul
if errorlevel 1 (
    powershell -Command "$content = Get-Content 'PetCarePlatform.API\PetCarePlatform.API.csproj'; $newContent = @(); foreach($line in $content) { $newContent += $line; if($line -match 'PetCarePlatform.Core') { $newContent += '    <ProjectReference Include=\"..\PetCarePlatform.Infrastructure\PetCarePlatform.Infrastructure.csproj\" />' } }; $newContent | Set-Content 'PetCarePlatform.API\PetCarePlatform.API.csproj'"
)

echo Step 5: Adding EF Tools to API project...
findstr /C:"Microsoft.EntityFrameworkCore.Tools" "PetCarePlatform.API\PetCarePlatform.API.csproj" >nul
if errorlevel 1 (
    powershell -Command "$content = Get-Content 'PetCarePlatform.API\PetCarePlatform.API.csproj'; $newContent = @(); foreach($line in $content) { $newContent += $line; if($line -match 'Swashbuckle.AspNetCore') { $newContent += '    <PackageReference Include=\"Microsoft.EntityFrameworkCore.Tools\" Version=\"8.0.16\">'; $newContent += '      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>'; $newContent += '      <PrivateAssets>all</PrivateAssets>'; $newContent += '    </PackageReference>' } }; $newContent | Set-Content 'PetCarePlatform.API\PetCarePlatform.API.csproj'"
)

echo Step 6: Updating API project package versions...
powershell -Command "(Get-Content 'PetCarePlatform.API\PetCarePlatform.API.csproj') -replace 'Version=\"8\.0\.0\"', 'Version=\"8.0.16\"' | Set-Content 'PetCarePlatform.API\PetCarePlatform.API.csproj'"

echo.
echo Entity Framework setup fixes applied successfully!
echo.
echo Next steps:
echo 1. Run: dotnet restore PetCarePlatform.sln
echo 2. Run: dotnet build PetCarePlatform.sln
echo 3. Configure connection string in appsettings.json
echo 4. Run: dotnet ef migrations add InitialCreate --project PetCarePlatform.Infrastructure --startup-project PetCarePlatform.API
echo 5. Run: dotnet ef database update --project PetCarePlatform.Infrastructure --startup-project PetCarePlatform.API
echo.
echo Backup files are stored in the ef-backup\ directory.
pause

