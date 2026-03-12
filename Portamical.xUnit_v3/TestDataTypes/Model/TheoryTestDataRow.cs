// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using static Portamical.Core.Identity.Model.NamedCase;

namespace Portamical.xUnit_v3.TestDataTypes.Model;

/// <summary>
/// Represents a theory test data row for xUnit v3 that combines xUnit v3's <see cref="TheoryDataRowBase"/>
/// with Portamical's <see cref="ITheoryTestDataRow"/> for enhanced test case naming and deduplication.
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Modern Test Framework:</strong>
/// </para>
/// <para>
/// This class extends xUnit v3's <see cref="TheoryDataRowBase"/> and implements Portamical's
/// <see cref="ITheoryTestDataRow"/> interface, providing:
/// <list type="bullet">
///   <item><description>
///     <strong>Custom Test Names:</strong> xUnit v3's <c>TestDisplayName</c> property populated from
///     <see cref="ITestData.TestCaseName"/>
///   </description></item>
///   <item><description>
///     <strong>Test Case Naming:</strong> Portamical's <see cref="INamedCase.TestCaseName"/> for consistent identification
///   </description></item>
///   <item><description>
///     <strong>Deduplication:</strong> Equality and hashing based on <see cref="TestCaseName"/> via <see cref="INamedCase"/>
///   </description></item>
///   <item><description>
///     <strong>xUnit v3 Features:</strong> Skip, timeout, traits, explicit test support via <see cref="TheoryDataRowBase"/>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Adapter + Immutable Data</strong>
/// </para>
/// <para>
/// This class adapts Portamical's <see cref="ITestData"/> to xUnit v3's <see cref="ITheoryDataRow"/> interface:
/// <code>
/// ITestData (Portamical)
///   ↓ adapts to
/// TheoryTestDataRow (this class)
///   ↓ extends
/// TheoryDataRowBase (xUnit v3 base)
///   ↓ implements
/// ITheoryDataRow (xUnit v3 interface)
/// 
/// TheoryTestDataRow (this class)
///   ↓ implements
/// ITheoryTestDataRow (Portamical)
///   ↓ extends
/// ITheoryDataRow + INamedCase
/// </code>
/// </para>
/// <para>
/// <strong>Immutability:</strong>
/// </para>
/// <para>
/// This class ensures immutability after construction:
/// <list type="bullet">
///   <item><description><c>_data</c> field is <c>readonly</c> (cannot be reassigned)</description></item>
///   <item><description><c>TestCaseName</c> is <c>init-only</c> (set during construction only)</description></item>
///   <item><description>Copy constructor performs deep copy of <c>Traits</c> (prevents shared mutable state)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Constructor Patterns:</strong>
/// </para>
/// <para>
/// This class provides three constructor patterns:
/// <list type="number">
///   <item><description>
///     <strong>Private Primary Constructor:</strong> Core initialization logic (validation, display name creation)
///   </description></item>
///   <item><description>
///     <strong>Protected ITestData Constructor:</strong> Creates row from <see cref="ITestData"/> with <see cref="ArgsCode"/>
///   </description></item>
///   <item><description>
///     <strong>Internal Copy Constructor:</strong> Creates row from existing <see cref="ITheoryTestDataRow"/> with optional
///     display name override
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Generic Variant:</strong>
/// </para>
/// <para>
/// A sealed generic class <see cref="TheoryTestDataRow{TTestData}"/> provides type-safe construction with
/// compile-time enforcement of the <c>TTestData : notnull, ITestData</c> constraint.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Creating from ITestData (Native Style)</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// // Create theory data row with ArgsCode.Properties (Native Style)
/// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,
///     ArgsCode.Properties,
///     testMethodName: "TestAdd");
/// 
/// // row.TestDisplayName = "TestAdd - Add(2,3)"
/// // row.TestCaseName = "Add(2,3)"
/// // row.GetData() = [2, 3, 5]  ← Flattened properties
/// </code>
/// 
/// <para><strong>Example 2: Creating from ITestData (Shared Style)</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// // Create theory data row with ArgsCode.Instance (Shared Style)
/// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,
///     ArgsCode.Instance,
///     testMethodName: null);
/// 
/// // row.TestDisplayName = "Add(2,3)"
/// // row.TestCaseName = "Add(2,3)"
/// // row.GetData() = [testData]  ← Entire object
/// </code>
/// 
/// <para><strong>Example 3: Copy Constructor (Display Name Override)</strong></para>
/// <code>
/// var original = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,
///     ArgsCode.Properties,
///     testMethodName: null);
/// 
/// // Copy with new display name
/// var renamed = new TheoryTestDataRow(
///     original,
///     testMethodName: "CustomTestMethod");
/// 
/// // renamed.TestDisplayName = "CustomTestMethod - Add(2,3)"
/// // renamed.TestCaseName = "Add(2,3)"  ← Same as original
/// // renamed.GetData() = [2, 3, 5]      ← Same as original (shared)
/// // renamed.Traits = deep copy of original.Traits
/// </code>
/// 
/// <para><strong>Example 4: Deduplication via INamedCase</strong></para>
/// <code>
/// var testData1 = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// var testData2 = new TestDataReturns&lt;int&gt;("Add(2,3)", [5, 7], 12);  // ← Same TestCaseName
/// 
/// var row1 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData1, ArgsCode.Properties, null);
/// var row2 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData2, ArgsCode.Properties, null);
/// 
/// // Equality based on TestCaseName:
/// bool equal = row1.Equals(row2);  // ← true (same TestCaseName)
/// 
/// // Deduplication:
/// var rows = new[] { row1, row2 }.Distinct().ToList();
/// // Result: 1 row (duplicate removed)
/// </code>
/// </example>
/// <seealso cref="TheoryTestDataRow{TTestData}"/>
/// <seealso cref="ITheoryTestDataRow"/>
/// <seealso cref="Xunit.v3.TheoryDataRowBase"/>
/// <seealso cref="INamedCase"/>
/// <seealso cref="ITestData"/>
internal class TheoryTestDataRow
: TheoryDataRowBase,
ITheoryTestDataRow
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TheoryTestDataRow"/> class with the specified
    /// named case, parameter name, data retrieval delegate, and optional test method name.
    /// </summary>
    /// <param name="namedCase">
    /// The named case providing the test case name. Must not be <c>null</c>.
    /// </param>
    /// <param name="paramName">
    /// The parameter name for validation error messages (used with <see cref="Guard.ArgumentNotNull"/>).
    /// </param>
    /// <param name="getData">
    /// A delegate that returns the test method arguments as an <c>object[]</c> array.
    /// This delegate is executed once during construction, and the result is cached in the <see cref="_data"/> field.
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// If provided, the display name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, the display name will be just "TestCaseName".
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Primary Constructor Pattern:</strong>
    /// </para>
    /// <para>
    /// This is the primary constructor that all other constructors chain to. It performs:
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Validation:</strong> Guards against <c>null</c> <paramref name="namedCase"/> using
    ///     <see cref="Guard.ArgumentNotNull"/>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Test Case Name:</strong> Extracts <see cref="TestCaseName"/> from <paramref name="namedCase"/>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Display Name:</strong> Creates <see cref="TheoryDataRowBase.TestDisplayName"/> via
    ///     <see cref="GetDisplayName(string?)"/>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Data Caching:</strong> Executes <paramref name="getData"/> delegate and caches result in
    ///     <see cref="_data"/> field
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Private Access Modifier:</strong>
    /// </para>
    /// <para>
    /// This constructor is <c>private</c> to ensure controlled initialization. All external construction
    /// must go through the public/protected constructors, which provide type-safe overloads for specific
    /// use cases (creating from <see cref="ITestData"/> or copying from <see cref="ITheoryTestDataRow"/>).
    /// </para>
    /// <para>
    /// <strong>Lazy Data Retrieval:</strong>
    /// </para>
    /// <para>
    /// The <paramref name="getData"/> delegate allows lazy evaluation of test method arguments:
    /// <code>
    /// // Delegate is not executed until constructor runs:
    /// new TheoryTestDataRow(
    ///     testData,
    ///     nameof(testData),
    ///     getData: () =&gt; testData.ToArgs(ArgsCode.Properties),
    ///     //       ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ///     //       ToArgs() not called until constructor body
    ///     testMethodName: null);
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="namedCase"/> is <c>null</c>.
    /// </exception>
    private TheoryTestDataRow(
        INamedCase? namedCase,
        string paramName,
        Func<object?[]> getData,
        string? testMethodName)
    {
        namedCase = Guard.ArgumentNotNull(namedCase, paramName);
        TestCaseName = namedCase.TestCaseName;
        TestDisplayName = GetDisplayName(testMethodName);
        _data = getData();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TheoryTestDataRow"/> class from Portamical test data
    /// with the specified argument conversion strategy.
    /// </summary>
    /// <param name="testData">
    /// The Portamical test data to convert to a theory data row. Must not be <c>null</c>.
    /// </param>
    /// <param name="argsCode">
    /// Specifies how to convert the test data to test method arguments:
    /// <list type="bullet">
    ///   <item><description>
    ///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
    ///     (Shared Style: framework-agnostic test methods)
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
    ///     (Native Style: idiomatic xUnit test methods)
    ///   </description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// If provided, the display name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, the display name will be just "TestCaseName".
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Protected Access Modifier:</strong>
    /// </para>
    /// <para>
    /// This constructor is <c>protected</c> to allow the derived generic class
    /// <see cref="TheoryTestDataRow{TTestData}"/> to call it while preventing direct external instantiation
    /// (the class is <c>internal</c>, so external code cannot access it anyway).
    /// </para>
    /// <para>
    /// <strong>Constructor Chaining:</strong>
    /// </para>
    /// <para>
    /// This constructor chains to the private primary constructor, providing:
    /// <list type="bullet">
    ///   <item><description>
    ///     <c>namedCase</c> = <paramref name="testData"/> (implements <see cref="INamedCase"/>)
    ///   </description></item>
    ///   <item><description>
    ///     <c>getData</c> = Lambda that calls <see cref="ITestData.ToArgs(ArgsCode)"/>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Lazy Data Conversion:</strong>
    /// </para>
    /// <para>
    /// The <see cref="ITestData.ToArgs(ArgsCode)"/> conversion is wrapped in a lambda delegate, so it's not
    /// executed until the primary constructor runs. This allows validation to occur before conversion.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testData"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <para><strong>Native Style (ArgsCode.Properties):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Properties,
    ///     "TestAdd");
    /// 
    /// // row.GetData() returns: [2, 3, 5]
    /// // row.TestDisplayName = "TestAdd - Add(2,3)"
    /// </code>
    /// 
    /// <para><strong>Shared Style (ArgsCode.Instance):</strong></para>
    /// <code>
    /// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Instance,
    ///     null);
    /// 
    /// // row.GetData() returns: [testData]
    /// // row.TestDisplayName = "Add(2,3)"
    /// </code>
    /// </example>
    /// <seealso cref="TheoryTestDataRow{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    protected TheoryTestDataRow(
        ITestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    : this(
        namedCase: testData,
        paramName: nameof(testData),
        getData: () => testData.ToArgs(argsCode),
        testMethodName: testMethodName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TheoryTestDataRow"/> class by copying from an existing
    /// <see cref="ITheoryTestDataRow"/> instance with an optional display name override.
    /// </summary>
    /// <param name="other">
    /// The existing theory test data row to copy from. Must not be <c>null</c>.
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// <list type="bullet">
    ///   <item><description>
    ///     If provided, the display name will be formatted as "testMethodName - TestCaseName"
    ///   </description></item>
    ///   <item><description>
    ///     If <c>null</c> and <paramref name="other"/> has a <see cref="TheoryDataRowBase.TestDisplayName"/>,
    ///     the original display name is preserved
    ///   </description></item>
    ///   <item><description>
    ///     If <c>null</c> and <paramref name="other"/> has no display name, the display name will be just "TestCaseName"
    ///   </description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Copy Constructor Pattern:</strong>
    /// </para>
    /// <para>
    /// This constructor creates a new instance by copying properties from an existing <see cref="ITheoryTestDataRow"/>.
    /// It performs:
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Data Reuse:</strong> Uses <paramref name="other"/>'s <see cref="ITheoryDataRow.GetData()"/> method
    ///     (does not re-execute conversion)
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Display Name Logic:</strong> Uses null-coalescing assignment (<c>??=</c>) to preserve original
    ///     display name if no new <paramref name="testMethodName"/> is provided
    ///   </description></item>
    ///   <item><description>
    ///     <strong>xUnit v3 Property Copy:</strong> Copies all xUnit v3 properties (Skip, Timeout, Traits, etc.)
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Deep Copy of Traits:</strong> Creates a new dictionary with new HashSet instances to prevent
    ///     shared mutable state
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Display Name Override Logic:</strong>
    /// </para>
    /// <code>
    /// // Constructor chaining creates new display name:
    /// TestDisplayName = GetDisplayName(testMethodName);
    /// 
    /// // Then, if no new name was created, use original:
    /// TestDisplayName ??= other.TestDisplayName;
    /// 
    /// // Result:
    /// // - If testMethodName provided: "testMethodName - TestCaseName"
    /// // - If testMethodName is null and other.TestDisplayName exists: original display name
    /// // - If both are null: just "TestCaseName"
    /// </code>
    /// <para>
    /// <strong>Deep Copy of Traits:</strong>
    /// </para>
    /// <para>
    /// The <see cref="TheoryDataRowBase.Traits"/> property is deeply copied to prevent shared mutable state:
    /// <code>
    /// Traits = other.Traits?.ToDictionary(
    ///     kvp =&gt; kvp.Key,
    ///     kvp =&gt; new HashSet&lt;string&gt;(kvp.Value))
    ///     //     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Deep copy
    ///     ?? [];
    /// 
    /// // Without deep copy, this would be dangerous:
    /// var copy = new TheoryTestDataRow(original, null);
    /// copy.Traits["Category"].Add("NewTrait");  // ← Would also modify original.Traits!
    /// 
    /// // With deep copy (current implementation):
    /// copy.Traits["Category"].Add("NewTrait");  // ← Only modifies copy, not original
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Use Case:</strong>
    /// </para>
    /// <para>
    /// This constructor is useful when you need to change the display name or xUnit v3 properties
    /// of an existing row without re-converting the test data:
    /// <code>
    /// // Create row with default display name:
    /// var original = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData, ArgsCode.Properties, null);
    /// 
    /// // Copy with new display name (avoids re-converting testData.ToArgs):
    /// var renamed = new TheoryTestDataRow(original, "CustomTestMethod");
    /// </code>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="other"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <para><strong>Copy with Display Name Override:</strong></para>
    /// <code>
    /// var original = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Properties,
    ///     testMethodName: null);
    /// 
    /// // original.TestDisplayName = "Add(2,3)"
    /// 
    /// var renamed = new TheoryTestDataRow(original, "TestAdd");
    /// 
    /// // renamed.TestDisplayName = "TestAdd - Add(2,3)"
    /// // renamed.TestCaseName = "Add(2,3)"  ← Same as original
    /// // renamed.GetData() = [2, 3, 5]      ← Same as original (shared)
    /// </code>
    /// 
    /// <para><strong>Copy Preserving Original Display Name:</strong></para>
    /// <code>
    /// var original = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Properties,
    ///     "OriginalMethod");
    /// 
    /// // original.TestDisplayName = "OriginalMethod - Add(2,3)"
    /// 
    /// var copy = new TheoryTestDataRow(original, testMethodName: null);
    /// 
    /// // copy.TestDisplayName = "OriginalMethod - Add(2,3)"  ← Preserved from original
    /// </code>
    /// </example>
    internal TheoryTestDataRow(
        ITheoryTestDataRow other,
        string? testMethodName)
    : this(
        namedCase: other,
        paramName: nameof(other),
        getData: other.GetData,
        testMethodName: testMethodName)
    {
        TestDisplayName ??= other.TestDisplayName;

        Explicit = other.Explicit;
        Skip = other.Skip;
        Label = other.Label;
        SkipType = other.SkipType;
        SkipUnless = other.SkipUnless;
        SkipWhen = other.SkipWhen;
        Timeout = other.Timeout;
        Traits = other.Traits?.ToDictionary(
            kvp => kvp.Key,
            kvp => new HashSet<string>(kvp.Value))
            ?? [];
    }

    #endregion

    #region Fields

    /// <summary>
    /// Cached test method arguments returned by <see cref="GetData()"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This field is populated during construction by executing the <c>getData</c> delegate
    /// passed to the primary constructor. The array is cached to avoid re-executing the conversion
    /// on each call to <see cref="GetData()"/>.
    /// </para>
    /// <para>
    /// <strong>Immutability:</strong>
    /// </para>
    /// <para>
    /// This field is <c>readonly</c>, meaning it cannot be reassigned after construction. However,
    /// the array contents are technically mutable (arrays in C# are always mutable). To prevent
    /// accidental modification, <see cref="GetData()"/> returns this array directly (xUnit v3
    /// framework design assumes immutability by convention).
    /// </para>
    /// </remarks>
    private readonly object?[] _data;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the test case name used for identification, equality comparison, and hashing.
    /// </summary>
    /// <value>
    /// The test case name extracted from the source <see cref="INamedCase"/> during construction.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is part of the <see cref="INamedCase"/> interface and is used for:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Identification:</strong> Unique identifier for the test case
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Equality:</strong> <see cref="Equals(INamedCase?)"/> compares <c>TestCaseName</c> values
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Hashing:</strong> <see cref="GetHashCode()"/> uses <c>TestCaseName</c> for hash code
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Display Name:</strong> Used in <see cref="GetDisplayName(string?)"/> to create
    ///     <see cref="TheoryDataRowBase.TestDisplayName"/>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Immutability:</strong>
    /// </para>
    /// <para>
    /// This property has an <c>init</c> accessor, meaning it can only be set during object initialization
    /// (constructor or object initializer). After construction, it is immutable.
    /// </para>
    /// <para>
    /// <strong>Relationship to TestDisplayName:</strong>
    /// </para>
    /// <code>
    /// // TestCaseName: "Add(2,3)"
    /// // TestDisplayName: "TestAdd - Add(2,3)"  ← Uses TestCaseName
    /// //                  ^^^^^^^^   ^^^^^^^^^
    /// //                  Method     TestCaseName
    /// </code>
    /// </remarks>
    /// <seealso cref="INamedCase.TestCaseName"/>
    /// <seealso cref="TheoryDataRowBase.TestDisplayName"/>
    /// <seealso cref="GetDisplayName(string?)"/>
    public string TestCaseName { get; init; }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether this test case is contained in the specified collection of named cases.
    /// </summary>
    /// <param name="namedCases">
    /// The collection of named cases to search. Can be <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if this test case's <see cref="TestCaseName"/> matches any <see cref="INamedCase.TestCaseName"/>
    /// in <paramref name="namedCases"/>; otherwise, <c>false</c>.
    /// Returns <c>false</c> if <paramref name="namedCases"/> is <c>null</c> or empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to the static helper <see cref="NamedCase.Contains(INamedCase, IEnumerable{INamedCase}?)"/>,
    /// which performs ordinal (case-sensitive) string comparison on <see cref="TestCaseName"/> values.
    /// </para>
    /// <para>
    /// <strong>Use Case:</strong>
    /// </para>
    /// <para>
    /// This method is useful for filtering test cases based on inclusion in a predefined collection:
    /// </para>
    /// <code>
    /// var allowedCases = new INamedCase[]
    /// {
    ///     new NamedCase("Add(2,3)"),
    ///     new NamedCase("Add(5,7)")
    /// };
    /// 
    /// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData, ArgsCode.Properties, null);
    /// 
    /// if (row.ContainedBy(allowedCases))
    /// {
    ///     // row.TestCaseName matches "Add(2,3)" or "Add(5,7)"
    /// }
    /// </code>
    /// </remarks>
    /// <seealso cref="NamedCase.Contains(INamedCase, IEnumerable{INamedCase}?)"/>
    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    /// <summary>
    /// Determines whether this test case is equal to another named case based on <see cref="TestCaseName"/>.
    /// </summary>
    /// <param name="other">
    /// The other named case to compare with. Can be <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if <paramref name="other"/> is not <c>null</c> and its <see cref="INamedCase.TestCaseName"/>
    /// equals this instance's <see cref="TestCaseName"/> (ordinal comparison); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements <see cref="INamedCase.Equals(INamedCase?)"/> by delegating to the
    /// <see cref="NamedCase.Comparer"/> (an <see cref="IEqualityComparer{INamedCase}"/>), which performs
    /// ordinal (case-sensitive) string comparison on <see cref="TestCaseName"/> values.
    /// </para>
    /// <para>
    /// <strong>Use Case - Deduplication:</strong>
    /// </para>
    /// <para>
    /// This equality implementation enables deduplication of test cases with identical <see cref="TestCaseName"/> values:
    /// </para>
    /// <code>
    /// var testData1 = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testData2 = new TestDataReturns&lt;int&gt;("Add(2,3)", [5, 7], 12);  // ← Same TestCaseName
    /// 
    /// var row1 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData1, ArgsCode.Properties, null);
    /// var row2 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData2, ArgsCode.Properties, null);
    /// 
    /// bool equal = row1.Equals(row2);  // ← true (same TestCaseName)
    /// 
    /// var distinct = new[] { row1, row2 }.Distinct().ToList();
    /// // Result: 1 row (duplicate removed via Equals)
    /// </code>
    /// </remarks>
    /// <seealso cref="Equals(object?)"/>
    /// <seealso cref="GetHashCode()"/>
    /// <seealso cref="NamedCase.Comparer"/>
    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    /// <summary>
    /// Determines whether this test case is equal to the specified object.
    /// </summary>
    /// <param name="obj">
    /// The object to compare with. Can be <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if <paramref name="obj"/> is an <see cref="INamedCase"/> and its
    /// <see cref="INamedCase.TestCaseName"/> equals this instance's <see cref="TestCaseName"/>
    /// (ordinal comparison); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="object.Equals(object?)"/> by delegating to the typed
    /// <see cref="Equals(INamedCase?)"/> method after safely casting <paramref name="obj"/> to
    /// <see cref="INamedCase"/> using the <c>as</c> operator.
    /// </para>
    /// <para>
    /// <strong>Implementation Pattern:</strong>
    /// </para>
    /// <code>
    /// public override bool Equals(object? obj)
    /// =&gt; Equals(obj as INamedCase);
    ///    ^^^^^^ Typed Equals
    ///    ^^^^^^^^^^^^^^^^^^^^^ Safe cast (returns null if cast fails)
    /// </code>
    /// <para>
    /// <strong>Why Override Object.Equals?</strong>
    /// </para>
    /// <para>
    /// Overriding <see cref="object.Equals(object?)"/> is necessary for:
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Collection Behavior:</strong> Enables correct behavior in collections like <see cref="List{T}.Contains(T)"/>,
    ///     <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource})"/>, etc.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Consistency:</strong> Ensures <see cref="GetHashCode()"/> and <see cref="Equals(object?)"/> are consistent
    ///     (required by C# guidelines)
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="Equals(INamedCase?)"/>
    /// <seealso cref="GetHashCode()"/>
    public override bool Equals(object? obj)
    => Equals(obj as INamedCase);

    /// <summary>
    /// Creates a display name for the test case by combining the test method name with the test case name.
    /// </summary>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name.
    /// <list type="bullet">
    ///   <item><description>
    ///     If provided and non-empty: Returns "testMethodName - TestCaseName"
    ///   </description></item>
    ///   <item><description>
    ///     If <c>null</c> or empty: Returns just "TestCaseName"
    ///   </description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// A display name formatted as "testMethodName - TestCaseName" or just "TestCaseName" if
    /// <paramref name="testMethodName"/> is <c>null</c> or empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to the static helper <see cref="NamedCase.CreateDisplayName(string?, string)"/>,
    /// which handles the logic for combining the method name and test case name with a " - " separator.
    /// </para>
    /// <para>
    /// <strong>Use Case:</strong>
    /// </para>
    /// <para>
    /// This method is called during construction to populate <see cref="TheoryDataRowBase.TestDisplayName"/>,
    /// which is used by xUnit v3 to display test names in the Test Explorer.
    /// </para>
    /// <para>
    /// <strong>Format Examples:</strong>
    /// </para>
    /// <code>
    /// // With testMethodName:
    /// GetDisplayName("TestAdd")
    /// // Returns: "TestAdd - Add(2,3)"
    /// 
    /// // Without testMethodName:
    /// GetDisplayName(null)
    /// // Returns: "Add(2,3)"
    /// </code>
    /// </remarks>
    /// <seealso cref="TheoryDataRowBase.TestDisplayName"/>
    /// <seealso cref="NamedCase.CreateDisplayName(string?, string)"/>
    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    /// <summary>
    /// Returns a hash code for this test case based on its <see cref="TestCaseName"/>.
    /// </summary>
    /// <returns>
    /// A hash code computed from <see cref="TestCaseName"/> using ordinal (case-sensitive) hashing.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="object.GetHashCode()"/> by delegating to the
    /// <see cref="NamedCase.Comparer"/> (an <see cref="IEqualityComparer{INamedCase}"/>), which computes
    /// the hash code using <see cref="StringComparer.Ordinal"/>.
    /// </para>
    /// <para>
    /// <strong>Consistency with Equals:</strong>
    /// </para>
    /// <para>
    /// This hash code implementation is consistent with <see cref="Equals(INamedCase?)"/> and
    /// <see cref="Equals(object?)"/>, ensuring that:
    /// <code>
    /// if (row1.Equals(row2))
    /// {
    ///     // Then: row1.GetHashCode() == row2.GetHashCode()
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Use Case - Deduplication:</strong>
    /// </para>
    /// <para>
    /// This hash code enables efficient deduplication in hash-based collections:
    /// </para>
    /// <code>
    /// var rows = new HashSet&lt;TheoryTestDataRow&gt;();
    /// 
    /// var row1 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData1, ArgsCode.Properties, null);
    /// var row2 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(testData2, ArgsCode.Properties, null);
    /// 
    /// rows.Add(row1);
    /// rows.Add(row2);  // ← If same TestCaseName, not added (duplicate detected via hash + equals)
    /// </code>
    /// </remarks>
    /// <seealso cref="Equals(INamedCase?)"/>
    /// <seealso cref="Equals(object?)"/>
    /// <seealso cref="NamedCase.Comparer"/>
    public override int GetHashCode()
    => Comparer.GetHashCode(this);

    #region Non-Public Methods

    /// <summary>
    /// Returns the cached test method arguments.
    /// </summary>
    /// <returns>
    /// The <c>object[]</c> array containing test method arguments, as populated during construction.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements the abstract <see cref="TheoryDataRowBase.GetData()"/> template method
    /// from xUnit v3's <see cref="TheoryDataRowBase"/> class. It is called by the xUnit v3 framework
    /// via the <see cref="ITheoryDataRow.GetData()"/> interface method to retrieve test method arguments.
    /// </para>
    /// <para>
    /// <strong>Sealed Modifier:</strong>
    /// </para>
    /// <para>
    /// This method is marked <c>sealed</c> to prevent further overriding in derived classes. The data
    /// is cached in the <see cref="_data"/> field during construction, so there is no need for derived
    /// classes to override this method.
    /// </para>
    /// <para>
    /// <strong>Protected Override Sealed Pattern:</strong>
    /// </para>
    /// <code>
    /// // xUnit v3 base class:
    /// public abstract class TheoryDataRowBase : ITheoryDataRow
    /// {
    ///     protected abstract object?[] GetData();  // ← Template method
    ///     
    ///     object?[] ITheoryDataRow.GetData() =&gt; GetData();
    ///     //                                     ^^^^^^^ Delegates to protected abstract
    /// }
    /// 
    /// // Portamical implementation:
    /// internal class TheoryTestDataRow : TheoryDataRowBase
    /// {
    ///     protected override sealed object?[] GetData()
    ///     //                 ^^^^^^ Prevents further overrides
    ///     =&gt; _data;
    ///     // ^^^^^ Returns cached readonly field
    /// }
    /// </code>
    /// <para>
    /// <strong>Immutability Consideration:</strong>
    /// </para>
    /// <para>
    /// The <see cref="_data"/> field is <c>readonly</c>, but arrays in C# are always mutable. This method
    /// returns the array directly (not a clone) for performance reasons. xUnit v3's framework design
    /// assumes immutability by convention (callers should not modify the returned array).
    /// </para>
    /// </remarks>
    /// <seealso cref="TheoryDataRowBase.GetData()"/>
    /// <seealso cref="ITheoryDataRow.GetData()"/>
    protected override sealed object?[] GetData()
    => _data;

    #endregion

    #endregion
}

/// <summary>
/// Represents a type-safe theory test data row for xUnit v3 that enforces compile-time constraints
/// on the test data type.
/// </summary>
/// <typeparam name="TTestData">
/// The type of test data, which must be a reference type implementing <see cref="ITestData"/>.
/// </typeparam>
/// <param name="testData">
/// The Portamical test data to convert to a theory data row. Must not be <c>null</c>.
/// </param>
/// <param name="argsCode">
/// Specifies how to convert the test data to test method arguments:
/// <list type="bullet">
///   <item><description>
///     <see cref="ArgsCode.Instance"/> - Pass entire <see cref="ITestData"/> object as single argument
///     (Shared Style)
///   </description></item>
///   <item><description>
///     <see cref="ArgsCode.Properties"/> - Pass flattened properties as separate arguments
///     (Native Style)
///   </description></item>
/// </list>
/// </param>
/// <param name="testMethodName">
/// Optional test method name to prepend to the test case name in the display name.
/// </param>
/// <remarks>
/// <para>
/// <strong>Generic Wrapper Pattern:</strong>
/// </para>
/// <para>
/// This sealed generic class is a thin wrapper over the non-generic <see cref="TheoryTestDataRow"/> base class.
/// It provides type-safe construction with compile-time enforcement of the <c>TTestData : notnull, ITestData</c>
/// constraint.
/// </para>
/// <para>
/// <strong>Why Generic Variant?</strong>
/// </para>
/// <para>
/// The generic variant provides:
/// <list type="bullet">
///   <item><description>
///     <strong>Type Safety:</strong> Compile-time enforcement that <typeparamref name="TTestData"/> implements
///     <see cref="ITestData"/>
///   </description></item>
///   <item><description>
///     <strong>Non-Null Constraint:</strong> <c>notnull</c> constraint prevents nullable reference types
///   </description></item>
///   <item><description>
///     <strong>Cleaner API:</strong> Explicit generic parameter makes the type constraint visible at call sites
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Primary Constructor (C# 12):</strong>
/// </para>
/// <para>
/// This class uses C# 12's primary constructor syntax, which defines constructor parameters directly in the
/// class declaration. The parameters are automatically passed to the base class constructor.
/// </para>
/// <para>
/// <strong>Sealed Modifier:</strong>
/// </para>
/// <para>
/// This class is <c>sealed</c> to indicate that the inheritance hierarchy is complete. There is no need
/// to derive from this generic class, as it's a thin wrapper with no additional functionality.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Type-Safe Construction:</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// 
/// // Generic class enforces TTestData : notnull, ITestData constraint:
/// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,
///     ArgsCode.Properties,
///     "TestAdd");
/// 
/// // ❌ This would not compile:
/// // var row = new TheoryTestDataRow&lt;string&gt;(...)
/// //                                  ^^^^^^ Error: 'string' does not implement ITestData
/// </code>
/// 
/// <para><strong>vs. Non-Generic Base Class:</strong></para>
/// <code>
/// // Non-generic base class (less safe):
/// var row = new TheoryTestDataRow(
///     testData,      // ← No compile-time type checking
///     ArgsCode.Properties,
///     "TestAdd");
/// 
/// // Generic variant (type-safe):
/// var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,      // ← Compile-time enforcement of ITestData constraint
///     ArgsCode.Properties,
///     "TestAdd");
/// </code>
/// </example>
/// <seealso cref="TheoryTestDataRow"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode"/>
internal sealed class TheoryTestDataRow<TTestData>(
    TTestData testData,
    ArgsCode argsCode,
    string? testMethodName)
: TheoryTestDataRow(testData, argsCode, testMethodName)
where TTestData : notnull, ITestData;