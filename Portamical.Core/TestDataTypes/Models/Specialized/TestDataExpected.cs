// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

/// <summary>
/// Abstract base class for test data with an expected result value.
/// </summary>
/// <typeparam name="TResult">
/// The type of the expected result. Must be a non-nullable type.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="TestDataBase"/> and implements <see cref="IExpected{TResult}"/>
/// to provide a foundation for test data types that verify expected outcomes (return values or exceptions).
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Captures expected result via primary constructor parameter</item>
///   <item>Automatically extends argument arrays with <see cref="Expected"/> value</item>
///   <item>Supports trimming of expected value based on <see cref="PropsCode"/></item>
///   <item>Provides helper methods for result formatting</item>
/// </list>
/// </para>
/// <para>
/// <strong>Derived Types:</strong>
/// <list type="bullet">
///   <item><c>TestDataReturns&lt;TResult&gt;</c> - For methods that return a value</item>
///   <item><c>TestDataThrows&lt;TException&gt;</c> - For methods that throw an exception</item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Patterns:</strong>
/// <list type="bullet">
///   <item><strong>Template Method:</strong> <see cref="GetResultPrefix()"/> is abstract</item>
///   <item><strong>Extension Point:</strong> <see cref="ToObjectArray(ArgsCode)"/> extends base arguments</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Derived class: TestDataReturns
/// public class TestDataReturns&lt;TResult&gt; : TestDataExpected&lt;TResult&gt;, IReturns&lt;TResult&gt;
/// {
///     public TestDataReturns(string definition, TResult expected)
///         : base(definition, expected)
///     {
///     }
///     
///     public override string GetResultPrefix() =&gt; "returns";
///     
///     public override string GetResult()
///     {
///         return GetExpectedResult(Expected?.ToString());
///         // Result: "returns 5" or "returns hello"
///     }
/// }
/// </code>
/// </example>
public abstract class TestDataExpected<TResult>
: TestDataBase,
IExpected<TResult>
where TResult : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataExpected{TResult}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The descriptive definition of the test case scenario (left side of "=&gt;").
    /// </param>
    /// <param name="expected">
    /// The expected result of the test case. Cannot be null due to <c>notnull</c> constraint.
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor automatically generates the <see cref="TestCaseName"/> by calling
    /// <see cref="TestDataBase.CreateTestCaseName()"/>, which combines the definition and
    /// result (derived from <paramref name="expected"/>).
    /// </para>
    /// </remarks>
    protected TestDataExpected(
        string definition,
        TResult expected)
    : base(definition)
    {
        Expected = expected;
        TestCaseName = CreateTestCaseName();
    }

    private const string ExpectedString = "expected";
    private const string ResultsString = "results";

    /// <summary>
    /// Gets the expected outcome of the test case.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property stores the expected result that will be compared against the actual
    /// result when executing the test. The value is set during construction via the
    /// <c>init</c> accessor and cannot be modified afterward, ensuring test data immutability.
    /// </para>
    /// <para>
    /// The <typeparamref name="TResult"/> constraint ensures the expected value is never null,
    /// providing type safety for test assertions.
    /// </para>
    /// </remarks>
    /// <value>
    /// The expected result value of type <typeparamref name="TResult"/>.
    /// </value>
    public TResult Expected { get; init; }

    /// <summary>
    /// Gets the unique name of the test case associated with this instance.
    /// </summary>
    public override sealed string TestCaseName { get; init; }

    /// <inheritdoc/>
    public abstract string GetResultPrefix();

    /// <summary>
    /// Gets the expected result value in a non-generic form.
    /// </summary>
    /// <returns>
    /// The <see cref="Expected"/> value as an <see cref="object"/>.
    /// </returns>
    /// <remarks>
    /// This method implements <see cref="IExpected.GetExpected()"/> by returning the
    /// strongly-typed <see cref="Expected"/> property as an object.
    /// </remarks>
    public object GetExpected()
    => Expected;

    /// <summary>
    /// Converts the test data to an argument array by extending the base arguments with the expected value.
    /// </summary>
    /// <param name="argsCode">Determines whether to include the instance itself or its properties.</param>
    /// <returns>
    /// An array containing:
    /// <list type="bullet">
    ///   <item>The test data instance itself when <see cref="ArgsCode.Instance"/></item>
    ///   <item>The base properties plus <see cref="Expected"/> when <see cref="ArgsCode.Properties"/></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method overrides <see cref="TestDataBase.ToObjectArray(ArgsCode)"/> to add the
    /// <see cref="Expected"/> value to the argument array using the
    /// <see cref="TestDataBase.Extend{T}(Func{ArgsCode, object?[]}, ArgsCode, T?)"/> helper.
    /// </remarks>
    protected override object?[] ToObjectArray(ArgsCode argsCode)
    => Extend(base.ToObjectArray, argsCode, Expected);

    /// <summary>
    /// Formats the expected result with the result prefix for display in test case names.
    /// </summary>
    /// <param name="expectedString">
    /// The string representation of the expected value. Can be null or whitespace.
    /// </param>
    /// <returns>
    /// A formatted string in the form <c>"{resultPrefix} {expected}"</c>, where
    /// <c>{resultPrefix}</c> comes from <see cref="GetResultPrefix()"/> and
    /// <c>{expected}</c> is validated via <see cref="Resolver.FallbackIfNullOrWhiteSpace"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If <paramref name="expectedString"/> is null or whitespace, a fallback value
    /// <c>"expected (N)"</c> is generated with a unique index for tracing.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // TestDataReturns with expected = 5
    /// var result = GetExpectedResult("5");
    /// // Returns: "returns 5"
    /// 
    /// // TestDataThrows with expected = ArgumentException
    /// var result = GetExpectedResult("ArgumentException");
    /// // Returns: "throws ArgumentException"
    /// 
    /// // Null expected (fallback)
    /// var result = GetExpectedResult(null);
    /// // Returns: "returns expected (1)" (with trace warning)
    /// </code>
    /// </example>
    protected string GetExpectedResult(string? expectedString)
    {
        var resultPrefix = GetResultPrefix();
        var expected = ExpectedString.FallbackIfNullOrWhiteSpace(
            expectedString,
            nameof(GetExpected));

        return $"{resultPrefix} {expected}";
    }

    /// <summary>
    /// Validates and returns the result prefix, falling back to "results" if null or whitespace.
    /// </summary>
    /// <param name="resultPrefix">
    /// The result prefix to validate (e.g., "returns", "throws").
    /// </param>
    /// <returns>
    /// The validated <paramref name="resultPrefix"/>, or <c>"results (N)"</c> if null/whitespace.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This helper method ensures that derived classes always return a valid result prefix.
    /// If <paramref name="resultPrefix"/> is null or whitespace, a fallback value
    /// <c>"results (N)"</c> is generated with a unique index and a trace warning is logged.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> This method has a confusing name (same as abstract method).
    /// Consider renaming to <c>ValidateResultPrefix</c> or <c>EnsureResultPrefix</c> for clarity.
    /// </para>
    /// </remarks>
    protected static string GetValidResultPrefix(string resultPrefix)
    => ResultsString.FallbackIfNullOrWhiteSpace(resultPrefix, nameof(GetResultPrefix));

    /// <summary>
    /// Converts the test data to a parameter array with optional trimming of the expected value.
    /// </summary>
    /// <param name="argsCode">Determines instance vs properties inclusion.</param>
    /// <param name="propsCode">Specifies which properties to include when using <see cref="ArgsCode.Properties"/>.</param>
    /// <returns>
    /// A parameter array, potentially with the first element (expected value) removed based on <paramref name="propsCode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataBase.ToArgs(ArgsCode, PropsCode)"/> to support
    /// trimming of the <see cref="Expected"/> value. The trimming logic:
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Keeps expected value (no trim)</item>
    ///   <item><see cref="PropsCode.TrimTestCaseName"/> - Keeps expected value (no trim)</item>
    ///   <item><see cref="PropsCode.TrimReturnsExpected"/> - Removes expected value (trim)</item>
    ///   <item><see cref="PropsCode.TrimThrowsExpected"/> - Removes expected value (trim)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public override object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode != PropsCode.All);
}
