// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Assertions;

/// <summary>
/// Provides framework-agnostic assertion helper methods for unit testing with async-first architecture.
/// </summary>
/// <remarks>
/// <para>
/// This abstract base class defines reusable assertion logic that can be adapted to any testing
/// framework (MSTest, NUnit, xUnit, TUnit, etc.) by passing framework-specific assertion delegates as parameters.
/// </para>
/// <para>
/// <strong>Architecture: Async-First with Sync Wrappers</strong>
/// </para>
/// <para>
/// Starting with version 2.0, this class follows modern .NET async-first design:
/// </para>
/// <list type="bullet">
///   <item>
///     <strong>Primary Implementation:</strong> All assertion logic is implemented in async methods
///     returning <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>.
///   </item>
///   <item>
///     <strong>Sync Wrappers:</strong> Synchronous methods are thin wrappers that delegate to async
///     implementations using <c>ConfigureAwait(false).GetAwaiter().GetResult()</c>.
///   </item>
///   <item>
///     <strong>Zero Allocation:</strong> Async methods complete synchronously when possible, using
///     <c>default(ValueTask)</c> for zero allocation.
///   </item>
///   <item>
///     <strong>Future-Proof:</strong> Ready for true async operations (I/O, cancellation, telemetry)
///     without breaking changes.
///   </item>
/// </list>
/// <para>
/// <strong>Design Pattern: Dependency Injection for Assertions</strong>
/// </para>
/// <para>
/// Rather than directly coupling to a specific testing framework, this class accepts assertion
/// delegates (e.g., <c>Func&lt;string, ValueTask&gt; assertFailAsync</c>) that encapsulate
/// framework-specific assertion behavior. This enables:
/// </para>
/// <list type="bullet">
///   <item>
///     <strong>Framework Independence:</strong> Core assertion logic works with MSTest, NUnit, xUnit,
///     TUnit, or custom test frameworks without modification.
///   </item>
///   <item>
///     <strong>Extension Projects:</strong> Framework-specific projects derive from this class to
///     provide convenience methods that pre-configure the assertion delegates.
///   </item>
///   <item>
///     <strong>Testability:</strong> Assertion behavior itself can be tested by providing mock delegates.
///   </item>
/// </list>
/// <para>
/// <strong>Usage Patterns:</strong>
/// </para>
/// <list type="number">
///   <item>
///     <strong>Async Frameworks (TUnit, Modern MSTest):</strong> Use async methods directly.
///   </item>
///   <item>
///     <strong>Sync Frameworks (NUnit, xUnit, Classic MSTest):</strong> Use sync wrappers.
///   </item>
///   <item>
///     <strong>Via Extension Projects:</strong> Use framework-specific derived classes with simplified APIs.
///   </item>
/// </list>
/// </remarks>
/// <example>
/// <para><strong>Async Usage (TUnit):</strong></para>
/// <code>
/// // Portamical.TUnit extension:
/// public static ValueTask DoesNotThrow(Action attempt)
/// => DoesNotThrowAsync(attempt, FailAsync);
/// 
/// // Test usage:
/// [Test]
/// public async Task TestOperation()
/// {
///     await PortamicalAssert.DoesNotThrow(() => myService.DoWork());
/// }
/// </code>
/// 
/// <para><strong>Sync Usage (NUnit):</strong></para>
/// <code>
/// // Portamical.NUnit extension:
/// public static void DoesNotThrow(Action attempt)
/// => DoesNotThrow(attempt, Assert.Fail);
/// 
/// // Test usage (sync):
/// [Test]
/// public void TestOperation()
/// {
///     PortamicalAssert.DoesNotThrow(() => myService.DoWork());
/// }
/// </code>
/// </example>
public abstract class PortamicalAssert
{
    #region Constructor

    /// <summary>
    /// Prevents external instantiation while allowing derived classes in extension scenarios.
    /// </summary>
    protected PortamicalAssert()
    {
    }

    #endregion

    #region Primary Implementation (Async)

