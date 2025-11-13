# Run tests with code coverage reporting
# As per AGENTS.md requirement for 80% coverage threshold

Write-Host "Running tests with code coverage..." -ForegroundColor Cyan
Write-Host ""

# Ensure output directory exists
$coverageDir = "docs/coverage"
if (-not (Test-Path $coverageDir)) {
    New-Item -ItemType Directory -Path $coverageDir -Force | Out-Null
}

# Run tests with coverage collection
dotnet test `
    --configuration Debug `
    --collect:"XPlat Code Coverage" `
    --results-directory:"./TestResults" `
    --logger:"console;verbosity=normal"

# Find the most recent coverage file
$latestCoverage = Get-ChildItem -Path "./TestResults" -Filter "coverage.cobertura.xml" -Recurse | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1

if ($latestCoverage) {
    Write-Host ""
    Write-Host "Coverage report generated: $($latestCoverage.FullName)" -ForegroundColor Green
    
    # Copy to docs/coverage for tracking
    Copy-Item -Path $latestCoverage.FullName -Destination "$coverageDir/coverage.cobertura.xml" -Force
    
    # Generate HTML report if reportgenerator is installed
    if (Get-Command reportgenerator -ErrorAction SilentlyContinue) {
        Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan
        reportgenerator `
            -reports:"$($latestCoverage.FullName)" `
            -targetdir:"$coverageDir" `
            -reporttypes:"Html"
        
        Write-Host "HTML report available at: $coverageDir/index.html" -ForegroundColor Green
    } else {
        Write-Host "Install ReportGenerator for HTML reports: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
    }
} else {
    Write-Host "No coverage file found." -ForegroundColor Red
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
