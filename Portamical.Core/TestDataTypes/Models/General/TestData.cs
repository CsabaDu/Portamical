// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.Core.Strategy;

namespace Portamical.Core.TestDataTypes.Models.General;

/// <summary>
/// Abstract base class for general-purpose test data with custom result formatting.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="TestDataBase"/> to provide a general-purpose test data foundation
/// without the constraints of <see cref="TestDataReturns{TStruct}"/> or <see cref="TestDataThrows{TException}"/>.
/// It allows fully custom result formatting specified as a string parameter in the constructor.
/// </para>
/// <para>
/// <strong>When to Use TestData:</strong>
/// <list type="bullet">
///   <item>Custom result formats beyond "returns {value}" or "throws {exception}"</item>
///   <item>Complex scenarios with descriptive outcomes (e.g., "succeeds with warnings", "partially completes")</item>
///   <item>Reference type testing without the constraints of TestDataReturns</item>
///   <item>Test cases where result formatting is determined at construction time</item>
/// </list>
/// </para>
/// <para>
/// <strong>Comparison with Specialized Classes:</strong>
/// <list type="bullet">
///   <item><see cref="TestDataReturns{TStruct}"/> - For value type returns with automatic "returns {value}" formatting</item>
///   <item><see cref="TestDataThrows{TException}"/> - For exception testing with automatic "throws {type}" formatting</item>
///   <item><see cref="TestData"/> - For custom result formatting with no type constraints</item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> This class uses a <c>readonly</c> field for the result string
/// to enforce immutability, ensuring the result cannot be changed after construction.
/// </para>
/// <para>
/// <strong>Derived Types:</strong> Further derived classes add argument properties (e.g.,
/// <c>TestData&lt;TArg1&gt;</c>, <c>TestData&lt;TArg1, TArg2&gt;</c>) and override
/// <see cref="TestDataBase.ToObjectArray(ArgsCode)"/> to include those arguments.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Custom result format
/// public class MyTestData : TestData
/// {
///     public MyTestData(string definition, string result, string arg)
///         : base(definition, result)
///     {
///         Arg1 = arg;
///     }
///     
///     public string Arg1 { get; }
///     
///     protected override object?[] ToObjectArray(ArgsCode argsCode)
///         =&gt; Extend(base.ToObjectArray, argsCode, Arg1);
/// }
/// 
/// // Usage with custom result
/// var test = new MyTestData(
///     "Process complex data",
///     "succeeds with warnings",  // ✅ Custom format
///     "input.json");
/// // Test case name: "Process complex data => succeeds with warnings"
/// 
/// // Compare with TestDataReturns (constrained):
/// var returnsTest = new TestDataReturns&lt;int&gt;("Add(2,3)", 5);
/// // Test case name: "Add(2,3) => returns 5"  ✅ Fixed format
/// 
/// // Compare with TestDataThrows (constrained):
/// var throwsTest = new TestDataThrows&lt;ArgumentException&gt;("Validate(null)", new ArgumentException());
/// // Test case name: "Validate(null) => throws ArgumentException"  ✅ Fixed format
/// </code>
/// </example>
public abstract class TestData
: TestDataBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestData"/> class.
    /// </summary>
    /// <param name="definition">
    /// The descriptive definition of the test case scenario (left side of "=&gt;").
    /// </param>
    /// <param name="result">
    /// The pre-formatted result string for the test case (right side of "=&gt;").
    /// Unlike <see cref="TestDataReturns{TStruct}"/> or <see cref="TestDataThrows{TException}"/>,
    /// this is the exact string that will appear in the test case name without additional formatting.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Accessibility:</strong> This constructor is <c>private protected</c> to prevent
    /// derivation outside the Portamical assembly. Users should use the provided concrete
    /// implementations (e.g., <c>TestData&lt;TArg1&gt;</c>) or create custom derived classes
    /// within the assembly.
    /// </para>
    /// <para>
    /// <strong>Result Immutability:</strong> The <paramref name="result"/> parameter is stored
    /// in a <c>readonly</c> field, ensuring the result cannot be changed after construction.
    /// This provides stronger immutability than an <c>init</c> property.
    /// </para>
    /// <para>
    /// <strong>Result Formatting:</strong> Unlike specialized classes that format results automatically
    /// (e.g., "returns {value}"), this class uses the result exactly as provided. This allows
    /// complete control over test case name formatting.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Derived class constructor:
    /// public class MyTestData : TestData
    /// {
    ///     public MyTestData(string definition, string result, string arg)
    ///         : base(definition, result)
    ///     {
    ///         Arg1 = arg;
    ///     }
    ///     
    ///     public string Arg1 { get; }
    /// }
    /// 
    /// // Usage:
    /// var test = new MyTestData(
    ///     definition: "Complex operation",
    ///     result: "succeeds with warnings",  // ✅ Used exactly as-is
    ///     arg: "data.json");
    /// // Test case name: "Complex operation => succeeds with warnings"
    /// </code>
    /// </example>
    private protected TestData(
        string definition,
        string result)
    : base(definition)
    {
        _result = result;
        TestCaseName = CreateTestCaseName();
    }

    private readonly string _result;
    private const string ResultString = "result";

    #region Properties
    /// <summary>
    /// Gets the unique name of the test case associated with this instance.
    /// </summary>
    public override sealed string TestCaseName { get; init; }
    #endregion

    /// <summary>
    /// Returns the result string, or a fallback value if the result is null or empty.
    /// </summary>
    /// <returns>A string containing the result. If the result is null or empty, a fallback value is returned instead.</returns>
    public override sealed string GetResult()
    => ResultString.FallbackIfNullOrWhiteSpace(_result, nameof(GetResult));

    /// <summary>
    /// Returns an array of argument values based on the specified argument and property codes.
    /// </summary>
    /// <param name="argsCode">A value that specifies which arguments to include in the returned array.</param>
    /// <param name="propsCode">A value that specifies which properties to include in the returned array.</param>
    /// <returns>An array of objects containing the argument values corresponding to the specified codes. The array may be empty
    /// if no arguments match the criteria.</returns>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode != PropsCode.All);
}
