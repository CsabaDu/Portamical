// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using NUnit.Framework.Interfaces;
using Portamical.Core.Identity.Model;
using System.Reflection;
using static Portamical.NUnit.TestDataTypes.TestCaseTestData;

namespace Portamical.NUnit.Attributes;

/// <summary>
/// Abstract base class for NUnit test data attributes that enhance test display names
/// with intelligent test case name detection.
/// </summary>
/// <remarks>
/// <para>
/// This attribute extends NUnit's <see cref="TestCaseSourceAttribute"/> by automatically detecting
/// and using test case names for improved test result displays in NUnit Test Explorer.
/// </para>
/// <para>
/// <strong>Design Pattern: Decorator + Builder</strong>
/// </para>
/// <para>
/// Wraps <see cref="TestCaseSourceAttribute"/> using the Decorator pattern:
/// <list type="bullet">
///   <item><description>Delegates data retrieval to inner <see cref="TestCaseSourceAttribute"/></description></item>
///   <item><description>Enhances display name logic via <see cref="BuildFrom"/></description></item>
///   <item><description>Falls back to standard NUnit behavior if no enhancement needed</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Display Name Enhancement Logic:</strong>
/// </para>
/// <para>
/// During test discovery, this attribute inspects the <see cref="HasFullNameProperty"/> on each
/// <see cref="TestMethod"/>. If the property is <c>false</c>, it prepends the test method name
/// to create a full display name: "MethodName - TestCaseName".
/// </para>
/// <para>
/// <strong>Integration with Portamical Test Data Infrastructure:</strong>
/// </para>
/// <para>
/// This attribute is designed to work seamlessly with:
/// <list type="bullet">
///   <item><description>
///     <see cref="TestCaseTestData{TTestData}"/> - Sets <see cref="HasFullNameProperty"/> during construction
///   </description></item>
///   <item><description>
///     <see cref="CollectionConverter"/> - Batch conversion methods
///   </description></item>
///   <item><description>
///     <see cref="TestBase"/> - Convenience wrapper in test base classes
///   </description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage (Drop-in Replacement for TestCaseSource):</strong></para>
/// <code>
/// // Replace:
/// [TestCaseSource(nameof(GetTestData))]
/// 
/// // With:
/// [PortamicalData(nameof(GetTestData))]
/// //               ^^^^^^^^^^^^^^^^^^^^
/// //               Same parameters as TestCaseSource!
/// </code>
/// 
/// <para><strong>Complete Working Example:</strong></para>
/// <code>
/// using NUnit.Framework;
/// using Portamical.NUnit.Attributes;
/// using Portamical.NUnit.TestBases;
/// 
/// [TestFixture]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     public static IEnumerable&lt;TestCaseData&gt; GetAddTestData()
///         =&gt; Convert(AddTestData, ArgsCode.Properties, nameof(TestAdd));
///     
///     [TestMethod]
///     [PortamicalData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y)
///     {
///         // NUnit Test Explorer shows:
///         // ✓ TestAdd - Add(2,3)
///         // ✓ TestAdd - Add(5,7)
///         // ✓ TestAdd - Add(-1,1)
///         
///         int result = Calculator.Add(x, y);
///         // NUnit automatically compares result with ExpectedResult
///     }
/// }
/// </code>
/// 
/// <para><strong>Comparison with Standard TestCaseSource:</strong></para>
/// <code>
/// // With PortamicalDataAttribute (enhanced):
/// ✓ TestAdd - Add(2,3)
/// ✓ TestAdd - Add(5,7)
/// 
/// // With standard TestCaseSourceAttribute (verbose):
/// ✓ TestAdd(2, 3)
/// ✓ TestAdd(5, 7)
/// </code>
/// </example>
/// <seealso cref="PortamicalDataAttribute"/>
/// <seealso cref="TestCaseSourceAttribute"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="TestBase"/>
/// <seealso cref="TestCaseTestData{TTestData}"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class PortamicalBaseDataAttribute(
    string sourceName,
    Type? sourceType = null,
    object?[]? methodParams = null)
