// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.NUnit.Assertions;

/// <summary>
/// Provides NUnit-specific assertion methods that extend Portamical's framework-agnostic
/// assertion base class.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Design Pattern: Adapter + Template Method</strong>
/// </para>
/// <para>
/// This class adapts NUnit's assertion API to Portamical's framework-agnostic assertion
/// interface by injecting NUnit-specific implementations into base class template methods:
/// <list type="bullet">
///   <item><description>
///     <see cref="DoesNotThrow"/> - Injects <c>Assert.Fail</c> for failure reporting
///   </description></item>
///   <item><description>
///     <see cref="IsTypeOf"/> - Injects <c>Assert.That</c> with <c>Is.EqualTo</c> constraint
///   </description></item>
///   <item><description>
///     <see cref="ThrowsDetails{TException}"/> - Injects <c>Assert.Catch</c>, <c>Is.TypeOf</c>, <c>Is.EqualTo</c>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>NUnit 4.x Multiple Assertions:</strong>
/// </para>
/// <para>
/// This class provides convenient wrappers for NUnit 4.x's multiple assertion feature
/// (<c>Assert.Multiple</c>), which collects all assertion failures in a single test instead
/// of stopping at the first failure:
/// <list type="bullet">
///   <item><description>
///     <see cref="AssertMultiple"/> - Synchronous multiple assertions
///   </description></item>
///   <item><description>
///     <see cref="AssertMultipleAsync"/> - Asynchronous multiple assertions
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// <code>
/// Portamical.Assertions.PortamicalAssert (framework-agnostic base)
///   ↓ inherits
/// Portamical.NUnit.Assertions.PortamicalAssert (NUnit-specific adapter) ← This class
///   ↓ used by
/// YourTestClass (concrete test fixture)
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Multiple Assertions (Synchronous)</strong></para>
/// <code>
/// using NUnit.Framework;
/// using Portamical.NUnit.Assertions;
/// 
/// [TestFixture]
/// public class PersonTests : PortamicalAssert
/// {
///     [Test]
///     public void TestPerson()
///     {
///         var person = new Person("John", 30, "NYC");
///         
///         // Without AssertMultiple (stops at first failure):
///         Assert.That(person.Name, Is.EqualTo("Jane"));  // ❌ FAILS, test stops here
///         Assert.That(person.Age, Is.EqualTo(25));       // ⚠️ NEVER EXECUTED
///         Assert.That(person.City, Is.EqualTo("LA"));    // ⚠️ NEVER EXECUTED
///         
///         // With AssertMultiple (collects all failures):
///         AssertMultiple(() =>
///         {
///             Assert.That(person.Name, Is.EqualTo("Jane"));  // ❌ FAILS
///             Assert.That(person.Age, Is.EqualTo(25));       // ❌ FAILS
///             Assert.That(person.City, Is.EqualTo("LA"));    // ❌ FAILS
///         });
///         
///         // Test report shows ALL 3 failures, not just the first one
///     }
/// }
/// </code>
/// 
/// <para><strong>Example 2: Multiple Assertions (Asynchronous)</strong></para>
/// <code>
/// [Test]
/// public async Task TestPersonAsync()
/// {
///     var person = await GetPersonAsync();
///     
///     await AssertMultipleAsync(async () =>
///     {
///         var profile = await GetProfileAsync(person.Id);
///         Assert.That(profile, Is.Not.Null);
///         
///         var preferences = await GetPreferencesAsync(person.Id);
///         Assert.That(preferences, Is.Not.Null);
///         
///         var history = await GetHistoryAsync(person.Id);
///         Assert.That(history, Is.Not.Empty);
///     });
/// }
/// </code>
/// 
/// <para><strong>Example 3: DoesNotThrow</strong></para>
/// <code>
/// [Test]
/// public void TestValidInput()
/// {
///     // Assert that operation completes without throwing
///     DoesNotThrow(() =>
///     {
///         var result = Calculator.Divide(10, 2);
///         // If any exception is thrown, test fails with detailed message
///     });
/// }
/// </code>
/// 
/// <para><strong>Example 4: IsTypeOf</strong></para>
/// <code>
/// [Test]
/// public void TestTypeMatch()
/// {
///     object value = "Hello";
///     
///     // Assert that runtime type matches expected type
///     IsTypeOf(typeof(string), value);
///     // If types don't match, shows: Expected &lt;System.String&gt; but was &lt;actual type&gt;
/// }
/// </code>
/// 
/// <para><strong>Example 5: ThrowsDetails (Complex Exception Validation)</strong></para>
/// <code>
/// [Test]
/// public void TestExceptionDetails()
/// {
///     var expected = new ArgumentException("Value cannot be negative", "amount");
///     
///     // Validates both exception type AND exception properties
///     var actual = ThrowsDetails(() =>
///     {
///         Calculator.Withdraw(-100);
///     }, expected);
///     
///     // AssertMultiple internally ensures:
///     // 1. Exception was thrown (not null)
///     // 2. Exception type matches (ArgumentException)
///     // 3. Exception message matches ("Value cannot be negative")
///     // 4. Exception ParamName matches ("amount")
///     
///     // Returns actual exception for further validation
///     Assert.That(actual.ParamName, Is.EqualTo("amount"));
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.Assertions.PortamicalAssert"/>
/// <seealso cref="NUnit.Framework.Assert"/>
public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    /// <summary>
    /// Executes multiple assertions and collects all failures instead of stopping at the first failure.
    /// </summary>
    /// <param name="assertions">
    /// A delegate containing multiple assertions to execute. All assertions will be evaluated,
    /// and all failures will be collected in a single test report.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>NUnit 4.x Multiple Assertions:</strong>
    /// </para>
    /// <para>
    /// This method wraps the provided assertions in NUnit 4.x's <c>Assert.EnterMultipleScope()</c>,
    /// which enables the collection of multiple assertion failures in a single test. Without this,
    /// NUnit stops at the first assertion failure and doesn't execute subsequent assertions.
    /// </para>
    /// <para>
    /// <strong>Benefits:</strong>
    /// <list type="bullet">
    ///   <item><description>
    ///     <strong>Complete Failure Report:</strong> See all failures in one test run, not just the first
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Faster Debugging:</strong> Fix all issues at once instead of running tests repeatedly
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Better Test Coverage:</strong> Ensures all aspects of an object are validated
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>How It Works:</strong>
    /// </para>
    /// <code>
    /// using (Assert.EnterMultipleScope())
    /// {
    ///     assertions();  // Execute all assertions
    /// }
    /// // On disposal:
    /// // - If any assertions failed, throws MultipleAssertException with ALL failures
    /// // - If no assertions failed, test passes
    /// </code>
    /// </remarks>
    /// <example>
    /// <para><strong>Without AssertMultiple (stops at first failure):</strong></para>
    /// <code>
    /// [Test]
    /// public void TestPerson_WithoutMultiple()
    /// {
    ///     var person = new Person("John", 30, "NYC");
    ///     
    ///     Assert.That(person.Name, Is.EqualTo("Jane"));  // ❌ FAILS HERE, test stops
    ///     Assert.That(person.Age, Is.EqualTo(25));       // ⚠️ NEVER EXECUTED
    ///     Assert.That(person.City, Is.EqualTo("LA"));    // ⚠️ NEVER EXECUTED
    /// }
    /// // Test Output:
    /// //   Expected: "Jane"
    /// //   But was:  "John"
    /// //   (Only see first failure)
    /// </code>
    /// 
    /// <para><strong>With AssertMultiple (collects all failures):</strong></para>
    /// <code>
    /// [Test]
    /// public void TestPerson_WithMultiple()
    /// {
    ///     var person = new Person("John", 30, "NYC");
    ///     
    ///     AssertMultiple(() =>
    ///     {
    ///         Assert.That(person.Name, Is.EqualTo("Jane"));  // ❌ FAILS
    ///         Assert.That(person.Age, Is.EqualTo(25));       // ❌ FAILS
    ///         Assert.That(person.City, Is.EqualTo("LA"));    // ❌ FAILS
    ///     });
    /// }
    /// // Test Output:
    /// //   Multiple assertion failures:
    /// //   1. Expected: "Jane", But was: "John"
    /// //   2. Expected: 25, But was: 30
    /// //   3. Expected: "LA", But was: "NYC"
    /// //   (See ALL 3 failures in one test run)
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultipleAsync"/>
    /// <seealso cref="NUnit.Framework.Assert.EnterMultipleScope"/>
    public static void AssertMultiple(Action assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            assertions();
        }
    }

    /// <summary>
    /// Executes multiple asynchronous assertions and collects all failures instead of stopping at the first failure.
    /// </summary>
    /// <param name="assertions">
    /// An asynchronous delegate containing multiple assertions to execute. All assertions will be evaluated,
    /// and all failures will be collected in a single test report.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous assertion operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the asynchronous version of <see cref="AssertMultiple"/>, designed for tests that
    /// perform asynchronous operations (e.g., API calls, database queries, file I/O) within assertions.
    /// </para>
    /// <para>
    /// <strong>ConfigureAwait(false):</strong>
    /// </para>
    /// <para>
    /// The method uses <c>ConfigureAwait(false)</c> to avoid deadlocks in synchronization contexts
    /// and improve performance by not capturing the current execution context. This is standard
    /// practice for library code.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Test]
    /// public async Task TestUserProfileAsync()
    /// {
    ///     var userId = 123;
    ///     
    ///     await AssertMultipleAsync(async () =>
    ///     {
    ///         // Fetch data asynchronously
    ///         var profile = await GetUserProfileAsync(userId);
    ///         Assert.That(profile, Is.Not.Null);
    ///         Assert.That(profile.Name, Is.Not.Empty);
    ///         
    ///         var preferences = await GetUserPreferencesAsync(userId);
    ///         Assert.That(preferences, Is.Not.Null);
    ///         Assert.That(preferences.Theme, Is.EqualTo("Dark"));
    ///         
    ///         var history = await GetUserHistoryAsync(userId);
    ///         Assert.That(history, Is.Not.Empty);
    ///     });
    ///     
    ///     // All assertions are evaluated, all failures reported together
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultiple"/>
    public static async Task AssertMultipleAsync(Func<Task> assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            await assertions().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asserts that the specified action completes without throwing any exceptions.
    /// </summary>
    /// <param name="attempt">
    /// The action to execute. If this action throws any exception, the test fails with a detailed message.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method delegates to the base class template method
    /// <see cref="Portamical.Assertions.PortamicalAssert.DoesNotThrow(Action, Action{string})"/>,
    /// injecting NUnit's <c>Assert.Fail</c> for failure reporting.
    /// </para>
    /// <para>
    /// <strong>Template Method Pattern:</strong>
    /// </para>
    /// <code>
    /// // Base class defines algorithm:
    /// protected static void DoesNotThrow(Action attempt, Action&lt;string&gt; assertFail)
    /// {
    ///     try { attempt(); }
    ///     catch (Exception ex) { assertFail($"Expected no exception, but got: {ex}"); }
    /// }
    /// 
    /// // Derived class injects NUnit implementation:
    /// public static void DoesNotThrow(Action attempt)
    /// =&gt; DoesNotThrow(attempt, assertFail: Assert.Fail);
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Test]
    /// public void TestDivision_ValidInput()
    /// {
    ///     // Assert operation completes without exception
    ///     DoesNotThrow(() =>
    ///     {
    ///         var result = Calculator.Divide(10, 2);
    ///         Assert.That(result, Is.EqualTo(5));
    ///     });
    /// }
    /// 
    /// [Test]
    /// public void TestDivision_InvalidInput()
    /// {
    ///     // This will FAIL because division by zero throws DivideByZeroException
    ///     DoesNotThrow(() =>
    ///     {
    ///         Calculator.Divide(10, 0);
    ///     });
    ///     
    ///     // Test Output:
    ///     //   Expected no exception, but got: System.DivideByZeroException: Attempted to divide by zero.
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.DoesNotThrow(Action, Action{string})"/>
    public static void DoesNotThrow(Action attempt)
    => DoesNotThrow(attempt,
        assertFail: Assert.Fail);

    /// <summary>
    /// Asserts that the runtime type of the actual object matches the expected type.
    /// </summary>
    /// <param name="expected">The expected type.</param>
    /// <param name="actual">The object whose type should be checked.</param>
    /// <remarks>
    /// <para>
    /// This method delegates to the base class template method
    /// <see cref="Portamical.Assertions.PortamicalAssert.IsTypeOf(Type, object, Action{Type, Type})"/>,
    /// injecting NUnit's <c>Assert.That</c> with <c>Is.EqualTo</c> constraint for type comparison.
    /// </para>
    /// <para>
    /// <strong>NUnit Constraint Model:</strong>
    /// </para>
    /// <para>
    /// NUnit uses a constraint-based assertion model, which provides better error messages than
    /// boolean expressions:
    /// </para>
    /// <code>
    /// // NUnit constraint (used by this method):
    /// Assert.That(actualType, Is.EqualTo(expectedType));
    /// // Error: Expected &lt;System.String&gt; but was &lt;System.Int32&gt;
    /// 
    /// // Boolean expression (not used):
    /// Assert.IsTrue(actualType == expectedType);
    /// // Error: Expected True but was False (less descriptive)
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Test]
    /// public void TestObjectType()
    /// {
    ///     object value = "Hello, World!";
    ///     
    ///     // Assert runtime type matches expected type
    ///     IsTypeOf(typeof(string), value);  // ✅ PASSES
    ///     
    ///     IsTypeOf(typeof(int), value);  // ❌ FAILS
    ///     // Error: Expected &lt;System.Int32&gt; but was &lt;System.String&gt;
    /// }
    /// 
    /// [Test]
    /// public void TestPolymorphism()
    /// {
    ///     Animal animal = new Dog();
    ///     
    ///     IsTypeOf(typeof(Dog), animal);     // ✅ PASSES (runtime type is Dog)
    ///     IsTypeOf(typeof(Animal), animal);  // ❌ FAILS (runtime type is Dog, not Animal)
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.IsTypeOf(Type, object, Action{Type, Type})"/>
    public static void IsTypeOf(Type expected, object actual)
    => IsTypeOf(expected, actual,
        assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)));

    /// <summary>
    /// Asserts that the specified action throws an exception of type <typeparamref name="TException"/>
    /// and validates that the thrown exception's details match the expected exception.
    /// </summary>
    /// <typeparam name="TException">
    /// The type of exception expected. Must be a reference type derived from <see cref="Exception"/>.
    /// </typeparam>
    /// <param name="attempt">
    /// The action to execute. This action is expected to throw an exception of type <typeparamref name="TException"/>.
    /// </param>
    /// <param name="expected">
    /// The expected exception instance containing the expected message, parameter name, and other properties.
    /// </param>
    /// <returns>
    /// The actual exception that was thrown, cast to <typeparamref name="TException"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs comprehensive exception validation using NUnit 4.x's multiple assertion feature:
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Catches Exception:</strong> Uses <c>Assert.Catch</c> to capture the thrown exception
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Validates Type:</strong> Uses <c>Assert.That(actual, Is.TypeOf(expected))</c> to ensure exact type match
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Validates Details:</strong> Uses <c>Assert.That(actual, Is.EqualTo(expected))</c> to compare exception properties
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Multiple Assertions:</strong>
    /// </para>
    /// <para>
    /// The method wraps validation in <see cref="AssertMultiple"/> to collect all failures:
    /// <list type="bullet">
    ///   <item><description>If type is wrong AND message is wrong, both failures are reported</description></item>
    ///   <item><description>Without <see cref="AssertMultiple"/>, only the first failure would be shown</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Template Method Pattern:</strong>
    /// </para>
    /// <para>
    /// This method delegates to the base class template method, injecting NUnit-specific implementations:
    /// <list type="bullet">
    ///   <item><description><c>catchException</c> = <c>Assert.Catch(() => att())</c></description></item>
    ///   <item><description><c>assertIsType</c> = <c>Assert.That(a, Is.TypeOf(e))</c></description></item>
    ///   <item><description><c>assertEquality</c> = <c>Assert.That(a, Is.EqualTo(e))</c></description></item>
    ///   <item><description><c>assertFail</c> = <c>Assert.Fail</c></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Basic Exception Validation</strong></para>
    /// <code>
    /// [Test]
    /// public void TestDivideByZero()
    /// {
    ///     var expected = new DivideByZeroException("Attempted to divide by zero.");
    ///     
    ///     var actual = ThrowsDetails(() =>
    ///     {
    ///         Calculator.Divide(10, 0);
    ///     }, expected);
    ///     
    ///     // Validates:
    ///     // 1. Exception was thrown (not null)
    ///     // 2. Exception type is DivideByZeroException
    ///     // 3. Exception message matches "Attempted to divide by zero."
    ///     
    ///     // Can perform additional validation on returned exception
    ///     Assert.That(actual.Source, Is.Not.Null);
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 2: ArgumentException with ParamName</strong></para>
    /// <code>
    /// [Test]
    /// public void TestInvalidArgument()
    /// {
    ///     var expected = new ArgumentException("Value cannot be negative", "amount");
    ///     
    ///     var actual = ThrowsDetails(() =>
    ///     {
    ///         BankAccount.Withdraw(-100);
    ///     }, expected);
    ///     
    ///     // Validates exception type, message, AND ParamName
    ///     Assert.That(actual.ParamName, Is.EqualTo("amount"));
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 3: Multiple Assertion Failures</strong></para>
    /// <code>
    /// [Test]
    /// public void TestWrongException()
    /// {
    ///     var expected = new InvalidOperationException("Wrong message");
    ///     
    ///     ThrowsDetails(() =>
    ///     {
    ///         throw new ArgumentException("Different message");
    ///     }, expected);
    ///     
    ///     // Test Output (with AssertMultiple):
    ///     //   Multiple assertion failures:
    ///     //   1. Expected type: InvalidOperationException, But was: ArgumentException
    ///     //   2. Expected message: "Wrong message", But was: "Different message"
    ///     //   (See BOTH failures, not just the first)
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultiple"/>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.ThrowsDetails{TException}(Action, TException, Func{Action, Exception}, Action{Type, Exception}, Action{Exception, Exception}, Action{string})"/>
    public static TException ThrowsDetails<TException>(Action attempt, TException expected)
    where TException : notnull, Exception
    {
        TException actual = default!;

        AssertMultiple(() =>
        {
            actual = ThrowsDetails(attempt, expected,
                catchException: att => Assert.Catch(() => att()),
                assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
                assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
                assertFail: Assert.Fail);
        });

        return actual;
    }
}