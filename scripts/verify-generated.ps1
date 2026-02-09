param()

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Include = Join-Path $RepoRoot 'Portamical.Core/T4/SharedHelpers.ttinclude'

if (-not (Test-Path $Include)) {
    Write-Error "Cannot find $Include"
    exit 1
}

# Extract MaxArity value
$content = Get-Content $Include -Raw
if ($content -match 'const\s+int\s+MaxArity\s*=\s*(\d+)') {
    $maxArity = [int]$matches[1]
} else {
    Write-Error "Failed to read MaxArity from $Include"
    exit 1
}

Write-Output "MaxArity = $maxArity"

$files = @(
    'Portamical.Core/TestDataTypes/Models/General/TestData.generated.cs',
    'Portamical.Core/TestDataTypes/Models/Specialized/TestDataReturns.generated.cs',
    'Portamical.Core/TestDataTypes/Models/Specialized/TestDataThrows.generated.cs',
    'Portamical.Core/Factories/TestDataFactory.generated.cs'
) | ForEach-Object { Join-Path $RepoRoot $_ }

$fail = $false

foreach ($f in $files) {
    if (-not (Test-Path $f)) {
        Write-Error "Missing generated file: $f"
        $fail = $true
        continue
    }
    Write-Output "Checking $f ..."

    $txt = Get-Content $f -Raw

    if (($txt -notmatch "Arg$maxArity\b") -and ($txt -notmatch "T$maxArity\b")) {
        Write-Error "  -> Expected to find references to Arg$maxArity or T$maxArity in $f but none found."
        $fail = $true
    }
}

if ($fail) {
    Write-Error ""
    Write-Error "Generated files are out of sync with SharedHelpers.ttinclude (MaxArity = $maxArity)."
    Write-Error "Please regenerate the .generated.cs files locally and include them in your PR."
    Write-Error ""
    Write-Error "In Visual Studio: Right-click each .tt file -> Run Custom Tool"
    exit 2
}

Write-Output "OK: generated files appear consistent with MaxArity = $maxArity"