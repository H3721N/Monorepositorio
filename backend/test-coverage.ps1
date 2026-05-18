param(
    [double]$LineThreshold = 90,
    [double]$BranchThreshold = 85
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $root

try {
    Remove-Item -Recurse -Force `
        "tests\Application.UnitTests\TestResults", `
        "tests\API.IntegrationTests\TestResults", `
        "coverage-report" `
        -ErrorAction SilentlyContinue

    dotnet restore ".\MonorepoBackend.sln"
    dotnet test ".\MonorepoBackend.sln" --no-restore --collect:"XPlat Code Coverage"
    dotnet tool restore
    dotnet tool run reportgenerator `
        -reports:"tests\**\coverage.cobertura.xml" `
        -targetdir:"coverage-report" `
        -reporttypes:"TextSummary;Cobertura;Html"

    [xml]$coverage = Get-Content ".\coverage-report\Cobertura.xml"
    $lineCoverage = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2)
    $branchCoverage = [math]::Round([double]$coverage.coverage.'branch-rate' * 100, 2)

    Write-Host "Line coverage: $lineCoverage%"
    Write-Host "Branch coverage: $branchCoverage%"

    if ($lineCoverage -lt $LineThreshold) {
        throw "Line coverage $lineCoverage% is below required threshold $LineThreshold%."
    }

    if ($branchCoverage -lt $BranchThreshold) {
        throw "Branch coverage $branchCoverage% is below required threshold $BranchThreshold%."
    }

    Write-Host "Coverage thresholds passed."
}
finally {
    Pop-Location
}
