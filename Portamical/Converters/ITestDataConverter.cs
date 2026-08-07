// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Converters;

/// <summary>
/// Defines a contract for converting test data into row representations suitable for test frameworks
/// or data-driven testing scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This interface is designed to work in conjunction with <see cref="ITestDataProvider{TTestData}"/>
/// to provide a complete test data management solution. Implementations typically implement both interfaces,
/// serving dual roles as data provider (managing test data collections) and data converter (transforming
/// test data into framework-specific formats).
/// </para>
/// <para>
/// <strong>Design Pattern: Provider + Converter</strong>
/// </para>
/// <para>
/// The recommended implementation pattern combines both interfaces:
/// </para>
/// <code>
/// public class TestDataProvider&lt;TTestData&gt; 
///     : ITestDataProvider&lt;TTestData&gt;,
///       ITestDataConverter&lt;TTestData, object[]&gt;
///     where TTestData : notnull, ITestData
/// {
///     // ITestDataProvider members
///     public string? TestMethodName { get; init; }
///     public void AddRow(TTestData testData) { /* ... */ }
///     
///     // ITestDataConverter members
///     public ArgsCode ArgsCode { get; init; } = ArgsCode.Instance;
///     public object[] ConvertRow(TTestData testData, string? testMethodName)
///     {
///         return testData.ToArgs(ArgsCode);
///     }
/// }
/// </code>
/// <para>
/// <strong>Benefits of Combined Implementation:</strong>
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///       <strong>Unified Configuration:</strong> <see cref="ArgsCode"/> and <see cref="ITestDataProvider{TTestData}.TestMethodName"/>
///       are set once during construction and used consistently throughout data provision and conversion.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Framework Integration:</strong> Providers can implement <see cref="System.Collections.IEnumerable"/>
///       to iterate test data, using <see cref="ConvertRow"/> internally to transform each row.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Stateful Conversion:</strong> The provider maintains state (test method name, args code)
///       while the converter interface provides the transformation logic.
///     </description>
///   </item>
///   <item>
///     <description>
///       <strong>Separation of Concerns:</strong> <see cref="ITestDataProvider{TTestData}"/> manages the collection,
///       while <c>ITestDataConverter</c> handles row-by-row conversion logic.
///     </description>
///   </item>
/// </list>
/// <para>
/// <strong>Common Use Cases:</strong>
/// </para>
/// <list type="bullet">
///   <item>Converting to <c>object[]</c> for xUnit/NUnit/MSTest parameterized tests</item>
///   <item>Converting to framework-specific types (e.g., xUnit v3's <c>TheoryDataRow</c>)</item>
///   <item>Creating custom row types with additional metadata (test names, categories)</item>
///   <item>Adding validation or transformation during conversion (e.g., argument filtering)</item>
/// </list>
/// <para>
/// <strong>ArgsCode Property Role:</strong>
/// </para>
/// <para>
/// The <see cref="ArgsCode"/> property determines how test data is converted into rows:
/// </para>
/// <list type="bullet">
///   <item>
///     <see cref="ArgsCode.Instance"/> (default): Pass entire test data object as single argument
///   </item>
///   <item>
///     <see cref="ArgsCode.Properties"/>: Flatten test data properties into individual arguments
///   </item>
/// </list>
/// <para>
/// This property is typically set during construction via the <c>init</c> accessor and remains constant
/// throughout the provider's lifetime, ensuring consistent conversion behavior.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations are not required to be thread-safe. Converters
/// are typically constructed and configured during test discovery/initialization, then used read-only
/// during test execution.
/// </para>
/// </remarks>
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
/// <para><strong>Example 1: Combined Implementation for xUnit v2</strong></para>
/// <code>
/// public class TestDataProvider&lt;TTestData&gt; 
///     : ITestDataProvider&lt;TTestData&gt;,
///       ITestDataConverter&lt;TTestData, object[]&gt;,
///       IEnumerable&lt;object[]&gt;
///     where TTestData : notnull, ITestData
/// {
///     private readonly List&lt;TTestData&gt; _rows = [];
///     
///     public string? TestMethodName { get; init; }
///     public ArgsCode ArgsCode { get; init; } = ArgsCode.Instance;
///     
///     public void AddRow(TTestData testData) => _rows.Add(testData);
///     
///     public object[] ConvertRow(TTestData testData, string? testMethodName)
///     {
///         return testData.ToArgs(ArgsCode);
///     }
///     
///     public IEnumerator&lt;object[]&gt; GetEnumerator()
///     {
///         foreach (var row in _rows)
///         {
///             yield return ConvertRow(row, TestMethodName);
///         }
///     }
///     
///     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
/// }
/// 
/// // Usage:
/// public static TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt; TestCases { get; } = new()
/// {
///     TestMethodName = "AddTest",
///     ArgsCode = ArgsCode.Properties
/// };
/// 
/// static MyTests()
/// {
///     TestCases.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
///     TestCases.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
/// }
/// 
/// [Theory, MemberData(nameof(TestCases))]
/// public void AddTest(int arg1, int arg2, int expected)
/// {
///     Assert.Equal(expected, Calculator.Add(arg1, arg2));
/// }
/// </code>
/// <para><strong>Example 2: Variance in Action</strong></para>
/// <code>
/// // General converter accepts any ITestData
/// ITestDataConverter&lt;ITestData, object[]&gt; generalConverter = new TestDataProvider&lt;ITestData&gt;();
/// 
/// // Contravariance: Can assign to variable expecting specific derived type
/// ITestDataConverter&lt;TestDataReturns&lt;int&gt;, object[]&gt; specificConverter = generalConverter;
/// // ✅ Works because TestDataReturns&lt;int&gt; IS AN ITestData
/// //    The converter can accept any ITestData, including TestDataReturns&lt;int&gt;
/// 
/// // Covariance: Can assign converter returning object[] to variable expecting object
/// ITestDataConverter&lt;ITestData, object&gt; baseConverter = specificConverter;
/// // ✅ Works because object[] IS AN object
/// //    The converter returns object[] which can be treated as object
/// </code>
/// </example>
public interface ITestDataConverter<in TTestData, out TRow>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Gets the argument code that determines how test data is converted into row format.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value specifying the conversion strategy. Common values:
    /// <list type="bullet">
    ///   <item><see cref="ArgsCode.Instance"/> - Pass entire test data object (default, object-oriented)</item>
    ///   <item><see cref="ArgsCode.Properties"/> - Pass flattened properties (functional style)</item>
    /// </list>
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is typically set during construction via the <c>init</c> accessor and remains
    /// constant throughout the converter's lifetime. It's used internally by <see cref="ConvertRow"/>
    /// to determine how to transform test data into row format.
    /// </para>
    /// <para>
    /// <strong>Conversion Strategies:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="ArgsCode.Instance"/>: Each row contains the test data object itself.
    ///     Test methods receive a single parameter of type <typeparamref name="TTestData"/>.
    ///   </item>
    ///   <item>
    ///     <see cref="ArgsCode.Properties"/>: Each row contains flattened property values.
    ///     Test methods receive individual parameters matching the test data structure.
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Framework Compatibility:</strong> This property works across all supported frameworks
    /// (MSTest, NUnit, xUnit v2/v3, TUnit), though the specific row format depends on
    /// <typeparamref name="TRow"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Instance mode: Pass entire test data object
    /// var instanceProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;
    /// {
    ///     ArgsCode = ArgsCode.Instance
    /// };
    /// // Result row: [testDataObject]
    /// // Test signature: void Test(TestDataReturns&lt;int&gt; testData)
    /// 
    /// // Properties mode: Pass flattened arguments
    /// var propsProvider = new TestDataProvider&lt;TestDataReturns&lt;int&gt;&gt;
    /// {
    ///     ArgsCode = ArgsCode.Properties
    /// };
    /// // Result row: [arg1, arg2, expected]
    /// // Test signature: void Test(int arg1, int arg2, int expected)
    /// </code>
    /// </example>
    ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Converts a single test data item into a row representation suitable for the target test framework.
    /// </summary>
    /// <param name="testData">
    /// The test data to convert. The <c>notnull</c> constraint ensures this parameter
    /// cannot be <see langword="null"/> when nullable reference types are enabled.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method for which this row is being generated, or <see langword="null"/>
    /// if not applicable. Some implementations may use this to customize row metadata or validation.
    /// </param>
    /// <returns>
    /// A row representation of type <typeparamref name="TRow"/>. The specific structure depends on
    /// <see cref="ArgsCode"/> and the target row type. Common return types include <c>object[]</c>
    /// for xUnit/NUnit/MSTest, or framework-specific types like xUnit v3's <c>TheoryDataRow</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the actual conversion of test data into a row format. Implementations
    /// typically use <see cref="ArgsCode"/> (set during construction) to determine the conversion strategy:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="ArgsCode.Instance"/>: Returns a row containing the test data object itself,
    ///     e.g., <c>new object[] { testData }</c>
    ///   </item>
    ///   <item>
    ///     <see cref="ArgsCode.Properties"/>: Returns a row with flattened property values,
    ///     e.g., <c>testData.ToArgs(ArgsCode.Properties)</c>
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Integration with ITestDataProvider:</strong> When implementing both interfaces,
    /// this method is typically called internally during enumeration:
    /// </para>
    /// <code>
    /// public IEnumerator&lt;TRow&gt; GetEnumerator()
    /// {
    ///     foreach (var testData in _rows)
    ///     {
    ///         yield return ConvertRow(testData, TestMethodName);
    ///     }
    /// }
    /// </code>
    /// <para>
    /// <strong>Stateless Design:</strong> This method should be stateless and depend only on its
    /// parameters and the <see cref="ArgsCode"/> property. Multiple calls with the same inputs
    /// should produce equivalent results.
    /// </para>
    /// <para>
    /// <strong>Framework Requirements:</strong> The returned row type must match the expectations
    /// of the target test framework. For example, xUnit v2's <c>[MemberData]</c> expects <c>object[]</c>,
    /// while xUnit v3 can use strongly-typed <c>TheoryDataRow</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example implementation for object[] conversion
    /// public object[] ConvertRow(TestDataReturns&lt;int&gt; testData, string? testMethodName)
    /// {
    ///     // Use ArgsCode property set during construction
    ///     return testData.ToArgs(ArgsCode);
    /// }
    /// 
    /// // With ArgsCode.Instance:
    /// var row = ConvertRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5), "AddTest");
    /// // Result: [testDataObject]
    /// 
    /// // With ArgsCode.Properties:
    /// var row = ConvertRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5), "AddTest");
    /// // Result: [2, 3, 5]
    /// </code>
    /// </example>
    TRow ConvertRow(
        TTestData testData,
        string? testMethodName);
}
