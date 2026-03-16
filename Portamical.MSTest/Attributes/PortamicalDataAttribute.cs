// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.Attributes;

/// <summary>
/// Abstract base class for MSTest data-driven test attributes that enhance test display names
/// with intelligent test case name detection.
/// </summary>
/// <remarks>
/// <para>
/// This attribute extends MSTest's <see cref="DynamicDataAttribute"/> by automatically detecting
/// and using test case names for improved test result displays in Visual Studio Test Explorer.
/// </para>
/// <para>
/// <strong>Design Pattern: Decorator</strong>
/// </para>
/// <para>
/// Wraps <see cref="DynamicDataAttribute"/> using the Decorator pattern:
/// <list type="bullet">
///   <item><description>
///     Delegates data retrieval to inner <see cref="DynamicDataAttribute"/>
///   </description></item>
///   <item><description>
///     Enhances display name logic with test case name detection
///   </description></item>
///   <item><description>
///     Falls back to standard behavior if no test case name detected
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Display Name Enhancement Logic:</strong>
/// </para>
/// <para>
/// If the first element of test data is a <c>string</c> or implements <see cref="INamedCase"/>,
/// it's treated as the test case name and used for display name generation via
/// <see cref="NamedCase.CreateDisplayName(MethodInfo, object?[]?)"/>. Otherwise, falls back to
/// <see cref="DynamicDataAttribute"/>'s default display name logic.
/// </para>
/// <para>
/// <strong>IMPORTANT: Parameter Order Assumption</strong>
/// </para>
/// <para>
/// This attribute assumes test data arrays have <strong>test case name as the first element</strong>,
/// as produced by <see cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>.
/// Test method signatures must match this pattern:
/// </para>
/// <code>
/// // Shared Style (ArgsCode.Instance):
/// public void TestMethod(string testCaseName, ITestData testData)
/// //                     ^^^^^^^^^^^^^^^^^^ FIRST parameter
/// //                                       ^^^^^^^^^^^^^ SECOND parameter (entire test data object)
/// 
/// // Native Style (ArgsCode.Properties):
/// public void TestMethod(string testCaseName, T1 arg1, T2 arg2, ...)
/// //                     ^^^^^^^^^^^^^^^^^^ FIRST parameter
/// //                                       ^^^^^^^^^^^^^ REMAINING parameters (flattened properties)
/// </code>
/// <para>
/// <strong>Integration with Portamical Test Data Infrastructure:</strong>
/// </para>
/// <para>
/// This attribute is designed to work seamlessly with:
/// <list type="bullet">
///   <item><description>
///     <see cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
///     - Prepends test case names to parameter arrays (supports both ArgsCode.Instance and ArgsCode.Properties)
///   </description></item>
///   <item><description>
///     <see cref="TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
///     - Convenience wrapper in test base classes
///   </description></item>
///   <item><description>
///     <see cref="ITestData"/> - Source test data interface
///   </description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Shared Style (ArgsCode.Instance)</strong></para>
/// <code>
/// using Microsoft.VisualStudio.TestTools.UnitTesting;
/// using Portamical.MSTest.Attributes;
/// using Portamical.MSTest.TestBases;
/// 
/// [TestClass]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     // Shared Style: Pass entire test data object
///     public static IEnumerable&lt;object[]&gt; GetSharedStyleTestData()
///         =&gt; Convert(AddTestData, ArgsCode.Instance);
///     //     Result: [["Add(2,3)", testDataInstance], ["Add(5,7)", testDataInstance]]
///     //              ^^^^^^^^^^ TestCaseName first
///     //                        ^^^^^^^^^^^^^^^^^ Entire ITestData object
///     
///     [DataTestMethod]
///     [PortamicalData(nameof(GetSharedStyleTestData), DynamicDataSourceType.Method)]
///     public void TestAddShared(string testCaseName, TestDataReturns&lt;int&gt; testData)
///     {
///         // Framework-agnostic test method
///         var args = testData.Args;
///         var expected = testData.Expected;
///         
///         var result = Calculator.Add((int)args[0], (int)args[1]);
///         Assert.AreEqual(expected, result, $"Failed: {testCaseName}");
///     }
/// }
/// 
/// // Test Explorer shows: "TestAddShared - Add(2,3)" ✓
/// //                      "TestAddShared - Add(5,7)" ✓
/// </code>
/// 
/// <para><strong>Example 2: Native Style (ArgsCode.Properties)</strong></para>
/// <code>
/// [TestClass]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12)
///     ];
///     
///     // Native Style: Pass flattened properties
///     public static IEnumerable&lt;object[]&gt; GetNativeStyleTestData()
///         =&gt; Convert(AddTestData, ArgsCode.Properties);
///     //     Result: [["Add(2,3)", 2, 3, 5], ["Add(5,7)", 5, 7, 12]]
///     //              ^^^^^^^^^^ TestCaseName first
///     //                        ^^^^^^^^^ Flattened properties
///     
///     [DataTestMethod]
///     [PortamicalData(nameof(GetNativeStyleTestData), DynamicDataSourceType.Method)]
///     public void TestAddNative(string testCaseName, int x, int y, int expected)
///     {
///         // Idiomatic MSTest with separate typed parameters
///         var result = Calculator.Add(x, y);
///         Assert.AreEqual(expected, result, $"Failed: {testCaseName}");
///     }
/// }
/// 
/// // Test Explorer shows: "TestAddNative - Add(2,3)" ✓
/// //                      "TestAddNative - Add(5,7)" ✓
/// </code>
/// 
/// <para><strong>Comparison with Standard DynamicDataAttribute:</strong></para>
/// <code>
/// // With PortamicalDataAttribute (enhanced):
/// ✓ TestAdd - Add(2,3)
/// ✓ TestAdd - Add(5,7)
/// 
/// // With standard DynamicDataAttribute (verbose):
/// ✓ TestAdd (Add(2,3), 2, 3, 5)
/// ✓ TestAdd (Add(5,7), 5, 7, 12)
/// </code>
/// </example>
/// <seealso cref="PortamicalDataAttribute"/>
/// <seealso cref="DynamicDataAttribute"/>
/// <seealso cref="Portamical.Converters.CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="ITestDataSource"/>
/// <seealso cref="NamedCase"/>
/// <seealso cref="ArgsCode"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class PortamicalBaseDataAttribute(
    string sourceName,
    Type? declaringType = null,
    DynamicDataSourceType? sourceType = null,
    object?[]? sourceArgs = null)
