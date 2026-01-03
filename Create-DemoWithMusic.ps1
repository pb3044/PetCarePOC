# Complete script to add background music to the demo video
param(
    [string]$MusicPath = "",
    [double]$MusicVolume = 0.3,
    [switch]$DownloadMusic = $false
)

Write-Host "=== PetCare Platform Demo Video with Background Music ===" -ForegroundColor Magenta

# Step 1: Check if FFmpeg is available
Write-Host "`n1. Checking FFmpeg installation..." -ForegroundColor Yellow
$ffmpegPath = Get-Command ffmpeg -ErrorAction SilentlyContinue
if (-not $ffmpegPath) {
    Write-Host "FFmpeg not found. Installing via winget..." -ForegroundColor Yellow
    try {
        winget install FFmpeg
        Write-Host "FFmpeg installed successfully!" -ForegroundColor Green
    } catch {
        Write-Host "Failed to install FFmpeg automatically." -ForegroundColor Red
        Write-Host "Please install FFmpeg manually from: https://ffmpeg.org/download.html" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "FFmpeg found: $($ffmpegPath.Source)" -ForegroundColor Green
}

# Step 2: Find the latest demo video
Write-Host "`n2. Finding latest demo video..." -ForegroundColor Yellow
$latestVideo = Get-ChildItem -Path "demo-scripts\test-results" -Filter "video.webm" -Recurse | 
               Sort-Object LastWriteTime -Descending | 
               Select-Object -First 1

if (-not $latestVideo) {
    Write-Host "No demo video found. Please run the demo recording first." -ForegroundColor Red
    Write-Host "Run: .\Record-Demo.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "Found video: $($latestVideo.Name)" -ForegroundColor Green

# Step 3: Handle music file
Write-Host "`n3. Setting up background music..." -ForegroundColor Yellow

if ([string]::IsNullOrEmpty($MusicPath)) {
    $MusicPath = "demo-music.mp3"
    
    if (-not (Test-Path $MusicPath)) {
        if ($DownloadMusic) {
            Write-Host "Downloading royalty-free music..." -ForegroundColor Yellow
            # You can implement actual music download here
            Write-Host "Please manually download a royalty-free music file and save as: $MusicPath" -ForegroundColor Yellow
        } else {
            Write-Host "No music file found. Creating a simple tone..." -ForegroundColor Yellow
            # Create a placeholder file
            $null = New-Item -Path $MusicPath -ItemType File -Force
            Write-Host "Placeholder created. Please replace with actual music file." -ForegroundColor Yellow
        }
    }
}

if (-not (Test-Path $MusicPath)) {
    Write-Host "Music file not found: $MusicPath" -ForegroundColor Red
    Write-Host "Please provide a music file using -MusicPath parameter" -ForegroundColor Yellow
    Write-Host "Example: .\Create-DemoWithMusic.ps1 -MusicPath 'C:\path\to\music.mp3'" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using music: $MusicPath" -ForegroundColor Green

# Step 4: Create output path
$videoDir = Split-Path $latestVideo.FullName -Parent
$videoName = [System.IO.Path]::GetFileNameWithoutExtension($latestVideo.Name)
$OutputPath = Join-Path $videoDir "$videoName-with-music.mp4"

Write-Host "`n4. Adding background music to video..." -ForegroundColor Yellow
Write-Host "Input video: $($latestVideo.FullName)" -ForegroundColor Cyan
Write-Host "Music file: $MusicPath" -ForegroundColor Cyan
Write-Host "Output file: $OutputPath" -ForegroundColor Cyan
Write-Host "Music volume: $MusicVolume" -ForegroundColor Cyan

# Step 5: Run FFmpeg
$ffmpegArgs = @(
    "-i", "`"$($latestVideo.FullName)`"",
    "-i", "`"$MusicPath`"",
    "-c:v", "copy",
    "-c:a", "aac",
    "-map", "0:v:0",
    "-map", "1:a:0",
    "-shortest",
    "-filter:a", "volume=$MusicVolume",
    "-y",
    "`"$OutputPath`""
)

try {
    Write-Host "`nProcessing video... This may take a few minutes." -ForegroundColor Yellow
    $process = Start-Process -FilePath "ffmpeg" -ArgumentList $ffmpegArgs -NoNewWindow -Wait -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Host "`n✅ Success! Demo video with background music created!" -ForegroundColor Green
        Write-Host "📁 Output file: $OutputPath" -ForegroundColor Cyan
        
        # Show file sizes
        $originalSize = (Get-Item $latestVideo.FullName).Length
        $newSize = (Get-Item $OutputPath).Length
        Write-Host "📊 Original size: $([math]::Round($originalSize/1MB, 2)) MB" -ForegroundColor Gray
        Write-Host "📊 New size: $([math]::Round($newSize/1MB, 2)) MB" -ForegroundColor Gray
        
        # Open the output folder
        Write-Host "`n🎬 Opening output folder..." -ForegroundColor Yellow
        Start-Process "explorer.exe" -ArgumentList "/select,`"$OutputPath`""
        
    } else {
        Write-Host "❌ FFmpeg failed with exit code: $($process.ExitCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error running FFmpeg: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Demo Video Creation Complete ===" -ForegroundColor Magenta

