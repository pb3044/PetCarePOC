param(
  [string]$BaseUrl = "http://localhost:5090"
)

Write-Host "Starting demo recording..." -ForegroundColor Cyan

Set-Location -Path "$PSScriptRoot/demo-scripts"

# Resolve npm/npx even if PATH isn't updated yet (compatible with PS 5)
$nodeDir = Join-Path $env:ProgramFiles 'nodejs'
if (Test-Path $nodeDir) {
  # Ensure node.exe is on PATH for child processes used by npm/npx
  $env:Path = "$nodeDir;$env:Path"
}

$npm = $null
$npx = $null
$cmd = Get-Command npm -ErrorAction SilentlyContinue
if ($cmd) { $npm = $cmd.Source }
$cmd = Get-Command npx -ErrorAction SilentlyContinue
if ($cmd) { $npx = $cmd.Source }
if (-not $npm) { $npm = Join-Path $env:ProgramFiles 'nodejs/npm.cmd' }
if (-not $npx) { $npx = Join-Path $env:ProgramFiles 'nodejs/npx.cmd' }

if (-not (Test-Path $npm)) { throw 'npm not found. Please restart shell or install Node.js' }
if (-not (Test-Path $npx)) { throw 'npx not found. Please restart shell or install Node.js' }

if (-not (Test-Path node_modules)) {
  Write-Host "Installing npm dependencies..." -ForegroundColor Yellow
  & $npm install | Out-Null
}

# Ensure Playwright browsers are installed (retry to handle transient network errors)
function Invoke-With-Retry {
  param(
    [Parameter(Mandatory=$true)][scriptblock]$Script,
    [int]$Retries = 3,
    [int]$DelaySeconds = 3
  )
  for ($i = 1; $i -le $Retries; $i++) {
    try {
      & $Script
      return
    } catch {
      if ($i -eq $Retries) { throw }
      Start-Sleep -Seconds $DelaySeconds
    }
  }
}

Write-Host "Ensuring Playwright Chromium browser is installed..." -ForegroundColor Yellow
Invoke-With-Retry -Script { & $npx playwright install chromium } -Retries 3 -DelaySeconds 5

$env:PETCARE_BASE_URL = $BaseUrl

Write-Host "Running Playwright tests..." -ForegroundColor Cyan
& $npx playwright test --reporter=line tests/e2e-overview.spec.ts

Write-Host "Video saved under demo-scripts/test-results for e2e-overview test." -ForegroundColor Green


