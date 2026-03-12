// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Converters;
using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Portamical.xUnit_v3.Attributes;

/// <summary>
/// Provides a data source for xUnit v3 theory tests, with the data coming from a member of the test class.
/// Extends xUnit v3's <see cref="MemberDataAttributeBase"/> with Portamical-specific enhancements.
/// </summary>
/// <remarks>
/// <para>
/// <strong>xUnit v3 Integration - Modern Async API:</strong>
/// </para>
/// <para>
/// This attribute extends xUnit v3's <see cref="MemberDataAttributeBase"/> (introduced in xUnit v3, released 2024)
/// with Portamical-specific features:
/// <list type="bullet">
///   <item><description>
///     <strong>Automatic Conversion:</strong> Converts <see cref="ITestData"/> to <see cref="ITheoryTestDataRow"/>
///   </description></item>
///   <item><description>
///     <strong>Display Name Enhancement:</strong> Automatically adds test method name to display names
///   </description></item>
///   <item><description>
///     <strong>Deduplication:</strong> Removes duplicate test cases based on <see cref="INamedCase.TestCaseName"/>
///   </description></item>
///   <item><description>
///     <strong>Trait Merging:</strong> Combines row traits with attribute traits
///   </description></item>
///   <item><description>
///     <strong>ArgsCode Support:</strong> Detects and applies <see cref="ArgsCode"/> from attribute arguments
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern: Adapter + Template Method + Async</strong>
/// </para>
/// <para>
/// This class adapts xUnit v3's member data attribute system to support Portamical test data:
/// <code>
/// ITestData (Portamical)
///   ↓ adapts to
/// ITheoryTestDataRow (Portamical + xUnit v3)
///   ↓ implements
/// ITheoryDataRow (xUnit v3 interface)
/// 
/// PortamicalBaseDataAttribute
///   ↓ extends
/// MemberDataAttributeBase (xUnit v3 base)
///   ↓ implements
/// IDataAttribute (xUnit v3 interface)
/// </code>
/// </para>
/// <para>
/// <strong>xUnit v3 Async-First Design:</strong>
/// </para>
/// <para>
/// xUnit v3 uses async methods throughout its API for better performance and scalability:
/// <code>
/// // xUnit v3 API:
/// public override async ValueTask&lt;IReadOnlyCollection&lt;ITheoryDataRow&gt;&gt; GetData(...)
/// //              ^^^^^                                                ^^^^^^^ Async API
/// {
///     var data = await base.GetData(testMethod, disposalTracker)
///                      .ConfigureAwait(false);
///     // Process data asynchronously...
/// }
/// </code>
/// </para>
/// <para>
/// <strong>Discovery Optimization:</strong>
/// </para>
/// <para>
/// This attribute sets <c>DisableDiscoveryEnumeration = true</c> to improve test discovery performance:
/// <list type="bullet">
///   <item><description>
///     Test data is not enumerated during discovery phase
///   </description></item>
///   <item><description>
///     Test data is enumerated only during execution
///   </description></item>
///   <item><description>
///     Faster test discovery for large data sources
///   </description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Basic Usage (Native Style)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.Attributes;
/// 
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     public static IEnumerable&lt;ITheoryTestDataRow&gt; GetAddTestData()
///     {
///         return AddTestData.ToTheoryTestDataRowCollection(
///             ArgsCode.Properties,
///             testMethodName: null);  // ← No method name (added by attribute)
///     }
///     
///     [Theory]
///     [PortamicalData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // xUnit v3 Test Explorer displays:
/// // ✓ TestAdd - Add(2,3)   ← Test method name added automatically
/// // ✓ TestAdd - Add(5,7)   ← Test method name added automatically
/// // ✓ TestAdd - Add(-1,1)  ← Test method name added automatically
/// </code>
/// 
/// <para><strong>Example 2: Using ArgsCode via Attribute Arguments</strong></para>
/// <code>
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static IEnumerable&lt;ITestData&gt; GetAddTestData()
///     {
///         return AddTestData;  // Returns ITestData (not rows)
///     }
///     
///     [Theory]
///     [PortamicalData(nameof(GetAddTestData), ArgsCode.Properties)]
///     //                                       ^^^^^^^^^^^^^^^^^^^^ ArgsCode passed via attribute
///     public void TestAdd(int x, int y, int expected)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// 
/// // Attribute detects ArgsCode.Properties in Arguments property
/// // Converts ITestData → ITheoryTestDataRow with Native Style
/// </code>
/// 
/// <para><strong>Example 3: Trait Merging</strong></para>
/// <code>
/// public static IEnumerable&lt;ITheoryTestDataRow&gt; GetTestData()
/// {
///     // Rows have traits:
///     var row1 = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(...)
///     {
///         Traits = new Dictionary&lt;string, HashSet&lt;string&gt;&gt;
///         {
///             ["Category"] = new HashSet&lt;string&gt; { "Unit" },
///             ["Priority"] = new HashSet&lt;string&gt; { "High" }
///         }
///     };
///     
///     return new[] { row1 };
/// }
/// 
/// [Theory]
/// [PortamicalData(nameof(GetTestData))]
/// [Trait("Category", "Integration")]  // ← Attribute trait
/// [Trait("Slow", "")]                  // ← Attribute trait
/// public void TestMethod(...)
/// {
///     // Test method...
/// }
/// 
/// // Result: Merged traits:
/// // {
/// //   "Category": ["Unit", "Integration"],  ← Merged
/// //   "Priority": ["High"],
/// //   "Slow": [""]  ← New from attribute
/// // }
/// </code>
/// </example>
/// <seealso cref="PortamicalDataAttribute"/>
/// <seealso cref="MemberDataAttributeBase"/>
/// <seealso cref="ITheoryTestDataRow"/>
/// <seealso cref="ITestData"/>
/// <seealso cref="ArgsCode"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class PortamicalBaseDataAttribute
: MemberDataAttributeBase
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PortamicalBaseDataAttribute"/> class.
    /// </summary>
    /// <param name="memberName">
    /// The name of the public member (method, property, or field) that will provide the test data.
    /// The member must be static and return one of:
    /// <list type="bullet">
    ///   <item><description><c>IEnumerable&lt;ITheoryDataRow&gt;</c> (xUnit v3 standard)</description></item>
    ///   <item><description><c>IEnumerable&lt;ITheoryTestDataRow&gt;</c> (Portamical)</description></item>
    ///   <item><description><c>IEnumerable&lt;ITestData&gt;</c> (Portamical - auto-converted)</description></item>
    ///   <item><description><c>IEnumerable&lt;object[]&gt;</c> (legacy xUnit)</description></item>
    /// </list>
    /// </param>
    /// <param name="arguments">
    /// Optional arguments to be passed to the member (only supported for static methods).
    /// <para>
    /// <strong>ArgsCode Detection:</strong> If the first argument is of type <see cref="ArgsCode"/>,
    /// it will be used to convert <see cref="ITestData"/> to <see cref="ITheoryTestDataRow"/>.
    /// </para>
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Access Modifier (private protected):</strong>
    /// </para>
    /// <para>
    /// The constructor is <c>private protected</c> to allow only the derived sealed class
    /// <see cref="PortamicalDataAttribute"/> to call it, while preventing external derivation.
    /// This ensures controlled inheritance and maintains API stability.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong>
    /// </para>
    /// <para>
    /// The constructor validates that <paramref name="memberName"/> is not <c>null</c> or empty using
    /// <see cref="ArgumentException.ThrowIfNullOrEmpty(string, string)"/> (.NET 7+).
    /// </para>
    /// <para>
    /// <strong>Discovery Optimization:</strong>
    /// </para>
    /// <para>
    /// Sets <see cref="MemberDataAttributeBase.DisableDiscoveryEnumeration"/> to <c>true</c> for improved
    /// test discovery performance:
    /// <list type="bullet">
    ///   <item><description>
    ///     xUnit v3 does not enumerate test data during discovery phase
    ///   </description></item>
    ///   <item><description>
    ///     Test data is enumerated only during test execution
    ///   </description></item>
    ///   <item><description>
    ///     Reduces discovery time for large data sources
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="memberName"/> is <c>null</c> or empty.
    /// </exception>
    private protected PortamicalBaseDataAttribute(
        string memberName,
        params object[] arguments)
    : base(memberName, arguments)
    {
        ArgumentException.ThrowIfNullOrEmpty(memberName);

        DisableDiscoveryEnumeration = true;
    }

    #endregion

    #region MemberDataAttributeBase Methods

    /// <summary>
    /// Asynchronously retrieves the test data from the specified member, with Portamical-specific enhancements.
    /// </summary>
    /// <param name="testMethod">
    /// The test method that will use the test data.
    /// </param>
    /// <param name="disposalTracker">
    /// A tracker for managing disposable objects created during data retrieval.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
    /// The result is a read-only collection of <see cref="ITheoryDataRow"/> instances containing the test data,
    /// with duplicates removed and display names enhanced.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>xUnit v3 Async-First API:</strong>
    /// </para>
    /// <para>
    /// This method overrides <see cref="MemberDataAttributeBase.GetData(MethodInfo, DisposalTracker)"/>,
    /// implementing xUnit v3's async-first design:
    /// <code>
    /// public override async ValueTask&lt;IReadOnlyCollection&lt;ITheoryDataRow&gt;&gt; GetData(...)
    /// //              ^^^^^                                                ^^^^^^^ Async API
    /// {
    ///     var data = await base.GetData(testMethod, disposalTracker)
    ///                      .ConfigureAwait(false);
    ///     // Process data asynchronously...
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Processing Pipeline:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Retrieve Base Data:</strong> Calls <see cref="MemberDataAttributeBase.GetData"/> to get raw test data
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Null Check:</strong> Returns empty collection if base returns <c>null</c>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Type Detection:</strong> Uses reflection to detect if data is <see cref="ITheoryTestDataRow"/>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Display Name Enhancement:</strong> Adds test method name to rows without display names
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Deduplication:</strong> Removes duplicates using <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/>
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Display Name Logic:</strong>
    /// </para>
    /// <code>
    /// // If row has no display name:
    /// if (string.IsNullOrEmpty(ttdr.TestDisplayName))
    /// {
    ///     // Create new row with test method name:
    ///     ttdr = new TheoryTestDataRow(ttdr, testMethod.Name);
    ///     // Result: "TestMethodName - TestCaseName"
    /// }
    /// </code>
    /// <para>
    /// <strong>Runtime Type Detection:</strong>
    /// </para>
    /// <para>
    /// The method uses reflection to detect if the data collection contains <see cref="ITheoryTestDataRow"/>:
    /// <code>
    /// var runtimeGenericType = theoryDataRowCollection
    ///     .GetType()
    ///     .GetGenericArguments()?[0];
    /// 
    /// if (runtimeGenericType?.IsAssignableTo(typeof(ITheoryTestDataRow)) != true)
    /// {
    ///     return theoryDataRowCollection;  // Not Portamical, return as-is
    /// }
    /// </code>
    /// This allows the attribute to work with both Portamical and non-Portamical data sources.
    /// </para>
    /// <para>
    /// <strong>Deduplication:</strong>
    /// </para>
    /// <para>
    /// Uses <see cref="HashSet{T}"/> with <see cref="NamedCase.Comparer"/> for efficient O(1) deduplication:
    /// <code>
    /// HashSet&lt;ITheoryTestDataRow&gt; ttdrCollection = new(NamedCase.Comparer);
    /// // Comparer uses TestCaseName for equality/hashing (ordinal)
    /// 
    /// ttdrCollection.Add(ttdr);  // Duplicate TestCaseNames ignored
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Data source with no display names:
    /// public static IEnumerable&lt;ITheoryTestDataRow&gt; GetTestData()
    /// {
    ///     return new[]
    ///     {
    ///         new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(
    ///             testData,
    ///             ArgsCode.Properties,
    ///             testMethodName: null)  // ← No display name
    ///     };
    /// }
    /// 
    /// [Theory]
    /// [PortamicalData(nameof(GetTestData))]
    /// public void TestAdd(int x, int y, int expected) { }
    /// 
    /// // GetData() processing:
    /// // 1. Base returns row with TestDisplayName = null
    /// // 2. This method detects null display name
    /// // 3. Creates new row with testMethodName = "TestAdd"
    /// // 4. Result: TestDisplayName = "TestAdd - TestCaseName"
    /// </code>
    /// </example>
    /// <seealso cref="MemberDataAttributeBase.GetData(MethodInfo, DisposalTracker)"/>
    /// <seealso cref="ConvertDataRow(object)"/>
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var testMethodName = testMethod.Name;
        var theoryDataRowCollection =
            await base.GetData(testMethod, disposalTracker)
            .ConfigureAwait(false);

        if (theoryDataRowCollection is null)
        {
            return [];
        }

        if (string.IsNullOrEmpty(testMethodName))
        {
            return theoryDataRowCollection;
        }

        var runtimeGenericType = theoryDataRowCollection
            .GetType()
            .GetGenericArguments()?[0];

        if (runtimeGenericType?.IsAssignableTo(typeof(ITheoryTestDataRow)) != true)
        {
            return theoryDataRowCollection;
        }

        HashSet<ITheoryTestDataRow> ttdrCollection = new(NamedCase.Comparer);

        foreach (var item in theoryDataRowCollection)
        {
            var ttdr = (item as ITheoryTestDataRow)!;

            if (string.IsNullOrEmpty(ttdr.TestDisplayName))
            {
                ttdr = new TheoryTestDataRow(ttdr, testMethodName);
            }

            ttdrCollection.Add(ttdr);
        }

        return ttdrCollection.CastOrToReadOnlyCollection();
    }

    /// <summary>
    /// Converts a single data row from the data source to an <see cref="ITheoryDataRow"/>, with Portamical-specific
    /// handling for <see cref="ITestData"/> and <see cref="ITheoryTestDataRow"/>.
    /// </summary>
    /// <param name="dataRow">
    /// A single data row from the data source. Can be:
    /// <list type="bullet">
    ///   <item><description><see cref="ITheoryTestDataRow"/> (Portamical row) - Traits are merged</description></item>
    ///   <item><description><see cref="ITestData"/> (Portamical test data) - Converted to <see cref="ITheoryTestDataRow"/></description></item>
    ///   <item><description><c>object[]</c> (legacy xUnit) - Passed to base class</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// An <see cref="ITheoryDataRow"/> instance representing the converted data row.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Conversion Logic:</strong>
    /// </para>
    /// <para>
    /// This method implements a three-tier conversion strategy:
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     <strong>ITheoryTestDataRow (Portamical row):</strong>
    ///     <list type="bullet">
    ///       <item><description>Merges attribute traits into row traits using <c>mergeTraits</c> local function</description></item>
    ///       <item><description>Creates copy via <see cref="TheoryTestDataRow"/> copy constructor</description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>ITestData (Portamical test data):</strong>
    ///     <list type="bullet">
    ///       <item><description>Detects <see cref="ArgsCode"/> from <see cref="MemberDataAttributeBase.Arguments"/> property using pattern matching</description></item>
    ///       <item><description>Defaults to <see cref="ArgsCode.Instance"/> if not provided</description></item>
    ///       <item><description>Converts to <see cref="ITheoryTestDataRow"/> via <see cref="TestDataConverter.ToTheoryTestDataRow{TTestData}"/></description></item>
    ///     </list>
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Other types:</strong> Delegates to <see cref="MemberDataAttributeBase.ConvertDataRow(object)"/>
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>ArgsCode Detection (Pattern Matching):</strong>
    /// </para>
    /// <code>
    /// if (Arguments is not [ArgsCode argsCode])
    /// //                   ^^^^^^^^^^^^^^^^^^^ Pattern matching (C# 11)
    /// {
    ///     argsCode = ArgsCode.Instance;  // Default
    /// }
    /// 
    /// // Equivalent to:
    /// // if (Arguments is null || Arguments.Length != 1 || Arguments[0] is not ArgsCode)
    /// // {
    /// //     argsCode = ArgsCode.Instance;
    /// // }
    /// // else
    /// // {
    /// //     argsCode = (ArgsCode)Arguments[0];
    /// // }
    /// </code>
    /// <para>
    /// <strong>Trait Merging:</strong>
    /// </para>
    /// <para>
    /// When the data row is <see cref="ITheoryTestDataRow"/>, attribute traits are merged into row traits:
    /// <code>
    /// // Row traits: { "Category": ["Unit"], "Priority": ["High"] }
    /// // Attribute traits: ["Category:Integration", "Slow"]
    /// 
    /// mergeTraits(theoryTestDataRow, Traits);
    /// 
    /// // Result:
    /// // {
    /// //   "Category": ["Unit", "Integration"],  ← Merged
    /// //   "Priority": ["High"],
    /// //   "Slow": [""]  ← New from attribute
    /// // }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Local Function (mergeTraits):</strong>
    /// </para>
    /// <para>
    /// The <c>mergeTraits</c> local function:
    /// <list type="bullet">
    ///   <item><description>Copies row traits to new dictionary (case-insensitive keys)</description></item>
    ///   <item><description>Uses <see cref="TestIntrospectionHelper.MergeTraitsInto"/> to merge attribute traits</description></item>
    ///   <item><description>Modifies row's <see cref="ITheoryDataRow.Traits"/> property in-place</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: ITestData Conversion with ArgsCode</strong></para>
    /// <code>
    /// // Data source returns ITestData:
    /// public static IEnumerable&lt;ITestData&gt; GetTestData()
    /// {
    ///     return new[] { new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5) };
    /// }
    /// 
    /// [Theory]
    /// [PortamicalData(nameof(GetTestData), ArgsCode.Properties)]
    /// //                                    ^^^^^^^^^^^^^^^^^^^^ Detected by ConvertDataRow
    /// public void TestAdd(int x, int y, int expected) { }
    /// 
    /// // ConvertDataRow processing:
    /// // 1. dataRow is ITestData (not ITheoryTestDataRow)
    /// // 2. Arguments = [ArgsCode.Properties]
    /// // 3. Pattern matching: argsCode = ArgsCode.Properties
    /// // 4. Converts to ITheoryTestDataRow with Native Style
    /// </code>
    /// 
    /// <para><strong>Example 2: ITheoryTestDataRow with Trait Merging</strong></para>
    /// <code>
    /// // Data source returns ITheoryTestDataRow:
    /// public static IEnumerable&lt;ITheoryTestDataRow&gt; GetTestData()
    /// {
    ///     var row = new TheoryTestDataRow&lt;TestDataReturns&lt;int&gt;&gt;(...)
    ///     {
    ///         Traits = new Dictionary&lt;string, HashSet&lt;string&gt;&gt;
    ///         {
    ///             ["Category"] = new HashSet&lt;string&gt; { "Unit" }
    ///         }
    ///     };
    ///     return new[] { row };
    /// }
    /// 
    /// [Theory]
    /// [PortamicalData(nameof(GetTestData))]
    /// [Trait("Category", "Integration")]
    /// public void TestAdd(int x, int y, int expected) { }
    /// 
    /// // ConvertDataRow processing:
    /// // 1. dataRow is ITheoryTestDataRow
    /// // 2. mergeTraits merges ["Category:Integration"] into row.Traits
    /// // 3. Result: { "Category": ["Unit", "Integration"] }
    /// </code>
    /// </example>
    /// <seealso cref="MemberDataAttributeBase.ConvertDataRow(object)"/>
    /// <seealso cref="GetData(MethodInfo, DisposalTracker)"/>
    /// <seealso cref="TestDataConverter.ToTheoryTestDataRow{TTestData}"/>
    protected override ITheoryDataRow ConvertDataRow(object dataRow)
    {
        if (dataRow is not ITheoryTestDataRow theoryTestDataRow)
        {
            if (dataRow is not ITestData testData)
            {
                return base.ConvertDataRow(dataRow);
            }

            if (Arguments is not [ArgsCode argsCode])
            {
                argsCode = ArgsCode.Instance;
            }

            return testData.ToTheoryTestDataRow(argsCode);
        }

        mergeTraits(theoryTestDataRow, Traits);

        return new TheoryTestDataRow(theoryTestDataRow, null);

        #region Local Methods
        static void mergeTraits(ITheoryTestDataRow ttdr, string[]? attributeTraits)
        {
            var ttdrTraits = ttdr.Traits;
            var traits = new Dictionary<string, HashSet<string>>(
                StringComparer.OrdinalIgnoreCase);

            if (ttdrTraits is not null)
            {
                foreach (var kvp in ttdrTraits)
                {
                    traits.AddOrGet(kvp.Key).AddRange(kvp.Value);
                }
            }

            TestIntrospectionHelper.MergeTraitsInto(traits, attributeTraits);
        }
        #endregion
    }

    #endregion
}

