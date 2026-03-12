// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity;
using Portamical.Core.Safety;
using System.Diagnostics.CodeAnalysis;
using static Portamical.Core.Identity.Model.NamedCase;

namespace Portamical.NUnit.TestDataTypes;

/// <summary>
/// Represents a base class for NUnit test case data that bridges Portamical test data
/// with NUnit's <see cref="TestCaseData"/> infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Design Pattern: Adapter + Template Method</strong>
/// </para>
/// <para>
/// This abstract class adapts Portamical's <see cref="ITestData"/> abstraction to NUnit's
/// <see cref="TestCaseData"/>, enabling identity-driven test data modeling with NUnit's
/// data-driven testing features (<c>[TestCaseSource]</c>).
/// </para>
/// <para>
/// <strong>Architecture:</strong>
/// <code>
/// NUnit.Framework.TestCaseData (NUnit primitive)
///   ↓ inherits
/// TestCaseTestData (abstract adapter) ← This class
///   ├── Implements: INamedCase (Portamical.Core)
///   ├── Provides: Static factory methods, type extraction helpers
///   └── Delegates: Equality, hashing, display names to NamedCase
///         ↓ inherits
/// TestCaseTestData&lt;TTestData&gt; (sealed, generic)
///   └── Configures: Arguments, ExpectedResult, TypeArgs, TestName, Properties
/// </code>
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item><description>Automatic test case naming with optional method prefix</description></item>
///   <item><description>Support for expected results (<see cref="IReturns"/>)</description></item>
///   <item><description>Generic type argument extraction for generic test methods</description></item>
///   <item><description>Test case description from <see cref="ITestData.GetDefinition()"/></description></item>
///   <item><description>Equality based on <see cref="TestCaseName"/> via <see cref="INamedCase"/></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage with Factory Method:</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// var testCase = TestCaseTestData.From(testData, ArgsCode.InOut, "TestAdd");
/// 
/// // Configured properties:
/// // testCase.Arguments = [2, 3]
/// // testCase.ExpectedResult = 5
/// // testCase.TestName = "TestAdd - Add(2,3)"
/// // testCase.Properties["Description"] = "When adding two positive numbers"
/// // testCase.TestCaseName = "Add(2,3)"
/// </code>
/// 
/// <para><strong>Integration with NUnit Test:</strong></para>
/// <code>
/// public static IEnumerable&lt;TestCaseData&gt; AddTestCases()
/// {
///     var testData = new[]
///     {
///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///         new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
///     };
///     
///     return testData.Select(td => 
///         TestCaseTestData.From(td, ArgsCode.InOut, nameof(TestAdd)));
/// }
/// 
/// [TestCaseSource(nameof(AddTestCases))]
/// public void TestAdd(int x, int y, int expected)
/// {
///     Assert.That(Calculator.Add(x, y), Is.EqualTo(expected));
/// }
/// 
/// // NUnit Test Explorer displays:
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(0,0)
/// </code>
/// </example>
/// <seealso cref="TestCaseTestData{TTestData}"/>
/// <seealso cref="INamedCase"/>
/// <seealso cref="ITestData"/>
[SuppressMessage("SonarLint", "S4035:Classes implementing 'IEqualityComparer<T>' should be sealed",
    Justification = "This abstract base class implements IEquatable<T>, not IEqualityComparer<T>. " +
                    "The nested NamedCaseEqualityComparer that implements IEqualityComparer<T> is properly sealed.")]
