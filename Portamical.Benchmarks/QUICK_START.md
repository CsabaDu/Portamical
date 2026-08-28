# Async Threshold Benchmark - Quick Start

## What Was Created

I've created a comprehensive benchmark suite to test the optimal threshold for the `smallCollectionCountLimit` constant in `CollectionConverter.cs` (currently set to 100).

### New Files

1. **`AsyncThresholdBenchmark.cs`** - Main benchmark class
   - Tests 4 conversion types: TestData, ObjectArray, TypedRow, NonDistinct
   - Tests 9 collection sizes: 10, 25, 50, 75, 100, 150, 200, 300, 500
   - Compares `Task.Run` vs `Task.FromResult` for each combination

2. **`Run-AsyncThresholdBenchmark.ps1`** - PowerShell runner script
   - Easy execution with optional filtering
   - Provides helpful output and next steps

3. **`ANALYSIS_GUIDE.md`** - Detailed analysis guide
   - How to interpret results
   - Decision framework for choosing thresholds
   - Questions to answer after benchmarking

4. **`AsyncThresholdBenchmark.md`** - Technical documentation
   - Benchmark methodology
   - Collection size rationale
   - Result interpretation guidelines

### Updated Files

- **`Program.cs`** - Now configured to run the new benchmark

## Quick Start

### Step 1: Run the Benchmark

```powershell
cd Portamical.Benchmarks
.\Run-AsyncThresholdBenchmark.ps1
```

?? **Time estimate**: 15-30 minutes for full benchmark

### Step 2: Review Results

Results will be in: `BenchmarkDotNet.Artifacts/results/`

Look for the markdown file with the most recent timestamp.

### Step 3: Find Break-Even Points

For each conversion type, find where **Ratio ? 1.0**:

| Conversion Type | Current Size 100 Ratio | Break-Even Point | Suggested Threshold |
|-----------------|------------------------|------------------|---------------------|
| TestData        | _________              | _____ items      | _____               |
| ObjectArray     | _________              | _____ items      | _____               |
| TypedRow        | _________              | _____ items      | _____               |
| NonDistinct     | _________              | _____ items      | _____               |

### Step 4: Make Decision

#### Option A: Single Threshold (Recommended)
- Choose the **highest break-even point** from the table above
- Add a 10-20 item buffer for safety
- **Simple, maintainable, works for all cases**

#### Option B: Per-Type Thresholds
- Implement different thresholds for each conversion type
- **More optimal but more complex**

## Expected Results

Based on typical CPU-bound deduplication work:

- **TestData/ObjectArray**: Break-even around **75-125 items**
- **TypedRow** (more complex): Break-even around **50-100 items**
- **NonDistinct** (minimal work): Break-even around **200-500 items** (or never)

### Current Threshold (100) Assessment

The current value of 100 is likely:
- ? **Good** for TestData and ObjectArray conversions
- ? **Good** for TypedRow conversions
- ?? **Too low** for NonDistinct conversions (minimal work doesn't justify Task.Run)

### Likely Recommendation

After benchmarking, you'll probably find:

1. **For distinct conversions** (ToDistinctArrayRow): Keep threshold at **75-125**
2. **For non-distinct conversions** (ToArrayRow): Raise threshold to **200-500** or remove async support

This means you might want to:
- Keep the current 100 for most conversions
- Add a higher threshold for simple conversions
- Or document that the current threshold is optimized for deduplication scenarios

## Advanced: Category-Specific Testing

Want to test just one conversion type?

```powershell
# Test TestData conversions only
.\Run-AsyncThresholdBenchmark.ps1 -Category TestData

# Test ObjectArray conversions only
.\Run-AsyncThresholdBenchmark.ps1 -Category ObjectArray

# Test a specific size across all types
.\Run-AsyncThresholdBenchmark.ps1 -Filter "*Size100*"
```

## Understanding the Results

### Key Columns

- **Mean**: Average execution time (lower is better)
- **Ratio**: Performance vs baseline (Task.FromResult)
  - `1.0` = Same performance (break-even)
  - `> 1.0` = Task.Run is slower (overhead)
  - `< 1.0` = Task.Run is faster (benefit)
- **Allocated**: Memory allocated (Task.Run adds ~100 bytes)

### Example Interpretation

```
| Method               | Mean      | Ratio | Allocated |
|----------------------|-----------|-------|-----------|
| TaskRun_Size50       | 45.2 us   | 1.35  | 512 B     |
| Sync_Size50          | 33.5 us   | 1.00  | 416 B     |
| TaskRun_Size100      | 82.3 us   | 1.02  | 912 B     |
| Sync_Size100         | 80.7 us   | 1.00  | 816 B     |
| TaskRun_Size150      | 125.1 us  | 0.93  | 1312 B    |
| Sync_Size150         | 134.2 us  | 1.00  | 1216 B    |
```

**Analysis:**
- At **size 50**: Task.Run is **35% slower** (too much overhead)
- At **size 100**: Task.Run is **2% slower** (nearly break-even) ?
- At **size 150**: Task.Run is **7% faster** (benefits outweigh overhead)

**Conclusion**: Threshold of **100-125** is optimal for this conversion type.

## Next Steps

1. ? Run benchmark: `.\Run-AsyncThresholdBenchmark.ps1`
2. ? Analyze results using `ANALYSIS_GUIDE.md`
3. ? Decide on threshold value(s)
4. ? Update `CollectionConverter.cs` if needed
5. ? Document rationale in code comments

## Need Help?

See the detailed guides:
- **`ANALYSIS_GUIDE.md`** - Full analysis framework
- **`AsyncThresholdBenchmark.md`** - Technical details and methodology

## Questions Answered by This Benchmark

1. ? Is 100 the right threshold for **all** conversion types?
2. ? Should we use **different thresholds** for different conversions?
3. ? What's the **optimal threshold** for:
   - Identity conversions (TestData ? TestData)
   - Argument extraction (TestData ? object[][])
   - Custom transformations (TestData ? string[])
   - Minimal work (non-distinct conversions)
4. ? How much **overhead** does Task.Run add for small collections?
5. ? At what point does **parallelism benefit** exceed Task.Run overhead?

---

**Ready to run?** Execute: `.\Run-AsyncThresholdBenchmark.ps1`

The benchmark will provide data-driven answers to optimize your async conversion strategy! ??