: Attribute, ITestDataSource
{
    private readonly DynamicDataAttribute _innerAttribute =
        Create(sourceName, declaringType, sourceType, sourceArgs);

    /// <summary>
    /// Factory method that creates the appropriate <see cref="DynamicDataAttribute"/> constructor overload
    /// based on which optional parameters are provided.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Constructor Selection Logic:</strong>
    /// </para>
    /// <para>
    /// Uses pattern matching on the combination of <paramref name="declaringType"/>,
    /// <paramref name="sourceType"/>, and <paramref name="sourceArgs"/> to select the correct
    /// <see cref="DynamicDataAttribute"/> constructor overload.
    /// </para>
    /// <para>
    /// <strong>Validation Rules:</strong>
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="sourceName"/> must not be null or empty
    ///   </description></item>
    ///   <item><description>
    ///     <paramref name="sourceType"/> and <paramref name="sourceArgs"/> cannot both be specified
    ///     (mutual exclusion - method can be identified by type OR by having arguments, not both)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Supported Combinations:</strong>
    /// </para>
    /// <para>
    /// Covers all 6 valid <see cref="DynamicDataAttribute"/> constructor overloads:
    /// <list type="table">
    ///   <listheader>
    ///     <term>Overload</term>
    ///     <description>Parameters</description>
    ///   </listheader>
    ///   <item>
    ///     <term>1</term>
    ///     <description><c>new(sourceName)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>2</term>
    ///     <description><c>new(sourceName, sourceType)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>3</term>
    ///     <description><c>new(sourceName, sourceArgs)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>4</term>
    ///     <description><c>new(sourceName, declaringType)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>5</term>
    ///     <description><c>new(sourceName, declaringType, sourceType)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>6</term>
    ///     <description><c>new(sourceName, declaringType, sourceArgs)</c></description>
    ///   </item>
    /// </list>
    /// Plus 1 invalid combination (sourceType + sourceArgs simultaneously).
    /// </para>
    /// </remarks>
    /// <param name="sourceName">
    /// The name of the method or property that provides test data. Cannot be null or empty.
    /// </param>
    /// <param name="declaringType">
    /// The type that declares the data source member. If <c>null</c>, uses the test class type.
    /// </param>
    /// <param name="sourceType">
    /// Specifies whether the data source is a <see cref="DynamicDataSourceType.Property"/> or
    /// <see cref="DynamicDataSourceType.Method"/>. If <c>null</c> and <paramref name="sourceArgs"/>
    /// is also <c>null</c>, MSTest infers the type automatically.
    /// </param>
    /// <param name="sourceArgs">
    /// Arguments to pass to the data source method. Only valid for method sources.
    /// Cannot be used simultaneously with <paramref name="sourceType"/>.
    /// </param>
    /// <returns>
    /// A <see cref="DynamicDataAttribute"/> configured with the appropriate constructor overload.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="sourceName"/> is <c>null</c> or empty
    ///   </description></item>
    ///   <item><description>
    ///     Both <paramref name="sourceType"/> and <paramref name="sourceArgs"/> are specified
    ///     (mutual exclusion violated)
    ///   </description></item>
    /// </list>
    /// </exception>
    private static DynamicDataAttribute Create(
        string sourceName,
        Type? declaringType,
        DynamicDataSourceType? sourceType,
        object?[]? sourceArgs)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        return (declaringType, sourceType, sourceArgs) switch
        {
            // Validation: mutual exclusion
            (_, not null, not null) => throw new ArgumentException(
                "Cannot specify both sourceType and sourceArgs. " +
                "Use sourceType to explicitly mark as Method/Property, " +
                "OR use sourceArgs for parameterized methods (implies Method type)."),

            // Overload 6: new DynamicDataAttribute(sourceName, declaringType, sourceArgs)
            (not null, null, not null) => new(sourceName, declaringType, sourceArgs),

            // Overload 5: new DynamicDataAttribute(sourceName, declaringType, sourceType)
            (not null, not null, null) => new(sourceName, declaringType, sourceType.Value),

            // Overload 4: new DynamicDataAttribute(sourceName, declaringType)
            (not null, null, null) => new(sourceName, declaringType),

            // Overload 2: new DynamicDataAttribute(sourceName, sourceType)
            (null, not null, null) => new(sourceName, sourceType.Value),

            // Overload 3: new DynamicDataAttribute(sourceName, sourceArgs)
            (null, null, not null) => new(sourceName, sourceArgs),

            // Overload 1: new DynamicDataAttribute(sourceName)
            _ => new(sourceName),
        };
    }

    /// <summary>
    /// Retrieves test data by delegating to the inner <see cref="DynamicDataAttribute"/>.
    /// </summary>
    /// <param name="methodInfo">
    /// The test method metadata. Used by MSTest to locate the data source member.
    /// </param>
    /// <returns>
    /// An enumerable collection of object arrays, where each array represents one test case's parameters.
    /// Each array should have test case name as the first element for enhanced display names.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates entirely to <see cref="DynamicDataAttribute.GetData(MethodInfo)"/>.
    /// The decorator pattern means this attribute doesn't modify data retrieval, only display name generation.
    /// </para>
    /// </remarks>
    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    => _innerAttribute.GetData(methodInfo);

    /// <summary>
    /// Gets the display name for a test case, with intelligent test case name detection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Display Name Selection Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Check first element:</strong> If <paramref name="data"/> has at least one element
    ///     and <c>data[0]</c> is a <c>string</c> or implements <see cref="INamedCase"/>, treat it as
    ///     the test case name.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Use custom display name:</strong> Call
    ///     <see cref="NamedCase.CreateDisplayName(MethodInfo, object?[]?)"/> to generate a friendly
    ///     display name like <c>"TestMethod - TestCaseName"</c>.
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Fallback:</strong> If step 1 fails (no test case name detected), delegate to
    ///     <see cref="DynamicDataAttribute.GetDisplayName(MethodInfo, object?[]?)"/> for standard behavior.
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Example Transformation:</strong>
    /// </para>
    /// <code>
    /// // Input data (ArgsCode.Properties):
    /// ["Add(2,3)", 2, 3, 5]
    /// ^^^^^^^^^^ Detected as test case name
    /// 
    /// // Enhanced output:
    /// "TestAdd - Add(2,3)"
    /// ^^^^^^^^   ^^^^^^^^^
    /// Method     TestCaseName
    /// 
    /// // vs. Standard DynamicDataAttribute output:
    /// "TestAdd (Add(2,3), 2, 3, 5)"  ← Verbose, includes all parameters
    /// 
    /// // Input data (ArgsCode.Instance):
    /// ["Add(2,3)", testDataInstance]
    /// ^^^^^^^^^^ Detected as test case name
    /// 
    /// // Enhanced output:
    /// "TestAdd - Add(2,3)"
    /// 
    /// // vs. Standard DynamicDataAttribute output:
    /// "TestAdd (Add(2,3), Portamical.Core.TestDataTypes.TestDataReturns`1[System.Int32])"  ← Very verbose
    /// </code>
    /// </remarks>
    /// <param name="methodInfo">
    /// The test method metadata. Cannot be <c>null</c>.
    /// </param>
    /// <param name="data">
    /// The test case parameter array. If the first element is a <c>string</c> or <see cref="INamedCase"/>,
    /// it's used for display name generation.
    /// <para>
    /// Expected formats:
    /// <list type="bullet">
    ///   <item><description>
    ///     ArgsCode.Instance: <c>[testCaseName, testDataInstance]</c>
    ///   </description></item>
    ///   <item><description>
    ///     ArgsCode.Properties: <c>[testCaseName, arg1, arg2, ..., argN]</c>
    ///   </description></item>
    /// </list>
    /// </para>
    /// </param>
    /// <returns>
    /// A display name for the test case, either enhanced with the test case name
    /// (format: <c>"MethodName - TestCaseName"</c>) or using standard formatting
    /// (format: <c>"MethodName (param1, param2, ...)"</c>).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="methodInfo"/> is <c>null</c>.
    /// </exception>
    public string? GetDisplayName(MethodInfo methodInfo, params object?[]? data)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        string? displayName =
            data is { Length: > 0 } &&
            data[0] is string or INamedCase ?
                NamedCase.CreateDisplayName(methodInfo, data)
                : null;

        return displayName
            ?? _innerAttribute.GetDisplayName(methodInfo, data);
    }
}

