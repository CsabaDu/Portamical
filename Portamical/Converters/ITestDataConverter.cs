// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Defines a contract for converting test data into a row representation suitable for test frameworks
/// or data-driven testing scenarios.
/// </summary>
/// <remarks>
/// <para>
/// Implementations of this interface transform structured test data into formats consumable by
/// test runners or data-driven test methods. The specific row structure depends on the requirements
/// of the target test framework or test method signature.
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// <list type="bullet">
///   <item>Converting to <c>object[]</c> for xUnit/NUnit/MSTest</item>
///   <item>Converting to framework-specific types (e.g., xUnit's <c>TheoryData</c>)</item>
///   <item>Creating custom row types with additional metadata</item>
///   <item>Adding logging, validation, or transformation during conversion</item>
/// </list>
/// </para>
/// <para>
/// <strong>Integration:</strong> This interface signature matches the converter function expected by
/// <see cref="CollectionConverter.ToDistinctReadOnly{TTestData, TRow}(IEnumerable{TTestData}, Func{TTestData, ArgsCode, string, TRow}, ArgsCode, string)"/>,
/// enabling seamless integration with Portamical's conversion infrastructure.
/// </para>
/// </remarks>
/// <typeparam name="TTestData">
/// The type of test data to convert. Must implement <see cref="ITestData"/> and cannot be null.
/// </typeparam>
/// <typeparam name="TRow">
/// The type representing a single row of converted test data. No constraints - can be <c>object[]</c>,
/// framework-specific types, or custom row types.
/// </typeparam>
/// <example>
/// <code>
/// // Example: Converting to object[] for framework compatibility
/// public class ObjectArrayConverter : ITestDataConverter&lt;TestDataReturns&lt;int&gt;, object[]&gt;
/// {
///     public object[] ConvertRow(
///         TestDataReturns&lt;int&gt; testData,
///         ArgsCode argsCode,
///         string? testMethodName)
///     {
///         return testData.ToArgs(argsCode);
///     }
/// }
/// 
/// // Usage:
/// var converter = new ObjectArrayConverter();
/// var rows = testData.ToDistinctReadOnly(
///     converter.ConvertRow,
///     ArgsCode.Properties,
///     "MyTestMethod");
/// </code>
/// </example>
/// <typeparam name="TTestData">
/// The type of test data to convert. Must implement <see cref="ITestData"/> and cannot be null.
/// Marked as contravariant (<c>in</c>) to enable using converters that accept base types
/// for variables typed with derived types.
/// </typeparam>
/// <typeparam name="TRow">
/// The type representing a single row of converted test data. No constraints - can be <c>object[]</c>,
/// framework-specific types, or custom row types.
/// Marked as covariant (<c>out</c>) to enable using converters that return derived types
/// for variables typed with base types.
/// </typeparam>
/// <example>
/// <code>
/// // Variance example:
/// ITestDataConverter&lt;ITestData, object[]&gt; generalConverter = new GeneralConverter();
/// 
/// // Contravariance: Can use general converter for specific type
/// ITestDataConverter&lt;TestDataReturns&lt;int&gt;, object[]&gt; specificConverter = generalConverter;
/// // ✅ Works because TestDataReturns&lt;int&gt; IS AN ITestData
/// </code>
/// </example>
public interface ITestDataConverter<in TTestData, out TRow>
where TTestData : notnull, ITestData
{
    TRow ConvertRow(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName);
}