public abstract class TestCaseTestData
: TestCaseData, INamedCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCaseTestData"/> class with the specified arguments.
    /// </summary>
    /// <param name="args">
    /// The arguments to pass to the test method. These are extracted from <see cref="ITestData"/>
    /// using <see cref="TestCaseDataArgsFrom"/> and exclude the expected result (which is set via
    /// <see cref="TestCaseData.ExpectedResult"/> for <see cref="IReturns"/> test data).
    /// </param>
    /// <remarks>
    /// <para>
    /// This constructor is <c>private protected</c> to prevent external inheritance while allowing
    /// derived classes within this assembly to call it. This design ensures that only
    /// <see cref="TestCaseTestData{TTestData}"/> can extend this base class.
    /// </para>
    /// </remarks>
    private protected TestCaseTestData(object?[] args)
    : base(args)
    {
    }

    /// <summary>
    /// Gets or initializes the test case name that uniquely identifies this test case.
    /// </summary>
    /// <value>
    /// A descriptive name for the test case (e.g., "Add(2,3)", "Divide by zero throws exception").
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is the core identifier from <see cref="INamedCase"/> and is used for:
    /// <list type="bullet">
    ///   <item><description>Equality comparisons (two test cases with same name are equal)</description></item>
    ///   <item><description>Display name generation (combined with test method name)</description></item>
    ///   <item><description>Test Explorer display in NUnit</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Derived classes must override this property and initialize it during construction.
    /// The <c>init</c> accessor ensures immutability after instantiation.
    /// </para>
    /// </remarks>
    public abstract string TestCaseName { get; init; }

    /// <summary>
    /// The property key used to store whether the test case has a full display name
    /// (test method name + test case name) in <see cref="TestCaseData.Properties"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This internal constant is used by <see cref="SetHasFullNameProperty{TTestCaseData, TTestData}"/>
    /// to mark test cases that include the test method name in their display name.
    /// </para>
    /// <para>
    /// <strong>Values:</strong>
    /// <list type="table">
    ///   <listheader><term>Value</term><description>Meaning</description></listheader>
    ///   <item><term>true</term><description>Display name: "TestMethodName - TestCaseName"</description></item>
    ///   <item><term>false</term><description>Display name: "TestCaseName"</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    internal const string HasFullNameProperty = "HasFullName";

    /// <summary>
    /// Determines whether this test case is contained in the specified collection of named cases.
    /// </summary>
    /// <param name="namedCases">
    /// The collection to check for containment. Can be <c>null</c>, in which case the method returns <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if this test case is found in the collection (based on <see cref="TestCaseName"/> equality);
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="NamedCase.Contains(INamedCase, IEnumerable{INamedCase}?)"/>,
    /// which uses <see cref="INamedCase.TestCaseName"/> for equality comparison.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase = TestCaseTestData.From(testData, ArgsCode.InOut);
    /// var collection = new[] { testCase, anotherTestCase };
    /// 
    /// bool exists = testCase.ContainedBy(collection);  // true
    /// </code>
    /// </example>
    /// <seealso cref="NamedCase.Contains(INamedCase, IEnumerable{INamedCase}?)"/>
    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    /// <summary>
    /// Determines whether the specified object is equal to the current test case.
    /// </summary>
    /// <param name="obj">The object to compare with the current test case.</param>
    /// <returns>
    /// <c>true</c> if the specified object is an <see cref="INamedCase"/> with the same
    /// <see cref="TestCaseName"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is <c>sealed</c> to prevent derived classes from overriding it, ensuring that
    /// equality is always based on <see cref="INamedCase"/> identity. This guarantees consistency
    /// with <see cref="GetHashCode"/>.
    /// </para>
    /// <para>
    /// Equality is determined by <see cref="NamedCase.Comparer"/>, which compares
    /// <see cref="TestCaseName"/> values using ordinal string comparison.
    /// </para>
    /// </remarks>
    /// <seealso cref="Equals(INamedCase?)"/>
    /// <seealso cref="GetHashCode"/>
    public override sealed bool Equals(object? obj)
    => Equals(obj as INamedCase);

    /// <summary>
    /// Determines whether the specified <see cref="INamedCase"/> is equal to the current test case.
    /// </summary>
    /// <param name="other">The named case to compare with the current test case.</param>
    /// <returns>
    /// <c>true</c> if the specified named case has the same <see cref="TestCaseName"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="NamedCase.Comparer"/>, which performs ordinal
    /// string comparison on <see cref="TestCaseName"/> values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase1 = TestCaseTestData.From(testData1, ArgsCode.InOut);  // TestCaseName: "Add(2,3)"
    /// var testCase2 = TestCaseTestData.From(testData2, ArgsCode.InOut);  // TestCaseName: "Add(2,3)"
    /// var testCase3 = TestCaseTestData.From(testData3, ArgsCode.InOut);  // TestCaseName: "Add(5,7)"
    /// 
    /// testCase1.Equals(testCase2);  // true (same TestCaseName)
    /// testCase1.Equals(testCase3);  // false (different TestCaseName)
    /// </code>
    /// </example>
    /// <seealso cref="NamedCase.Comparer"/>
    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    /// <summary>
    /// Gets the display name for this test case, optionally prefixed with the test method name.
    /// </summary>
    /// <param name="testMethodName">
    /// The name of the test method. If provided and non-empty, the display name will be
    /// formatted as "testMethodName - TestCaseName". If <c>null</c> or empty, returns only
    /// <see cref="TestCaseName"/>.
    /// </param>
    /// <returns>
    /// A formatted display name: "testMethodName - TestCaseName" if <paramref name="testMethodName"/>
    /// is provided; otherwise, just <see cref="TestCaseName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="NamedCase.CreateDisplayName(string?, string)"/>,
    /// which handles the formatting logic.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase = TestCaseTestData.From(testData, ArgsCode.InOut);
    /// testCase.TestCaseName = "Add(2,3)";
    /// 
    /// testCase.GetDisplayName("TestAdd");  // "TestAdd - Add(2,3)"
    /// testCase.GetDisplayName(null);       // "Add(2,3)"
    /// </code>
    /// </example>
    /// <seealso cref="NamedCase.CreateDisplayName(string?, string)"/>
    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    /// <summary>
    /// Returns the hash code for this test case based on its <see cref="TestCaseName"/>.
    /// </summary>
    /// <returns>
    /// A hash code value for the current test case, computed from <see cref="TestCaseName"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is <c>sealed</c> to prevent derived classes from overriding it, ensuring that
    /// the hash code is always consistent with <see cref="Equals(object?)"/>.
    /// </para>
    /// <para>
    /// Hash code computation is delegated to <see cref="NamedCase.Comparer"/>, which uses
    /// ordinal string hashing on <see cref="TestCaseName"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Equals(object?)"/>
    /// <seealso cref="NamedCase.Comparer"/>
    public override sealed int GetHashCode()
    => Comparer.GetHashCode(this);

    /// <summary>
    /// Configures a test case with display name properties and returns it for fluent chaining.
    /// </summary>
    /// <typeparam name="TTestCaseData">
    /// The type of the test case data, which must inherit from <see cref="TestCaseData"/>.
    /// This generic parameter preserves the specific derived type in the return value.
    /// </typeparam>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testCaseData">The test case to configure.</param>
    /// <param name="testData">The test data containing the test case name and definition.</param>
    /// <param name="testMethodName">
    /// Optional test method name. If provided and non-empty, generates a full display name
    /// ("MethodName - TestCaseName") and sets <see cref="HasFullNameProperty"/> to <c>true</c>.
    /// If <c>null</c> or empty, uses only the test case name and sets the property to <c>false</c>.
    /// </param>
    /// <param name="testName">
    /// Outputs the computed test name based on whether <paramref name="testMethodName"/> is provided:
    /// <list type="bullet">
    ///   <item><description>With method name: "MethodName - TestCaseName"</description></item>
    ///   <item><description>Without method name: "TestCaseName"</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// The configured <paramref name="testCaseData"/> instance, enabling fluent API usage.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs two operations:
    /// <list type="number">
    ///   <item><description>
    ///     Sets the <see cref="HasFullNameProperty"/> property on <paramref name="testCaseData"/>
    ///     to indicate whether a full display name is used.
    ///   </description></item>
    ///   <item><description>
    ///     Computes the display name via <see cref="ITestData.GetDisplayName(string?)"/> and
    ///     outputs it through the <paramref name="testName"/> parameter.
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The method uses two generic type parameters to preserve type information:
    /// <list type="bullet">
    ///   <item><description>
    ///     <typeparamref name="TTestCaseData"/>: Allows returning the exact type passed in
    ///   </description></item>
    ///   <item><description>
    ///     <typeparamref name="TTestData"/>: Ensures compile-time type safety for test data
    ///   </description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>With Test Method Name (Full Display Name):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testCase = new TestCaseData(2, 3);
    /// 
    /// var configured = SetHasFullNameProperty(testCase, testData, "TestAdd", out var name);
    /// 
    /// // Results:
    /// // configured.Properties["HasFullName"] = true
    /// // name = "TestAdd - Add(2,3)"
    /// // configured == testCase (same instance)
    /// </code>
    /// 
    /// <para><strong>Without Test Method Name (Simple Display Name):</strong></para>
    /// <code>
    /// var configured = SetHasFullNameProperty(testCase, testData, null, out var name);
    /// 
    /// // Results:
    /// // configured.Properties["HasFullName"] = false
    /// // name = "Add(2,3)"
    /// </code>
    /// </example>
    /// <seealso cref="HasFullNameProperty"/>
    /// <seealso cref="ITestData.GetDisplayName(string?)"/>
    public static TTestCaseData SetHasFullNameProperty<TTestCaseData, TTestData>(
        TTestCaseData testCaseData,
        TTestData testData,
        string? testMethodName,
        out string testName)
    where TTestCaseData : notnull, TestCaseData
    where TTestData : notnull, ITestData
    {
        bool hasFullName = !string.IsNullOrEmpty(testMethodName);

        testCaseData.Properties.Set(
            HasFullNameProperty,
            hasFullName);

        testName = hasFullName ?
            testData.GetDisplayName(testMethodName)!
            : testData.TestCaseName;

        return testCaseData;
    }

    /// <summary>
    /// Creates a new <see cref="TestCaseTestData{TTestData}"/> instance from Portamical test data.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// This generic parameter is preserved in the returned <see cref="TestCaseTestData{TTestData}"/>.
    /// </typeparam>
    /// <param name="testData">
    /// The Portamical test data containing test case name, arguments, expected result (if applicable),
    /// and test definition.
    /// </param>
    /// <param name="argsCode">
    /// Specifies which components of the test data to include as test method arguments:
    /// <list type="bullet">
    ///   <item><term><see cref="ArgsCode.In"/></term><description>Only input arguments</description></item>
    ///   <item><term><see cref="ArgsCode.Out"/></term><description>Only expected output (if <see cref="IReturns"/>)</description></item>
    ///   <item><term><see cref="ArgsCode.InOut"/></term><description>Input arguments + expected output</description></item>
    ///   <item><term><see cref="ArgsCode.Properties"/></term><description>Flattened properties (for generic tests)</description></item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// If provided, the test name will be formatted as "testMethodName - TestCaseName".
    /// If <c>null</c>, only the test case name is used.
    /// </param>
    /// <returns>
    /// A fully configured <see cref="TestCaseTestData{TTestData}"/> instance with:
    /// <list type="bullet">
    ///   <item><description>Arguments extracted via <paramref name="argsCode"/></description></item>
    ///   <item><description>Expected result set (if <see cref="IReturns"/>)</description></item>
    ///   <item><description>Type arguments set (if <see cref="ArgsCode.Properties"/>)</description></item>
    ///   <item><description>Test name and description configured</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the primary factory method for creating NUnit test cases from Portamical test data.
    /// It encapsulates all the complexity of configuring <see cref="TestCaseData"/> properties.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Basic Usage (Simple Test):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testCase = TestCaseTestData.From(testData, ArgsCode.InOut, "TestAdd");
    /// 
    /// // Configured properties:
    /// // testCase.Arguments = [2, 3]
    /// // testCase.ExpectedResult = 5
    /// // testCase.TestName = "TestAdd - Add(2,3)"
    /// // testCase.TypeArgs = null (non-generic test)
    /// </code>
    /// 
    /// <para><strong>Generic Test with Type Arguments:</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testCase = TestCaseTestData.From(testData, ArgsCode.Properties, "TestGenericAdd");
    /// 
    /// // Configured properties:
    /// // testCase.Arguments = [2, 3]
    /// // testCase.ExpectedResult = 5
    /// // testCase.TypeArgs = [typeof(int)]  ← For generic test method
    /// // testCase.TestName = "TestGenericAdd - Add(2,3)"
    /// </code>
    /// 
    /// <para><strong>In Test Case Source Method:</strong></para>
    /// <code>
    /// public static IEnumerable&lt;TestCaseData&gt; GetTestCases()
    /// {
    ///     var testData = new[]
    ///     {
    ///         new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
    ///         new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0)
    ///     };
    ///     
    ///     return testData.Select(td => 
    ///         TestCaseTestData.From(td, ArgsCode.InOut, nameof(TestAdd)));
    /// }
    /// 
    /// [TestCaseSource(nameof(GetTestCases))]
    /// public void TestAdd(int x, int y, int expected)
    /// {
    ///     Assert.That(Calculator.Add(x, y), Is.EqualTo(expected));
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TestCaseTestData{TTestData}"/>
    /// <seealso cref="ArgsCode"/>
    public static TestCaseTestData<TTestData> From<TTestData>(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => new(testData, argsCode, testMethodName);

    /// <summary>
    /// Extracts the type arguments for a generic test method based on test data and argument strategy.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of the test data, which must implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testData">The test data instance (used to check if it implements <see cref="IReturns"/>).</param>
    /// <param name="argsCode">
    /// The argument strategy. Type arguments are only extracted when <paramref name="argsCode"/> is
    /// <see cref="ArgsCode.Properties"/> (indicating a generic test method with flattened parameters).
    /// </param>
    /// <returns>
    /// An array of <see cref="Type"/> objects representing the generic type arguments for the test method,
    /// or <c>null</c> if <paramref name="argsCode"/> is not <see cref="ArgsCode.Properties"/>.
    /// <para>
    /// For <see cref="IReturns"/> test data, the first generic argument (TExpected) is excluded because
    /// it represents the expected result (set via <see cref="TestCaseData.ExpectedResult"/>), not a test parameter.
    /// </para>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// <list type="number">
    ///   <item><description>
    ///     If <paramref name="argsCode"/> is not <see cref="ArgsCode.Properties"/>, return <c>null</c>
    ///     (non-generic test or instance-based test).
    ///   </description></item>
    ///   <item><description>
    ///     Extract generic type arguments from <typeparamref name="TTestData"/> via reflection.
    ///   </description></item>
    ///   <item><description>
    ///     If <paramref name="testData"/> implements <see cref="IReturns"/>, skip the first type argument
    ///     (TExpected) using range indexer <c>[1..]</c>.
    ///   </description></item>
    ///   <item><description>
    ///     Return the remaining type arguments (test method parameters).
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Why Exclude TExpected for IReturns?</strong>
    /// </para>
    /// <para>
    /// For <c>TestDataReturns&lt;TExpected, TArg1, TArg2&gt;</c>:
    /// <list type="bullet">
    ///   <item><description>Generic arguments: [TExpected, TArg1, TArg2]</description></item>
    ///   <item><description>Test method signature: <c>void Test&lt;TArg1, TArg2&gt;(TArg1 arg1, TArg2 arg2)</c></description></item>
    ///   <item><description>TExpected is not a parameter (it's <see cref="TestCaseData.ExpectedResult"/>)</description></item>
    ///   <item><description>TypeArgs should be: [TArg1, TArg2] (exclude TExpected)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Non-Generic Test (Returns null):</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var typeArgs = TestCaseTestData.GetTypeArgs(testData, ArgsCode.InOut);
    /// 
    /// // Result: null (non-generic test method)
    /// </code>
    /// 
    /// <para><strong>Generic Test with IReturns (Excludes TExpected):</strong></para>
    /// <code>
    /// // TestDataReturns&lt;int, string, bool&gt;
    /// //                 ^^^  ^^^^^^  ^^^^
    /// //                 |    |       |
    /// //                 Expected  Arg1  Arg2
    /// 
    /// var testData = new TestDataReturns&lt;int, string, bool&gt;(
    ///     "Parse('42')",
    ///     ["42"],
    ///     42);
    /// 
    /// var typeArgs = TestCaseTestData.GetTypeArgs(testData, ArgsCode.Properties);
    /// 
    /// // Result: [typeof(string), typeof(bool)]
    /// // Test method: void Test&lt;TArg1, TArg2&gt;(TArg1 arg1, TArg2 arg2)
    /// //                        ^^^^^^^^^^^^^^
    /// //                        NUnit uses typeArgs to instantiate generics
    /// </code>
    /// 
    /// <para><strong>Generic Test without IReturns (Includes All Types):</strong></para>
    /// <code>
    /// // TestData&lt;string, int&gt;
    /// 
    /// var testData = new TestData&lt;string, int&gt;("Print('hello', 5)", ["hello", 5]);
    /// var typeArgs = TestCaseTestData.GetTypeArgs(testData, ArgsCode.Properties);
    /// 
    /// // Result: [typeof(string), typeof(int)]
    /// // Test method: void Test&lt;T1, T2&gt;(T1 arg1, T2 arg2)
    /// </code>
    /// </example>
    /// <seealso cref="ArgsCode.Properties"/>
    /// <seealso cref="IReturns"/>
    /// <seealso cref="TestCaseData.TypeArgs"/>
    public static Type[]? GetTypeArgs<TTestData>(
        TTestData testData,
        ArgsCode argsCode)
    where TTestData : notnull, ITestData
    {
        if (argsCode.Defined(nameof(argsCode)) != ArgsCode.Properties)
        {
            return null;
        }

        var typeArgs = typeof(TTestData).GetGenericArguments();
        var typeArgsLength = typeArgs.Length;

        if (typeArgsLength == 0)
        {
            return null;
        }

        if (testData is IReturns)
        {
            // Validation: Ensure we have at least 2 type args (TExpected + TArg1)
            if (typeArgs.Length < 2)
            {
                throw new InvalidOperationException(
                    $"{typeof(TTestData).Name} must have at least 2 generic arguments " +
                    $"when implementing IReturns (TExpected + argument types). " +
                    $"Found: {typeArgsLength}");
            }

            return typeArgs[1..];
        }

        return typeArgs;
    }

    /// <summary>
    /// Converts Portamical test data to an argument array suitable for <see cref="TestCaseData"/>.
    /// </summary>
    /// <param name="testData">The test data containing arguments and expected result.</param>
    /// <param name="argsCode">
    /// Specifies which components to include in the argument array:
    /// <list type="bullet">
    ///   <item><term><see cref="ArgsCode.Instance"/></term><description>TestData instance</description></item>
    ///   <item><term><see cref="ArgsCode.Properties"/></term><description>Flattened properties</description></item>
    /// </list>
    /// </param>
    /// <returns>
    /// An array of arguments for the test method, with expected result excluded (if present).
    /// The expected result is handled separately via <see cref="TestCaseData.ExpectedResult"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses <see cref="PropsCode.TrimReturnsExpected"/> to ensure that:
    /// <list type="bullet">
    ///   <item><description>Test case name is excluded (not needed in args array)</description></item>
    ///   <item><description>Expected result is excluded (set via <see cref="TestCaseData.ExpectedResult"/>)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Why TrimReturnsExpected?</strong>
    /// </para>
    /// <para>
    /// NUnit's <see cref="TestCaseData"/> has this structure:
    /// <code>
    /// new TestCaseData(arg1, arg2, ...)      // ← Arguments array (this method's output)
    /// {
    ///     ExpectedResult = expectedValue     // ← Separate property (not in args)
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Using <see cref="PropsCode.TrimReturnsExpected"/> ensures the expected result is not duplicated
    /// in the arguments array.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Test with Input and Expected Result:</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var args = TestCaseTestData.TestCaseDataArgsFrom(testData, ArgsCode.InOut);
    /// 
    /// // Result: [2, 3]
    /// // Note: Expected result (5) is excluded from args
    /// //       It will be set via: testCase.ExpectedResult = 5
    /// </code>
    /// 
    /// <para><strong>Test with Only Input:</strong></para>
    /// <code>
    /// var testData = new TestData&lt;int&gt;("Print(42)", [42]);
    /// var args = TestCaseTestData.TestCaseDataArgsFrom(testData, ArgsCode.In);
    /// 
    /// // Result: [42]
    /// </code>
    /// 
    /// <para><strong>Comparison: With vs Without TrimReturnsExpected:</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// 
    /// // ✅ CORRECT (with TrimReturnsExpected):
    /// testData.ToArgs(ArgsCode.InOut, PropsCode.TrimReturnsExpected)
    /// // Result: [2, 3]
    /// 
    /// // ❌ WRONG (without TrimReturnsExpected):
    /// testData.ToArgs(ArgsCode.InOut, PropsCode.All)
    /// // Result: ["Add(2,3)", 2, 3, 5]  ← Includes TestCaseName + Expected (breaks NUnit)
    /// </code>
    /// </example>
    /// <seealso cref="ITestData.ToArgs(ArgsCode, PropsCode)"/>
    /// <seealso cref="PropsCode.TrimReturnsExpected"/>
    /// <seealso cref="TestCaseData.ExpectedResult"/>
    public static object?[] TestCaseDataArgsFrom(
        ITestData testData,
        ArgsCode argsCode)
    => testData.ToArgs(argsCode, PropsCode.TrimReturnsExpected);
}

