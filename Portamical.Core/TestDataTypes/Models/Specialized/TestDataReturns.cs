// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

/// <summary>
/// Abstract base class for test data that verifies method return values of value types.
/// </summary>
/// <typeparam name="TStruct">
/// The type of the expected return value. Must be a value type (struct).
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="TestDataExpected{TResult}"/> and implements <see cref="IReturns{TResult}"/>
/// to provide a foundation for test data types that verify successful execution paths with value-type return values.
/// </para>
/// <para>
/// <strong>Constraint Rationale:</strong> The <c>struct</c> constraint is intentional and provides three key benefits:
/// <list type="number">
///   <item><strong>Non-null guarantee:</strong> Value types cannot be null (without <c>Nullable&lt;T&gt;</c>), ensuring test expectations are always concrete values</item>
///   <item><strong>Meaningful ToString():</strong> All value types have predictable string representations, crucial for generating readable test case names</item>
///   <item><strong>Value-based assessment:</strong> Designed for testing methods that return primitive types, structs, and other value types with value-based equality semantics</item>
/// </list>
/// </para>
/// <para>
/// <strong>For Reference Types:</strong> To test methods that return reference types (strings, objects, custom classes),
/// use <c>TestDataReturnsRef&lt;TResult&gt;</c> or <c>TestDataReturnsString</c> which handle reference type-specific concerns.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Implements <see cref="IReturns{TResult}"/> marker interface for type discrimination</item>
///   <item>Returns "returns" as the result prefix via <see cref="GetResultPrefix()"/></item>
///   <item>Formats expected value as "returns {value}" via <see cref="GetResult()"/> using <see cref="object.ToString()"/></item>
///   <item>Supports trimming of expected value via <see cref="PropsCode.TrimReturnsExpected"/></item>
/// </list>
/// </para>
/// <para>
/// <strong>Derived Types:</strong> Further derived classes add argument properties (e.g.,
/// <c>TestDataReturns&lt;TResult, TArg1&gt;</c>, <c>TestDataReturns&lt;TResult, TArg1, TArg2&gt;</c>).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Testing integer return values
/// var intTest = new TestDataReturns&lt;int, int, int&gt;
/// {
///     TestCaseName = "Add(2,3) =&gt; returns 5",
///     Expected = 5,  // Guaranteed non-null
///     Arg1 = 2,
///     Arg2 = 3
/// };
/// // Test case name: "Add(2,3) => returns 5" ✅ Meaningful
/// 
/// // Testing boolean return values
/// var boolTest = new TestDataReturns&lt;bool, string&gt;
/// {
///     TestCaseName = "IsValid(input) =&gt; returns True",
///     Expected = true,  // Guaranteed non-null
///     Arg1 = "input"
/// };
/// // Test case name: "IsValid(input) => returns True" ✅ Meaningful
/// 
/// // Testing custom struct return values
/// public struct Point
/// {
///     public int X { get; init; }
///     public int Y { get; init; }
///     public override string ToString() =&gt; $"({X}, {Y})";
/// }
/// 
/// var pointTest = new TestDataReturns&lt;Point&gt;
/// {
///     TestCaseName = "Origin() =&gt; returns (0, 0)",
///     Expected = new Point { X = 0, Y = 0 }  // Guaranteed non-null
/// };
/// // Test case name: "Origin() => returns (0, 0)" ✅ Meaningful
/// </code>
/// </example>
public abstract class TestDataReturns<TStruct>
: TestDataExpected<TStruct>,
IReturns<TStruct>
where TStruct : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataReturns{TStruct}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The descriptive definition of the test case scenario (left side of "=&gt;").
    /// </param>
    /// <param name="expected">
    /// The expected return value. Guaranteed to be non-null due to <c>struct</c> constraint.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Accessibility:</strong> This constructor is <c>private protected</c> to prevent
    /// derivation outside the Portamical assembly. Users should use the provided concrete
    /// implementations (e.g., <c>TestDataReturns&lt;TResult, TArg1&gt;</c>) rather than
    /// deriving custom classes.
    /// </para>
    /// <para>
    /// <strong>Non-null Guarantee:</strong> The <c>struct</c> constraint ensures <paramref name="expected"/>
    /// can never be null, eliminating null-reference concerns in test assertions.
    /// </para>
    /// </remarks>
    private protected TestDataReturns(
        string definition,
        TStruct expected)
    : base(definition, expected)
    {
    }

    private const string ReturnsString = "returns";

    /// <inheritdoc/>
    public override sealed string GetResultPrefix()
    => GetValidResultPrefix(ReturnsString);

    /// <summary>
    /// Gets the formatted result string for the test case name.
    /// </summary>
    /// <returns>
    /// A formatted string in the form <c>"returns {expected}"</c>, where <c>{expected}</c>
    /// is the string representation of the <see cref="TestDataExpected{TResult}.Expected"/> value.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataBase.GetResult()"/> to provide the expected
    /// outcome portion of the test case name. It calls <see cref="TestDataExpected{TResult}.GetExpectedResult(string?)"/>
    /// with the string representation of <see cref="TestDataExpected{TResult}.Expected"/>.
    /// </para>
    /// <para>
    /// <strong>ToString() Guarantee:</strong> The <c>struct</c> constraint ensures that
    /// <see cref="object.ToString()"/> produces a meaningful string representation, as all value types
    /// have well-defined <c>ToString()</c> implementations. This creates readable test case names
    /// like <c>"Add(2,3) =&gt; returns 5"</c> rather than <c>"Add(2,3) =&gt; returns MyNamespace.MyClass"</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Integer expected value
    /// var intTest = new TestDataReturns&lt;int&gt;("Add(2,3)", 5);
    /// string result = intTest.GetResult();
    /// // Returns: "returns 5" ✅ Meaningful
    /// 
    /// // Boolean expected value
    /// var boolTest = new TestDataReturns&lt;bool&gt;("IsValid(input)", true);
    /// string result2 = boolTest.GetResult();
    /// // Returns: "returns True" ✅ Meaningful
    /// 
    /// // DateTime expected value
    /// var dateTest = new TestDataReturns&lt;DateTime&gt;("Now()", new DateTime(2026, 1, 15));
    /// string result3 = dateTest.GetResult();
    /// // Returns: "returns 1/15/2026 12:00:00 AM" ✅ Meaningful
    /// 
    /// // Custom struct with ToString() override
    /// public struct Point
    /// {
    ///     public int X { get; init; }
    ///     public int Y { get; init; }
    ///     public override string ToString() =&gt; $"({X}, {Y})";
    /// }
    /// 
    /// var pointTest = new TestDataReturns&lt;Point&gt;("Origin()", new Point { X = 0, Y = 0 });
    /// string result4 = pointTest.GetResult();
    /// // Returns: "returns (0, 0)" ✅ Meaningful
    /// </code>
    /// </example>
    public override sealed string GetResult()
    => GetExpectedResult(Expected.ToString());

    /// <inheritdoc/>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimReturnsExpected);
}
