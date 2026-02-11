// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity;
using static Portamical.Core.Identity.Model.NamedCase;

namespace Portamical.NUnit.TestDataTypes;

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

    internal const string HasFullNameProperty = "HasFullName";

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

    public static TTestCaseData SetHasFullNameProperty<TTestCaseData, TTestData>(
        TTestCaseData testCaseData,
        TTestData testData,
        string? testMethodName,
        out string testName)
    where TTestCaseData : notnull, TestCaseData
    where TTestData : notnull, ITestData
    {
        bool hasFullName = !string.IsNullOrEmpty(testMethodName);

        testCaseData.Properties.Set(
            HasFullNameProperty,
            hasFullName);

        testName = hasFullName ?
            testData.GetDisplayName(testMethodName)!
            : testData.TestCaseName;

        return testCaseData;
    }   

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
    => testData.ToArgs(argsCode, PropsCode.TrimReturnsExpected);
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
    internal TestCaseTestData(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    : base(TestCaseDataArgsFrom(testData, argsCode))
    {
        Properties.Set(
            PropertyNames.Description,
            testData.GetDefinition());

        SetHasFullNameProperty(
            this,
            testData,
            testMethodName,
            out string testName);

        if (testData is IReturns returns)
        {
            ExpectedResult = returns.GetExpected();
        }

        TestName = testName;
        TypeArgs = GetTypeArgs(testData, argsCode);
        TestCaseName = testData.TestCaseName;
    }

    public override string TestCaseName { get; init; }
}