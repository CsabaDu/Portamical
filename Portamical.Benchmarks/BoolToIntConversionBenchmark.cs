// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Portamical.Benchmarks;

/// <summary>
/// Micro-benchmark comparing bool-to-int conversion strategies for loop start index calculation.
/// Tests the performance difference between ternary operator and cast-based conversion.
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
public class BoolToIntConversionBenchmark
{
    private bool _skipFirstTrue = true;
    private bool _skipFirstFalse = false;
    private const int Iterations = 10000;

    [Benchmark(Baseline = true)]
    public int TernaryOperator_True()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = _skipFirstTrue ? 1 : 0;
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int CastConversion_True()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = (int)(object)_skipFirstTrue;
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int TernaryOperator_False()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = _skipFirstFalse ? 1 : 0;
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int CastConversion_False()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = (int)(object)_skipFirstFalse;
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int UnsafeConversion_True()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = Unsafe.As<bool, byte>(ref _skipFirstTrue);
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int UnsafeConversion_False()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = Unsafe.As<bool, byte>(ref _skipFirstFalse);
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int ConditionalExpression_True()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = _skipFirstTrue ? 1 : 0;
            sum += startIndex;
        }
        return sum;
    }

    [Benchmark]
    public int ConditionalExpression_False()
    {
        int sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            var startIndex = _skipFirstFalse ? 1 : 0;
            sum += startIndex;
        }
        return sum;
    }
}

/// <summary>
/// Helper program to analyze IL code generation for different bool-to-int conversion strategies.
/// </summary>
public class ILAnalysis
{
    public static void ShowILComparison()
    {
        Console.WriteLine("=== IL Comparison: bool to int conversion ===\n");

        // Method 1: Ternary operator
        Console.WriteLine("Method 1: Ternary operator (skipFirst ? 1 : 0)");
        Console.WriteLine("Expected IL: ldarg -> brtrue -> ldc.i4.0/1 -> br");
        Console.WriteLine("Characteristics: 4-6 instructions, branch-based\n");

        // Method 2: Cast via object
        Console.WriteLine("Method 2: Cast via object ((int)(object)skipFirst)");
        Console.WriteLine("Expected IL: ldarg -> box -> unbox.any");
        Console.WriteLine("Characteristics: 3 instructions, boxing overhead, heap allocation\n");

        // Method 3: Unsafe.As
        Console.WriteLine("Method 3: Unsafe.As<bool, byte>");
        Console.WriteLine("Expected IL: ldarga -> call Unsafe.As");
        Console.WriteLine("Characteristics: Reinterprets memory directly, no conversion\n");

        Console.WriteLine("=== Performance Expectations ===\n");
        Console.WriteLine("Ternary (?:)       ? Fast, branch predictor friendly, no allocations");
        Console.WriteLine("Cast (int)(object) ? SLOW, boxing allocation (~96 bytes), GC pressure");
        Console.WriteLine("Unsafe.As          ? Fastest, direct memory reinterpretation, no allocations");
        Console.WriteLine("\nRecommendation: Use ternary operator or Unsafe.As, NEVER cast via object!");
    }
}
