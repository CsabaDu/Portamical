// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Portamical.Core.Converters;
using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;

namespace Portamical.Benchmarks;

[MemoryDiagnoser]
public class ToDistinctArrayTaskBenchmark
{
    private ITestData[] _size1 = null!;
    private ITestData[] _size5 = null!;
    private ITestData[] _size10 = null!;
    private ITestData[] _size25 = null!;
    private ITestData[] _size50 = null!;
    private ITestData[] _size100 = null!;
    private ITestData[] _size500 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _size1 = CreateTestData(1);
        _size5 = CreateTestData(5);
        _size10 = CreateTestData(10);
        _size25 = CreateTestData(25);
        _size50 = CreateTestData(50);
        _size100 = CreateTestData(100);
        _size500 = CreateTestData(500);
    }

    private static ITestData[] CreateTestData(int count)
    {
        var data = new ITestData[count];
        for (int i = 0; i < count; i++)
        {
            data[i] = TestDataFactory.CreateTestData<int>($"Test_{i}", "result", i);
        }
        return data;
    }

    // Current approach: Task.Run
    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size1()
        => await _size1.ToDistinctArrayTask();

    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size5()
        => await _size5.ToDistinctArrayTask();

    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size10()
        => await _size10.ToDistinctArrayTask();

    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size25()
        => await _size25.ToDistinctArrayTask();

    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size50()
        => await _size50.ToDistinctArrayTask();

    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size100()
        => await _size100.ToDistinctArrayTask();

    [Benchmark]
    public async Task<ITestData[]> TaskRun_Size500()
        => await _size500.ToDistinctArrayTask();

    // Synchronous baseline (wrapped in Task.FromResult)
    [Benchmark]
    public Task<ITestData[]> Sync_Size1()
        => Task.FromResult(_size1.ToDistinctArray());

    [Benchmark]
    public Task<ITestData[]> Sync_Size5()
        => Task.FromResult(_size5.ToDistinctArray());

    [Benchmark]
    public Task<ITestData[]> Sync_Size10()
        => Task.FromResult(_size10.ToDistinctArray());

    [Benchmark]
    public Task<ITestData[]> Sync_Size25()
        => Task.FromResult(_size25.ToDistinctArray());

    [Benchmark]
    public Task<ITestData[]> Sync_Size50()
        => Task.FromResult(_size50.ToDistinctArray());

    [Benchmark]
    public Task<ITestData[]> Sync_Size100()
        => Task.FromResult(_size100.ToDistinctArray());

    [Benchmark]
    public Task<ITestData[]> Sync_Size500()
        => Task.FromResult(_size500.ToDistinctArray());
}
