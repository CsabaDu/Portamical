// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.TestDataTypes.Patterns;

/// <summary>
/// <summary>
/// Marker interface for test cases validating exception throwing behavior.
/// </summary>
/// <remarks>
/// <para>
/// This is a <strong>marker interface</strong> (also called a tag interface) with no members,
/// used purely for semantic categorization. It inherits from <see cref="IExpected"/> and marks
/// test data designed to verify error handling and exceptional execution paths.
/// </para>
/// <para>
/// <strong>Marker Pattern Purpose:</strong>
/// <list type="bullet">
///   <item>Enables type discrimination via <c>is</c> operator or <c>OfType&lt;T&gt;()</c></item>
///   <item>Distinguishes exception tests from return value tests (e.g., <c>IReturns</c>)</item>
///   <item>Allows test frameworks to filter test data by category</item>
///   <item>Provides semantic clarity in type system without runtime overhead</item>
/// </list>
/// </para>
/// <para>
/// <strong>Typical Implementation:</strong> <c>TestDataThrows&lt;TException&gt;</c> where <c>TException : Exception</c>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Type discrimination
/// ITestData testData = GetTestData();
/// 
/// if (testData is IThrows throwsTest)
/// {
///     Console.WriteLine("This test verifies an exception");
///     object expected = throwsTest.GetExpected();
///     Console.WriteLine($"Expected exception: {expected.GetType().Name}");
/// }
/// else if (testData is IReturns returnsTest)
/// {
///     Console.WriteLine("This test verifies a return value");
/// }
/// 
/// // LINQ filtering
/// IEnumerable&lt;ITestData&gt; allTests = GetAllTests();
/// var exceptionTests = allTests.OfType&lt;IThrows&gt;();
/// 
/// foreach (var test in exceptionTests)
/// {
///     // Process only exception tests
///     ValidateExceptionThrown(test);
/// }
/// </code>
/// </example>
public interface IThrows
    : IExpected;

/// <summary>
/// Defines a strongly-typed contract for test cases that verify exception throwing behavior.
/// </summary>
/// <typeparam name="TException">
/// The type of exception expected to be thrown. Must derive from <see cref="Exception"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface combines:
/// <list type="bullet">
///   <item><see cref="IExpected{TResult}"/> - Type-safe access to expected exception via <see cref="IExpected{TResult}.Expected"/></item>
///   <item><see cref="IThrows"/> - Marker interface for semantic categorization of exception tests</item>
/// </list>
/// </para>
/// <para>
/// <strong>Covariance:</strong> The <c>out</c> modifier enables covariant type substitution,
/// allowing an <c>IThrows&lt;DerivedException&gt;</c> to be treated as <c>IThrows&lt;BaseException&gt;</c>:
/// <code>
/// IThrows&lt;ArgumentException&gt; argThrows = new TestDataThrows&lt;ArgumentException&gt; { Expected = new ArgumentException() };
/// IThrows&lt;Exception&gt; exceptionThrows = argThrows; // Valid (covariance)
/// </code>
/// </para>
/// <para>
/// <strong>Generic Constraint:</strong> The <c>Exception</c> constraint ensures only throwable types
/// can be used as expected values, enforcing that test expectations are valid exception types.
/// This prevents invalid types like <c>IThrows&lt;string&gt;</c> or <c>IThrows&lt;int&gt;</c>.
/// </para>
/// <para>
/// <strong>Typical Implementation:</strong> <c>TestDataThrows&lt;TException&gt;</c> - for testing methods
/// that throw an exception of type <typeparamref name="TException"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implementing IThrows&lt;TException&gt;
/// public class TestDataThrows&lt;TException&gt; : TestDataBase, IThrows&lt;TException&gt;
///     where TException : Exception
/// {
///     public TException Expected { get; init; } = default!;
///     
///     public object GetExpected() =&gt; Expected;
///     
///     public string GetValidResultPrefix() =&gt; "throws";
/// }
/// 
/// // Usage
/// var testData = new TestDataThrows&lt;ArgumentException&gt;
/// {
///     TestCaseName = "Validate(null) =&gt; throws ArgumentException",
///     Expected = new ArgumentException("Value cannot be null", "input"),
///     Arg1 = null
/// };
/// 
/// // Type-safe access via IThrows&lt;ArgumentException&gt;
/// ArgumentException expectedException = testData.Expected; // No casting
/// 
/// // Marker interface check
/// if (testData is IThrows)
/// {
///     Console.WriteLine("This is an exception test");
/// }
/// 
/// // Covariance (exception hierarchy)
/// IThrows&lt;Exception&gt; exceptionThrows = testData; // ArgumentException → Exception
/// 
/// // Access expected exception type
/// Console.WriteLine($"Expected: {expectedException.GetType().Name}");
/// Console.WriteLine($"Message: {expectedException.Message}");
/// </code>
/// </example>
public interface IThrows<out TException>
    : IExpected<TException>,
      IThrows
where TException : Exception;