/// <summary>
/// MSTest attribute for data-driven tests with enhanced display names showing test case names.
/// </summary>
/// <remarks>
/// <para>
/// This sealed attribute provides a drop-in replacement for <see cref="DynamicDataAttribute"/> with
/// automatic test case name detection for improved test result readability in Visual Studio Test Explorer.
/// </para>
/// <para>
/// <strong>Key Benefits:</strong>
/// <list type="bullet">
///   <item><description>
///     <strong>Cleaner Test Explorer:</strong> Shows <c>"TestMethod - TestCaseName"</c> instead of
///     <c>"TestMethod (param1, param2, ...)"</c>
///   </description></item>
///   <item><description>
///     <strong>API Compatibility:</strong> Drop-in replacement for <see cref="DynamicDataAttribute"/>
///     (all 6 constructor overloads supported)
///   </description></item>
///   <item><description>
///     <strong>Automatic Detection:</strong> No configuration needed if test data follows Portamical conventions
///   </description></item>
///   <item><description>
///     <strong>Graceful Fallback:</strong> Works like standard <see cref="DynamicDataAttribute"/> if
///     no test case name detected
///   </description></item>
///   <item><description>
///     <strong>ArgsCode Strategy Support:</strong> Works with both <see cref="ArgsCode.Instance"/> (Shared Style)
///     and <see cref="ArgsCode.Properties"/> (Native Style)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Requirements:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     Test data arrays must have test case name (<c>string</c> or <see cref="INamedCase"/>) as
///     <strong>first element</strong>
///   </description></item>
///   <item><description>
///     Test method must have <c>string testCaseName</c> as <strong>first parameter</strong>
///   </description></item>
///   <item><description>
///     Use with <see cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
///     for automatic format compliance
///   </description></item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage (Drop-in Replacement):</strong></para>
/// <code>
/// // Replace:
/// [DynamicDataAttribute(nameof(GetTestData), DynamicDataSourceType.Method)]
/// 
/// // With:
/// [PortamicalData(nameof(GetTestData), DynamicDataSourceType.Method)]
/// //               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
/// //               Same parameters as DynamicDataAttribute!
/// </code>
/// 
/// <para><strong>Example 1: Shared Style (ArgsCode.Instance)</strong></para>
/// <code>
/// using Microsoft.VisualStudio.TestTools.UnitTesting;
/// using Portamical.MSTest.Attributes;
/// using Portamical.MSTest.TestBases;
/// 
/// [TestClass]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     // Shared Style: Pass entire test data object
///     public static IEnumerable&lt;object[]&gt; GetSharedStyleTestData()
///         =&gt; Convert(AddTestData, ArgsCode.Instance);
///     
///     [DataTestMethod]
///     [PortamicalData(nameof(GetSharedStyleTestData), DynamicDataSourceType.Method)]
///     public void TestAddShared(string testCaseName, TestDataReturns&lt;int&gt; testData)
///     {
///         // Visual Studio Test Explorer shows:
///         // ✓ TestAddShared - Add(2,3)
///         // ✓ TestAddShared - Add(5,7)
///         // ✓ TestAddShared - Add(-1,1)
///         
///         var args = testData.Args;
///         var expected = testData.Expected;
///         
///         var result = Calculator.Add((int)args[0], (int)args[1]);
///         Assert.AreEqual(expected, result, $"Failed: {testCaseName}");
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 2: Native Style (ArgsCode.Properties)</strong></para>
/// <code>
/// [TestClass]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     // Native Style: Pass flattened properties
///     public static IEnumerable&lt;object[]&gt; GetNativeStyleTestData()
///         =&gt; Convert(AddTestData, ArgsCode.Properties);
///     
///     [DataTestMethod]
///     [PortamicalData(nameof(GetNativeStyleTestData), DynamicDataSourceType.Method)]
///     public void TestAddNative(string testCaseName, int x, int y, int expected)
///     {
///         // Visual Studio Test Explorer shows:
///         // ✓ TestAddNative - Add(2,3)
///         // ✓ TestAddNative - Add(5,7)
///         // ✓ TestAddNative - Add(-1,1)
///         
///         var result = Calculator.Add(x, y);
///         Assert.AreEqual(expected, result, $"Failed: {testCaseName}");
///     }
/// }
/// </code>
/// 
/// <para><strong>All Constructor Overloads (API Parity with DynamicDataAttribute):</strong></para>
/// <code>
/// // 1. Simple (method or property in same class)
/// [PortamicalData("GetTestData")]
/// 
/// // 2. With source type (explicitly mark as Method or Property)
/// [PortamicalData("GetTestData", DynamicDataSourceType.Method)]
/// 
/// // 3. With source arguments (implies Method type)
/// [PortamicalData("GetTestData", "arg1", "arg2")]
/// 
/// // 4. With declaring type (data source in different class)
/// [PortamicalData("GetTestData", typeof(TestDataProvider))]
/// 
/// // 5. With declaring type and source type
/// [PortamicalData("GetTestData", typeof(TestDataProvider), DynamicDataSourceType.Method)]
/// 
/// // 6. With declaring type and source arguments
/// [PortamicalData("GetTestData", typeof(TestDataProvider), "arg1", "arg2")]
/// </code>
/// </example>
/// <seealso cref="PortamicalBaseDataAttribute"/>
/// <seealso cref="DynamicDataAttribute"/>
/// <seealso cref="Portamical.Converters.CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="ArgsCode"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class PortamicalDataAttribute : PortamicalBaseDataAttribute
{
    /// <summary>
    /// Initializes a new instance with a data source name.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the method or property providing test data. MSTest will search for this member
    /// in the test class.
    /// </param>
    /// <remarks>
    /// <para>
    /// Uses the simplest <see cref="DynamicDataAttribute"/> constructor. MSTest will:
    /// <list type="number">
    ///   <item><description>Search for a static member named <paramref name="sourceName"/> in the test class</description></item>
    ///   <item><description>Automatically determine if it's a method or property</description></item>
    ///   <item><description>Invoke it to retrieve test data</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData")]  // Property or method in same class
    /// public void TestMethod(string testCaseName, ...) { }
    /// 
    /// // MSTest will find:
    /// public static IEnumerable&lt;object[]&gt; GetTestData { get; }  // Property
    /// // OR
    /// public static IEnumerable&lt;object[]&gt; GetTestData() { }     // Method
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName)
    : base(sourceName) { }

    /// <summary>
    /// Initializes a new instance with a data source name and explicit source type.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the method or property providing test data.
    /// </param>
    /// <param name="sourceType">
    /// Explicitly specifies whether the source is a <see cref="DynamicDataSourceType.Method"/> or
    /// <see cref="DynamicDataSourceType.Property"/>. Use when you want to avoid ambiguity.
    /// </param>
    /// <remarks>
    /// <para>
    /// Use this overload when you want to be explicit about the member type, or when MSTest's
    /// automatic detection might be ambiguous.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", DynamicDataSourceType.Method)]
    /// public void TestMethod(string testCaseName, ...) { }
    /// 
    /// // MSTest will specifically look for a method (not property):
    /// public static IEnumerable&lt;object[]&gt; GetTestData() { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, DynamicDataSourceType sourceType)
    : base(sourceName, sourceType: sourceType) { }

    /// <summary>
    /// Initializes a new instance with a data source method name and arguments to pass to it.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the method providing test data. Must be a method (not property) since you're passing arguments.
    /// </param>
    /// <param name="sourceArgs">
    /// Arguments to pass to the data source method. The method signature must match these argument types.
    /// </param>
    /// <remarks>
    /// <para>
    /// Use this overload when your data source method accepts parameters. This is useful for
    /// parameterized test data generation.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Implies <see cref="DynamicDataSourceType.Method"/> (properties cannot accept arguments).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", "arg1", "arg2")]
    /// public void TestMethod(string testCaseName, ...) { }
    /// 
    /// // MSTest will invoke:
    /// public static IEnumerable&lt;object[]&gt; GetTestData(string arg1, string arg2) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, params object?[] sourceArgs)
    : base(sourceName, sourceArgs: sourceArgs) { }

    /// <summary>
    /// Initializes a new instance with a data source name and the type that declares it.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the method or property providing test data.
    /// </param>
    /// <param name="declaringType">
    /// The type that declares the data source member. Use this to reference data sources in other classes
    /// (e.g., shared test data providers).
    /// </param>
    /// <remarks>
    /// <para>
    /// Use this overload when your test data is defined in a different class, such as a shared
    /// test data provider or a base test class.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Shared test data provider class
    /// public static class SharedTestData
    /// {
    ///     public static IEnumerable&lt;object[]&gt; GetCalculatorTestData() { }
    /// }
    /// 
    /// // Test class
    /// [DataTestMethod]
    /// [PortamicalData("GetCalculatorTestData", typeof(SharedTestData))]
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, Type declaringType)
    : base(sourceName, declaringType: declaringType) { }

    /// <summary>
    /// Initializes a new instance with a data source name, declaring type, and explicit source type.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the method or property providing test data.
    /// </param>
    /// <param name="declaringType">
    /// The type that declares the data source member.
    /// </param>
    /// <param name="sourceType">
    /// Explicitly specifies whether the source is a <see cref="DynamicDataSourceType.Method"/> or
    /// <see cref="DynamicDataSourceType.Property"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// Combines declaring type specification with explicit source type. Use when referencing
    /// test data from another class and you want to be explicit about the member type.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", typeof(SharedTestData), DynamicDataSourceType.Method)]
    /// public void TestMethod(string testCaseName, ...) { }
    /// 
    /// // MSTest will look for a method in SharedTestData:
    /// public static IEnumerable&lt;object[]&gt; GetTestData() { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, Type declaringType, DynamicDataSourceType sourceType)
    : base(sourceName, declaringType: declaringType, sourceType: sourceType) { }

    /// <summary>
    /// Initializes a new instance with a data source method name, declaring type, and arguments.
    /// </summary>
    /// <param name="sourceName">
    /// The name of the method providing test data. Must be a method (not property).
    /// </param>
    /// <param name="declaringType">
    /// The type that declares the data source method.
    /// </param>
    /// <param name="sourceArgs">
    /// Arguments to pass to the data source method.
    /// </param>
    /// <remarks>
    /// <para>
    /// Combines declaring type specification with method arguments. Use when referencing
    /// a parameterized test data method from another class.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", typeof(SharedTestData), "arg1", "arg2")]
    /// public void TestMethod(string testCaseName, ...) { }
    /// 
    /// // MSTest will invoke:
    /// public static IEnumerable&lt;object[]&gt; GetTestData(string arg1, string arg2) { }
    /// // in SharedTestData class
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, Type declaringType, params object?[] sourceArgs)
    : base(sourceName, declaringType: declaringType, sourceArgs: sourceArgs) { }
}