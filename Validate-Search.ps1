param(
  [string]$BaseUrl = "http://localhost:5090"
)

Write-Host "Launching headed browser to validate search..." -ForegroundColor Cyan

Set-Location -Path "$PSScriptRoot/demo-scripts"

# Ensure Node is on PATH for this session
$nodeDir = Join-Path $env:ProgramFiles 'nodejs'
if (Test-Path $nodeDir) { $env:Path = "$nodeDir;$env:Path" }

$env:PETCARE_BASE_URL = $BaseUrl

if (-not (Test-Path node_modules)) {
  npm install | Out-Null
}

# Install browsers if missing
npx playwright install chromium | Out-Null

# Run the validate test headed
npx playwright test tests/validate-search.spec.ts --headed --reporter=line


