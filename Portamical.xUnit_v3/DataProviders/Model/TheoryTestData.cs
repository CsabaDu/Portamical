// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;
using static Portamical.Core.Safety.Validator;

namespace Portamical.xUnit_v3.DataProviders.Model;

/// <summary>
/// Represents a theory test data provider for xUnit v3 that combines builder pattern, template method,
/// and automatic deduplication based on test case names.
/// </summary>
/// <typeparam name="TTestData">
/// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Modern Test Framework:</strong>
/// </para>
/// <para>
/// This class extends xUnit v3's <see cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}"/> and
/// implements Portamical's <see cref="ITheoryTestData{TTestData}"/> interface, providing:
/// <list type="bullet">
///   <item><description>
///     <strong>Builder Pattern:</strong> Incremental test data construction via <see cref="AddRow"/>
///   </description></item>
///   <item><description>
///     <strong>Template Method:</strong> Abstract <see cref="Convert(TTestData)"/> implementation using instance configuration
///   </description></item>
///   <item><description>
///     <strong>Automatic Deduplication:</strong> Duplicate test cases (same <see cref="INamedCase.TestCaseName"/>) are
///     silently ignored
///   </description></item>
///   <item><description>
///     <strong>Configuration:</strong> <see cref="ArgsCode"/> and <see cref="TestMethodName"/> properties control conversion
///   </description></item>
///   <item><description>
///     <strong>Type Safety:</strong> Runtime validation ensures generic type parameter matches <typeparamref name="TTestData"/>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Patterns: Builder + Template Method + Deduplication</strong>
/// </para>
/// <para>
/// This class combines three patterns:
/// <code>
/// // Builder Pattern:
/// TheoryTestData&lt;TTestData&gt; data = TheoryTestData.Create(...);
/// data.AddRow(testData2);  // ← Incremental construction
/// data.AddRow(testData3);
/// 
/// // Template Method Pattern:
/// protected override ITheoryTestDataRow Convert(TTestData row)
/// =&gt; ConvertRow(row, ArgsCode, TestMethodName);
/// //             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Uses instance config
/// 
/// // Deduplication:
/// if (_namedCases.Add(row))  // ← HashSet with INamedCase.Comparer
/// {
///     base.Add(row);  // ← Only unique TestCaseNames added
/// }
/// </code>
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// </para>
/// <code>
/// xUnit.v3.TheoryDataBase&lt;ITheoryTestDataRow, TTestData&gt; (xUnit v3 base)
///   ↓ inherits
/// TheoryTestData&lt;TTestData&gt; (this class)
///   ↓ implements
/// ITheoryTestData&lt;TTestData&gt; (Portamical)
///   ↓ extends
/// ITestDataProvider&lt;TTestData&gt; + ITestDataConverter&lt;TTestData, ITheoryTestDataRow&gt;
/// 
/// Also inherits from:
/// List&lt;ITheoryTestDataRow&gt; (via TheoryDataBase)
/// </code>
/// </para>
/// <para>
/// <strong>Deduplication Strategy:</strong>
/// </para>
/// <para>
/// Deduplication is performed using a <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>:
/// <list type="bullet">
///   <item><description>
///     <strong>Equality:</strong> Two test cases are considered equal if their <see cref="INamedCase.TestCaseName"/>
///     values are equal (ordinal string comparison)
///   </description></item>
///   <item><description>
///     <strong>Hashing:</strong> Hash code is computed from <see cref="INamedCase.TestCaseName"/> using
///     <see cref="StringComparer.Ordinal"/>
///   </description></item>
///   <item><description>
///     <strong>Silent Deduplication:</strong> Duplicate test cases are silently ignored (not added to collection)
///   </description></item>
///   <item><description>
///     <strong>First-Wins:</strong> When duplicates are encountered, the first occurrence is kept
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Type Safety:</strong>
/// </para>
/// <para>
/// The <see cref="Add"/> method performs runtime validation to ensure type safety:
/// <list type="bullet">
///   <item><description>
///     Validates that the row is a generic type
///   </description></item>
///   <item><description>
///     Validates that the generic parameter matches <typeparamref name="TTestData"/>
///   </description></item>
///   <item><description>
///     Provides detailed error messages with expected vs. actual type information
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Factory Method Pattern:</strong>
/// </para>
/// <para>
/// The constructor is <c>internal</c> to encourage usage of static factory methods (assumed to exist in a
/// companion static class, e.g., <c>TheoryTestData.Create(...)</c>).
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Builder Pattern (Native Style)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.DataProviders.Model;
/// 
/// public class CalculatorTests
/// {
///     public static IEnumerable&lt;ITheoryDataRow&gt; GetAddTestData()
///     {
///         // Create with first test data
///         var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///             ArgsCode.Properties,
///             testMethodName: "TestAdd");
///         
///         // Add rows incrementally (builder pattern)
///         data.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
///         data.AddRow(new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0));
///         
///         return data;  // Returns IEnumerable&lt;ITheoryDataRow&gt;
///     }
///     
///     [Theory]
///     [MemberData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit v3 Test Explorer displays:
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(5,7)
/// // ✓ TestAdd - Add(-1,1)
/// </code>
/// 
/// <para><strong>Example 2: Automatic Deduplication</strong></para>
/// <code>
/// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///     ArgsCode.Properties,
///     null);
/// 
/// data.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));  // ← Duplicate TestCaseName
/// data.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
/// 
/// // Result: 2 rows (duplicate "Add(2,3)" silently ignored)
/// // data.Count == 2
/// </code>
/// 
/// <para><strong>Example 3: Type Validation Error</strong></para>
/// <code>
/// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(...);
/// 
/// // Wrong generic type:
/// var wrongRow = new TheoryTestDataRow&lt;TestDataReturns&lt;string&gt;&gt;(
///     testData,
///     ArgsCode.Properties,
///     null);
/// 
/// data.Add(wrongRow);  // ← Throws ArgumentException
/// // Message: "The provided test data row has a mismatched generic type parameter. 
/// //           Expected: TheoryTestDataRow&lt;TestDataReturns&lt;Int32&gt;&gt;, 
/// //           Actual: TheoryTestDataRow&lt;TestDataReturns&lt;String&gt;&gt;"
/// </code>
/// 
/// <para><strong>Example 4: Manual Conversion (ConvertRow)</strong></para>
/// <code>
/// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,
///     ArgsCode.Properties,
///     null);
/// 
/// // Convert with explicit parameters (ignores instance config)
/// ITheoryTestDataRow row = data.ConvertRow(
///     new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30),
///     ArgsCode.Instance,      // ← Override instance ArgsCode
///     "CustomTestMethod");    // ← Override instance TestMethodName
/// 
/// // row uses ArgsCode.Instance (not data.ArgsCode)
/// // row.TestDisplayName = "CustomTestMethod - Add(10,20)"
/// </code>
/// </example>
/// <seealso cref="ITheoryTestData{TTestData}"/>
/// <seealso cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}"/>
/// <seealso cref="TheoryTestDataRow{TTestData}"/>
/// <seealso cref="ITheoryTestDataRow"/>
public sealed class TheoryTestData<TTestData>
: TheoryDataBase<ITheoryTestDataRow, TTestData>,
ITheoryTestData<TTestData>
where TTestData : notnull, ITestData
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TheoryTestData{TTestData}"/> class with the specified
    /// test data, argument conversion strategy, and optional test method name.
    /// </summary>
    /// <param name="testData">
    /// The first test data item to add to the collection. Additional rows can be added via <see cref="AddRow"/>.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument (Shared Style)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments (Native Style)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to test case names in display names.
    /// If provided, display names will be formatted as "testMethodName - TestCaseName".
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Internal Access Modifier:</strong>
    /// </para>
    /// <para>
    /// This constructor is <c>internal</c> to encourage usage of static factory methods (assumed to exist in a
    /// companion static class) that provide cleaner API syntax.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <para>
    /// The <paramref name="argsCode"/> parameter is validated using <c>argsCode.Defined(nameof(argsCode))</c>,
    /// which ensures it contains a valid <see cref="ArgsCode"/> enum value.
    /// </para>
    /// <para>
    /// <strong>Immediate Population:</strong>
    /// </para>
    /// <para>
    /// The first test data item is added immediately via <see cref="AddRow"/>, ensuring the collection is
    /// never empty after construction.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="argsCode"/> is not a defined <see cref="ArgsCode"/> enum value.
    /// </exception>
    /// <example>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     ArgsCode.Properties,
    ///     testMethodName: "TestAdd");
    /// 
    /// // data.Count == 1 (first row added in constructor)
    /// // data.ArgsCode == ArgsCode.Properties
    /// // data.TestMethodName == "TestAdd"
    /// </code>
    /// </example>
    internal TheoryTestData(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;

        AddRow(testData);
    }

    #endregion

    #region Fields

    /// <summary>
    /// HashSet used for deduplication of test cases based on <see cref="INamedCase.TestCaseName"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This field maintains a set of unique test case names to prevent duplicate test cases from being
    /// added to the collection. It uses <see cref="NamedCase.Comparer"/> for equality comparison and hashing:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Equality:</strong> Two test cases are equal if <see cref="INamedCase.TestCaseName"/> values
    ///     are equal (ordinal string comparison)
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Hashing:</strong> Hash code is computed from <see cref="INamedCase.TestCaseName"/> using
    ///     <see cref="StringComparer.Ordinal"/>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// </para>
    /// <para>
    /// Using <see cref="HashSet{T}"/> provides O(1) average-case performance for deduplication checks,
    /// compared to O(n) for linear searches in lists.
    /// </para>
    /// </remarks>
    private readonly HashSet<INamedCase> _namedCases =
        new(NamedCase.Comparer);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the argument conversion strategy used by this theory test data provider.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value indicating how test data is converted to test method arguments.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is immutable after construction (init-only). It is used by <see cref="Convert(TTestData)"/>
    /// and <see cref="AddRow"/> to determine how to convert test data to <see cref="ITheoryTestDataRow"/>.
    /// </para>
    /// <para>
    /// <strong>Values:</strong>
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Test method receives entire <see cref="ITestData"/> object
    ///     (Shared Style: framework-agnostic)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Test method receives flattened property values
    ///     (Native Style: idiomatic xUnit)
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="ITestDataProvider{TTestData}.ArgsCode"/>
    public ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Gets the test method name to prepend to test case names in display names.
    /// </summary>
    /// <value>
    /// The test method name, or <c>null</c> if no prefix should be added to display names.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is immutable after construction (init-only). It is used by <see cref="Convert(TTestData)"/>
    /// to create display names for test cases.
    /// </para>
    /// <para>
    /// <strong>Display Name Format:</strong>
    /// <list type="bullet">
    ///   <item><description>
    ///     If <c>null</c>: Display name is just "TestCaseName"
    ///   </description></item>
    ///   <item><description>
    ///     If not <c>null</c>: Display name is "TestMethodName - TestCaseName"
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="ITestDataProvider{TTestData}.TestMethodName"/>
    public string? TestMethodName { get; init; }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a theory test data row to the collection with automatic deduplication and runtime type validation.
    /// </summary>
    /// <param name="row">
    /// The theory test data row to add. Must be of type <see cref="TheoryTestDataRow{TTestData}"/> with
    /// matching generic parameter <typeparamref name="TTestData"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Override of xUnit v3 Template Method:</strong>
    /// </para>
    /// <para>
    /// This method overrides <see cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}.Add(TTheoryDataRow)"/>,
    /// providing three additional features:
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Null Validation:</strong> Ensures <paramref name="row"/> is not <c>null</c> using
    ///     <see cref="Validator.NotNull{T}(T, string)"/>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Type Validation:</strong> Ensures <paramref name="row"/> is a generic type with matching
    ///     generic parameter <typeparamref name="TTestData"/>. This is necessary because the base class signature
    ///     accepts <see cref="ITheoryTestDataRow"/> (interface), which could be implemented by incompatible generic types.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Deduplication:</strong> Uses <see cref="_namedCases"/> HashSet to prevent duplicate test cases
    ///     (same <see cref="INamedCase.TestCaseName"/>) from being added. Duplicates are silently ignored.
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Type Validation Logic:</strong>
    /// </para>
    /// <para>
    /// The type validation uses reflection to ensure the row is of the correct generic type:
    /// <code>
    /// // 1. Check if row is a generic type:
    /// if (!rowType.IsGenericType)
    /// {
    ///     throw new ArgumentException(...);
    /// }
    /// 
    /// // 2. Check if generic parameter matches TTestData:
    /// if (genericArgs.Length != 1 || genericArgs[0] != typeof(TTestData))
    /// {
    ///     throw new ArgumentException(...);
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Deduplication Logic:</strong>
    /// </para>
    /// <code>
    /// if (_namedCases.Add(row))
    /// {
    ///     // _namedCases.Add returns:
    ///     // - true: row.TestCaseName is unique → add to base collection
    ///     // - false: row.TestCaseName is duplicate → skip silently
    ///     
    ///     base.Add(row);
    /// }
    /// </code>
    /// <para>
    /// <strong>Runtime Type Check Limitation:</strong>
    /// </para>
    /// <para>
    /// The type validation is performed at runtime (not compile-time) because the base class method signature
    /// accepts <see cref="ITheoryTestDataRow"/> (interface). While this is not ideal, it's necessary to prevent
    /// type mismatches that could occur when mixing generic types:
    /// </para>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(...);
    /// var wrongRow = new TheoryTestDataRow&lt;TestDataReturns&lt;string&gt;&gt;(...);
    /// 
    /// data.Add(wrongRow);  // ← Would compile (both implement ITheoryTestDataRow)
    ///                      //   But throws at runtime (generic type mismatch)
    /// </code>
    /// <para>
    /// <strong>Error Message Format:</strong>
    /// </para>
    /// <para>
    /// When validation fails, the exception message includes:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Non-Generic Type:</strong> "Expected: TheoryTestDataRow&lt;TTestData&gt;, Actual: [ActualTypeName]"
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Mismatched Generic Parameter:</strong> "Expected: TheoryTestDataRow&lt;TTestData&gt;, Actual: [ActualTypeName]&lt;[GenericArgs]&gt;"
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="row"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="row"/> is not a generic type, or has a mismatched generic parameter.
    /// </exception>
    /// <example>
    /// <para><strong>Normal Usage (via AddRow):</strong></para>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(...);
    /// 
    /// // AddRow calls Add internally:
    /// data.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));
    /// data.AddRow(new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5));  // ← Duplicate, silently ignored
    /// 
    /// // data.Count == 1 (duplicate not added)
    /// </code>
    /// 
    /// <para><strong>Type Mismatch Error (Non-Generic):</strong></para>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(...);
    /// 
    /// ITheoryTestDataRow wrongRow = new CustomNonGenericRow();  // ← Not generic
    /// 
    /// data.Add(wrongRow);  // ← Throws ArgumentException
    /// // Message: "The provided test data row must be a generic type. 
    /// //           Expected: TheoryTestDataRow&lt;TestDataReturns&lt;Int32&gt;&gt;, 
    /// //           Actual: CustomNonGenericRow"
    /// </code>
    /// 
    /// <para><strong>Type Mismatch Error (Wrong Generic Parameter):</strong></para>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(...);
    /// 
    /// var wrongRow = new TheoryTestDataRow&lt;TestDataReturns&lt;string&gt;&gt;(...);
    /// 
    /// data.Add(wrongRow);  // ← Throws ArgumentException
    /// // Message: "The provided test data row has a mismatched generic type parameter. 
    /// //           Expected: TheoryTestDataRow&lt;TestDataReturns&lt;Int32&gt;&gt;, 
    /// //           Actual: TheoryTestDataRow&lt;TestDataReturns&lt;String&gt;&gt;"
    /// </code>
    /// </example>
    /// <seealso cref="AddRow"/>
    /// <seealso cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}.Add(TTheoryDataRow)"/>
    public override void Add(ITheoryTestDataRow row)
    {
        var rowType = NotNull(row, nameof(row)).GetType();

        if (!rowType.IsGenericType)
        {
            throw new ArgumentException(
                $"The provided test data row must be a generic type. " +
                $"Expected: TheoryTestDataRow<{typeof(TTestData).Name}>, " +
                $"Actual: {rowType.Name}",
                nameof(row));
        }

        var genericArgs = rowType.GetGenericArguments();

        if (genericArgs.Length != 1 || genericArgs[0] != typeof(TTestData))
        {
            throw new ArgumentException(
                $"The provided test data row has a mismatched generic type parameter. " +
                $"Expected: TheoryTestDataRow<{typeof(TTestData).Name}>, " +
                $"Actual: {rowType.Name}<{string.Join(", ", genericArgs.Select(t => t.Name))}>",
                nameof(row));
        }

        // Deduplication: only add if TestCaseName is unique
        if (_namedCases.Add(row))
        {
            base.Add(row);
        }
    }

    /// <summary>
    /// Converts Portamical test data to an xUnit v3 theory test data row with explicit parameters.
    /// </summary>
    /// <param name="testData">
    /// The Portamical test data to convert.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert the test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// </param>
    /// <returns>
    /// An <see cref="ITheoryTestDataRow"/> instance containing the converted test data.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Public Conversion API:</strong>
    /// </para>
    /// <para>
    /// This method provides a public API for converting test data with explicit parameters, allowing callers
    /// to override the instance configuration (<see cref="ArgsCode"/> and <see cref="TestMethodName"/>).
    /// </para>
    /// <para>
    /// <strong>vs. Protected Convert:</strong>
    /// </para>
    /// <para>
    /// Unlike the protected <see cref="Convert(TTestData)"/> method (which uses instance configuration),
    /// this method accepts explicit parameters:
    /// </para>
    /// <code>
    /// // Convert (protected) - uses instance config:
    /// protected override ITheoryTestDataRow Convert(TTestData row)
    /// =&gt; ConvertRow(row, ArgsCode, TestMethodName);
    /// //             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Uses this.ArgsCode, this.TestMethodName
    /// 
    /// // ConvertRow (public) - explicit parameters:
    /// public ITheoryTestDataRow ConvertRow(TTestData testData, ArgsCode argsCode, string? testMethodName)
    /// =&gt; new TheoryTestDataRow&lt;TTestData&gt;(testData, argsCode, testMethodName);
    /// //                                               ^^^^^^^^^^^^^^^^^^^^^^^^^^^ Explicit parameters
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Properties,
    ///     "TestAdd");
    /// 
    /// // data.ArgsCode == ArgsCode.Properties
    /// // data.TestMethodName == "TestAdd"
    /// 
    /// // Convert with explicit parameters (overrides instance config):
    /// ITheoryTestDataRow row = data.ConvertRow(
    ///     new TestDataReturns&lt;int&gt;("Add(10,20)", [10, 20], 30),
    ///     ArgsCode.Instance,      // ← Override instance ArgsCode
    ///     "CustomMethod");        // ← Override instance TestMethodName
    /// 
    /// // row uses ArgsCode.Instance (not data.ArgsCode)
    /// // row.TestDisplayName = "CustomMethod - Add(10,20)" (not "TestAdd - ...")
    /// </code>
    /// </example>
    /// <seealso cref="Convert(TTestData)"/>
    /// <seealso cref="AddRow"/>
    public ITheoryTestDataRow ConvertRow(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    => new TheoryTestDataRow<TTestData>(
        testData,
        argsCode,
        testMethodName);

    /// <summary>
    /// Converts Portamical test data to an xUnit v3 theory test data row using instance configuration.
    /// </summary>
    /// <param name="row">
    /// The Portamical test data to convert.
    /// </param>
    /// <returns>
    /// An <see cref="ITheoryTestDataRow"/> instance containing the converted test data, using
    /// <see cref="ArgsCode"/> and <see cref="TestMethodName"/> from this instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Template Method Implementation:</strong>
    /// </para>
    /// <para>
    /// This method implements the abstract <see cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}.Convert(TDataDeclarationPointer)"/>
    /// template method from xUnit v3's <see cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}"/> base class.
    /// </para>
    /// <para>
    /// It delegates to <see cref="ConvertRow"/> with instance configuration:
    /// </para>
    /// <code>
    /// protected override ITheoryTestDataRow Convert(TTestData row)
    /// =&gt; ConvertRow(testData: row, ArgsCode, TestMethodName);
    /// //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Uses this.ArgsCode, this.TestMethodName
    /// </code>
    /// <para>
    /// <strong>Usage:</strong>
    /// </para>
    /// <para>
    /// This method is called internally by <see cref="AddRow"/> to convert test data using the instance's
    /// configured <see cref="ArgsCode"/> and <see cref="TestMethodName"/> properties.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Properties,
    ///     "TestAdd");
    /// 
    /// // AddRow uses Convert internally:
    /// data.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
    /// 
    /// // Internally:
    /// // 1. AddRow calls Convert(testData)
    /// // 2. Convert calls ConvertRow(testData, ArgsCode.Properties, "TestAdd")
    /// // 3. ConvertRow creates TheoryTestDataRow with those parameters
    /// </code>
    /// </example>
    /// <seealso cref="ConvertRow"/>
    /// <seealso cref="AddRow"/>
    /// <seealso cref="TheoryDataBase{TTheoryDataRow, TDataDeclarationPointer}.Convert(TDataDeclarationPointer)"/>
    protected override ITheoryTestDataRow Convert(TTestData row)
    => ConvertRow(testData: row, ArgsCode, TestMethodName);

    /// <summary>
    /// Adds a test data item to the collection using the builder pattern.
    /// </summary>
    /// <param name="testData">
    /// The Portamical test data to convert and add to the collection.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Builder Pattern:</strong>
    /// </para>
    /// <para>
    /// This method implements the builder pattern, allowing incremental construction of test data:
    /// </para>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(...);
    /// data.AddRow(testData2);  // ← Builder method
    /// data.AddRow(testData3);  // ← Builder method
    /// </code>
    /// <para>
    /// <strong>Delegation Chain:</strong>
    /// </para>
    /// <para>
    /// This method delegates to <see cref="Add"/> after converting the test data:
    /// </para>
    /// <code>
    /// AddRow(testData)
    ///   ↓ calls
    /// Convert(testData)  // Uses ArgsCode, TestMethodName
    ///   ↓ calls
    /// ConvertRow(testData, ArgsCode, TestMethodName)
    ///   ↓ creates
    /// TheoryTestDataRow&lt;TTestData&gt;
    ///   ↓ passed to
    /// Add(row)  // Type validation + Deduplication + add to base collection
    /// </code>
    /// <para>
    /// <strong>Automatic Deduplication:</strong>
    /// </para>
    /// <para>
    /// Duplicate test cases (same <see cref="INamedCase.TestCaseName"/>) are silently ignored by
    /// <see cref="Add"/>. See <see cref="Add"/> for details.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var data = new TheoryTestData&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///     ArgsCode.Properties,
    ///     "TestAdd");
    /// 
    /// // Add rows using builder pattern:
    /// data.AddRow(new TestDataReturns&lt;int&gt;("Add(5,7)", [5, 7], 12));
    /// data.AddRow(new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0));
    /// 
    /// // data.Count == 3
    /// </code>
    /// </example>
    /// <seealso cref="Add"/>
    /// <seealso cref="Convert(TTestData)"/>
    /// <seealso cref="ITestDataProvider{TTestData}.AddRow"/>
    public void AddRow(TTestData testData)
    => Add(Convert(testData));
    #endregion
}