: NUnitAttribute, ITestBuilder, IImplyFixture
{
    private readonly TestCaseSourceAttribute _innerAttribute =
    Create(sourceName, sourceType, methodParams);

    /// <summary>
    /// Factory method that creates the appropriate <see cref="TestCaseSourceAttribute"/> constructor overload
    /// based on which optional parameters are provided.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Constructor Selection Logic:</strong>
    /// </para>
    /// <para>
    /// Uses pattern matching on the combination of <paramref name="sourceType"/> and
    /// <paramref name="methodParams"/> to select the correct <see cref="TestCaseSourceAttribute"/>
    /// constructor overload.
    /// </para>
    /// <para>
    /// <strong>Validation Rules:</strong>
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="sourceName"/> must not be null or empty
    ///   </description></item>
    ///   <item><description>
    ///     If <paramref name="sourceType"/> is provided, it must be a class or interface (not struct)
    ///   </description></item>
    ///   <item><description>
    ///     If <paramref name="sourceType"/> is provided, the source member must exist and be public static
    ///   </description></item>
    ///   <item><description>
    ///     The source member must return <see cref="System.Collections.IEnumerable"/>
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Supported Combinations:</strong>
    /// </para>
    /// <para>
    /// Covers all 4 valid <see cref="TestCaseSourceAttribute"/> constructor overloads:
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
    ///     <description><c>new(sourceName, methodParams)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>3</term>
    ///     <description><c>new(sourceType, sourceName)</c></description>
    ///   </item>
    ///   <item>
    ///     <term>4</term>
    ///     <description><c>new(sourceType, sourceName, methodParams)</c></description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="sourceName">
    /// The name of the method or property that provides test data. Cannot be null or empty.
    /// </param>
    /// <param name="sourceType">
    /// The type that declares the data source member. If <c>null</c>, uses the test class type.
    /// Must be a class or interface (not struct).
    /// </param>
    /// <param name="methodParams">
    /// Arguments to pass to the data source method. Only valid for method sources.
    /// </param>
    /// <returns>
    /// A <see cref="TestCaseSourceAttribute"/> configured with the appropriate constructor overload.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    ///   <item><description>
    ///     <paramref name="sourceName"/> is <c>null</c> or empty
    ///   </description></item>
    ///   <item><description>
    ///     <paramref name="sourceType"/> is a struct (must be class or interface)
    ///   </description></item>
    ///   <item><description>
    ///     Source member not found or not accessible on <paramref name="sourceType"/>
    ///   </description></item>
    ///   <item><description>
    ///     Source member does not return <see cref="System.Collections.IEnumerable"/>
    ///   </description></item>
    /// </list>
    /// </exception>
    private static TestCaseSourceAttribute Create(
        string sourceName,
        Type? sourceType,
        object?[]? methodParams)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        if (sourceType is not null)
        {
            validateSourceType(sourceType, sourceName);
        }

        return (sourceType, methodParams) switch
        {
            (null, null) => new(sourceName),
            (null, not null) => new(sourceName, methodParams),
            (not null, null) => new(sourceType, sourceName),
            _ => new(sourceType, sourceName, methodParams),
        };

        #region Local Methods
        /// <summary>
        /// Validates that the source type is appropriate for test data provision.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Validation Steps:</strong>
        /// <list type="number">
        ///   <item><description>Checks that <paramref name="sourceType"/> is a class or interface (not struct)</description></item>
        ///   <item><description>Verifies the source member exists and is public static</description></item>
        ///   <item><description>Ensures the source member returns <see cref="System.Collections.IEnumerable"/></description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="sourceType">The type to validate.</param>
        /// <param name="sourceName">The member name to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        static void validateSourceType(Type sourceType, string sourceName)
        {
            var sourceTypeFullName = sourceType.FullName;

            if (sourceType.IsClass || sourceType.IsInterface)
            {
                var memberInfo = getMemberInfo(sourceType, sourceName, sourceTypeFullName);
                var memberType = getMemberReturnType(memberInfo);

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(memberType))
                {
                    return;
                }

                throw new ArgumentException(
                    $"Source member '{sourceName}' must return an IEnumerable, " +
                    $"but returns '{memberType.FullName}'.",
                    nameof(sourceName));
            }

            var message = sourceType.IsValueType ?
                $"Source type cannot be a struct: {sourceTypeFullName}. " +
                $"Use a class or interface instead."
                : $"Source type must be a class or interface: {sourceTypeFullName}. " +
                    $"Actual type: {sourceType.Name}";

            throw new ArgumentException(message, nameof(sourceType));
        }

        /// <summary>
        /// Extracts the return type from a member (field, property, or method).
        /// </summary>
        /// <param name="memberInfo">The member to inspect.</param>
        /// <returns>The type returned by the member.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown when the member type is not Field, Property, or Method.
        /// </exception>
        static Type getMemberReturnType(MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                FieldInfo field => field.FieldType,
                PropertyInfo property => property.PropertyType,
                MethodInfo method => method.ReturnType,
                _ => throw new NotSupportedException(
                    $"Member type {memberInfo.MemberType} is not supported.")
            };
        }

        /// <summary>
        /// Retrieves member metadata using reflection.
        /// </summary>
        /// <param name="sourceType">The type containing the member.</param>
        /// <param name="sourceName">The member name to find.</param>
        /// <param name="sourceTypeFullName">The full name of the source type (for error messages).</param>
        /// <returns>The <see cref="MemberInfo"/> for the source member.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the member is not found or not accessible.
        /// </exception>
        static MemberInfo getMemberInfo(Type sourceType, string sourceName, string? sourceTypeFullName)
        {
            var member = sourceType.GetMember(sourceName,
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);

            if (member.Length == 0)
            {
                throw new ArgumentException(
                    $"Source member '{sourceName}' " +
                    $"not found or not accessible on type '{sourceTypeFullName}'. " +
                    "Ensure it exists and is public static.",
                    nameof(sourceName));
            }

            return member[0];
        }
        #endregion
    }

    /// <summary>
    /// Gets or sets the category for all tests generated from this attribute.
    /// </summary>
    /// <value>
    /// A category name to apply to all tests. Can be <c>null</c> if no category is assigned.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is delegated to the inner <see cref="TestCaseSourceAttribute"/>.
    /// Categories are used by NUnit to filter and group tests in the Test Explorer.
    /// </para>
    /// </remarks>
    public string? Category
    {
        get => _innerAttribute.Category;
        set => _innerAttribute.Category = value;
    }

    /// <summary>
    /// Gets the name of the data source member.
    /// </summary>
    /// <value>
    /// The name of the method or property providing test data.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is read-only and set during attribute construction.
    /// </para>
    /// </remarks>
    public string? SourceName
    => _innerAttribute.SourceName;

    /// <summary>
    /// Gets the type that declares the data source member.
    /// </summary>
    /// <value>
    /// The type containing the source member, or <c>null</c> if using the test class type.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is read-only and set during attribute construction.
    /// </para>
    /// </remarks>
    public Type? SourceType
    => _innerAttribute.SourceType;

    /// <summary>
    /// Gets the parameters to pass to the data source method.
    /// </summary>
    /// <value>
    /// An array of arguments for the source method, or <c>null</c> if no parameters are needed.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is read-only and set during attribute construction.
    /// Only applicable when the data source is a method (not property or field).
    /// </para>
    /// </remarks>
    public object?[]? MethodParams
    => _innerAttribute.MethodParams;

    /// <summary>
    /// Builds test methods from the data source, enhancing display names with test case names.
    /// </summary>
    /// <param name="method">The test method metadata.</param>
    /// <param name="suite">The test suite containing the test methods.</param>
    /// <returns>
    /// An enumerable collection of <see cref="TestMethod"/> instances with enhanced display names.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Display Name Enhancement Algorithm:</strong>
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     Delegate to <see cref="TestCaseSourceAttribute.BuildFrom"/> to retrieve test methods
    ///   </description></item>
    ///   <item><description>
    ///     For each test method, check the <see cref="HasFullNameProperty"/> in its properties
    ///   </description></item>
    ///   <item><description>
    ///     If property is <c>false</c>, enhance the display name to "MethodName - TestCaseName"
    ///     using <see cref="NamedCase.CreateDisplayName"/>
    ///   </description></item>
    ///   <item><description>
    ///     Yield return the test method (enhanced or unmodified)
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Example Transformation:</strong>
    /// </para>
    /// <code>
    /// // Input test method name:  "Add(2,3)"
    /// // HasFullNameProperty:     false
    /// // Method name:             "TestAdd"
    /// 
    /// // Output:                  "TestAdd - Add(2,3)"
    /// //                          ^^^^^^^^   ^^^^^^^^^
    /// //                          Method     TestCaseName
    /// 
    /// // vs. Standard TestCaseSource output:
    /// // "TestAdd(2, 3)"  ← Verbose, shows all parameters
    /// </code>
    /// </remarks>
    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
    {
        foreach (var testMethod in _innerAttribute.BuildFrom(method, suite))
        {
            if (shouldRename(testMethod))
            {
                testMethod.Name = NamedCase.CreateDisplayName(
                    method.Name,
                    testMethod.Name)!;
            }

            yield return testMethod;
        }

        #region Local Functions
        /// <summary>
        /// Determines whether a test method should have its display name enhanced.
        /// </summary>
        /// <param name="testMethod">The test method to check.</param>
        /// <returns>
        /// <c>true</c> if the test method's <see cref="HasFullNameProperty"/> is <c>false</c>;
        /// otherwise, <c>false</c>.
        /// </returns>
        static bool shouldRename(TestMethod testMethod)
        {
            var properties = testMethod.Properties;
            var hasFullNameProperty =
                properties.Get(HasFullNameProperty);

            return hasFullNameProperty is bool hasFullName &&
                !hasFullName;
        }
        #endregion
    }
}

