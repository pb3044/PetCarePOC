# PowerShell script to add background music to the demo video using FFmpeg
param(
    [string]$VideoPath = "",
    [string]$MusicPath = "",
    [string]$OutputPath = "",
    [double]$MusicVolume = 0.3
)

# Check if FFmpeg is available
$ffmpegPath = Get-Command ffmpeg -ErrorAction SilentlyContinue
if (-not $ffmpegPath) {
    Write-Host "FFmpeg not found. Please install FFmpeg first:" -ForegroundColor Red
    Write-Host "1. Download from: https://ffmpeg.org/download.html" -ForegroundColor Yellow
    Write-Host "2. Or install via winget: winget install FFmpeg" -ForegroundColor Yellow
    Write-Host "3. Or install via chocolatey: choco install ffmpeg" -ForegroundColor Yellow
    exit 1
}

# Find the latest video file if no path specified
if ([string]::IsNullOrEmpty($VideoPath)) {
    $latestVideo = Get-ChildItem -Path "demo-scripts\test-results" -Filter "video.webm" -Recurse | 
                   Sort-Object LastWriteTime -Descending | 
                   Select-Object -First 1
    
    if (-not $latestVideo) {
        Write-Host "No video file found in demo-scripts\test-results" -ForegroundColor Red
        exit 1
    }
    $VideoPath = $latestVideo.FullName
    Write-Host "Using latest video: $($latestVideo.Name)" -ForegroundColor Green
}

# Check if video file exists
if (-not (Test-Path $VideoPath)) {
    Write-Host "Video file not found: $VideoPath" -ForegroundColor Red
    exit 1
}

# Set default music file if not provided
if ([string]::IsNullOrEmpty($MusicPath)) {
    # You can replace this with your preferred royalty-free music
    $MusicPath = "demo-music.mp3"
    
    if (-not (Test-Path $MusicPath)) {
        Write-Host "Music file not found: $MusicPath" -ForegroundColor Red
        Write-Host "Please provide a music file using -MusicPath parameter" -ForegroundColor Yellow
        Write-Host "Example: .\Add-BackgroundMusic.ps1 -MusicPath 'C:\path\to\your\music.mp3'" -ForegroundColor Yellow
        exit 1
    }
}

# Set default output path if not provided
if ([string]::IsNullOrEmpty($OutputPath)) {
    $videoDir = Split-Path $VideoPath -Parent
    $videoName = [System.IO.Path]::GetFileNameWithoutExtension($VideoPath)
    $OutputPath = Join-Path $videoDir "$videoName-with-music.mp4"
}

Write-Host "Adding background music to video..." -ForegroundColor Green
Write-Host "Video: $VideoPath" -ForegroundColor Cyan
Write-Host "Music: $MusicPath" -ForegroundColor Cyan
Write-Host "Output: $OutputPath" -ForegroundColor Cyan
Write-Host "Music Volume: $MusicVolume" -ForegroundColor Cyan

# FFmpeg command to add background music
# -i video.webm: input video file
# -i music.mp3: input music file
# -c:v copy: copy video stream without re-encoding (faster)
# -c:a aac: encode audio as AAC
# -map 0:v:0: use video from first input
# -map 1:a:0: use audio from second input
# -shortest: end when shortest input ends
# -filter:a "volume=$MusicVolume": adjust music volume
$ffmpegArgs = @(
    "-i", "`"$VideoPath`"",
    "-i", "`"$MusicPath`"",
    "-c:v", "copy",
    "-c:a", "aac",
    "-map", "0:v:0",
    "-map", "1:a:0",
    "-shortest",
    "-filter:a", "volume=$MusicVolume",
    "-y",  # Overwrite output file if it exists
    "`"$OutputPath`""
)

try {
    $process = Start-Process -FilePath "ffmpeg" -ArgumentList $ffmpegArgs -NoNewWindow -Wait -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Host "Success! Video with background music created: $OutputPath" -ForegroundColor Green
        
        # Get file sizes for comparison
        $originalSize = (Get-Item $VideoPath).Length
        $newSize = (Get-Item $OutputPath).Length
        Write-Host "Original size: $([math]::Round($originalSize/1MB, 2)) MB" -ForegroundColor Gray
        Write-Host "New size: $([math]::Round($newSize/1MB, 2)) MB" -ForegroundColor Gray
    } else {
        Write-Host "FFmpeg failed with exit code: $($process.ExitCode)" -ForegroundColor Red
    }
} catch {
    Write-Host "Error running FFmpeg: $($_.Exception.Message)" -ForegroundColor Red
}