/// <summary>
/// Provides a data source for xUnit v3 theory tests, with the data coming from a member of the test class.
/// </summary>
/// <param name="memberName">
/// The name of the public member (method, property, or field) that will provide the test data.
/// </param>
/// <param name="arguments">
/// Optional arguments to be passed to the member (only supported for static methods).
/// If the first argument is of type <see cref="ArgsCode"/>, it will be used to convert
/// <see cref="ITestData"/> to <see cref="ITheoryTestDataRow"/>.
/// </param>
/// <remarks>
/// <para>
/// <strong>Sealed Class - Complete Inheritance Hierarchy:</strong>
/// </para>
/// <para>
/// This class is <c>sealed</c> to indicate that the inheritance hierarchy is complete.
/// It's a thin wrapper over <see cref="PortamicalBaseDataAttribute"/> with no additional functionality.
/// </para>
/// <para>
/// <strong>Primary Constructor (C# 12):</strong>
/// </para>
/// <para>
/// Uses C# 12's primary constructor syntax, which defines constructor parameters directly in the
/// class declaration. The parameters are automatically passed to the base class constructor.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.Attributes;
/// 
/// public class CalculatorTests
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     public static IEnumerable&lt;ITheoryTestDataRow&gt; GetAddTestData()
///     {
///         return AddTestData.ToTheoryTestDataRowCollection(
///             ArgsCode.Properties,
///             testMethodName: null);
///     }
///     
///     [Theory]
///     [PortamicalData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         int result = Calculator.Add(x, y);
///         Assert.Equal(expected, result);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="PortamicalBaseDataAttribute"/>
/// <seealso cref="MemberDataAttributeBase"/>
/// <seealso cref="ITheoryTestDataRow"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class PortamicalDataAttribute(
    string memberName,
    params object[] arguments)
: PortamicalBaseDataAttribute(
    memberName,
    arguments);