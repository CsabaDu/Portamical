// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.Identity;
using static Adatamiq.Identity.Model.NamedCase;

namespace Adatamiq.NUnit.TestDataTypes;

/// <summary>
/// Represents a test case data class for NUnit.
/// It inherits from <see cref="TestCaseData"/>
/// </summary>
public abstract class TestCaseTestData
: TestCaseData, INamedCase
{
    private protected TestCaseTestData(object?[] args)
    : base(args)
    {
    }

    public abstract string TestCaseName { get; init; }

    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    public override sealed bool Equals(object? obj)
    => Equals(obj as INamedCase);

    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    public override sealed int GetHashCode()
    => Comparer.GetHashCode(this);

    public static TestCaseTestData<TTestData> From<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => new(testData, argsCode, testMethodName);

    public static Type[]? GetTypeArgs<TTestData>(
        TTestData testData,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    {
        if (argsCode != ArgsCode.Properties) return null;

        var typeArgs = typeof(TTestData).GetGenericArguments();

        return testData is IReturns ?
            typeArgs[1..]
            : typeArgs;

    }

    public static object?[] TestCaseDataArgsFrom(
        ITestData testData,
        ArgsCode argsCode)
    => testData.ToArgs(argsCode, PropsCode.TrimReturned);
}

/// <summary>
/// Represents test case data for a specific test, parameterized by a type that implements <see cref="ITestData"/>.
/// </summary>
/// <remarks>This class encapsulates the test data, argument code, and optional test method name for a test case.
/// It also determines the type arguments based on the provided <typeparamref name="TTestData"/> and the argument
/// code.</remarks>
/// <typeparam name="TTestData">The type of the test data, which must implement <see cref="ITestData"/>.</typeparam>
public sealed class TestCaseTestData<TTestData>
: TestCaseTestData
where TTestData : notnull, ITestData
{
    public TestCaseTestData(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    : base(TestCaseDataArgsFrom(testData, argsCode))
    {
        TestCaseName = testData.TestCaseName;
        TypeArgs = GetTypeArgs(testData, argsCode);

        ApplyMetadata(testData, testMethodName);
    }

    public override string TestCaseName { get; init; }

    private void ApplyMetadata(TTestData testData, string? testMethodName)
    {
        Properties.Set(PropertyNames.Description, TestCaseName);

        if (!string.IsNullOrEmpty(testMethodName))
        {
            TestName = GetDisplayName(testMethodName);
        }

        if (testData is IReturns returns)
        {
            ExpectedResult = returns.GetExpected();
        }
    }
}