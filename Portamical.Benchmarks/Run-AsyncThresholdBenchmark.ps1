#!/usr/bin/env pwsh
# SPDX-License-Identifier: MIT
# Copyright (c) 2025. Csaba Dudas (CsabaDu)

<#
.SYNOPSIS
	Runs the AsyncThresholdBenchmark to determine optimal threshold for async conversions.

.DESCRIPTION
	This script runs comprehensive benchmarks to test the performance threshold for switching
	between Task.FromResult (synchronous) and Task.Run (thread pool) across different
	conversion types: TestData, ObjectArray, TypedRow, and NonDistinct conversions.

.PARAMETER Filter
	Optional filter to run specific benchmark categories.
	Examples: "*TestData*", "*ObjectArray*", "*Size100*"

.PARAMETER Category
	Run benchmarks for a specific category.
	Valid values: TestData, ObjectArray, TypedRow, NonDistinct, All

.EXAMPLE
	.\Run-AsyncThresholdBenchmark.ps1
	Runs all benchmarks

.EXAMPLE
	.\Run-AsyncThresholdBenchmark.ps1 -Category TestData
	Runs only TestData conversion benchmarks

.EXAMPLE
	.\Run-AsyncThresholdBenchmark.ps1 -Filter "*Size100*"
	Runs only benchmarks for size 100 collections
#>

param(
	[string]$Filter,
	[ValidateSet('TestData', 'ObjectArray', 'TypedRow', 'NonDistinct', 'All')]
	[string]$Category = 'All'
)

$ErrorActionPreference = 'Stop'

# Navigate to benchmark project directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$benchmarkDir = Join-Path $scriptDir "."
Push-Location $benchmarkDir

try {
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host "  Async Threshold Benchmark Runner" -ForegroundColor Cyan
	Write-Host "========================================" -ForegroundColor Cyan
	Write-Host ""

	# Build arguments
	$arguments = @('run', '-c', 'Release')

	if ($Category -ne 'All') {
		$Filter = "*$Category*"
	}

	if ($Filter) {
		$arguments += '--'
		$arguments += '--filter'
		$arguments += $Filter
		Write-Host "Filter: $Filter" -ForegroundColor Yellow
	} else {
		Write-Host "Running all benchmarks..." -ForegroundColor Yellow
	}

	Write-Host ""
	Write-Host "This will take several minutes. Please wait..." -ForegroundColor Yellow
	Write-Host ""

	# Run benchmarks
	& dotnet @arguments

	if ($LASTEXITCODE -eq 0) {
		Write-Host ""
		Write-Host "========================================" -ForegroundColor Green
		Write-Host "  Benchmark Complete!" -ForegroundColor Green
		Write-Host "========================================" -ForegroundColor Green
		Write-Host ""
		Write-Host "Results saved in: BenchmarkDotNet.Artifacts/results/" -ForegroundColor Cyan
		Write-Host ""
		Write-Host "Next steps:" -ForegroundColor Cyan
		Write-Host "  1. Review the results markdown file" -ForegroundColor White
		Write-Host "  2. Identify the break-even point for each conversion type" -ForegroundColor White
		Write-Host "  3. Update the threshold constant in CollectionConverter.cs" -ForegroundColor White
		Write-Host "  4. See AsyncThresholdBenchmark.md for analysis guidelines" -ForegroundColor White
	} else {
		Write-Host ""
		Write-Host "Benchmark failed with exit code: $LASTEXITCODE" -ForegroundColor Red
		exit $LASTEXITCODE
	}
} finally {
	Pop-Location
}
