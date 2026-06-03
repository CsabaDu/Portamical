// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;
using System.Collections;

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
///         return GetResult(Expected?.ToString());
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
    /// The <c>notnull</c> constraint on <typeparamref name="TResult"/> ensures the expected value is never null,
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

    public override sealed string GetResult()
    {
        const string resultsString = "results";

        var resultPrefix = resultsString
            .FallbackIfNullOrWhiteSpace(GetResultPrefix(), nameof(GetResultPrefix));
        var expected = Expected.GetType().ToString()
            .FallbackIfNullOrWhiteSpace(Format(Expected), nameof(GetExpected));

        return $"{resultPrefix} {expected}";
    }


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

    private static string? Format(object expected)
    {
        if (expected is Exception exception)
        {
            return $"{exception.GetType().Name}: {exception.Message}";
        }

        var expectedType = expected.GetType();
        var expectedString = expected.ToString();

        if (expectedType.IsValueType)
        {
            return expectedString;
        }

        return formatExpected();

        #region Local methods
        string? formatExpected()
        {
            return expected switch
            {
                string str => $"\"{str}\"",
                IEnumerable enumerable when expected is not string => formatCollection(enumerable),
                _ => expectedString,
            };
        }

        string formatCollection(IEnumerable enumerable)
        {
            const int maxCount = 3;

            var objects = enumerable.Cast<object>();
            var count = objects.Count();
            var items = objects.Take(maxCount).Select(Format);
            var prefix = count > maxCount ? $"[First {maxCount} of {count}]: " : $"[{count}]: ";

            if (enumerable is IDictionary dict)
            {
                items = dict.Cast<DictionaryEntry>().Take(maxCount).Select(
                    kvp => $"{{{Format(kvp.Key)}: {Format(kvp.Value ?? string.Empty)}}}");
            }

            return $"{prefix}{string.Join(", ", items)}";
        }
        #endregion
    }
}
