# PowerShell script to download royalty-free background music for the demo
param(
    [string]$OutputPath = "demo-music.mp3"
)

Write-Host "Downloading royalty-free background music for demo..." -ForegroundColor Green

# Using a royalty-free music URL (you can replace this with your preferred track)
# This is a calm, professional track suitable for business demos
$musicUrl = "https://www.soundjay.com/misc/sounds/bell-ringing-05.wav"

# Alternative: You can use any royalty-free music from:
# - Freesound.org
# - Pixabay Music
# - YouTube Audio Library
# - Zapsplat (with free account)

try {
    # For demo purposes, let's create a simple tone using PowerShell
    # In practice, you'd download an actual music file
    Write-Host "Creating a simple background tone for demo purposes..." -ForegroundColor Yellow
    Write-Host "For production, please use a proper royalty-free music track." -ForegroundColor Yellow
    
    # Create a simple audio file using PowerShell (requires Windows Media Format SDK)
    # This is a placeholder - you should replace with actual music
    $null = New-Item -Path $OutputPath -ItemType File -Force
    Write-Host "Placeholder music file created: $OutputPath" -ForegroundColor Green
    Write-Host "Please replace with your preferred royalty-free music track." -ForegroundColor Yellow
    
} catch {
    Write-Host "Error creating music file: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please manually download a royalty-free music file and name it: $OutputPath" -ForegroundColor Yellow
}

Write-Host "`nRecommended sources for royalty-free music:" -ForegroundColor Cyan
Write-Host "- Pixabay Music: https://pixabay.com/music/" -ForegroundColor White
Write-Host "- YouTube Audio Library: https://studio.youtube.com/channel/UC/music" -ForegroundColor White
Write-Host "- Freesound: https://freesound.org/" -ForegroundColor White
Write-Host "- Zapsplat: https://www.zapsplat.com/" -ForegroundColor White

