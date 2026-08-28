# Async Threshold Benchmark

This benchmark determines the optimal threshold for switching between `Task.FromResult` (synchronous) and `Task.Run` (thread pool) when converting test data collections asynchronously.

## Purpose

The `ToConvertedTask` method in `CollectionConverter.cs` uses a threshold-based strategy:
- **Small collections (< threshold)**: Execute synchronously via `Task.FromResult` to avoid Task.Run overhead
- **Large collections (? threshold)**: Offload to thread pool via `Task.Run` for parallel execution benefits

**Current threshold: 100 items**

This benchmark validates whether 100 is optimal across different conversion scenarios.

## Benchmark Categories

### 1. **TestData** (Identity Conversion)
Tests converting `ITestData[]` ? `ITestData[]` (no transformation, just deduplication).
- **Method**: `ToDistinctArrayRow()`
- **Complexity**: Low (HashSet deduplication only)

### 2. **ObjectArray** (ToArgs Conversion)
Tests converting `ITestData[]` ? `object?[][]` (extracting arguments).
- **Method**: `ToDistinctArrayRow(ArgsCode.Instance)`
- **Complexity**: Medium (deduplication + argument extraction)

### 3. **TypedRow** (Custom Transformation)
Tests converting `ITestData[]` ? `string[]` (custom delegate transformation).
- **Method**: `ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ...)`
- **Complexity**: Medium-High (deduplication + string formatting)

### 4. **NonDistinct** (No Deduplication)
Tests converting without deduplication for baseline comparison.
- **Method**: `ToArrayRow()`
- **Complexity**: Minimal (validation + snapshot only)

## Collection Sizes Tested

| Size | Purpose |
|------|---------|
| 10   | Very small collections |
| 25   | Small collections |
| 50   | Approaching threshold |
| 75   | Near threshold |
| **100** | **Current threshold** |
| 150  | Moderate above threshold |
| 200  | Medium collections |
| 300  | Larger collections |
| 500  | Large collections |

## Running the Benchmark

### Option 1: Using PowerShell Script
```powershell
.\Run-AsyncThresholdBenchmark.ps1
```

### Option 2: Manual Command
```powershell
cd Portamical.Benchmarks
dotnet run -c Release
```

### Option 3: Run Specific Category
```powershell
cd Portamical.Benchmarks
dotnet run -c Release -- --filter *TestData*
dotnet run -c Release -- --filter *ObjectArray*
dotnet run -c Release -- --filter *TypedRow*
dotnet run -c Release -- --filter *NonDistinct*
```

## Interpreting Results

### Key Metrics

1. **Mean Time**: Average execution time
2. **Ratio**: Performance relative to baseline (Task.FromResult)
3. **Allocated Memory**: Memory allocations (Task.Run adds ~100 bytes overhead)

### What to Look For

**Break-Even Point**: The collection size where `TaskRun` and `Sync` have similar performance:
- **Ratio ? 1.0**: Break-even (no significant difference)
- **Ratio > 1.0**: Task.Run is slower (overhead exceeds benefits)
- **Ratio < 1.0**: Task.Run is faster (parallelism benefits outweigh overhead)

### Expected Results

For **CPU-bound work** (deduplication + transformation):
- Small collections (< 50): Task.Run adds 20-50% overhead
- Medium collections (50-100): Overhead reduces to 5-20%
- Large collections (> 100): Task.Run breaks even or becomes beneficial

For **minimal work** (NonDistinct):
- Task.Run overhead likely persists even at 500 items
- Threshold might need to be higher (200-500)

## Analysis Guidelines

### If Current Threshold (100) is Optimal:
- Ratios at size 100 should be ? 1.0 across categories
- Ratios below 100 should be > 1.0 (slower)
- Ratios above 100 should be ? 1.0 (same or faster)

### If Threshold Should Be Lower (e.g., 50):
- Task.Run becomes beneficial (ratio < 1.0) at size 50-75
- High overhead at very small sizes (10-25)

### If Threshold Should Be Higher (e.g., 150-200):
- Task.Run still has overhead (ratio > 1.0) at size 100
- Break-even occurs around 150-200

### Different Thresholds by Conversion Type:

If results vary significantly across categories, consider:
- **Lower threshold** (50-75) for complex conversions (TypedRow)
- **Higher threshold** (150-200) for simple conversions (NonDistinct)
- **Middle threshold** (100-150) for moderate conversions (ObjectArray, TestData)

## Recommendation Template

Based on benchmark results, fill in:

```
Benchmark Results Summary:
==========================

TestData Conversion:
- Break-even point: ~[X] items
- Ratio at size 100: [Y]
- Recommended threshold: [Z]

ObjectArray Conversion:
- Break-even point: ~[X] items
- Ratio at size 100: [Y]
- Recommended threshold: [Z]

TypedRow Conversion:
- Break-even point: ~[X] items
- Ratio at size 100: [Y]
- Recommended threshold: [Z]

NonDistinct Conversion:
- Break-even point: ~[X] items
- Ratio at size 100: [Y]
- Recommended threshold: [Z]

GENERAL RECOMMENDATION:
======================
Use threshold of [N] items as a conservative value that:
- Avoids overhead for all conversion types at small sizes
- Provides benefits for most conversion types at large sizes
- Balances simplicity (single threshold) vs. optimization (per-type thresholds)

Rationale: [Explain why this value was chosen]
```

## Notes

- Benchmarks run in Release mode with optimizations enabled
- Results are environment-dependent (CPU, memory, .NET version)
- Run multiple times to ensure consistency
- Consider real-world usage patterns (typical collection sizes in your test suites)
