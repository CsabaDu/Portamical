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

    public bool ContainedBy(IEnumerable<INamedCase>? identifiables)
    => Contains(this, identifiables);

    public override sealed bool Equals(object? obj)
    => Equals(obj as INamedCase);

    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    public override sealed int GetHashCode()
    => Comparer.GetHashCode(this);

    public static Type[]? GetTypeArgs<TTestData>(
        TTestData testData,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    {
        var testDataType = typeof(TTestData);
        var typeArgs = testDataType.GetGenericArguments();

        typeArgs = testData is IReturns ?
            typeArgs[1..]
            : typeArgs;

        return argsCode == ArgsCode.Properties ?
            typeArgs
            : null;
    }

    public static object?[] ConvertToReturnsArgs(
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
    : base(ConvertToReturnsArgs(testData, argsCode))
    {
        TestCaseName = testData.TestCaseName;
        TypeArgs = GetTypeArgs(testData, argsCode);
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

    public override string TestCaseName { get; init; }
}