// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.TestDataTypes.Patterns;

/// <summary>
/// Represents a base interface for test data that has an expected result value.
/// </summary>
/// <remarks>
/// <para>
/// This non-generic interface provides polymorphic access to expected test results
/// when the specific type is not known at compile time. It serves as the base for
/// the generic <see cref="IExpected{TResult}"/> interface.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Generic Specialization Pattern - provides both
/// non-generic (polymorphic) and generic (type-safe) access to the same data.
/// </para>
/// <para>
/// <strong>Typical Implementations:</strong>
/// <list type="bullet">
///   <item><strong>TestDataReturns:</strong> For methods that return a value</item>
///   <item><strong>TestDataThrows:</strong> For methods that throw an exception</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Polymorphic usage
/// void ProcessExpectation(IExpected expectation)
/// {
///     string prefix = expectation.GetResultPrefix();
///     object expected = expectation.GetExpected();
///     Console.WriteLine($"{prefix}: {expected}");
/// }
/// 
/// // Works with any implementation
/// ProcessExpectation(new TestDataReturns&lt;int&gt; { Expected = 42 });
/// ProcessExpectation(new TestDataThrows&lt;Exception&gt; { Expected = new Exception() });
/// </code>
/// </example>
public interface IExpected
{
    /// <summary>
    /// Gets the prefix applied to the expected result portion of the test case name.
    /// </summary>
    /// <returns>
    /// A string prefix that describes the type of expected result, typically:
    /// <list type="bullet">
    ///   <item>"returns" - for methods that return a value</item>
    ///   <item>"throws" - for methods that throw an exception</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This prefix is used to construct the test case name in the format:
    /// <c>"{scenario} =&gt; {prefix} {expected}"</c>
    /// </para>
    /// <para>
    /// <strong>Example:</strong> For a test case with definition "Adding 2+3" and expected result 5,
    /// this method returns "returns", resulting in the test case name:
    /// <c>"Adding 2+3 =&gt; returns 5"</c>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // TestDataReturns implementation
    /// public class TestDataReturns&lt;TResult&gt; : IExpected&lt;TResult&gt;
    /// {
    ///     public TResult Expected { get; init; }
    ///     
    ///     public string GetResultPrefix() =&gt; "returns";
    ///     
    ///     // Test case name: "scenario =&gt; returns {Expected}"
    /// }
    /// 
    /// // TestDataThrows implementation
    /// public class TestDataThrows&lt;TException&gt; : IExpected&lt;TException&gt;
    /// {
    ///     public TException Expected { get; init; }
    ///     
    ///     public string GetResultPrefix() =&gt; "throws";
    ///     
    ///     // Test case name: "scenario =&gt; throws {Expected}"
    /// }
    /// </code>
    /// </example>
    string GetResultPrefix();

    /// <summary>
    /// Gets the expected value for the test case in a non-generic form.
    /// </summary>
    /// <returns>
    /// An object representing the expected value. The actual type depends on the implementation:
    /// <list type="bullet">
    ///   <item>For <see cref="IExpected{TResult}"/> implementations, returns the value of <see cref="IExpected{TResult}.Expected"/></item>
    ///   <item>For return value tests, this is the expected return value</item>
    ///   <item>For exception tests, this is the expected exception instance</item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Usage:</strong> This method enables polymorphic access to expected values when the
    /// generic type parameter is not known at compile time. For type-safe access, use
    /// <see cref="IExpected{TResult}.Expected"/> instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Polymorphic access
    /// IExpected expectation = GetTestExpectation();
    /// object expected = expectation.GetExpected(); // Returns object
    /// 
    /// // Type-safe access
    /// IExpected&lt;int&gt; intExpectation = GetIntExpectation();
    /// int typedExpected = intExpectation.Expected; // Returns int
    /// </code>
    /// </example>
    object GetExpected();
}

/// <summary>
/// Defines a contract for accessing the expected result of a test case in a strongly typed manner.
/// </summary>
/// <typeparam name="TResult">
/// The type of the expected result. Must be a non-nullable type.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface extends <see cref="IExpected"/> to provide type-safe access to expected values
/// through the <see cref="Expected"/> property. It uses covariance (<c>out TResult</c>) to enable
/// flexible type substitution in inheritance hierarchies.
/// </para>
/// <para>
/// <strong>Covariance Benefits:</strong> The <c>out</c> modifier allows an <c>IExpected&lt;Derived&gt;</c>
/// to be treated as <c>IExpected&lt;Base&gt;</c>, enabling flexible polymorphic usage:
/// <code>
/// IExpected&lt;Dog&gt; dogExpected = ...;
/// IExpected&lt;Animal&gt; animalExpected = dogExpected; // Valid
/// </code>
/// </para>
/// <para>
/// <strong>Generic Constraint:</strong> The <c>notnull</c> constraint ensures expected values
/// cannot be null, enforcing that test expectations always have concrete values. For scenarios
/// requiring nullable expected values, wrap the value in a container type.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implementing IExpected&lt;TResult&gt;
/// public class TestDataReturns&lt;TResult&gt; : TestDataBase, IExpected&lt;TResult&gt;
///     where TResult : notnull
/// {
///     public TResult Expected { get; init; } = default!;
///     
///     public object GetExpected() =&gt; Expected;
///     
///     public string GetResultPrefix() =&gt; "returns";
///     
///     public override string GetResult() =&gt; $"{GetResultPrefix()} {Expected}";
/// }
/// 
/// // Usage
/// var testData = new TestDataReturns&lt;int&gt;
/// {
///     TestCaseName = "Add(2,3) =&gt; returns 5",
///     Expected = 5
/// };
/// 
/// // Type-safe access
/// int expected = testData.Expected; // No casting
/// 
/// // Polymorphic access
/// IExpected baseExpected = testData;
/// object expectedObj = baseExpected.GetExpected(); // Returns 5 as object
/// </code>
/// </example>
public interface IExpected<out TResult>
: IExpected
where TResult : notnull
{
    /// <summary>
    /// Gets the strongly typed expected result of the test case.
    /// </summary>
    /// <value>
    /// The expected result of type <typeparamref name="TResult"/>. This value is guaranteed to be non-null
    /// due to the <c>notnull</c> constraint.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property provides type-safe access to the expected value, eliminating the need for casting
    /// compared to <see cref="IExpected.GetExpected()"/>.
    /// </para>
    /// <para>
    /// <strong>Immutability:</strong> This property is read-only (getter only) to support covariance
    /// and ensure expected values cannot be changed after initialization.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testData = new TestDataReturns&lt;int&gt;
    /// {
    ///     Expected = 42,
    ///     // ... other properties
    /// };
    /// 
    /// // Type-safe access
    /// int expected = testData.Expected; // No casting needed
    /// 
    /// // Compare with non-generic
    /// object expectedObj = ((IExpected)testData).GetExpected();
    /// int expectedInt = (int)expectedObj; // Requires cast
    /// </code>
    /// </example>
    TResult Expected { get; }
}
