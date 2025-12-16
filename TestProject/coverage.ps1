# 1) Clean old coverage files
Write-Host "Cleaning old coverage output..." -ForegroundColor Yellow
Remove-Item -Recurse -Force ".\TestProject\TestResults\" -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force ".\coverage-report\" -ErrorAction SilentlyContinue

# 2) Run tests AND collect coverage (FIXED SYNTAX)
Write-Host "Running dotnet tests with coverage..." -ForegroundColor Yellow
dotnet test --collect:"XPlat Code Coverage" `
  --results-directory ".\TestProject\TestResults" `
  /p:ExcludeByFile=\"**\Migrations\*.cs%3b**\*ModelSnapshot.cs\"

# 3) Grab the coverage file path automatically
$coverageFile = Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1

if (-not $coverageFile) {
    Write-Error "Coverage file not found. Something failed."
    exit 1
}

Write-Host "Found coverage file: $($coverageFile.FullName)" -ForegroundColor Cyan

# 4) See what files are in the coverage report BEFORE filtering
Write-Host "`nMigration/Snapshot files in coverage report:" -ForegroundColor Cyan
Select-Xml -Path $coverageFile.FullName -XPath "//class/@filename" | 
    Select-Object -ExpandProperty Node | 
    Select-Object -ExpandProperty Value |
    Where-Object { $_ -match "Migration|Snapshot" } |
    ForEach-Object { Write-Host "  $_" -ForegroundColor DarkYellow }

# 5) Run ReportGenerator with corrected filters
Write-Host "`nGenerating HTML coverage report..." -ForegroundColor Yellow
reportgenerator `
    "-reports:$($coverageFile.FullName)" `
    "-targetdir:coverage-report" `
    "-reporttypes:Html" `
    "-classfilters:-*.Migrations.*;-*ModelSnapshot*" `
    "-filefilters:-**/Migrations/*.cs;-**/*ModelSnapshot.cs"

# 6) Open the report in your browser
$reportPath = ".\coverage-report\index.html"
if (Test-Path $reportPath) {
    Write-Host "`nOpening coverage report..." -ForegroundColor Green
    Start-Process $reportPath
} else {
    Write-Error "index.html not found. Something failed."
}