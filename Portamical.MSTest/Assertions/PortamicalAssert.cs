// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.Assertions;

/// <summary>
/// Provides MSTest-specific assertion helper methods with simplified APIs.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="Portamical.Assertions.PortamicalAssert"/> by pre-configuring
/// framework-agnostic assertion methods with MSTest-specific delegates (<see cref="Assert.AreEqual"/>,
/// <see cref="Assert.Fail"/>, etc.). This eliminates the need for test authors to pass assertion
/// delegates explicitly.
/// </para>
/// <para>
/// <strong>Design Pattern: Adapter</strong>
/// </para>
/// <para>
/// Adapts the framework-agnostic base class to MSTest by:
/// <list type="bullet">
///   <item>Inheriting all assertion logic from <see cref="Portamical.Assertions.PortamicalAssert"/></item>
///   <item>Providing simplified method overloads that inject MSTest-specific assertion delegates</item>
///   <item>Wrapping MSTest's <see cref="Assert"/> class methods in a consistent API</item>
/// </list>
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong> Test classes can use these methods directly via static import
/// or derive from a test base class that exposes them.
/// </para>
/// <para>
/// <strong>MSTest-Specific Behaviors:</strong>
/// <list type="bullet">
///   <item>
///     <strong>AssertMultiple:</strong> MSTest does not support assertion aggregation (collecting
///     multiple failures). The <see cref="AssertMultiple(Action)"/> method provides API consistency
///     with NUnit but executes like standard MSTest assertions (stops at first failure).
///   </item>
///   <item>
///     <strong>Exception Assertions:</strong> Uses <see cref="Assert.AreEqual"/> for metadata
///     comparison and <see cref="Assert.Fail"/> for assertion failures.
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage via Static Import:</strong></para>
/// <code>
/// using static Portamical.MSTest.Assertions.PortamicalAssert;
/// using Microsoft.VisualStudio.TestTools.UnitTesting;
/// 
/// [TestClass]
/// public class CalculatorTests
/// {
///     [TestMethod]
///     public void TestNoException()
///     {
///         // Simplified - no need to pass Assert.Fail
///         DoesNotThrow(() => Calculator.Add(2, 3));
///     }
///     
///     [TestMethod]
///     public void TestExceptionDetails()
///     {
///         var expected = new ArgumentException("Invalid input", "value");
///         
///         // Simplified - MSTest assertions injected automatically
///         var actual = ThrowsDetails(
///             () => Calculator.Divide(10, 0),
///             expected);
///         
///         // 'actual' is the caught exception, verified against 'expected'
///     }
/// }
/// </code>
/// 
/// <para><strong>Comparison with Base Class (Framework-Agnostic):</strong></para>
/// <code>
/// // Base class (verbose - requires delegates):
/// Portamical.Assertions.PortamicalAssert.DoesNotThrow(
///     () => Calculator.Add(2, 3),
///     assertFail: Assert.Fail);  // Must provide MSTest delegate
/// 
/// // MSTest adapter (simplified - delegates pre-configured):
/// Portamical.MSTest.Assertions.PortamicalAssert.DoesNotThrow(
///     () => Calculator.Add(2, 3));  // No delegate needed!
/// </code>
/// 
/// <para><strong>AssertMultiple Limitation Example:</strong></para>
/// <code>
/// [TestMethod]
/// public void TestMultipleAssertions()
/// {
///     int result1 = Calculator.Add(2, 3);
///     int result2 = Calculator.Add(5, 6);
///     int result3 = Calculator.Add(3, 4);
///     
///     // ⚠️ MSTest Limitation: Stops at first failure
///     AssertMultiple(() =>
///     {
///         Assert.AreEqual(5, result1);   // ✅ Passes
///         Assert.AreEqual(10, result2);  // ❌ Fails - STOPS HERE
///         Assert.AreEqual(7, result3);   // 🚫 Never executed
///     });
///     
///     // Use separate test methods for independent assertions in MSTest
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.Assertions.PortamicalAssert"/>
/// <seealso cref="Microsoft.VisualStudio.TestTools.UnitTesting.Assert"/>
/// <seealso cref="DoesNotThrow(Action)"/>
/// <seealso cref="ThrowsDetails{TException}(Action, TException)"/>
/// <seealso cref="IsTypeOf(Type, object)"/>
/// <seealso cref="AssertMultiple(Action)"/>
public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    /// <summary>
    /// Prevents external instantiation while allowing potential future derived classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This protected constructor maintains consistency with the base class
    /// <see cref="Portamical.Assertions.PortamicalAssert"/> design pattern. While this MSTest
    /// adapter is sealed in its abstraction (contains only static methods and is not intended
    /// for further inheritance in typical usage), the protected constructor:
    /// </para>
    /// <list type="bullet">
    ///   <item>Maintains architectural consistency with the base class</item>
    ///   <item>Explicitly documents that instantiation is not intended</item>
    ///   <item>Allows for potential future specialized adapters if needed</item>
    /// </list>
    /// <para>
    /// The class is abstract and contains only static methods, so instantiation is neither
    /// necessary nor intended in normal usage scenarios.
    /// </para>
    /// </remarks>
    protected PortamicalAssert()
    {
    }

    /// <summary>
    /// Executes multiple assertions. <strong>Note:</strong> MSTest does not support assertion
    /// aggregation; execution stops at the first failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>MSTest Limitation:</strong> Unlike NUnit's <c>Assert.Multiple</c>, this method
    /// does <strong>not</strong> collect multiple assertion failures and report them together.
    /// It provides API consistency across test frameworks (NUnit, xUnit, MSTest) but behaves
    /// like standard MSTest assertions: <strong>execution stops at the first failure</strong>.
    /// </para>
    /// <para>
    /// <strong>Framework Comparison:</strong>
    /// </para>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Framework</term>
    ///     <description>Behavior</description>
    ///   </listheader>
    ///   <item>
    ///     <term>NUnit</term>
    ///     <description>Collects all failures, reports them together</description>
    ///   </item>
    ///   <item>
    ///     <term>xUnit</term>
    ///     <description>No native support; stops at first failure</description>
    ///   </item>
    ///   <item>
    ///     <term>MSTest</term>
    ///     <description>No native support; stops at first failure</description>
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Recommendation:</strong> In MSTest, use separate test methods or <c>[DataTestMethod]</c>
    /// with multiple test cases instead of relying on assertion aggregation.
    /// </para>
    /// </remarks>
    /// <param name="assertions">
    /// An action containing multiple assertion statements. Execution stops at the first failure.
    /// </param>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// public void TestCalculations()
    /// {
    ///     int result1 = Calculator.Add(2, 3);
    ///     int result2 = Calculator.Multiply(4, 5);
    ///     
    ///     // ⚠️ Stops at first failure in MSTest
    ///     AssertMultiple(() =>
    ///     {
    ///         Assert.AreEqual(5, result1);   // ✅ Passes
    ///         Assert.AreEqual(19, result2);  // ❌ Fails - stops here
    ///         // Any assertions below never execute
    ///     });
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultipleAsync(Func{Task})"/>
    public static void AssertMultiple(Action assertions)
    => assertions();

    /// <summary>
    /// Executes multiple asynchronous assertions. <strong>Note:</strong> MSTest does not support
    /// assertion aggregation; execution stops at the first failure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Async version of <see cref="AssertMultiple(Action)"/>. See that method for MSTest limitation details.
    /// </para>
    /// </remarks>
    /// <param name="assertions">
    /// A function containing multiple assertion statements that returns a <see cref="Task"/>.
    /// Execution stops at the first failure.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// public async Task TestAsyncCalculations()
    /// {
    ///     int result1 = await Calculator.AddAsync(2, 3);
    ///     int result2 = await Calculator.MultiplyAsync(4, 5);
    ///     
    ///     await AssertMultipleAsync(async () =>
    ///     {
    ///         Assert.AreEqual(5, result1);
    ///         Assert.AreEqual(20, result2);
    ///     });
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultiple(Action)"/>
    public static Task AssertMultipleAsync(Func<Task> assertions)
    => assertions();

    /// <summary>
    /// Creates an equality assertion delegate for MSTest using <see cref="Assert.AreEqual"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This factory method generates a delegate that compares two values of type <typeparamref name="T"/>
    /// using MSTest's <see cref="Assert.AreEqual"/> method. It's used internally to inject MSTest-specific
    /// equality assertions into framework-agnostic base class methods.
    /// </para>
    /// <para>
    /// <strong>Design Rationale:</strong> Centralizes the assertion logic in one place for
    /// maintainability and consistency. All methods that require equality assertions use this factory.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <returns>
    /// A delegate that takes two parameters (expected and actual) and asserts their equality
    /// using <see cref="Assert.AreEqual"/>.
    /// </returns>
    private static Action<T, T?> AssertEquality<T>()
    => (e, a) => Assert.AreEqual(e, a);

    /// <summary>
    /// Verifies that an action does not throw any exception.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Simplified MSTest version of <see cref="Portamical.Assertions.PortamicalAssert.DoesNotThrow(Action, Action{string})"/>.
    /// Automatically uses <see cref="Assert.Fail"/> for assertion failures.
    /// </para>
    /// </remarks>
    /// <param name="attempt">The action to execute and verify for absence of exceptions.</param>
    /// <exception cref="AssertFailedException">
    /// Thrown via <see cref="Assert.Fail"/> if <paramref name="attempt"/> throws any exception.
    /// </exception>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// public void TestNoExceptionThrown()
    /// {
    ///     DoesNotThrow(() => Calculator.Add(2, 3));
    ///     // Fails if Calculator.Add throws any exception
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.DoesNotThrow(Action, Action{string})"/>
    public static void DoesNotThrow(Action attempt)
    => DoesNotThrow(attempt,
        assertFail: Assert.Fail);

    /// <summary>
    /// Verifies that an object is of the expected type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Simplified MSTest version of <see cref="Portamical.Assertions.PortamicalAssert.IsTypeOf(Type, object, Action{Type, Type})"/>.
    /// Automatically uses <see cref="Assert.AreEqual"/> for type comparison.
    /// </para>
    /// </remarks>
    /// <param name="expected">The expected type.</param>
    /// <param name="actual">The object whose type to verify.</param>
    /// <exception cref="AssertFailedException">
    /// Thrown via <see cref="Assert.AreEqual"/> if <paramref name="actual"/>'s type does not match <paramref name="expected"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// public void TestObjectType()
    /// {
    ///     object result = Calculator.Create();
    ///     IsTypeOf(typeof(Calculator), result);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.IsTypeOf(Type, object, Action{Type, Type})"/>
    public static void IsTypeOf(Type expected, object actual)
    => IsTypeOf(expected, actual,
        assertEquality: AssertEquality<object>());

    /// <summary>
    /// Verifies that an action throws an exception matching the expected exception's type and metadata.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Simplified MSTest version of <see cref="Portamical.Assertions.PortamicalAssert.ThrowsDetails{TException}(Action, TException, Func{Action, Exception}, Action{Type, Exception}, Action{string, string}, Action{string})"/>.
    /// Automatically uses:
    /// <list type="bullet">
    ///   <item><see cref="CatchException(Action)"/> from base class for exception capture</item>
    ///   <item><see cref="IsTypeOf(Type, object)"/> for type verification</item>
    ///   <item><see cref="Assert.AreEqual"/> for metadata (message, param name) comparison</item>
    ///   <item><see cref="Assert.Fail"/> for assertion failures</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Validation:</strong> Verifies exception type, message, and (for <see cref="ArgumentException"/>)
    /// parameter name match the expected exception.
    /// </para>
    /// </remarks>
    /// <typeparam name="TException">
    /// The type of exception expected. Must be a non-null reference type inheriting from <see cref="Exception"/>.
    /// </typeparam>
    /// <param name="attempt">The action expected to throw an exception.</param>
    /// <param name="expected">
    /// The expected exception instance with message and (if applicable) parameter name to match against.
    /// </param>
    /// <returns>The actual exception that was thrown, after verification.</returns>
    /// <exception cref="AssertFailedException">
    /// Thrown if:
    /// <list type="bullet">
    ///   <item>No exception is thrown by <paramref name="attempt"/></item>
    ///   <item>The thrown exception's type doesn't match <typeparamref name="TException"/></item>
    ///   <item>The exception message doesn't match <paramref name="expected"/>'s message</item>
    ///   <item>The exception parameter name (for <see cref="ArgumentException"/>) doesn't match <paramref name="expected"/></item>
    /// </list>
    /// </exception>
    /// <example>
    /// <code>
    /// [TestMethod]
    /// public void TestArgumentException()
    /// {
    ///     var expected = new ArgumentException("Value cannot be negative", "amount");
    ///     
    ///     var actual = ThrowsDetails(
    ///         () => Calculator.SetAmount(-10),
    ///         expected);
    ///     
    ///     // Verifies:
    ///     // - Exception type: ArgumentException
    ///     // - Message: "Value cannot be negative"
    ///     // - ParamName: "amount"
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.ThrowsDetails{TException}(Action, TException, Func{Action, Exception}, Action{Type, Exception}, Action{string, string}, Action{string})"/>
    /// <seealso cref="DoesNotThrow(Action)"/>
    public static TException ThrowsDetails<TException>(Action attempt, TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(attempt, expected,
        catchException: CatchException,
        assertIsType: IsTypeOf,
        assertEquality: AssertEquality<string>(),
        assertFail: Assert.Fail);
}