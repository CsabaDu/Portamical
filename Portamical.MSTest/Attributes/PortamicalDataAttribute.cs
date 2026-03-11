// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.Attributes;

/// <summary>
/// Abstract base class for MSTest data-driven test attributes that enhance test display names
/// with test case name intelligence.
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
///   <item>Delegates data retrieval to inner <see cref="DynamicDataAttribute"/></item>
///   <item>Enhances display name logic with test case name detection</item>
///   <item>Falls back to standard behavior if no test case name detected</item>
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
/// public void TestMethod(string testCaseName, arg1, arg2, ...)
/// //                     ^^^^^^^^^^^^^^^^^^ FIRST parameter
/// </code>
/// <para>
/// <strong>Integration with Portamical Test Data Infrastructure:</strong>
/// </para>
/// <para>
/// This attribute is designed to work seamlessly with:
/// <list type="bullet">
///   <item><see cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/> - Prepends test case names to parameter arrays</item>
///   <item><see cref="TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/> - Convenience wrapper in test base classes</item>
///   <item><see cref="ITestData"/> - Source test data interface</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Complete Integration Example:</strong></para>
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
///     public static IEnumerable&lt;object[]&gt; GetAddTestData()
///         =&gt; Convert(AddTestData, ArgsCode.InOut);
///     //     Result: [["Add(2,3)", 2, 3, 5], ["Add(5,7)", 5, 7, 12]]
///     //              ^^^^^^^^^^ TestCaseName first
///     
///     [DataTestMethod]
///     [PortamicalData(nameof(GetAddTestData), DynamicDataSourceType.Method)]
///     //              ^^^^^^^^^^^^^^^^^^^^^^ Uses enhanced display names
///     public void TestAdd(string testCaseName, int x, int y, int expected)
///     {
///         // Test Explorer shows: "TestAdd - Add(2,3)" instead of "TestAdd (Add(2,3), 2, 3, 5)"
///         var result = Calculator.Add(x, y);
///         Assert.AreEqual(expected, result, $"Failed: {testCaseName}");
///     }
/// }
/// </code>
/// 
/// <para><strong>Result in Visual Studio Test Explorer:</strong></para>
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
/// <seealso cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="ITestDataSource"/>
/// <seealso cref="NamedCase"/>
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
    /// <strong>Constructor Selection Logic:</strong> Uses pattern matching on the combination of
    /// <paramref name="declaringType"/>, <paramref name="sourceType"/>, and <paramref name="sourceArgs"/>
    /// to select the correct <see cref="DynamicDataAttribute"/> constructor overload.
    /// </para>
    /// <para>
    /// <strong>Validation:</strong>
    /// <list type="bullet">
    ///   <item><paramref name="sourceName"/> must not be null or empty</item>
    ///   <item><paramref name="sourceType"/> and <paramref name="sourceArgs"/> cannot both be specified (mutual exclusion)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Supported Combinations:</strong> Covers all 6 valid <see cref="DynamicDataAttribute"/> constructor overloads
    /// plus 1 invalid combination (sourceType + sourceArgs).
    /// </para>
    /// </remarks>
    /// <param name="sourceName">The name of the method or property that provides test data. Cannot be null or empty.</param>
    /// <param name="declaringType">
    /// The type that declares the data source member. If null, uses the test class type.
    /// </param>
    /// <param name="sourceType">
    /// Specifies whether the data source is a Property or Method. If null and <paramref name="sourceArgs"/> is also null,
    /// MSTest infers the type.
    /// </param>
    /// <param name="sourceArgs">
    /// Arguments to pass to the data source method. Only valid for method sources. Cannot be used with <paramref name="sourceType"/>.
    /// </param>
    /// <returns>
    /// A <see cref="DynamicDataAttribute"/> configured with the appropriate constructor overload.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><paramref name="sourceName"/> is null or empty</item>
    ///   <item>Both <paramref name="sourceType"/> and <paramref name="sourceArgs"/> are specified</item>
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
                "Cannot specify both sourceType and sourceArgs"),

            // new DynamicDataAttribute(sourceName, declaringType, sourceType)
            (not null, not null, null) => new(sourceName, declaringType, sourceType.Value),

            // new DynamicDataAttribute(sourceName, declaringType, sourceArgs)
            (not null, null, not null) => new(sourceName, declaringType, sourceArgs),

            // new DynamicDataAttribute(sourceName, declaringType)
            (not null, null, null) => new(sourceName, declaringType),

            // new DynamicDataAttribute(sourceName, sourceType)
            (null, not null, null) => new(sourceName, sourceType.Value),

            // new DynamicDataAttribute(sourceName, sourceArgs)
            (null, null, not null) => new(sourceName, sourceArgs),

            // new DynamicDataAttribute(sourceName)
            _ => new(sourceName),
        };
    }

    /// <summary>
    /// Retrieves test data by delegating to the inner <see cref="DynamicDataAttribute"/>.
    /// </summary>
    /// <param name="methodInfo">The test method metadata.</param>
    /// <returns>
    /// An enumerable collection of object arrays, where each array represents one test case's parameters.
    /// </returns>
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
    ///   <item>
    ///     <strong>Check first element:</strong> If <paramref name="data"/> has at least one element
    ///     and <c>data[0]</c> is a <c>string</c> or implements <see cref="INamedCase"/>, treat it as
    ///     the test case name.
    ///   </item>
    ///   <item>
    ///     <strong>Use custom display name:</strong> Call <see cref="NamedCase.CreateDisplayName(MethodInfo, object?[]?)"/>
    ///     to generate a friendly display name like <c>"TestMethod - TestCaseName"</c>.
    ///   </item>
    ///   <item>
    ///     <strong>Fallback:</strong> If step 1 fails (no test case name detected), delegate to
    ///     <see cref="DynamicDataAttribute.GetDisplayName(MethodInfo, object?[]?)"/> for standard behavior.
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Example Transformation:</strong>
    /// </para>
    /// <code>
    /// // Input data:  ["Add(2,3)", 2, 3, 5]
    /// //              ^^^^^^^^^^ Detected as test case name
    /// 
    /// // Output:     "TestAdd - Add(2,3)"
    /// //             ^^^^^^^^   ^^^^^^^^^
    /// //             Method     TestCaseName
    /// 
    /// // vs. Standard DynamicDataAttribute output:
    /// // "TestAdd (Add(2,3), 2, 3, 5)"  ← Verbose, includes all parameters
    /// </code>
    /// </remarks>
    /// <param name="methodInfo">The test method metadata. Cannot be null.</param>
    /// <param name="data">
    /// The test case parameter array. If the first element is a <c>string</c> or <see cref="INamedCase"/>,
    /// it's used for display name generation.
    /// </param>
    /// <returns>
    /// A display name for the test case, either enhanced with the test case name or using standard formatting.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="methodInfo"/> is null.
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
///   <item>
///     <strong>Cleaner Test Explorer:</strong> Shows <c>"TestMethod - TestCaseName"</c> instead of
///     <c>"TestMethod (param1, param2, ...)"</c>
///   </item>
///   <item>
///     <strong>API Compatibility:</strong> Drop-in replacement for <see cref="DynamicDataAttribute"/>
///     (all 6 constructor overloads supported)
///   </item>
///   <item>
///     <strong>Automatic Detection:</strong> No configuration needed if test data follows Portamical conventions
///   </item>
///   <item>
///     <strong>Graceful Fallback:</strong> Works like standard <see cref="DynamicDataAttribute"/> if no test case name detected
///   </item>
/// </list>
/// </para>
/// <para>
/// <strong>Requirements:</strong>
/// </para>
/// <list type="bullet">
///   <item>Test data arrays must have test case name (<c>string</c> or <see cref="INamedCase"/>) as <strong>first element</strong></item>
///   <item>Test method must have <c>string testCaseName</c> as <strong>first parameter</strong></item>
///   <item>Use with <see cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/> for automatic format compliance</item>
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
/// <para><strong>Complete Working Example:</strong></para>
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
///     public static IEnumerable&lt;object[]&gt; GetAddTestData()
///         =&gt; Convert(AddTestData, ArgsCode.InOut);
///     
///     [DataTestMethod]
///     [PortamicalData(nameof(GetAddTestData), DynamicDataSourceType.Method)]
///     public void TestAdd(string testCaseName, int x, int y, int expected)
///     {
///         // Visual Studio Test Explorer shows:
///         // ✓ TestAdd - Add(2,3)
///         // ✓ TestAdd - Add(5,7)
///         // ✓ TestAdd - Add(-1,1)
///         
///         var result = Calculator.Add(x, y);
///         Assert.AreEqual(expected, result, $"Failed: {testCaseName}");
///     }
/// }
/// </code>
/// 
/// <para><strong>All Constructor Overloads:</strong></para>
/// <code>
/// // 1. Simple (method or property in same class)
/// [PortamicalData("GetTestData")]
/// 
/// // 2. With source type
/// [PortamicalData("GetTestData", DynamicDataSourceType.Method)]
/// 
/// // 3. With source arguments
/// [PortamicalData("GetTestData", "arg1", "arg2")]
/// 
/// // 4. With declaring type
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
/// <seealso cref="CollectionConverter.ToArgsWithTestCaseName{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
/// <seealso cref="TestBase.Convert{TTestData}(IEnumerable{TTestData}, ArgsCode)"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class PortamicalDataAttribute : PortamicalBaseDataAttribute
{
    /// <summary>
    /// Initializes a new instance with a data source name.
    /// </summary>
    /// <param name="sourceName">The name of the method or property providing test data.</param>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData")]  // Property or method in same class
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName)
    : base(sourceName) { }

    /// <summary>
    /// Initializes a new instance with a data source name and source type.
    /// </summary>
    /// <param name="sourceName">The name of the method or property providing test data.</param>
    /// <param name="sourceType">Specifies whether the source is a Method or Property.</param>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", DynamicDataSourceType.Method)]
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, DynamicDataSourceType sourceType)
    : base(sourceName, sourceType: sourceType) { }

    /// <summary>
    /// Initializes a new instance with a data source name and arguments.
    /// </summary>
    /// <param name="sourceName">The name of the method providing test data.</param>
    /// <param name="sourceArgs">Arguments to pass to the data source method.</param>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", "arg1", "arg2")]
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, params object?[] sourceArgs)
    : base(sourceName, sourceArgs: sourceArgs) { }

    /// <summary>
    /// Initializes a new instance with a data source name and declaring type.
    /// </summary>
    /// <param name="sourceName">The name of the method or property providing test data.</param>
    /// <param name="declaringType">The type that declares the data source member.</param>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", typeof(SharedTestData))]
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, Type declaringType)
    : base(sourceName, declaringType: declaringType) { }

    /// <summary>
    /// Initializes a new instance with a data source name, declaring type, and source type.
    /// </summary>
    /// <param name="sourceName">The name of the method or property providing test data.</param>
    /// <param name="declaringType">The type that declares the data source member.</param>
    /// <param name="sourceType">Specifies whether the source is a Method or Property.</param>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", typeof(SharedTestData), DynamicDataSourceType.Method)]
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, Type declaringType, DynamicDataSourceType sourceType)
    : base(sourceName, declaringType: declaringType, sourceType: sourceType) { }

    /// <summary>
    /// Initializes a new instance with a data source name, declaring type, and arguments.
    /// </summary>
    /// <param name="sourceName">The name of the method providing test data.</param>
    /// <param name="declaringType">The type that declares the data source method.</param>
    /// <param name="sourceArgs">Arguments to pass to the data source method.</param>
    /// <example>
    /// <code>
    /// [DataTestMethod]
    /// [PortamicalData("GetTestData", typeof(SharedTestData), "arg1", "arg2")]
    /// public void TestMethod(string testCaseName, ...) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, Type declaringType, params object?[] sourceArgs)
    : base(sourceName, declaringType: declaringType, sourceArgs: sourceArgs) { }
}