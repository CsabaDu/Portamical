// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Converters.ObjectArray;
using Portamical.Converters.TestData;
using Portamical.Converters.TypedRow;
using Portamical.Core.TestData;

namespace Portamical.Benchmarks;

/// <summary>
/// Benchmarks to determine the optimal threshold for switching between Task.FromResult and Task.Run
/// across different conversion types: TestData, ObjectArray, and TypedRow conversions.
/// </summary>
[MemoryDiagnoser]
[MarkdownExporter]
public class AsyncThresholdBenchmark
{
    private ITestData[] _size10 = null!;
    private ITestData[] _size25 = null!;
    private ITestData[] _size50 = null!;
    private ITestData[] _size75 = null!;
    private ITestData[] _size100 = null!;
    private ITestData[] _size150 = null!;
    private ITestData[] _size200 = null!;
    private ITestData[] _size300 = null!;
    private ITestData[] _size500 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _size10 = CreateTestData(10);
        _size25 = CreateTestData(25);
        _size50 = CreateTestData(50);
        _size75 = CreateTestData(75);
        _size100 = CreateTestData(100);
        _size150 = CreateTestData(150);
        _size200 = CreateTestData(200);
        _size300 = CreateTestData(300);
        _size500 = CreateTestData(500);
    }

    private static ITestData[] CreateTestData(int count)
    {
        var data = new ITestData[count];
        for (int i = 0; i < count; i++)
        {
            data[i] = TestDataFactory.CreateTestData($"Test_{i}", "result", i, i * 2, i * 3);
        }
        return data;
    }

    #region TestData Array Conversion (Identity)

    [Benchmark]
    [BenchmarkCategory("TestData", "Size10")]
    public async Task<ITestData[]> TestData_TaskRun_Size10()
        => await Task.Run(() => _size10.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size10")]
    public Task<ITestData[]> TestData_Sync_Size10()
        => Task.FromResult(_size10.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size25")]
    public async Task<ITestData[]> TestData_TaskRun_Size25()
        => await Task.Run(() => _size25.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size25")]
    public Task<ITestData[]> TestData_Sync_Size25()
        => Task.FromResult(_size25.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size50")]
    public async Task<ITestData[]> TestData_TaskRun_Size50()
        => await Task.Run(() => _size50.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size50")]
    public Task<ITestData[]> TestData_Sync_Size50()
        => Task.FromResult(_size50.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size75")]
    public async Task<ITestData[]> TestData_TaskRun_Size75()
        => await Task.Run(() => _size75.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size75")]
    public Task<ITestData[]> TestData_Sync_Size75()
        => Task.FromResult(_size75.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size100")]
    public async Task<ITestData[]> TestData_TaskRun_Size100()
        => await Task.Run(() => _size100.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size100")]
    public Task<ITestData[]> TestData_Sync_Size100()
        => Task.FromResult(_size100.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size150")]
    public async Task<ITestData[]> TestData_TaskRun_Size150()
        => await Task.Run(() => _size150.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size150")]
    public Task<ITestData[]> TestData_Sync_Size150()
        => Task.FromResult(_size150.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size200")]
    public async Task<ITestData[]> TestData_TaskRun_Size200()
        => await Task.Run(() => _size200.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size200")]
    public Task<ITestData[]> TestData_Sync_Size200()
        => Task.FromResult(_size200.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size300")]
    public async Task<ITestData[]> TestData_TaskRun_Size300()
        => await Task.Run(() => _size300.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size300")]
    public Task<ITestData[]> TestData_Sync_Size300()
        => Task.FromResult(_size300.ToDistinctArrayRow());

    [Benchmark]
    [BenchmarkCategory("TestData", "Size500")]
    public async Task<ITestData[]> TestData_TaskRun_Size500()
        => await Task.Run(() => _size500.ToDistinctArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TestData", "Size500")]
    public Task<ITestData[]> TestData_Sync_Size500()
        => Task.FromResult(_size500.ToDistinctArrayRow());

    #endregion

    #region ObjectArray Conversion (ToArgs)

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size10")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size10()
        => await Task.Run(() => _size10.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size10")]
    public Task<object?[][]> ObjectArray_Sync_Size10()
        => Task.FromResult(_size10.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size25")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size25()
        => await Task.Run(() => _size25.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size25")]
    public Task<object?[][]> ObjectArray_Sync_Size25()
        => Task.FromResult(_size25.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size50")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size50()
        => await Task.Run(() => _size50.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size50")]
    public Task<object?[][]> ObjectArray_Sync_Size50()
        => Task.FromResult(_size50.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size75")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size75()
        => await Task.Run(() => _size75.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size75")]
    public Task<object?[][]> ObjectArray_Sync_Size75()
        => Task.FromResult(_size75.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size100")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size100()
        => await Task.Run(() => _size100.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size100")]
    public Task<object?[][]> ObjectArray_Sync_Size100()
        => Task.FromResult(_size100.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size150")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size150()
        => await Task.Run(() => _size150.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size150")]
    public Task<object?[][]> ObjectArray_Sync_Size150()
        => Task.FromResult(_size150.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size200")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size200()
        => await Task.Run(() => _size200.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size200")]
    public Task<object?[][]> ObjectArray_Sync_Size200()
        => Task.FromResult(_size200.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size300")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size300()
        => await Task.Run(() => _size300.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size300")]
    public Task<object?[][]> ObjectArray_Sync_Size300()
        => Task.FromResult(_size300.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark]
    [BenchmarkCategory("ObjectArray", "Size500")]
    public async Task<object?[][]> ObjectArray_TaskRun_Size500()
        => await Task.Run(() => _size500.ToDistinctArrayRow(ArgsCode.Instance));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ObjectArray", "Size500")]
    public Task<object?[][]> ObjectArray_Sync_Size500()
        => Task.FromResult(_size500.ToDistinctArrayRow(ArgsCode.Instance));

    #endregion

    #region TypedRow Conversion (Custom Transformation)

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size10")]
    public async Task<string[]> TypedRow_TaskRun_Size10()
        => await Task.Run(() => _size10.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size10")]
    public Task<string[]> TypedRow_Sync_Size10()
        => Task.FromResult(_size10.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size25")]
    public async Task<string[]> TypedRow_TaskRun_Size25()
        => await Task.Run(() => _size25.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size25")]
    public Task<string[]> TypedRow_Sync_Size25()
        => Task.FromResult(_size25.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size50")]
    public async Task<string[]> TypedRow_TaskRun_Size50()
        => await Task.Run(() => _size50.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size50")]
    public Task<string[]> TypedRow_Sync_Size50()
        => Task.FromResult(_size50.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size75")]
    public async Task<string[]> TypedRow_TaskRun_Size75()
        => await Task.Run(() => _size75.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size75")]
    public Task<string[]> TypedRow_Sync_Size75()
        => Task.FromResult(_size75.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size100")]
    public async Task<string[]> TypedRow_TaskRun_Size100()
        => await Task.Run(() => _size100.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size100")]
    public Task<string[]> TypedRow_Sync_Size100()
        => Task.FromResult(_size100.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size150")]
    public async Task<string[]> TypedRow_TaskRun_Size150()
        => await Task.Run(() => _size150.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size150")]
    public Task<string[]> TypedRow_Sync_Size150()
        => Task.FromResult(_size150.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size200")]
    public async Task<string[]> TypedRow_TaskRun_Size200()
        => await Task.Run(() => _size200.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size200")]
    public Task<string[]> TypedRow_Sync_Size200()
        => Task.FromResult(_size200.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size300")]
    public async Task<string[]> TypedRow_TaskRun_Size300()
        => await Task.Run(() => _size300.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size300")]
    public Task<string[]> TypedRow_Sync_Size300()
        => Task.FromResult(_size300.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark]
    [BenchmarkCategory("TypedRow", "Size500")]
    public async Task<string[]> TypedRow_TaskRun_Size500()
        => await Task.Run(() => _size500.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TypedRow", "Size500")]
    public Task<string[]> TypedRow_Sync_Size500()
        => Task.FromResult(_size500.ToDistinctArrayRow((td, ac, _) => td.GetDisplayName(null) ?? "", ArgsCode.Instance, null));

    #endregion

    #region Non-Distinct Conversion (ToArrayRow - without deduplication)

    [Benchmark]
    [BenchmarkCategory("NonDistinct", "Size10")]
    public async Task<ITestData[]> NonDistinct_TaskRun_Size10()
        => await Task.Run(() => _size10.ToArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("NonDistinct", "Size10")]
    public Task<ITestData[]> NonDistinct_Sync_Size10()
        => Task.FromResult(_size10.ToArrayRow());

    [Benchmark]
    [BenchmarkCategory("NonDistinct", "Size50")]
    public async Task<ITestData[]> NonDistinct_TaskRun_Size50()
        => await Task.Run(() => _size50.ToArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("NonDistinct", "Size50")]
    public Task<ITestData[]> NonDistinct_Sync_Size50()
        => Task.FromResult(_size50.ToArrayRow());

    [Benchmark]
    [BenchmarkCategory("NonDistinct", "Size100")]
    public async Task<ITestData[]> NonDistinct_TaskRun_Size100()
        => await Task.Run(() => _size100.ToArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("NonDistinct", "Size100")]
    public Task<ITestData[]> NonDistinct_Sync_Size100()
        => Task.FromResult(_size100.ToArrayRow());

    [Benchmark]
    [BenchmarkCategory("NonDistinct", "Size200")]
    public async Task<ITestData[]> NonDistinct_TaskRun_Size200()
        => await Task.Run(() => _size200.ToArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("NonDistinct", "Size200")]
    public Task<ITestData[]> NonDistinct_Sync_Size200()
        => Task.FromResult(_size200.ToArrayRow());

    [Benchmark]
    [BenchmarkCategory("NonDistinct", "Size500")]
    public async Task<ITestData[]> NonDistinct_TaskRun_Size500()
        => await Task.Run(() => _size500.ToArrayRow());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("NonDistinct", "Size500")]
    public Task<ITestData[]> NonDistinct_Sync_Size500()
        => Task.FromResult(_size500.ToArrayRow());

    #endregion
}
