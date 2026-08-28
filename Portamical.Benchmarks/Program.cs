using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

// Uncomment the benchmark you want to run:

// Original benchmark - testing ToDistinctArrayTask overall
// BenchmarkRunner.Run<Portamical.Benchmarks.ToDistinctArrayTaskBenchmark>();

// New comprehensive benchmark - testing threshold across different conversion types
BenchmarkRunner.Run<Portamical.Benchmarks.AsyncThresholdBenchmark>();