/// <summary>
/// NUnit attribute for data-driven tests with enhanced display names showing test case names.
/// </summary>
/// <remarks>
/// <para>
/// This sealed attribute provides a drop-in replacement for <see cref="TestCaseSourceAttribute"/> with
/// automatic test case name detection for improved test result readability in NUnit Test Explorer.
/// </para>
/// <para>
/// <strong>Key Benefits:</strong>
/// <list type="bullet">
///   <item><description>
///     <strong>Cleaner Test Explorer:</strong> Shows <c>"TestMethod - TestCaseName"</c> instead of
///     <c>"TestMethod(param1, param2, ...)"</c>
///   </description></item>
///   <item><description>
///     <strong>API Compatibility:</strong> Drop-in replacement for <see cref="TestCaseSourceAttribute"/>
///     (all 4 constructor overloads supported)
///   </description></item>
///   <item><description>
///     <strong>Automatic Detection:</strong> No configuration needed if test data follows Portamical conventions
///   </description></item>
///   <item><description>
///     <strong>Graceful Fallback:</strong> Works like standard <see cref="TestCaseSourceAttribute"/> if no enhancement needed
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Requirements:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>
///     Test data must be created via <see cref="TestCaseTestData.From{TTestData}"/> or
///     <see cref="CollectionConverter"/> extension methods
///   </description></item>
///   <item><description>
///     Source member must return <see cref="System.Collections.IEnumerable"/> of <see cref="TestCaseData"/>
///   </description></item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage (Drop-in Replacement):</strong></para>
/// <code>
/// // Replace:
/// [TestCaseSource(nameof(GetTestData))]
/// 
/// // With:
/// [PortamicalData(nameof(GetTestData))]
/// //               ^^^^^^^^^^^^^^^^^^^^
/// //               Same parameters as TestCaseSource!
/// </code>
/// 
/// <para><strong>Complete Working Example:</strong></para>
/// <code>
/// using NUnit.Framework;
/// using Portamical.NUnit.Attributes;
/// using Portamical.NUnit.TestBases;
/// 
/// [TestFixture]
/// public class CalculatorTests : TestBase
/// {
///     private static readonly TestDataReturns&lt;int&gt;[] AddTestData =
///     [
///         new("Add(2,3)", [2, 3], 5),
///         new("Add(5,7)", [5, 7], 12),
///         new("Add(-1,1)", [-1, 1], 0)
///     ];
///     
///     public static IEnumerable&lt;TestCaseData&gt; GetAddTestData()
///         =&gt; Convert(AddTestData, ArgsCode.Properties, nameof(TestAdd));
///     
///     [TestMethod]
///     [PortamicalData(nameof(GetAddTestData))]
///     public void TestAdd(int x, int y)
///     {
///         // Visual Studio Test Explorer shows:
///         // ✓ TestAdd - Add(2,3)
///         // ✓ TestAdd - Add(5,7)
///         // ✓ TestAdd - Add(-1,1)
///         
///         int result = Calculator.Add(x, y);
///         // NUnit automatically compares result with ExpectedResult
///     }
/// }
/// </code>
/// 
/// <para><strong>All Constructor Overloads:</strong></para>
/// <code>
/// // 1. Simple (method or property in same class)
/// [PortamicalData("GetTestData")]
/// 
/// // 2. With method parameters
/// [PortamicalData("GetTestData", "arg1", "arg2")]
/// 
/// // 3. With source type
/// [PortamicalData(typeof(TestDataProvider), "GetTestData")]
/// 
/// // 4. With source type and method parameters
/// [PortamicalData(typeof(TestDataProvider), "GetTestData", "arg1", "arg2")]
/// </code>
/// </example>
/// <seealso cref="PortamicalBaseDataAttribute"/>
/// <seealso cref="TestCaseSourceAttribute"/>
/// <seealso cref="CollectionConverter"/>
/// <seealso cref="TestBase"/>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class PortamicalDataAttribute : PortamicalBaseDataAttribute
{
    /// <summary>
    /// Initializes a new instance with a data source name.
    /// </summary>
    /// <param name="sourceName">The name of the method or property providing test data.</param>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// [PortamicalData("GetTestData")]  // Property or method in same class
    /// public void TestMethod(int x, int y) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName)
    : base(sourceName)
    {
    }

    /// <summary>
    /// Initializes a new instance with a data source name and method parameters.
    /// </summary>
    /// <param name="sourceName">The name of the method providing test data.</param>
    /// <param name="methodParams">Arguments to pass to the data source method.</param>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// [PortamicalData("GetTestData", "arg1", "arg2")]
    /// public void TestMethod(int x, int y) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(string sourceName, object?[] methodParams)
    : base(sourceName, methodParams: methodParams)
    {
    }

    /// <summary>
    /// Initializes a new instance with a data source type and name.
    /// </summary>
    /// <param name="sourceType">The type that declares the data source member.</param>
    /// <param name="sourceName">The name of the method or property providing test data.</param>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// [PortamicalData(typeof(SharedTestData), "GetTestData")]
    /// public void TestMethod(int x, int y) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(Type sourceType, string sourceName)
    : base(sourceName, sourceType: sourceType)
    {
    }

    /// <summary>
    /// Initializes a new instance with a data source type, name, and method parameters.
    /// </summary>
    /// <param name="sourceType">The type that declares the data source method.</param>
    /// <param name="sourceName">The name of the method providing test data.</param>
    /// <param name="methodParams">Arguments to pass to the data source method.</param>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// [PortamicalData(typeof(SharedTestData), "GetTestData", "arg1", "arg2")]
    /// public void TestMethod(int x, int y) { }
    /// </code>
    /// </example>
    public PortamicalDataAttribute(Type sourceType, string sourceName, object?[] methodParams)
    : base(sourceName, sourceType: sourceType, methodParams: methodParams)
    {
    }
}