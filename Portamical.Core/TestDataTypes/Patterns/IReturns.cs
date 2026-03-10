// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.TestDataTypes.Patterns;

/// <summary>
/// Marker interface for test cases validating method return values.
/// </summary>
/// <remarks>
/// <para>
/// This is a <strong>marker interface</strong> (also called a tag interface) with no members,
/// used purely for semantic categorization. It inherits from <see cref="IExpected"/> and marks
/// test data designed to verify successful execution paths with return values.
/// </para>
/// <para>
/// <strong>Marker Pattern Purpose:</strong>
/// <list type="bullet">
///   <item>Enables type discrimination via <c>is</c> operator or <c>OfType&lt;T&gt;()</c></item>
///   <item>Distinguishes return value tests from exception tests (e.g., <c>IThrows</c>)</item>
///   <item>Allows test frameworks to filter test data by category</item>
///   <item>Provides semantic clarity in type system without runtime overhead</item>
/// </list>
/// </para>
/// <para>
/// <strong>Typical Implementation:</strong> <c>TestDataReturns&lt;TResult&gt;</c>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Type discrimination
/// ITestData testData = GetTestData();
/// 
/// if (testData is IReturns returnsTest)
/// {
///     Console.WriteLine("This test verifies a return value");
///     object expected = returnsTest.GetExpected();
///     Console.WriteLine($"Expected: {expected}");
/// }
/// 
/// // Filtering
/// IEnumerable&lt;ITestData&gt; allTests = GetAllTests();
/// var returnTests = allTests.OfType&lt;IReturns&gt;();
/// 
/// foreach (var test in returnTests)
/// {
///     // Process only return value tests
///     ProcessReturnTest(test);
/// }
/// </code>
/// </example>
public interface IReturns
: IExpected;

/// <summary>
/// Defines a strongly-typed contract for test cases that verify method return values.
/// </summary>
/// <typeparam name="TResult">
/// The type of the expected return value. Must be a non-nullable type.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface combines:
/// <list type="bullet">
///   <item><see cref="IExpected{TResult}"/> - Type-safe access to expected return value via <see cref="IExpected{TResult}.Expected"/></item>
///   <item><see cref="IReturns"/> - Marker interface for semantic categorization of return value tests</item>
/// </list>
/// </para>
/// <para>
/// <strong>Covariance:</strong> The <c>out</c> modifier enables covariant type substitution,
/// allowing an <c>IReturns&lt;Derived&gt;</c> to be treated as <c>IReturns&lt;Base&gt;</c>:
/// <code>
/// IReturns&lt;string&gt; stringReturns = new TestDataReturns&lt;string&gt; { Expected = "hello" };
/// IReturns&lt;object&gt; objectReturns = stringReturns; // Valid
/// </code>
/// </para>
/// <para>
/// <strong>Generic Constraint:</strong> The <c>notnull</c> constraint ensures expected return values
/// cannot be null, enforcing concrete expectations. This constraint is inherited from
/// <see cref="IExpected{TResult}"/>.
/// </para>
/// <para>
/// <strong>Typical Implementation:</strong> <c>TestDataReturns&lt;TResult&gt;</c> - for testing methods
/// that return a value of type <typeparamref name="TResult"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implementing IReturns&lt;TResult&gt;
/// public class TestDataReturns&lt;TResult&gt; : TestDataBase, IReturns&lt;TResult&gt;
///     where TResult : notnull
/// {
///     public TResult Expected { get; init; } = default!;
///     
///     public object GetExpected() =&gt; Expected;
///     
///     public string GetResultPrefix() =&gt; "returns";
/// }
/// 
/// // Usage
/// var testData = new TestDataReturns&lt;int&gt;
/// {
///     TestCaseName = "Add(2,3) =&gt; returns 5",
///     Expected = 5,
///     Arg1 = 2,
///     Arg2 = 3
/// };
/// 
/// // Type-safe access via IReturns&lt;int&gt;
/// int expected = testData.Expected; // No casting
/// 
/// // Marker interface check
/// if (testData is IReturns)
/// {
///     Console.WriteLine("This is a return value test");
/// }
/// 
/// // Covariance
/// IReturns&lt;object&gt; objReturns = testData; // string → object
/// </code>
/// </example>
public interface IReturns<out TResult>
: IExpected<TResult>,
IReturns
where TResult : notnull;