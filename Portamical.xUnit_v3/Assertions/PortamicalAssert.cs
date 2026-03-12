// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit_v3.Assertions;

/// <summary>
/// Provides xUnit v3-specific assertion methods that extend Portamical's framework-agnostic
/// assertion base class.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Design Pattern: Adapter + Template Method</strong>
/// </para>
/// <para>
/// This class adapts xUnit v3's assertion API to Portamical's framework-agnostic assertion
/// interface by injecting xUnit-specific implementations into base class template methods:
/// <list type="bullet">
///   <item><description>
///     <see cref="DoesNotThrow"/> - Injects <c>Assert.Fail</c> for failure reporting
///   </description></item>
///   <item><description>
///     <see cref="IsTypeOf"/> - Injects <c>Assert.Equal</c> for type equality
///   </description></item>
///   <item><description>
///     <see cref="ThrowsDetails{TException}"/> - Injects <c>Record.Exception</c>, <c>Assert.IsType</c>, <c>Assert.Equal</c>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>⚠️ xUnit v3 Limitation - No Multiple Assertions:</strong>
/// </para>
/// <para>
/// Like xUnit v2, xUnit v3 does <strong>not</strong> have a built-in multiple assertion feature
/// (though it's on the roadmap). The <see cref="AssertMultiple"/> and <see cref="AssertMultipleAsync"/>
/// methods are provided for API consistency with NUnit/MSTest but are implemented as no-ops
/// (they simply execute the assertions directly without collecting failures):
/// <list type="bullet">
///   <item><description>
///     <strong>NUnit 4.x:</strong> <c>AssertMultiple</c> collects all assertion failures and reports them together
///   </description></item>
///   <item><description>
///     <strong>xUnit v3:</strong> Test stops at the first assertion failure (standard xUnit behavior)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v3 vs xUnit v2 Assertion API:</strong>
/// </para>
/// <para>
/// xUnit v3 maintains backward compatibility with xUnit v2's assertion API while adding improvements:
/// <list type="bullet">
///   <item><description>
///     <strong>Same API:</strong> <c>Assert.Equal</c>, <c>Assert.IsType</c>, <c>Record.Exception</c> work identically
///   </description></item>
///   <item><description>
///     <strong>Better Messages:</strong> Improved assertion failure messages with more context
///   </description></item>
///   <item><description>
///     <strong>Async-First:</strong> Better async assertion support (though not used in this file)
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>xUnit v3 Assertion Style:</strong>
/// </para>
/// <para>
/// xUnit v3 (like v2) uses a different assertion style compared to NUnit:
/// <list type="bullet">
///   <item><description>
///     <strong>xUnit v3:</strong> Static methods like <c>Assert.Equal(expected, actual)</c>
///   </description></item>
///   <item><description>
///     <strong>NUnit:</strong> Constraint model like <c>Assert.That(actual, Is.EqualTo(expected))</c>
///   </description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Inheritance Chain:</strong>
/// </para>
/// <code>
/// Portamical.Assertions.PortamicalAssert (framework-agnostic base)
///   ↓ inherits
/// Portamical.xUnit_v3.Assertions.PortamicalAssert (xUnit v3-specific adapter) ← This class
///   ↓ used by
/// YourTestClass (concrete test fixture)
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Example 1: Multiple Assertions (xUnit v3 Limitation)</strong></para>
/// <code>
/// using Xunit;
/// using Portamical.xUnit_v3.Assertions;
/// 
/// public class PersonTests : PortamicalAssert
/// {
///     [Fact]
///     public void TestPerson()
///     {
///         var person = new Person("John", 30, "NYC");
///         
///         // ⚠️ xUnit v3 limitation: Stops at first failure (same as v2)
///         AssertMultiple(() =>
///         {
///             Assert.Equal("Jane", person.Name);  // ❌ FAILS, test stops here
///             Assert.Equal(25, person.Age);       // ⚠️ NEVER EXECUTED
///             Assert.Equal("LA", person.City);    // ⚠️ NEVER EXECUTED
///         });
///         
///         // Test report shows only first failure:
///         // Assert.Equal() Failure
///         // Expected: Jane
///         // Actual:   John
///     }
/// }
/// 
/// // Compare with NUnit 4.x (collects all failures):
/// // [Test]
/// // public void TestPerson()
/// // {
/// //     var person = new Person("John", 30, "NYC");
/// //     
/// //     AssertMultiple(() =>
/// //     {
/// //         Assert.That(person.Name, Is.EqualTo("Jane"));  // ❌ FAILS
/// //         Assert.That(person.Age, Is.EqualTo(25));       // ❌ FAILS
/// //         Assert.That(person.City, Is.EqualTo("LA"));    // ❌ FAILS
/// //     });
/// //     
/// //     // Test report shows ALL 3 failures
/// // }
/// </code>
/// 
/// <para><strong>Example 2: Multiple Assertions (Async)</strong></para>
/// <code>
/// [Fact]
/// public async Task TestPersonAsync()
/// {
///     var person = await GetPersonAsync();
///     
///     await AssertMultipleAsync(async () =>
///     {
///         var profile = await GetProfileAsync(person.Id);
///         Assert.NotNull(profile);
///         
///         var preferences = await GetPreferencesAsync(person.Id);
///         Assert.NotNull(preferences);  // ← Stops here if fails (xUnit v3 limitation)
///         
///         var history = await GetHistoryAsync(person.Id);
///         Assert.NotEmpty(history);
///     });
/// }
/// </code>
/// 
/// <para><strong>Example 3: DoesNotThrow</strong></para>
/// <code>
/// [Fact]
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
/// [Fact]
/// public void TestTypeMatch()
/// {
///     object value = "Hello";
///     
///     // Assert that runtime type matches expected type
///     IsTypeOf(typeof(string), value);
///     // If types don't match, shows: Expected: System.String, Actual: [actual type]
/// }
/// </code>
/// 
/// <para><strong>Example 5: ThrowsDetails (Complex Exception Validation)</strong></para>
/// <code>
/// [Fact]
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
///     // Validates:
///     // 1. Exception was thrown (not null)
///     // 2. Exception type matches (ArgumentException)
///     // 3. Exception message matches ("Value cannot be negative")
///     // 4. Exception ParamName matches ("amount")
///     
///     // Returns actual exception for further validation
///     Assert.Equal("amount", actual.ParamName);
/// }
/// </code>
/// </example>
/// <seealso cref="Portamical.Assertions.PortamicalAssert"/>
/// <seealso cref="Xunit.Assert"/>
/// <seealso cref="Xunit.Record"/>
public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    /// <summary>
    /// Executes multiple assertions. ⚠️ <strong>xUnit v3 Limitation:</strong> Stops at first failure (no-op wrapper).
    /// </summary>
    /// <param name="assertions">
    /// A delegate containing multiple assertions to execute.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>⚠️ xUnit v3 Limitation - No Multiple Assertion Collection:</strong>
    /// </para>
    /// <para>
    /// Like xUnit v2, xUnit v3 does <strong>not</strong> have a feature to collect multiple assertion failures
    /// (though it may be added in future versions). This method is implemented as a no-op (it simply executes
    /// the delegate directly) and is provided only for API consistency with NUnit and MSTest adapters.
    /// </para>
    /// <para>
    /// <strong>Behavior:</strong>
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     Test execution stops at the <strong>first assertion failure</strong>
    ///   </description></item>
    ///   <item><description>
    ///     Subsequent assertions are <strong>not executed</strong>
    ///   </description></item>
    ///   <item><description>
    ///     Only the <strong>first failure</strong> is reported
    ///   </description></item>
    /// </list>
    /// <para>
    /// <strong>Comparison with NUnit 4.x:</strong>
    /// </para>
    /// <code>
    /// // xUnit v3 (this method - stops at first failure):
    /// AssertMultiple(() =>
    /// {
    ///     Assert.Equal(expected1, actual1);  // ❌ FAILS, test stops
    ///     Assert.Equal(expected2, actual2);  // ⚠️ NEVER EXECUTED
    ///     Assert.Equal(expected3, actual3);  // ⚠️ NEVER EXECUTED
    /// });
    /// // Test Output: Only first failure shown
    /// 
    /// // NUnit 4.x (collects all failures):
    /// AssertMultiple(() =>
    /// {
    ///     Assert.That(actual1, Is.EqualTo(expected1));  // ❌ FAILS
    ///     Assert.That(actual2, Is.EqualTo(expected2));  // ❌ FAILS
    ///     Assert.That(actual3, Is.EqualTo(expected3));  // ❌ FAILS
    /// });
    /// // Test Output: ALL 3 failures shown together
    /// </code>
    /// <para>
    /// <strong>Why This Method Exists:</strong>
    /// </para>
    /// <para>
    /// This method exists for API consistency, allowing framework-agnostic test code to compile and run across
    /// NUnit, MSTest, and xUnit. However, the behavior differs between frameworks:
    /// <list type="bullet">
    ///   <item><description><strong>NUnit 4.x:</strong> Collects all failures</description></item>
    ///   <item><description><strong>MSTest:</strong> No-op (stops at first failure)</description></item>
    ///   <item><description><strong>xUnit v2:</strong> No-op (stops at first failure)</description></item>
    ///   <item><description><strong>xUnit v3:</strong> No-op (stops at first failure) - same as v2</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Implementation:</strong>
    /// </para>
    /// <code>
    /// public static void AssertMultiple(Action assertions)
    /// =&gt; assertions();  // ← Just execute - no special handling
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Fact]
    /// public void TestPerson_xUnit_StopsAtFirstFailure()
    /// {
    ///     var person = new Person("John", 30, "NYC");
    ///     
    ///     AssertMultiple(() =>
    ///     {
    ///         Assert.Equal("Jane", person.Name);  // ❌ FAILS HERE, test stops
    ///         Assert.Equal(25, person.Age);       // ⚠️ NEVER EXECUTED
    ///         Assert.Equal("LA", person.City);    // ⚠️ NEVER EXECUTED
    ///     });
    /// }
    /// // Test Output:
    /// //   Assert.Equal() Failure
    /// //   Expected: Jane
    /// //   Actual:   John
    /// //   (Only first failure shown)
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultipleAsync"/>
    public static void AssertMultiple(Action assertions)
    => assertions();

    /// <summary>
    /// Executes multiple asynchronous assertions. ⚠️ <strong>xUnit v3 Limitation:</strong> Stops at first failure (no-op wrapper).
    /// </summary>
    /// <param name="assertions">
    /// An asynchronous delegate containing multiple assertions to execute.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous assertion operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the asynchronous version of <see cref="AssertMultiple"/>. Like its synchronous counterpart,
    /// it is implemented as a no-op (it simply returns the <see cref="Task"/> directly) because xUnit v3 does not
    /// support collecting multiple assertion failures.
    /// </para>
    /// <para>
    /// <strong>Why No async/await?</strong>
    /// </para>
    /// <para>
    /// The method does not use <c>async/await</c> because:
    /// <list type="bullet">
    ///   <item><description>xUnit v3 has no multiple assertion scope to manage</description></item>
    ///   <item><description>Just returning the <see cref="Task"/> directly is more efficient (no state machine)</description></item>
    ///   <item><description>No <c>ConfigureAwait</c> needed (no await)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Fact]
    /// public async Task TestUserProfileAsync()
    /// {
    ///     var userId = 123;
    ///     
    ///     await AssertMultipleAsync(async () =>
    ///     {
    ///         var profile = await GetUserProfileAsync(userId);
    ///         Assert.NotNull(profile);  // ← Stops here if fails (xUnit v3 limitation)
    ///         
    ///         var preferences = await GetUserPreferencesAsync(userId);
    ///         Assert.NotNull(preferences);
    ///         
    ///         var history = await GetUserHistoryAsync(userId);
    ///         Assert.NotEmpty(history);
    ///     });
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="AssertMultiple"/>
    public static Task AssertMultipleAsync(Func<Task> assertions)
    => assertions();

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
    /// injecting xUnit's <c>Assert.Fail</c> for failure reporting.
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
    /// // Derived class injects xUnit implementation:
    /// public static void DoesNotThrow(Action attempt)
    /// =&gt; DoesNotThrow(attempt, assertFail: Assert.Fail);
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Fact]
    /// public void TestDivision_ValidInput()
    /// {
    ///     // Assert operation completes without exception
    ///     DoesNotThrow(() =>
    ///     {
    ///         var result = Calculator.Divide(10, 2);
    ///         Assert.Equal(5, result);
    ///     });
    /// }
    /// 
    /// [Fact]
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
    /// injecting xUnit's <c>Assert.Equal</c> for type comparison.
    /// </para>
    /// <para>
    /// <strong>xUnit v3 Assertion Style:</strong>
    /// </para>
    /// <para>
    /// xUnit v3 (like v2) uses static methods for assertions, unlike NUnit's constraint model:
    /// </para>
    /// <code>
    /// // xUnit v3 (used by this method):
    /// Assert.Equal(expectedType, actualType);
    /// // Error: Assert.Equal() Failure
    /// //        Expected: System.String
    /// //        Actual:   System.Int32
    /// 
    /// // NUnit (constraint model):
    /// Assert.That(actualType, Is.EqualTo(expectedType));
    /// // Error: Expected &lt;System.String&gt; but was &lt;System.Int32&gt;
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// [Fact]
    /// public void TestObjectType()
    /// {
    ///     object value = "Hello, World!";
    ///     
    ///     // Assert runtime type matches expected type
    ///     IsTypeOf(typeof(string), value);  // ✅ PASSES
    ///     
    ///     IsTypeOf(typeof(int), value);  // ❌ FAILS
    ///     // Error: Assert.Equal() Failure
    ///     //        Expected: System.Int32
    ///     //        Actual:   System.String
    /// }
    /// 
    /// [Fact]
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
        assertEquality: (e, a) => Assert.Equal(e, a));

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
    /// This method performs comprehensive exception validation using xUnit v3's exception recording and assertion APIs:
    /// <list type="number">
    ///   <item><description>
    ///     <strong>Records Exception:</strong> Uses <c>Record.Exception</c> to capture the thrown exception without rethrowing
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Validates Type:</strong> Uses <c>Assert.IsType</c> to ensure exact type match
    ///   </description></item>
    ///   <item><description>
    ///     <strong>Validates Details:</strong> Uses <c>Assert.Equal</c> to compare exception properties
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>xUnit v3's Record.Exception:</strong>
    /// </para>
    /// <para>
    /// xUnit v3 (like v2) provides <c>Record.Exception</c> for capturing exceptions without rethrowing them,
    /// which allows multiple assertions on the captured exception:
    /// </para>
    /// <code>
    /// Exception ex = Record.Exception(() => ThrowException());
    /// // If no exception: ex is null
    /// // If exception thrown: ex is the captured exception (not rethrown)
    /// </code>
    /// <para>
    /// <strong>Template Method Pattern:</strong>
    /// </para>
    /// <para>
    /// This method delegates to the base class template method, injecting xUnit-specific implementations:
    /// <list type="bullet">
    ///   <item><description><c>catchException</c> = <c>Record.Exception</c></description></item>
    ///   <item><description><c>assertIsType</c> = <c>Assert.IsType</c></description></item>
    ///   <item><description><c>assertEquality</c> = <c>Assert.Equal</c></description></item>
    ///   <item><description><c>assertFail</c> = <c>Assert.Fail</c></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>⚠️ No Multiple Assertion Collection:</strong>
    /// </para>
    /// <para>
    /// Unlike NUnit's version (which wraps in <c>AssertMultiple</c>), this method does not collect multiple
    /// failures because xUnit v3 lacks this feature. If both type and message are wrong, only the first failure
    /// is reported.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para><strong>Example 1: Basic Exception Validation</strong></para>
    /// <code>
    /// [Fact]
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
    ///     Assert.NotNull(actual.Source);
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 2: ArgumentException with ParamName</strong></para>
    /// <code>
    /// [Fact]
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
    ///     Assert.Equal("amount", actual.ParamName);
    /// }
    /// </code>
    /// 
    /// <para><strong>Example 3: xUnit v3 Limitation (Stops at First Failure)</strong></para>
    /// <code>
    /// [Fact]
    /// public void TestWrongException_xUnit_OnlyFirstFailure()
    /// {
    ///     var expected = new InvalidOperationException("Wrong message");
    ///     
    ///     ThrowsDetails(() =>
    ///     {
    ///         throw new ArgumentException("Different message");
    ///     }, expected);
    ///     
    ///     // Test Output (xUnit v3 - stops at first failure):
    ///     //   Assert.IsType() Failure
    ///     //   Expected: InvalidOperationException
    ///     //   Actual:   ArgumentException
    ///     //   (Message mismatch is NOT shown - test stopped at type check)
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Portamical.Assertions.PortamicalAssert.ThrowsDetails{TException}(Action, TException, Func{Action, Exception}, Action{Type, Exception}, Action{Exception, Exception}, Action{string})"/>
    /// <seealso cref="Xunit.Record"/>
    public static TException ThrowsDetails<TException>(Action attempt, TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(attempt, expected,
        catchException: Record.Exception,
        assertIsType: Assert.IsType,
        assertEquality: Assert.Equal,
        assertFail: Assert.Fail);
}