    /// <summary>
    /// Verifies that the specified asynchronous action does not throw an exception.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a PRIMARY implementation</strong> for async actions. For synchronous actions,
    /// use the overload <see cref="DoesNotThrowAsync(Action, Func{string, ValueTask})"/>.
    /// </para>
    /// <para>
    /// <strong>Architecture:</strong> Uses <see cref="CatchExceptionAsync(Func{Task})"/> to execute
    /// the async action and capture any non-fatal exceptions. Fatal exceptions propagate immediately.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Completes synchronously with zero allocation when no exception
    /// is thrown (uses <c>default(ValueTask)</c>).
    /// </para>
    /// </remarks>
    /// <param name="attempt">
    /// The async action to execute and verify for the absence of exceptions. Cannot be null.
    /// </param>
    /// <param name="assertFailAsync">
    /// A callback to invoke with an error message if a non-fatal exception is thrown. Cannot be null.
    /// The callback should throw an assertion exception or complete the returned <see cref="ValueTask"/>.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the async assertion operation.</returns>
    /// <exception cref="StackOverflowException">Propagated if thrown (fatal).</exception>
    /// <exception cref="OutOfMemoryException">Propagated if thrown (fatal).</exception>
    /// <exception cref="AccessViolationException">Propagated if thrown (fatal).</exception>
    /// <exception cref="ThreadAbortException">Propagated if thrown (fatal).</exception>
    /// <example>
    /// <code>
    /// // TUnit async usage:
    /// await DoesNotThrowAsync(
    ///     async () => await myService.ProcessAsync(),
    ///     msg => throw new AssertionException(msg));
    /// 
    /// // Success path (no exception)
    /// await DoesNotThrowAsync(
    ///     async () => await Task.Delay(10),
    ///     msg => throw new AssertionException(msg));
    /// // ✅ Passes (no exception thrown)
    /// 
    /// // Failure path (non-fatal exception)
    /// await DoesNotThrowAsync(
    ///     async () => throw new InvalidOperationException("error"),
    ///     msg => throw new AssertionException(msg));
    /// // ❌ Fails with assertion message
    /// 
    /// // Fatal exception propagates (not caught)
    /// try
    /// {
    ///     await DoesNotThrowAsync(
    ///         async () => throw new StackOverflowException(),
    ///         msg => throw new AssertionException(msg));
    /// }
    /// catch (StackOverflowException)
    /// {
    ///     // Fatal exception bypassed assertion logic
    /// }
    /// </code>
    /// </example>
    public static async ValueTask DoesNotThrowAsync(
        Func<Task> attempt,
        Func<string, ValueTask> assertFailAsync)
    {
        _ = NotNull(attempt, nameof(attempt));
        _ = NotNull(assertFailAsync, nameof(assertFailAsync));

        var exception = await CatchExceptionAsync(attempt)
            .ConfigureAwait(false);

        if (exception is not null)
        {
            await assertFailAsync(
                GetNotExpectedExceptionMessage(exception))
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes the specified async function and verifies that it throws an exception of the expected type
    /// with matching details (async version for async test methods).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The <see cref="Action"/> overload and the sync version
    /// <see cref="ThrowsDetails{TException}(Action, TException, Func{Action, TException}, Action{Type, object}, Action{string, string?})"/>
    /// both delegate to this method.
    /// </para>
    /// <para>
    /// Use this overload when testing async methods that need to be awaited within the exception assertion.
    /// </para>
    /// <para>
    /// <strong>Design:</strong> This method delegates exception capture to <paramref name="assertThrowsAnyAsync"/>,
    /// which is expected to be a test framework's assertion method (e.g., <c>Assert.ThrowsExactlyAsync</c>).
    /// The assertion delegate should throw an assertion failure if no exception or wrong exception type is thrown.
    /// </para>
    /// </remarks>
    /// <typeparam name="TException">
    /// The type of exception expected to be thrown. Must be a non-null reference type derived from Exception.
    /// </typeparam>
    /// <param name="attempt">The async function to execute, which is expected to throw an exception.</param>
    /// <param name="expected">The expected exception instance, used as a reference for type and detail comparisons.</param>
    /// <param name="assertThrowsAnyAsync">
    /// A delegate that executes the async action and returns the exception thrown. Typically a test framework's
    /// assertion method like <c>Assert.ThrowsExactlyAsync&lt;TException&gt;</c>. Should throw an assertion failure
    /// if no exception or wrong exception type is thrown. Returns a <see cref="ValueTask{TException}"/>.
    /// </param>
    /// <param name="assertIsTypeAsync">
    /// A delegate that asserts the actual exception is of the expected type. Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <param name="assertEqualityAsync">
    /// A delegate that asserts equality between expected and actual values (for exception metadata comparison).
    /// Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the actual exception that was thrown and verified.
    /// </returns>
    public static async ValueTask<TException> ThrowsDetailsAsync<TException>(
        Func<Task> attempt,
        TException expected,
        Func<Func<Task>, ValueTask<Exception>> assertThrowsAnyAsync,
        Func<Type, object, ValueTask> assertIsTypeAsync,
        Func<string, string?, ValueTask> assertEqualityAsync)
    where TException : notnull, Exception
    {
        _ = NotNull(attempt, nameof(attempt));
        _ = NotNull(assertThrowsAnyAsync, nameof(assertThrowsAnyAsync));
        _ = NotNull(assertIsTypeAsync, nameof(assertIsTypeAsync));
        _ = NotNull(assertEqualityAsync, nameof(assertEqualityAsync));

        var actual = await assertThrowsAnyAsync(attempt)
            .ConfigureAwait(false);

        var expectedType = expected.GetType();

        // Type assertion - delegate to the injected assertion callback
        await assertIsTypeAsync(expectedType, actual)
            .ConfigureAwait(false);

        // Metadata equality
        await MetadataEqualityAsync(expected, actual, assertEqualityAsync)
            .ConfigureAwait(false);

        return (TException)actual;
    }

    /// <summary>
    /// Verifies value equality by delegating to caller-provided async callbacks (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The sync version
    /// <see cref="Equality{T}(T, T, Func{T, T, bool}, Action{string}, string)"/>
    /// delegates to this method.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The value type being compared.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="equals">A delegate that determines whether values are equal.</param>
    /// <param name="assertFailAsync">A delegate invoked when the values are not equal.</param>
    /// <param name="message">The failure message to pass to <paramref name="assertFailAsync"/>.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async assertion operation.</returns>
    public static ValueTask EqualityAsync<T>(
        T? expected,
        T? actual,
        Func<T?, T?, bool> equals,
        Func<string?, ValueTask> assertFailAsync,
        string? message)
    {
        _ = NotNull(equals, nameof(equals));
        _ = NotNull(assertFailAsync, nameof(assertFailAsync));

        if (equals(expected, actual))
        {
            return default;
        }

        return assertFailAsync(message);
    }

    /// <summary>
    /// Verifies value equality for common primitive and framework types (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The sync version
    /// <see cref="Equality(object, object, Action, double?)"/> delegates to this method.
    /// </para>
    /// </remarks>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="assertFailAsync">A delegate invoked when the values are not equal.</param>
    /// <param name="floatingPointTolerance">
    /// Epsilon for floating-point comparisons. Default: 1e-10 for double, 1e-6f for float.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the async assertion operation.</returns>
    public static ValueTask EqualityAsync(
        object expected,
        object? actual,
        Func<ValueTask> assertFailAsync,
        double? floatingPointTolerance = null)
    {
        _ = NotNull(assertFailAsync, nameof(assertFailAsync));

        if (AreEqual(expected, actual, floatingPointTolerance))
        {
            return default;
        }

        return assertFailAsync();
    }

    /// <summary>
    /// Verifies that the runtime type matches the expected type (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The sync version
    /// <see cref="IsTypeOf(Type, object, Action{Type, Type})"/> delegates to this method.
    /// </para>
    /// </remarks>
    /// <param name="expected">
    /// The expected type. Cannot be null.
    /// </param>
    /// <param name="actual">
    /// The actual object whose type is being verified. May be null, in which case <see langword="null"/>
    /// is passed to <paramref name="assertEqualityAsync"/> as the actual type.
    /// </param>
    /// <param name="assertEqualityAsync">
    /// A delegate that asserts type equality. The first parameter is the expected type, the second is the
    /// actual type (may be null if <paramref name="actual"/> is null). Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the async assertion operation.</returns>
    public static ValueTask IsTypeOfAsync(
        Type expected,
        object? actual,
        Func<Type, Type?, ValueTask> assertEqualityAsync)
    {
        _ = NotNull(assertEqualityAsync, nameof(assertEqualityAsync));

        return assertEqualityAsync(
            NotNull(expected, nameof(expected)),
            actual?.GetType());
    }

    /// <summary>
    /// Verifies that exception metadata (message and parameter name) matches expected values (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The sync version
    /// <see cref="MetadataEquality{TException}(TException, TException, Action{string, string?})"/>
    /// delegates to this method.
    /// </para>
    /// <para>
    /// <strong>Selective Assertion Logic:</strong> This method intelligently handles framework-generated
    /// exception messages that may vary across runtime versions or locales:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <strong>ArgumentException:</strong> Skips message assertion for guard clauses like "The value cannot
    ///     be an empty string" or parameter-specific patterns like "'paramName' ('value'...)" which are
    ///     generated by framework validation.
    ///   </item>
    ///   <item>
    ///     <strong>ObjectDisposedException:</strong> Skips message assertion for framework-generated patterns
    ///     like "Cannot access a disposed object.\nObject name: 'objectName'" which vary by runtime.
    ///   </item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="ValueTask"/> to minimize allocations. Completes
    /// synchronously when assertion delegates complete synchronously.
    /// </para>
    /// </remarks>
    /// <typeparam name="TException">
    /// The type of exception being verified. Must be a non-null reference type derived from <see cref="Exception"/>.
    /// </typeparam>
    /// <param name="expected">
    /// The expected exception instance containing the metadata to verify against. Cannot be null.
    /// </param>
    /// <param name="actual">
    /// The actual exception instance whose metadata is being verified. Cannot be null.
    /// </param>
    /// <param name="assertEqualityAsync">
    /// A delegate that asserts equality between expected and actual string values. The first parameter
    /// is the expected value, the second is the actual value (may be null). Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the async assertion operation.</returns>
    /// <example>
    /// <code>
    /// // TUnit async usage:
    /// var expected = new ArgumentException("Invalid value", "paramName");
    /// var actual = new ArgumentException("Invalid value", "paramName");
    /// 
    /// await MetadataEqualityAsync(
    ///     expected,
    ///     actual,
    ///     async (exp, act) => await Assert.Equal(exp, act));
    /// 
    /// // ObjectDisposedException handling:
    /// var expectedDisposed = new ObjectDisposedException("stream");
    /// var actualDisposed = new ObjectDisposedException("stream");
    /// 
    /// await MetadataEqualityAsync(
    ///     expectedDisposed,
    ///     actualDisposed,
    ///     async (exp, act) => await Assert.Equal(exp, act));
    /// // ✅ Passes even if framework-generated messages differ
    /// </code>
    /// </example>
    public static async ValueTask MetadataEqualityAsync<TException>(
        TException expected,
        TException actual,
        Func<string, string?, ValueTask> assertEqualityAsync)
    where TException : notnull, Exception
    {
        _ = NotNull(assertEqualityAsync, nameof(assertEqualityAsync));

        const string ArgumentExceptionGuardMessageStart = "The value cannot be an empty string";
        const string ObjectDisposedExceptionGuardMessageStart = "Cannot access a disposed object.\nObject name: '";

        var expectedMessage = expected.Message;
        var actualMessage = actual.Message;

        if (expected is ArgumentException argExpected && actual is ArgumentException argActual)
        {
            var actualParamName = argActual.ParamName;
            bool shouldAssertMessage =
                actualMessageDoesNotStartWith(ArgumentExceptionGuardMessageStart) &&
                actualMessageDoesNotStartWith(getArgumentOutOfRangeExceptionGuardMessageStart());

            if (shouldAssertMessage && expectedMessage is not null)
            {
                await assertEqualityAsync(expectedMessage, actualMessage)
                    .ConfigureAwait(false);
            }

            if (argExpected.ParamName is string expectedParamName)
            {
                await assertEqualityAsync(expectedParamName, actualParamName)
                    .ConfigureAwait(false);
            }

            #region Local methods
            string getArgumentOutOfRangeExceptionGuardMessageStart()
            => $"'{actualParamName}' ('";
            #endregion
        }
        else if (expectedMessage is not null)
        {
            bool shouldAssertMessage =
                expected is not ObjectDisposedException ||
                actualMessageDoesNotStartWith(ObjectDisposedExceptionGuardMessageStart);

            if (shouldAssertMessage)
            {
                await assertEqualityAsync(expectedMessage, actualMessage)
                    .ConfigureAwait(false);
            }
        }

        #region Local methods
        bool actualMessageDoesNotStartWith(string guardMessageStart)
        => !actualMessage.StartsWith(guardMessageStart);
        #endregion
    }

    public static async ValueTask<Exception> ThrowsAnyAsync(
        Func<Task> attempt,
        Func<string, ValueTask> assertFailAsync)
    {
        var exception = await CatchExceptionAsync(attempt)
            .ConfigureAwait(false);

        if (exception is not null)
        {
            return exception;
        }

        await assertFailAsync(ExpectedExceptionMessageStart + NoExceptionThrownMessageEnd)
            .ConfigureAwait(false);

        throw GetAssertionFailedException(AssertFailDelegateFailedMessage);
    }

    #endregion

    #region Convenience Wrappers (Sync delegates to Async)

    /// <summary>
    /// Verifies that the specified action does not throw an exception (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="DoesNotThrowAsync(Func{Task}, Func{string, ValueTask})"/> via
    /// <see cref="ThreadSafeSync(ValueTask)"/>.
    /// </para>
    /// <para>
    /// Safe for use in test contexts where <see cref="SynchronizationContext"/> is typically
    /// not present (NUnit, xUnit, MSTest). Uses <c>ConfigureAwait(false)</c> internally to
    /// prevent potential deadlocks.
    /// </para>
    /// </remarks>
    /// <param name="attempt">
    /// The action to execute and verify for the absence of exceptions. Cannot be null.
    /// </param>
    /// <param name="assertFail">
    /// A callback to invoke with an error message if the action throws an exception. Cannot be null.
    /// </param>
    /// <example>
    /// <code>
    /// // NUnit usage:
    /// DoesNotThrow(
    ///     () => myService.DoWork(),
    ///     Assert.Fail);
    /// </code>
    /// </example>
    public static void DoesNotThrow(Action attempt, Action<string> assertFail)
    {
        _ = NotNull(attempt, nameof(attempt));
        _ = NotNull(assertFail, nameof(assertFail));

        ThreadSafeSync(DoesNotThrowAsync(() =>
        {
            attempt();
            return Task.CompletedTask;
        },
        msg =>
        {
            assertFail(msg);
            return new ValueTask();
        }));
    }

    /// <summary>
    /// Verifies that the specified action throws any exception (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="ThrowsAnyAsync(Func{Task}, Func{string, ValueTask})"/> via
    /// <see cref="ThreadSafeSync{T}(ValueTask{T})"/>.
    /// </para>
    /// <para>
    /// Safe for use in test contexts where <see cref="SynchronizationContext"/> is typically
    /// not present (NUnit, xUnit, MSTest). Uses <c>ConfigureAwait(false)</c> internally to
    /// prevent potential deadlocks.
    /// </para>
    /// <para>
    /// <strong>Design:</strong> This method captures any non-fatal exception thrown by the action.
    /// If no exception is thrown, it invokes <paramref name="assertFail"/> to fail the test.
    /// Fatal exceptions (<see cref="StackOverflowException"/>, <see cref="OutOfMemoryException"/>,
    /// <see cref="AccessViolationException"/>, <see cref="ThreadAbortException"/>) are not caught
    /// and will propagate immediately.
    /// </para>
    /// </remarks>
    /// <param name="attempt">
    /// The action to execute, which is expected to throw an exception. Cannot be null.
    /// </param>
    /// <param name="assertFail">
    /// A callback to invoke with an error message if no exception is thrown. Cannot be null.
    /// The callback should throw an assertion exception to fail the test.
    /// </param>
    /// <returns>
    /// The non-fatal <see cref="Exception"/> that was thrown by <paramref name="attempt"/>.
    /// </returns>
    /// <exception cref="StackOverflowException">Propagated if thrown (fatal).</exception>
    /// <exception cref="OutOfMemoryException">Propagated if thrown (fatal).</exception>
    /// <exception cref="AccessViolationException">Propagated if thrown (fatal).</exception>
    /// <exception cref="ThreadAbortException">Propagated if thrown (fatal).</exception>
    /// <example>
    /// <code>
    /// // NUnit usage:
    /// var ex = ThrowsAny(
    ///     () => myService.Process(null),
    ///     Assert.Fail);
    /// 
    /// Assert.IsInstanceOf&lt;ArgumentNullException&gt;(ex);
    /// 
    /// // Success path (exception thrown)
    /// var thrown = ThrowsAny(
    ///     () => throw new InvalidOperationException("error"),
    ///     Assert.Fail);
    /// // ✅ Returns the InvalidOperationException
    /// 
    /// // Failure path (no exception)
    /// ThrowsAny(
    ///     () => Console.WriteLine("no error"),
    ///     Assert.Fail);
    /// // ❌ Calls Assert.Fail with message
    /// </code>
    /// </example>
    public static Exception ThrowsAny(
        Action attempt,
        Action<string> assertFail)
    {
        _ = NotNull(attempt, nameof(attempt));
        _ = NotNull(assertFail, nameof(assertFail));

        return ThreadSafeSync(ThrowsAnyAsync(() =>
        {
            attempt();
            return Task.CompletedTask;
        },
        msg =>
        {
            assertFail(msg);
            return new ValueTask();
        }));
    }

    /// <summary>
    /// Executes the specified action and verifies that it throws an exception of the expected type
    /// with matching details (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="ThrowsDetailsAsync{TException}(Func{Task}, TException, Func{Func{Task}, ValueTask{TException}}, Func{Type, object, ValueTask}, Func{string, string?, ValueTask})"/>
    /// via <see cref="ThreadSafeSync{T}(ValueTask{T})"/>.
    /// </para>
    /// <para>
    /// Safe for use in test contexts where <see cref="SynchronizationContext"/> is typically
    /// not present (NUnit, xUnit, MSTest). Uses <c>ConfigureAwait(false)</c> internally to
    /// prevent potential deadlocks.
    /// </para>
    /// <para>
    /// <strong>Type Checking:</strong> Verifies exact type equality. Derived types are not accepted.
    /// </para>
    /// <para>
    /// <strong>Design:</strong> This method delegates exception capture to <paramref name="assertThrowsAny"/>,
    /// which is expected to be a test framework's assertion method (e.g., <c>Assert.ThrowsExactly</c>).
    /// The assertion delegate should throw an assertion failure if no exception or wrong exception type is thrown.
    /// </para>
    /// </remarks>
    /// <typeparam name="TException">
    /// The type of exception expected to be thrown. Must be a non-null reference type derived from Exception.
    /// </typeparam>
    /// <param name="attempt">
    /// The action to execute, which is expected to throw an exception. Cannot be null.
    /// </param>
    /// <param name="expected">
    /// The expected exception instance, used as a reference for type and detail comparisons. Cannot be null.
    /// </param>
    /// <param name="assertThrowsAny">
    /// A delegate that executes the action and returns the exception thrown. Typically a test framework's
    /// assertion method like <c>Assert.ThrowsExactly&lt;TException&gt;</c>. Should throw an assertion failure
    /// if no exception or wrong exception type is thrown. Cannot be null.
    /// </param>
    /// <param name="assertIsType">
    /// A delegate that asserts the actual exception is of the expected type. Cannot be null.
    /// </param>
    /// <param name="assertEquality">
    /// A delegate that asserts equality between expected and actual string values (for exception metadata comparison).
    /// Cannot be null.
    /// </param>
    /// <returns>
    /// The actual exception that was thrown and verified.
    /// </returns>
    /// <exception cref="StackOverflowException">Propagated if thrown (fatal).</exception>
    /// <exception cref="OutOfMemoryException">Propagated if thrown (fatal).</exception>
    /// <exception cref="AccessViolationException">Propagated if thrown (fatal).</exception>
    /// <exception cref="ThreadAbortException">Propagated if thrown (fatal).</exception>
    /// <example>
    /// <code>
    /// // MSTest usage:
    /// var expected = new ArgumentNullException("paramName");
    /// var actual = ThrowsDetails(
    ///     () => myService.Process(null),
    ///     expected,
    ///     attempt => Assert.ThrowsExactly&lt;ArgumentNullException&gt;(attempt),
    ///     (expectedType, actualEx) => Assert.IsInstanceOfType(actualEx, expectedType),
    ///     (expectedStr, actualStr) => Assert.AreEqual(expectedStr, actualStr));
    /// 
    /// Assert.AreEqual("paramName", actual.ParamName);
    /// </code>
    /// </example>
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected,
        Func<Action, Exception> assertThrowsAny,
        Action<Type, object> assertIsType,
        Action<string, string?> assertEquality)
    where TException : notnull, Exception
    {
        _ = NotNull(attempt, nameof(attempt));
        _ = NotNull(assertThrowsAny, nameof(assertThrowsAny));
        _ = NotNull(assertIsType, nameof(assertIsType));
        _ = NotNull(assertEquality, nameof(assertEquality));

        return ThreadSafeSync(ThrowsDetailsAsync(
            attempt: () =>
            {
                attempt();
                return Task.CompletedTask;
            },
            expected: expected,
            assertThrowsAnyAsync: attemptAsync => new ValueTask<Exception>(
                assertThrowsAny(() => ThreadSafeSync(attemptAsync))),
            assertIsTypeAsync: (e, a) =>
            {
                assertIsType(e, a);
                return new ValueTask();
            },
            assertEqualityAsync: (e, a) =>
            {
                assertEquality(e, a);
                return new ValueTask();
            }));
    }

    /// <summary>
    /// Verifies value equality by delegating to caller-provided callbacks (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="EqualityAsync{T}(T, T, Func{T, T, bool}, Func{string, ValueTask}, string)"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The value type being compared.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="equals">A delegate that determines whether values are equal.</param>
    /// <param name="assertFail">A delegate invoked when the values are not equal.</param>
    /// <param name="message">The failure message to pass to <paramref name="assertFail"/>.</param>
    public static void Equality<T>(
        T? expected,
        T? actual,
        Func<T?, T?, bool> equals,
        Action<string?> assertFail,
        string? message)
    {
        _ = NotNull(assertFail, nameof(assertFail));

        ThreadSafeSync(EqualityAsync(
            expected,
            actual,
            equals,
            msg =>
            {
                assertFail(msg);
                return new ValueTask();
            },
            message));
    }

    /// <summary>
    /// Verifies value equality for common primitive and framework types (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="EqualityAsync(object, object, Func{ValueTask}, double?)"/>.
    /// </para>
    /// </remarks>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="assertFail">A delegate invoked when the values are not equal.</param>
    /// <param name="floatingPointTolerance">
    /// Epsilon for floating-point comparisons. Default: 1e-10 for double, 1e-6f for float.
    /// </param>
    public static void Equality(
        object expected,
        object? actual,
        Action assertFail,
        double? floatingPointTolerance = null)
    {
        _ = NotNull(assertFail, nameof(assertFail));

        ThreadSafeSync(EqualityAsync(
            expected,
            actual,
            () =>
            {
                assertFail();
                return new ValueTask();
            },
            floatingPointTolerance));
    }

    /// <summary>
    /// Verifies that the runtime type matches the expected type (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="IsTypeOfAsync(Type, object, Func{Type, Type, ValueTask})"/>.
    /// </para>
    /// </remarks>
    /// <param name="expected">The expected type. Cannot be null.</param>
    /// <param name="actual">
    /// The actual object whose type is being verified. May be null, in which case <see langword="null"/>
    /// is passed to <paramref name="assertEquality"/> as the actual type.
    /// </param>
    /// <param name="assertEquality">
    /// A delegate that asserts type equality. The first parameter is the expected type, the second is the
    /// actual type (may be null if <paramref name="actual"/> is null).
    /// </param>
    public static void IsTypeOf(
        Type expected,
        object? actual,
        Action<Type, Type?> assertEquality)
    {
        _ = NotNull(assertEquality, nameof(assertEquality));

        ThreadSafeSync(IsTypeOfAsync(
            expected,
            actual,
            (e, a) =>
            {
                assertEquality(e, a);
                return new ValueTask();
            }));
    }

    /// <summary>
    /// Verifies that exception metadata (message and parameter name) matches expected values (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="MetadataEqualityAsync{TException}(TException, TException, Func{string, string?, ValueTask})"/>
    /// via <see cref="ThreadSafeSync(ValueTask)"/>.
    /// </para>
    /// <para>
    /// Safe for use in test contexts where <see cref="SynchronizationContext"/> is typically
    /// not present (NUnit, xUnit, MSTest). Uses <c>ConfigureAwait(false)</c> internally to
    /// prevent potential deadlocks.
    /// </para>
    /// <para>
    /// For details on the selective assertion logic for framework-generated messages, see
    /// <see cref="MetadataEqualityAsync{TException}(TException, TException, Func{string, string?, ValueTask})"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TException">
    /// The type of exception being verified. Must be a non-null reference type derived from <see cref="Exception"/>.
    /// </typeparam>
    /// <param name="expected">
    /// The expected exception instance containing the metadata to verify against. Cannot be null.
    /// </param>
    /// <param name="actual">
    /// The actual exception instance whose metadata is being verified. Cannot be null.
    /// </param>
    /// <param name="assertEquality">
    /// A delegate that asserts equality between expected and actual string values. The first parameter
    /// is the expected value, the second is the actual value (may be null).
    /// </param>
    /// <example>
    /// <code>
    /// // NUnit sync usage:
    /// var expected = new ArgumentException("Invalid value", "paramName");
    /// var actual = new ArgumentException("Invalid value", "paramName");
    /// 
    /// MetadataEquality(
    ///     expected,
    ///     actual,
    ///     (exp, act) => Assert.AreEqual(exp, act));
    /// 
    /// // Handling ArgumentOutOfRangeException:
    /// var expectedRange = new ArgumentOutOfRangeException("count", "Count must be positive");
    /// var actualRange = new ArgumentOutOfRangeException("count", "Count must be positive");
    /// 
    /// MetadataEquality(
    ///     expectedRange,
    ///     actualRange,
    ///     (exp, act) => Assert.AreEqual(exp, act));
    /// // ✅ Passes - parameter name and custom message verified
    /// </code>
    /// </example>
    public static void MetadataEquality<TException>(
        TException expected,
        TException actual,
        Action<string, string?> assertEquality)
    where TException : notnull, Exception
    {
        _ = NotNull(assertEquality, nameof(assertEquality));

        ThreadSafeSync(MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                assertEquality(e, a);
                return new ValueTask();
            }));
    }

    #endregion

    #region Helper methods

    #region Shared Helper Methods

    /// <summary>
    /// Asynchronously executes an action and catches any non-fatal exception that occurs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the async counterpart to <see cref="CatchException(Action)"/>. It executes an async
    /// action and returns any non-fatal exception that occurs during execution, or <see langword="null"/>
    /// if the action completes successfully.
    /// </para>
    /// <para>
    /// <strong>Fatal Exception Handling:</strong> This method uses <see cref="IsNotFatal(Exception)"/>
    /// to filter exceptions. Fatal exceptions (<see cref="OutOfMemoryException"/>,
    /// <see cref="AccessViolationException"/>, <see cref="StackOverflowException"/>,
    /// <see cref="ThreadAbortException"/>) are not caught and will propagate to terminate the process.
    /// This is critical for process safety - catching fatal exceptions can lead to undefined behavior.
    /// </para>
    /// <para>
    /// <strong>Design Pattern:</strong> Exception handler that converts non-fatal exceptions to return
    /// values, enabling functional error handling without <c>try/catch</c> at call sites. Fatal exceptions
    /// bypass this pattern and propagate immediately.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Zero allocations when <paramref name="attempt"/> completes
    /// successfully. Non-fatal exception path allocates only the exception object itself (unavoidable).
    /// Uses <see cref="ValueTask{TResult}"/> to minimize allocations when used in hot paths.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe as it maintains no shared state.
    /// Multiple concurrent invocations are safe. However, the caller is responsible for ensuring
    /// <paramref name="attempt"/> is safe to execute concurrently if needed.
    /// </para>
    /// </remarks>
    /// <param name="attempt">
    /// The async action to execute. Must not be <see langword="null"/>. If the action throws a
    /// non-fatal exception, it will be caught and returned. Fatal exceptions propagate.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that resolves to:
    /// <list type="bullet">
    ///   <item>
    ///     <see langword="null"/> if <paramref name="attempt"/> completes successfully without
    ///     throwing an exception.
    ///   </item>
    ///   <item>
    ///     The non-fatal <see cref="Exception"/> that was thrown by <paramref name="attempt"/> if
    ///     execution failed with a non-fatal exception.
    ///   </item>
    /// </list>
    /// </returns>
    /// <exception cref="StackOverflowException">Propagated if thrown (fatal).</exception>
    /// <exception cref="OutOfMemoryException">Propagated if thrown (fatal).</exception>
    /// <exception cref="AccessViolationException">Propagated if thrown (fatal).</exception>
    /// <exception cref="ThreadAbortException">Propagated if thrown (fatal).</exception>
    /// <example>
    /// <para><strong>Non-Fatal Exception (Caught and Returned):</strong></para>
    /// <code>
    /// var exception = await CatchExceptionAsync(async () => 
    /// {
    ///     await Task.Delay(10);
    ///     throw new InvalidOperationException("Invalid state");
    /// });
    /// 
    /// Assert.IsNotNull(exception);
    /// Assert.IsInstanceOfType(exception, typeof(InvalidOperationException));
    /// </code>
    /// 
    /// <para><strong>Fatal Exception (Propagates):</strong></para>
    /// <code>
    /// try
    /// {
    ///     var exception = await CatchExceptionAsync(async () => 
    ///     {
    ///         await Task.Delay(10);
    ///         throw new StackOverflowException();
    ///     });
    ///     // This line never executes - StackOverflowException propagates
    /// }
    /// catch (StackOverflowException)
    /// {
    ///     // Fatal exception caught at outer level
    ///     // Process should terminate
    /// }
    /// </code>
    /// 
    /// <para><strong>Success Path (No Exception):</strong></para>
    /// <code>
    /// var exception = await CatchExceptionAsync(async () => 
    ///     await service.ValidOperationAsync());
    /// 
    /// Assert.IsNull(exception);
    /// </code>
    /// 
    /// <para><strong>Using in Assertion Methods:</strong></para>
    /// <code>
    /// protected static async ValueTask DoesNotThrowAsync(
    ///     Func&lt;Task&gt; attempt,
    ///     Func&lt;string, ValueTask&gt; assertFailAsync)
    /// {
    ///     var exception = await CatchExceptionAsync(attempt);
    ///     
    ///     if (exception is not null)
    ///     {
    ///         await assertFailAsync($"Expected no exception but got: {exception.Message}");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="CatchException(Action)"/>
    /// <seealso cref="IsNotFatal(Exception)"/>
    /// <seealso cref="DoesNotThrowAsync(Func{Task}, Func{string, ValueTask})"/>
    public static async ValueTask<Exception?> CatchExceptionAsync(Func<Task> attempt)
    {
        _ = NotNull(attempt, nameof(attempt));

        try
        {
            await attempt();
            return null;
        }
        catch (Exception exception) when (IsNotFatal(exception))
        {
            return exception;
        }
    }

    /// <summary>
    /// Executes a synchronous action and catches any non-fatal exception that occurs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="CatchExceptionAsync(Func{Task})"/> via <see cref="ThreadSafeSync{T}(ValueTask{T})"/>.
    /// </para>
    /// <para>
    /// <strong>Fatal Exception Handling:</strong> Uses <see cref="IsNotFatal(Exception)"/>
    /// to filter exceptions. Fatal exceptions propagate immediately without being caught.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Zero allocations on success path. Uses <c>ConfigureAwait(false)</c>
    /// internally for thread safety.
    /// </para>
    /// </remarks>
    /// <param name="attempt">
    /// The synchronous action to execute. Must not be <see langword="null"/>. Non-fatal
    /// exceptions are caught and returned; fatal exceptions propagate.
    /// </param>
    /// <returns>
    /// <see langword="null"/> if execution completes successfully; otherwise, the non-fatal
    /// <see cref="Exception"/> that was thrown.
    /// </returns>
    /// <exception cref="StackOverflowException">Propagated (fatal).</exception>
    /// <exception cref="OutOfMemoryException">Propagated (fatal).</exception>
    /// <exception cref="AccessViolationException">Propagated (fatal).</exception>
    /// <exception cref="ThreadAbortException">Propagated (fatal).</exception>
    /// <example>
    /// <code>
    /// // Non-fatal exception is caught
    /// var a = CatchException(() => throw new ArgumentException("invalid"));
    /// Assert.IsNotNull(a);
    /// 
    /// // Fatal exception propagates
    /// try
    /// {
    ///     CatchException(() => throw new StackOverflowException());
    /// }
    /// catch (StackOverflowException)
    /// {
    ///     // Caught at outer level
    /// }
    /// </code>
    /// </example>
    public static Exception? CatchException(Action attempt)
    {
        _ = NotNull(attempt, nameof(attempt));

        return ThreadSafeSync(CatchExceptionAsync(() =>
        {
            attempt();
            return Task.CompletedTask;
        }));
    }

    #endregion

    #region Protected Helper Methods

    /// <summary>
    /// Gets the full runtime a name of the supplied object.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static string? GetTypeFullName(object? obj)
    => GetFullName(obj?.GetType());

    /// <summary>
    /// Gets the full name of the supplied a, or "null" when no a is available.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static string GetFullName(Type? obj)
    => obj?.FullName ?? "null";

    /// <summary>
    /// Creates the fallback exception used when an injected assertion callback does not throw.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static InvalidOperationException GetAssertionFailedException(string message)
    => new($"Assertion failed: {message}");

    /// <summary>
    /// Creates a message describing an unexpected exception a for assertion failures.
    /// </summary>
    protected static string GetNotExpectedTypeExceptionThrownMessage(Type expectedType, Type? actualType)
    => GetExpectedExceptionOfTypeMessage(
        expectedType,
        GetNotExpectedExceptionOfTypeWasThrownMessageInsert(actualType));

    /// <summary>
    /// Formats a message indicating that the exception value does not match the expected value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static string GetNotExpectedValueMessage(object expected, object? actual)
    => $"Expected '{expected}' but got '{actual ?? "null"}'.";

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Compares two float values with configurable tolerance.
    /// </summary>
    /// <param name="expected">The expected float value.</param>
    /// <param name="actual">The actual float value.</param>
    /// <param name="floatingPointTolerance">
    /// Optional epsilon for comparison. If null, uses default of 1e-6f.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the values are approximately equal within tolerance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreApproximatelyEqual(float expected, float actual, double? floatingPointTolerance)
    {
        const float defaultEpsilon = 1e-6f;
        float tolerance = (float)(floatingPointTolerance ?? defaultEpsilon);

        // Fast path: exact bitwise equality
        if (BitConverter.SingleToInt32Bits(expected) == BitConverter.SingleToInt32Bits(actual))
        {
            return true;
        }

        // Special values: treat all NaN representations as equal
        if (float.IsNaN(expected) || float.IsNaN(actual))
        {
            return float.IsNaN(expected) && float.IsNaN(actual);
        }

        // Infinities: only equal if bitwise equal (already checked above)
        if (float.IsInfinity(expected) || float.IsInfinity(actual))
        {
            return false;
        }

        // Normal values: tolerance-based comparison
        float diff = Math.Abs(expected - actual);
        float maxAbs = Math.Max(Math.Abs(expected), Math.Abs(actual));

        return diff <= tolerance || diff <= maxAbs * tolerance;
    }

    /// <summary>
    /// Compares two double values with configurable tolerance.
    /// </summary>
    /// <param name="expected">The expected double value.</param>
    /// <param name="actual">The actual double value.</param>
    /// <param name="floatingPointTolerance">
    /// Optional epsilon for comparison. If null, uses default of 1e-10.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the values are approximately equal within tolerance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreApproximatelyEqual(double expected, double actual, double? floatingPointTolerance)
    {
        const double defaultEpsilon = 1e-10;
        double tolerance = floatingPointTolerance ?? defaultEpsilon;

        // Fast path: exact bitwise equality
        if (BitConverter.DoubleToInt64Bits(expected) == BitConverter.DoubleToInt64Bits(actual))
        {
            return true;
        }

        // Special values: treat all NaN representations as equal
        if (double.IsNaN(expected) || double.IsNaN(actual))
        {
            return double.IsNaN(expected) && double.IsNaN(actual);
        }

        // Infinities: only equal if bitwise equal (already checked above)
        if (double.IsInfinity(expected) || double.IsInfinity(actual))
        {
            return false;
        }

        // Normal values: tolerance-based comparison
        double diff = Math.Abs(expected - actual);
        double maxAbs = Math.Max(Math.Abs(expected), Math.Abs(actual));

        return diff <= tolerance || diff <= maxAbs * tolerance;
    }

    /// <summary>
    /// Determines whether an exception is non-fatal and safe to catch for testing purposes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Fatal Exception Classification:</strong> This method identifies exceptions that indicate
    /// catastrophic process failures and should never be caught in normal error handling. These exceptions
    /// represent unrecoverable states where continuing execution could lead to undefined behavior or
    /// data corruption.
    /// </para>
    /// <para>
    /// <strong>Fatal Exception Types:</strong>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="OutOfMemoryException"/> - Process has exhausted available memory. Catching this
    ///     can prevent proper cleanup and may cause cascading failures.
    ///   </item>
    ///   <item>
    ///     <see cref="AccessViolationException"/> - Unmanaged code attempted to read/write protected
    ///     memory. Indicates memory corruption or invalid pointer usage.
    ///   </item>
    ///   <item>
    ///     <see cref="StackOverflowException"/> - Call stack exceeded available space (typically from
    ///     infinite recursion). The CLR cannot reliably execute catch blocks in this state.
    ///   </item>
    ///   <item>
    ///     <see cref="ThreadAbortException"/> - Thread termination was requested via <c>Thread.Abort()</c>.
    ///     Catching this can interfere with proper thread shutdown.
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> This method is used as a filter in exception handlers to ensure
    /// test infrastructure only catches testable exceptions. Fatal exceptions are allowed to propagate
    /// immediately, ensuring the process terminates cleanly.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/> to
    /// eliminate method call overhead in hot paths (exception handling).
    /// </para>
    /// </remarks>
    /// <param name="exception">The exception to evaluate. Cannot be null.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="exception"/> is non-fatal (safe to catch for testing);
    /// <see langword="false"/> if <paramref name="exception"/> is fatal (must propagate immediately).
    /// </returns>
    /// <example>
    /// <para><strong>Non-Fatal Exception (Returns true):</strong></para>
    /// <code>
    /// try
    /// {
    ///     throw new ArgumentException("invalid");
    /// }
    /// catch (Exception a) when (IsNotFatal(a))
    /// {
    ///     // Safe to catch and handle
    ///     return a; // Non-fatal exception caught
    /// }
    /// </code>
    /// 
    /// <para><strong>Fatal Exception (Returns false, propagates):</strong></para>
    /// <code>
    /// try
    /// {
    ///     throw new StackOverflowException();
    /// }
    /// catch (Exception a) when (IsNotFatal(a))
    /// {
    ///     // This block never executes - filter returns false
    ///     return a;
    /// }
    /// // StackOverflowException propagates, terminating process
    /// </code>
    /// 
    /// <para><strong>Usage in CatchException:</strong></para>
    /// <code>
    /// public static Exception? CatchException(Action attempt)
    /// {
    ///     try
    ///     {
    ///         attempt();
    ///         return null;
    ///     }
    ///     catch (Exception exception) when (IsNotFatal(exception))
    ///     {
    ///         // Only non-fatal exceptions are caught
    ///         return exception;
    ///     }
    ///     // Fatal exceptions bypass this catch and propagate
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNotFatal(Exception exception)
    => exception is not (
        OutOfMemoryException or
        AccessViolationException or
        StackOverflowException or
        ThreadAbortException);

    /// <summary>
    /// Executes a ValueTask synchronously in a thread-safe manner.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses <c>ConfigureAwait(false)</c> to prevent deadlocks in synchronization contexts.
    /// This is safe for test frameworks (NUnit, xUnit, MSTest) which typically don't have
    /// a <see cref="SynchronizationContext"/>.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked for aggressive inlining to eliminate method call overhead.
    /// </para>
    /// </remarks>
    /// <param name="assertion">The ValueTask to execute synchronously.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThreadSafeSync(ValueTask assertion)
    => assertion.ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Executes a ValueTask&lt;TConverted&gt; synchronously in a thread-safe manner and returns the result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses <c>ConfigureAwait(false)</c> to prevent deadlocks in synchronization contexts.
    /// This is safe for test frameworks (NUnit, xUnit, MSTest) which typically don't have
    /// a <see cref="SynchronizationContext"/>.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked for aggressive inlining to eliminate method call overhead.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of value returned by the ValueTask.</typeparam>
    /// <param name="assertion">The ValueTask to execute synchronously.</param>
    /// <returns>The result of the ValueTask operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ThreadSafeSync<T>(ValueTask<T> assertion)
    => assertion.ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Executes a <see cref="Func{Task}"/> synchronously in a thread-safe manner (void-returning overload).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload handles async operations that do not return a value. It invokes the provided
    /// function, waits for the resulting <see cref="Task"/> to complete, and returns control to
    /// the caller synchronously.
    /// </para>
    /// <para>
    /// Uses <c>ConfigureAwait(false)</c> to prevent deadlocks in synchronization contexts.
    /// This is safe for test frameworks (NUnit, xUnit, MSTest) which typically don't have
    /// a <see cref="SynchronizationContext"/>.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked for aggressive inlining to eliminate method call overhead
    /// in hot paths (exception handling and assertion validation).
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> This method is used internally to bridge async implementation
    /// methods with synchronous public APIs. For example, sync wrappers like <c>ThrowsDetails</c>
    /// delegate to async implementations like <c>ThrowsDetailsAsync</c>, using this method to
    /// execute the async code synchronously.
    /// </para>
    /// </remarks>
    /// <param name="attemptAsync">
    /// A function that returns a <see cref="Task"/> representing the asynchronous operation to execute.
    /// The function is invoked immediately and the resulting task is awaited synchronously.
    /// </param>
    /// <exception cref="AggregateException">
    /// If the task completes with an exception, that exception is propagated to the caller.
    /// Multiple exceptions may be wrapped in an <see cref="AggregateException"/>.
    /// </exception>
    /// <example>
    /// <para><strong>Internal usage in sync wrapper:</strong></para>
    /// <code>
    /// // Sync wrapper delegates to async implementation
    /// public static TException ThrowsDetails&lt;TException&gt;(
    ///     Action attempt,
    ///     TException expected,
    ///     Func&lt;Action, TException&gt; assertThrowsAny,
    ///     ...)
    /// {
    ///     return ThreadSafeSync(ThrowsDetailsAsync(
    ///         () =&gt; {
    ///             attempt();
    ///             return Task.CompletedTask;
    ///         },
    ///         expected,
    ///         assertThrowsAnyAsync: attemptAsync =&gt; 
    ///             new ValueTask&lt;TException&gt;(
    ///                 assertThrowsAny(() =&gt; ThreadSafeSync(attemptAsync))),
    ///         ...));
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThreadSafeSync(Func<Task> attemptAsync)
    => attemptAsync().ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Determines whether two values are equal using built-in type support and tolerance for floating-point.
    /// </summary>
    /// <remarks>This is a pure function with no I/O - does not need async version.</remarks>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The actual value.</param>
    /// <param name="tolerance">
    /// Epsilon for floating-point comparisons. Default: 1e-10 for double, 1e-6f for float.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the values are equal according to type-specific rules;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool AreEqual(object? expected, object? actual, double? tolerance)
    {
        if (ReferenceEquals(expected, actual)) return true;
        if (expected is null || actual is null) return false;

        return (expected, actual) switch
        {
            // Integer types
            (byte e, byte a) => e == a,
            (sbyte e, sbyte a) => e == a,
            (short e, short a) => e == a,
            (ushort e, ushort a) => e == a,
            (int e, int a) => e == a,
            (uint e, uint a) => e == a,
            (long e, long a) => e == a,
            (ulong e, ulong a) => e == a,
            (nint e, nint a) => e == a,
            (nuint e, nuint a) => e == a,

            // Other primitives
            (bool e, bool a) => e == a,
            (char e, char a) => e == a,
            (string e, string a) => e == a,
            (decimal e, decimal a) => e == a,

            // Floating-point with tolerance
            (float e, float a) => AreApproximatelyEqual(e, a, tolerance),
            (double e, double a) => AreApproximatelyEqual(e, a, tolerance),

            // Framework types
            (Guid e, Guid a) => e == a,
            (DateTime e, DateTime a) => e == a,
            (DateOnly e, DateOnly a) => e == a,
            (TimeOnly e, TimeOnly a) => e == a,
            (TimeSpan e, TimeSpan a) => e == a,
            (DateTimeOffset e, DateTimeOffset a) => e == a,
            (BigInteger e, BigInteger a) => e == a,

            // Collections (recursive comparison)
            (IEnumerable e, IEnumerable a) =>
                e.Cast<object?>().SequenceEqual(
                    a.Cast<object?>(),
                    EqualityComparer<object?>.Create(
                        (x, y) => AreEqual(x, y, tolerance))),

            // Fallback to object.Equals
            _ => expected.Equals(actual),
        };
    }

    #endregion

    #region Assertion message helpers

    private static string GetNotExpectedExceptionMessage(Exception exception)
    => $"Did not expect exception to be thrown, " +
        $"but exception of type {GetTypeFullName(exception)} was thrown. " +
        $"Message: '{exception.Message}'";

    private static string GetExpectedExceptionOfTypeMessage(Type expectedType, string end)
    => $"{ExpectedExceptionMessageStart} of type {GetFullName(expectedType)}{end}";

    private static string GetNotExpectedExceptionOfTypeWasThrownMessageInsert(Type? actualType)
    => $", but exception of type {GetFullName(actualType)} was thrown.";

    private const string ExpectedExceptionMessageStart = "Expected exception";
    private const string NoExceptionThrownMessageEnd = " to be thrown, but no exception was thrown.";
    private const string AssertFailDelegateFailedMessage = "The assertFail delegate completed without terminating test execution. The delegate must throw an assertion exception to fail the test.";

    #endregion

    #endregion
}