/// <summary>
/// Represents a sealed, generic implementation of <see cref="TestCaseTestData"/> that fully configures
/// an NUnit test case from Portamical test data.
/// </summary>
/// <typeparam name="TTestData">
/// The type of the test data, which must implement <see cref="ITestData"/>. This generic parameter
/// is preserved to maintain type information and enable type-specific operations (e.g., extracting
/// generic type arguments).
/// </typeparam>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong>
/// </para>
/// <para>
/// This class completes the adapter pattern by implementing the abstract <see cref="TestCaseName"/>
/// property and configuring all NUnit-specific properties during construction:
/// <list type="bullet">
///   <item><description><see cref="TestCaseData.Arguments"/> - Test method parameters</description></item>
///   <item><description><see cref="TestCaseData.ExpectedResult"/> - Expected return value (if <see cref="IReturns"/>)</description></item>
///   <item><description><see cref="TestCaseData.TestName"/> - Display name in Test Explorer</description></item>
///   <item><description><see cref="TestCaseData.TypeArgs"/> - Generic type arguments (if <see cref="ArgsCode.Properties"/>)</description></item>
///   <item><description><see cref="TestCaseData.Properties"/>["Description"] - Test case description</description></item>
///   <item><description><see cref="TestCaseData.Properties"/>["HasFullName"] - Display name format indicator</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Why Sealed?</strong>
/// </para>
/// <para>
/// This class is sealed to indicate that the inheritance hierarchy is complete. Further derivation
/// is unnecessary because this class provides full <see cref="TestCaseData"/> configuration.
/// </para>
/// <para>
/// <strong>Why Generic?</strong>
/// </para>
/// <para>
/// The generic parameter <typeparamref name="TTestData"/> enables:
/// <list type="bullet">
///   <item><description>Reflection on the exact test data type for <see cref="TestCaseData.TypeArgs"/> extraction</description></item>
///   <item><description>Preservation of type information in the factory method return value</description></item>
///   <item><description>Type-safe operations on test data properties</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Direct Construction (Rare - Use Factory Method Instead):</strong></para>
/// <code>
/// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
/// var testCase = new TestCaseTestData&lt;TestDataReturns&lt;int&gt;&gt;(
///     testData,
///     ArgsCode.InOut,
///     "TestAdd");
/// 
/// // Configured properties:
/// // testCase.Arguments = [2, 3]
/// // testCase.ExpectedResult = 5
/// // testCase.TestName = "TestAdd - Add(2,3)"
/// // testCase.TestCaseName = "Add(2,3)"
/// // testCase.TypeArgs = null
/// // testCase.Properties["Description"] = "When adding two positive numbers"
/// // testCase.Properties["HasFullName"] = true
/// </code>
/// 
/// <para><strong>Recommended: Use Factory Method:</strong></para>
/// <code>
/// var testCase = TestCaseTestData.From(testData, ArgsCode.InOut, "TestAdd");
/// // Same result as direct construction, but cleaner syntax
/// </code>
/// 
/// <para><strong>Integration with NUnit:</strong></para>
/// <code>
/// [TestFixture]
/// public class CalculatorTests
/// {
///     private static IEnumerable&lt;TestCaseData&gt; AddTestCases()
///     {
///         var testData = new[]
///         {
///             new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5),
///             new TestDataReturns&lt;int&gt;("Add(0,0)", [0, 0], 0),
///             new TestDataReturns&lt;int&gt;("Add(-1,1)", [-1, 1], 0)
///         };
///         
///         return testData.Select(td => 
///             TestCaseTestData.From(td, ArgsCode.InOut, nameof(TestAdd)));
///     }
///     
///     [TestCaseSource(nameof(AddTestCases))]
///     public void TestAdd(int x, int y, int expected)
///     {
///         Assert.That(Calculator.Add(x, y), Is.EqualTo(expected));
///     }
/// }
/// 
/// // NUnit Test Explorer Output:
/// // ✓ TestAdd - Add(2,3)
/// // ✓ TestAdd - Add(0,0)
/// // ✓ TestAdd - Add(-1,1)
/// </code>
/// </example>
/// <seealso cref="TestCaseTestData"/>
/// <seealso cref="TestCaseTestData.From{TTestData}(TTestData, ArgsCode, string?)"/>
/// <seealso cref="ITestData"/>
public sealed class TestCaseTestData<TTestData>
: TestCaseTestData
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCaseTestData{TTestData}"/> class by
    /// configuring all NUnit test case properties from Portamical test data.
    /// </summary>
    /// <param name="testData">
    /// The Portamical test data containing:
    /// <list type="bullet">
    ///   <item><description>Test case name (<see cref="ITestData.TestCaseName"/>)</description></item>
    ///   <item><description>Input arguments (<see cref="ITestData.Args"/>)</description></item>
    ///   <item><description>Expected result (if <see cref="IReturns"/>)</description></item>
    ///   <item><description>Test definition/description (<see cref="ITestData.GetDefinition()"/>)</description></item>
    /// </list>
    /// </param>
    /// <param name="argsCode">
    /// Specifies which components to include in the test method arguments:
    /// <list type="bullet">
    ///   <item><term><see cref="ArgsCode.In"/></term><description>Only input arguments</description></item>
    ///   <item><term><see cref="ArgsCode.Out"/></term><description>Only expected output</description></item>
    ///   <item><term><see cref="ArgsCode.InOut"/></term><description>Both input and expected output</description></item>
    ///   <item><term><see cref="ArgsCode.Properties"/></term>
    ///     <description>Flattened properties (sets <see cref="TestCaseData.TypeArgs"/> for generic tests)</description>
    ///   </item>
    /// </list>
    /// </param>
    /// <param name="testMethodName">
    /// Optional test method name to prepend to the test case name in the display name.
    /// <list type="bullet">
    ///   <item><description>If provided: <see cref="TestCaseData.TestName"/> = "testMethodName - TestCaseName"</description></item>
    ///   <item><description>If <c>null</c>: <see cref="TestCaseData.TestName"/> = "TestCaseName"</description></item>
    /// </list>
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Constructor Flow:</strong>
    /// <list type="number">
    ///   <item><description>
    ///     Call base constructor with arguments from <see cref="TestCaseTestData.TestCaseDataArgsFrom"/>
    ///   </description></item>
    ///   <item><description>
    ///     Set <see cref="TestCaseData.Properties"/>["Description"] to <see cref="ITestData.GetDefinition()"/>
    ///   </description></item>
    ///   <item><description>
    ///     Configure display name and set <see cref="HasFullNameProperty"/> via
    ///     <see cref="TestCaseTestData.SetHasFullNameProperty{TTestCaseData, TTestData}"/>
    ///   </description></item>
    ///   <item><description>
    ///     If <paramref name="testData"/> implements <see cref="IReturns"/>, set
    ///     <see cref="TestCaseData.ExpectedResult"/> to the expected value
    ///   </description></item>
    ///   <item><description>
    ///     Set <see cref="TestCaseData.TestName"/> to the computed display name
    ///   </description></item>
    ///   <item><description>
    ///     Set <see cref="TestCaseData.TypeArgs"/> via <see cref="TestCaseTestData.GetTypeArgs{TTestData}"/>
    ///     (only if <paramref name="argsCode"/> is <see cref="ArgsCode.Properties"/>)
    ///   </description></item>
    ///   <item><description>
    ///     Set <see cref="TestCaseName"/> to <see cref="ITestData.TestCaseName"/> (implements abstract property)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Why Internal Constructor?</strong>
    /// </para>
    /// <para>
    /// The constructor is <c>internal</c> to encourage usage of the static factory method
    /// <see cref="TestCaseTestData.From{TTestData}"/>, which provides cleaner syntax and better
    /// discoverability. Direct construction is allowed within the assembly for flexibility.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Test with Expected Result:</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;("Add(2,3)", [2, 3], 5);
    /// var testCase = new TestCaseTestData&lt;TestDataReturns&lt;int&gt;&gt;(
    ///     testData,
    ///     ArgsCode.InOut,
    ///     "TestAdd");
    /// 
    /// // Configured properties:
    /// // testCase.Arguments = [2, 3]
    /// // testCase.ExpectedResult = 5
    /// // testCase.TestName = "TestAdd - Add(2,3)"
    /// // testCase.TypeArgs = null (non-generic test)
    /// </code>
    /// 
    /// <para><strong>Generic Test with Type Arguments:</strong></para>
    /// <code>
    /// var testData = new TestDataReturns&lt;int, string, bool&gt;(
    ///     "Parse('42')",
    ///     ["42"],
    ///     42);
    /// 
    /// var testCase = new TestCaseTestData&lt;TestDataReturns&lt;int, string, bool&gt;&gt;(
    ///     testData,
    ///     ArgsCode.Properties,  // ← Triggers TypeArgs extraction
    ///     "TestGenericParse");
    /// 
    /// // Configured properties:
    /// // testCase.Arguments = ["42"]
    /// // testCase.ExpectedResult = 42
    /// // testCase.TestName = "TestGenericParse - Parse('42')"
    /// // testCase.TypeArgs = [typeof(string), typeof(bool)]  ← For generic method
    /// </code>
    /// 
    /// <para><strong>Test without Expected Result:</strong></para>
    /// <code>
    /// var testData = new TestData&lt;string&gt;("Print('hello')", ["hello"]);
    /// var testCase = new TestCaseTestData&lt;TestData&lt;string&gt;&gt;(
    ///     testData,
    ///     ArgsCode.In,
    ///     null);  // ← No method name prefix
    /// 
    /// // Configured properties:
    /// // testCase.Arguments = ["hello"]
    /// // testCase.ExpectedResult = null (no IReturns)
    /// // testCase.TestName = "Print('hello')"
    /// // testCase.TypeArgs = null
    /// </code>
    /// </example>
    /// <seealso cref="TestCaseTestData.From{TTestData}(TTestData, ArgsCode, string?)"/>
    internal TestCaseTestData(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    : base(TestCaseDataArgsFrom(testData, argsCode))
    {
        Properties.Set(
            PropertyNames.Description,
            testData.GetDefinition());

        SetHasFullNameProperty(
            this,
            testData,
            testMethodName,
            out string testName);

        if (testData is IReturns returns)
        {
            ExpectedResult = returns.GetExpected();
        }

        TestName = testName;
        TypeArgs = GetTypeArgs(testData, argsCode);
        TestCaseName = testData.TestCaseName;
    }

    /// <summary>
    /// Gets or initializes the test case name that uniquely identifies this test case.
    /// </summary>
    /// <value>
    /// A descriptive name for the test case, derived from <see cref="ITestData.TestCaseName"/>
    /// (e.g., "Add(2,3)", "Divide by zero throws exception").
    /// </value>
    /// <remarks>
    /// <para>
    /// This property overrides the abstract <see cref="TestCaseTestData.TestCaseName"/> and is
    /// set during construction from the <see cref="ITestData.TestCaseName"/> property of the
    /// provided test data.
    /// </para>
    /// <para>
    /// The <c>init</c> accessor ensures immutability after construction while satisfying the
    /// abstract property requirement from the base class.
    /// </para>
    /// </remarks>
    /// <seealso cref="ITestData.TestCaseName"/>
    /// <seealso cref="TestCaseTestData.TestCaseName"/>
    public override string TestCaseName { get; init; }
}