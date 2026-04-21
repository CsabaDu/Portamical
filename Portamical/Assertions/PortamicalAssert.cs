// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

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
    /// <summary>
    /// Prevents external instantiation while allowing derived classes in extension scenarios.
    /// </summary>
    protected PortamicalAssert()
    {
    }

    #region Primary Implementation (Async)

    /// <summary>
    /// Verifies that the specified action does not throw an exception (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The sync version
    /// <see cref="DoesNotThrow(Action, Action{string})"/> delegates to this method.
    /// </para>
    /// <para>
    /// Completes synchronously with zero allocation when no exception is thrown.
    /// </para>
    /// </remarks>
    /// <param name="attempt">The action to execute and verify for the absence of exceptions. Cannot be null.</param>
    /// <param name="assertFailAsync">
    /// A callback to invoke with an error message if the action throws an exception. Cannot be null.
    /// The callback should throw an assertion exception or complete the returned <see cref="ValueTask"/>.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the async assertion operation.</returns>
    /// <example>
    /// <code>
    /// // TUnit usage:
    /// await DoesNotThrowAsync(
    ///     () => myService.DoWork(),
    ///     msg => throw new AssertionException(msg));
    /// </code>
    /// </example>
    protected static ValueTask DoesNotThrowAsync(
        Action attempt,
        Func<string, ValueTask> assertFailAsync)
    {
        var exception = CatchException(attempt);
        _ = NotNull(assertFailAsync, nameof(assertFailAsync));

        if (exception is not null)
        {
            return assertFailAsync(GetNotExpectedExceptionMessage(exception));
        }

        return default;  // ← Zero allocation success path
    }

    /// <summary>
    /// Executes the specified action and verifies that it throws an exception of the expected type
    /// with matching details (async version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is the PRIMARY implementation.</strong> The sync version
    /// <see cref="ThrowsDetails{TException}(Action, TException, Func{Action, Exception}, Action{Type, Exception}, Action{string, string}, Action{string})"/>
    /// delegates to this method.
    /// </para>
    /// </remarks>
    /// <typeparam name="TException">
    /// The type of exception expected to be thrown. Must be a non-null reference type derived from Exception.
    /// </typeparam>
    /// <param name="attempt">The action to execute, which is expected to throw an exception.</param>
    /// <param name="expected">The expected exception instance, used as a reference for type and detail comparisons.</param>
    /// <param name="catchException">A delegate that executes the action and returns the exception thrown, or null if no exception is thrown.</param>
    /// <param name="assertIsTypeAsync">
    /// A delegate that asserts the actual exception is of the expected type. Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <param name="assertEqualityAsync">
    /// A delegate that asserts equality between expected and actual values. Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <param name="assertFailAsync">
    /// A delegate that is called to indicate a failed assertion. Returns a <see cref="ValueTask"/>.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the actual exception that was thrown and verified.
    /// </returns>
    protected static async ValueTask<TException> ThrowsDetailsAsync<TException>(
        Action attempt,
        TException expected,
        Func<Action, Exception?> catchException,
        Func<Type, Exception, ValueTask> assertIsTypeAsync,
        Func<object, object?, ValueTask> assertEqualityAsync,
        Func<string, ValueTask> assertFailAsync)
    where TException : notnull, Exception
    {
        var actual = NotNull(catchException, nameof(catchException))(attempt);

        if (actual is null)
        {
            var message = GetExpectedExceptionOfTypeMessage(
                expected,
                GetThrownMessageEnd(false));
            await assertFailAsync(message).ConfigureAwait(false);
            throw GetAssertionFailedException(message);  // Fallback
        }

        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        if (actualType != expectedType)
        {
            var message = GetExpectedExceptionOfTypeMessage(
                expectedType,
                GetNotExpectedExceptionOfTypeWasThrownMessageInsert(actualType));
            await assertFailAsync(message).ConfigureAwait(false);
            throw GetAssertionFailedException(message);
        }

        var typedActual = (TException)actual;

        // Type assertion
        await assertIsTypeAsync(expectedType, typedActual).ConfigureAwait(false);

        // Metadata equality
        await MetadataEqualityAsync(expected, typedActual, assertEqualityAsync)
            .ConfigureAwait(false);

        return typedActual;
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
    protected static ValueTask EqualityAsync<T>(
        T? expected,
        T? actual,
        Func<T?, T?, bool> equals,
        Func<string, ValueTask> assertFailAsync,
        string message)
    {
        _ = NotNull(equals, nameof(equals));
        _ = NotNull(assertFailAsync, nameof(assertFailAsync));

        if (equals(expected, actual))
        {
            return default;  // ← Zero allocation success path
        }

        return assertFailAsync(message);
    }

    /// <summary>
    /// Verifies value equality for common primitive and framework types (async version).
    /// </remarks>
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
    protected static ValueTask EqualityAsync(
        object expected,
        object? actual,
        Func<ValueTask> assertFailAsync,
        double? floatingPointTolerance = null)
    {
        _ = NotNull(assertFailAsync, nameof(assertFailAsync));

        if (AreEqual(expected, actual, floatingPointTolerance))
        {
            return default;  // ← Zero allocation success path
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
    protected static ValueTask IsTypeOfAsync(
        Type expected,
        object? actual,
        Func<Type, Type?, ValueTask> assertEqualityAsync)
    {
        _ = NotNull(assertEqualityAsync, nameof(assertEqualityAsync));
        return assertEqualityAsync(
            NotNull(expected, nameof(expected)),
            actual?.GetType());
    }

    #endregion

    #region Convenience Wrappers (Sync delegates to Async)
    /// <summary>
    /// Verifies that the specified action does not throw an exception (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="DoesNotThrowAsync(Action, Func{string, ValueTask})"/>.
    /// </para>
    /// <para>
    /// Safe for use in test contexts where <see cref="SynchronizationContext"/> is typically
    /// not present (NUnit, xUnit, MSTest). Uses <c>ConfigureAwait(false)</c> to prevent
    /// potential deadlocks.
    /// </para>
    /// </remarks>
    /// <param name="attempt">The action to execute and verify for the absence of exceptions. Cannot be null.</param>
    /// <param name="assertFail">A callback to invoke with an error message if the action throws an exception. Cannot be null.</param>
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
        _ = NotNull(assertFail, nameof(assertFail));

#pragma warning disable CA2012
        DoesNotThrowAsync(attempt, msg =>
        {
            assertFail(msg);
            return new ValueTask();
        });
#pragma warning restore CA2012
    }

    /// <summary>
    /// Executes the specified action and verifies that it throws an exception of the expected type
    /// with matching details (sync version).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>This is a CONVENIENCE WRAPPER</strong> that delegates to
    /// <see cref="ThrowsDetailsAsync{TException}(Action, TException, Func{Action, Exception}, Func{Type, Exception, ValueTask}, Func{object, object, ValueTask}, Func{string, ValueTask})"/>.
    /// </para>
    /// </remarks>
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected,
        Func<Action, Exception?> catchException,
        Action<Type, Exception> assertIsType,
        Action<string, string?> assertEquality,
        Action<string> assertFail)
    where TException : notnull, Exception
    {
        _ = NotNull(assertIsType, nameof(assertIsType));
        _ = NotNull(assertEquality, nameof(assertEquality));
        _ = NotNull(assertFail, nameof(assertFail));

        return ThreadSafeSyncAssertion(
            ThrowsDetailsAsync(
            attempt,
            expected,
            catchException,
            assertIsTypeAsync: (t, e) =>
            {
                assertIsType(t, e);
                return new ValueTask();
            },
            assertEqualityAsync: (e, a) =>
            {
                assertEquality(e?.ToString() ?? string.Empty, a?.ToString());
                return new ValueTask();
            },
            assertFailAsync: msg =>
            {
                assertFail(msg);
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
    public static void Equality<T>(
        T? expected,
        T? actual,
        Func<T?, T?, bool> equals,
        Action<string?> assertFail,
        string? message)
    {
        _ = NotNull(assertFail, nameof(assertFail));

        ThreadSafeSyncAssertion(
            EqualityAsync(expected, actual, equals, msg =>
            {
                assertFail(msg);
                return new ValueTask();
            }, message ?? string.Empty));
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
    public static void Equality(
        object expected,
        object? actual,
        Action assertFail,
        double? floatingPointTolerance = null)
    {
        _ = NotNull(assertFail, nameof(assertFail));

        ThreadSafeSyncAssertion(
            EqualityAsync(expected, actual, () =>
            {
                assertFail();
                return new ValueTask();
            }, floatingPointTolerance));
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
    public static void IsTypeOf(
        Type expected,
        object? actual,
        Action<Type, Type?> assertEquality)
    {
        _ = NotNull(assertEquality, nameof(assertEquality));

        ThreadSafeSyncAssertion(
            IsTypeOfAsync(expected, actual, (e, a) =>
            {
                assertEquality(e, a);
                return new ValueTask();
            }));
    }
    #endregion

    #region Helper methods
    #region Shared Helper Methods

    /// <summary>
    /// Invokes the specified action and returns any exception that is thrown, or null if the action
    /// completes successfully.
    /// </summary>
    /// <remarks>
    /// This method is truly synchronous with no I/O - does not need async version.
    /// </remarks>
    /// <param name="attempt">The action to execute. Cannot be null.</param>
    /// <returns>The exception thrown by the action, or null if no exception is thrown.</returns>
    public static Exception? CatchException(Action attempt)
    {
        _ = NotNull(attempt, nameof(attempt));

        try
        {
            attempt();
        }
        catch (Exception exception) when (IsNotFatal(exception))
        {
            return exception;
        }

        return null;
    }
    #endregion

    #region Protected Helper Methods
    /// <summary>
    /// Gets the full runtime type name of the supplied object.
    /// </summary>
    protected static string? GetTypeFullName(object? obj)
    => GetFullName(obj?.GetType());

    /// <summary>
    /// Gets the full name of the supplied type, or "null" when no type is available.
    /// </summary>
    protected static string GetFullName(Type? obj)
    => obj?.FullName ?? "null";

    /// <summary>
    /// Creates the fallback exception used when an injected assertion callback does not throw.
    /// </summary>
    protected static InvalidOperationException GetAssertionFailedException(string message)
    => new($"Assertion failed: {message}");

    /// <summary>
    /// Creates a message describing an unexpected exception type for assertion failures.
    /// </summary>
    protected static string GetNotExpectedTypeExceptionThrownMessage(Type expectedType, Type? actualType)
    => GetExpectedExceptionOfTypeMessage(
        expectedType,
        GetNotExpectedExceptionOfTypeWasThrownMessageInsert(actualType));

    /// <summary>
    /// Formats a message indicating that the actual value does not match the expected value.
    /// </summary>
    protected static string GetNotExpectedValueMessage(object expected, object? actual)
    => $"Expected '{expected}' but got '{actual ?? "null"}'.";
    #endregion

    #region Private Helper Methods
    /// <summary>
    /// Compares two float values with configurable tolerance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreApproximatelyEqual(float expected, float actual, double? floatingPointTolerance)
    {
        const float DefaultEpsilonFloat = 1e-6f;
        float tolerance = (float)(floatingPointTolerance ?? DefaultEpsilonFloat);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreApproximatelyEqual(double expected, double actual, double? floatingPointTolerance)
    {
        const double DefaultEpsilon = 1e-10;
        double tolerance = floatingPointTolerance ?? DefaultEpsilon;

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
    private static void ThreadSafeSyncAssertion(ValueTask assertion)
    => assertion.ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Executes a ValueTask&lt;T&gt; synchronously in a thread-safe manner and returns the result.
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
    private static T ThreadSafeSyncAssertion<T>(ValueTask<T> assertion)
    => assertion.ConfigureAwait(false).GetAwaiter().GetResult();
    /// <summary>
    /// Determines whether two values are equal using built-in type support and tolerance for floating-point.
    /// </summary>
    /// <remarks>This is a pure function with no I/O - does not need async version.</remarks>
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
                e.Cast<object?>().SequenceEqual(a.Cast<object?>(),
                    EqualityComparer<object?>.Create((x, y) => AreEqual(x, y, tolerance))),

            // Fallback to object.Equals
            _ => expected.Equals(actual),
        };
    }

    /// <summary>
    /// Asserts exception metadata equality with selective assertion control (async helper).
    /// </summary>
    private static async ValueTask MetadataEqualityAsync<TException>(
        TException expected,
        TException actual,
        Func<object, object?, ValueTask> assertEqualityAsync)
    where TException : notnull, Exception
    {
        const string ArgumentExceptionGuardMessageStart = "The value cannot be an empty string";
        const string ObjectDisposedExceptionGuardMessageStart = "Cannot access a disposed object.\nObject name: '";

        var expectedMessage = expected.Message;
        var actualMessage = actual.Message;

        if (expected is ArgumentException argExpected && actual is ArgumentException argActual)
        {
            var actualParamName = argActual.ParamName;
            bool shouldAssertMessage =
                !actualMessage.StartsWith(ArgumentExceptionGuardMessageStart) &&
                !actualMessage.StartsWith($"'{actualParamName}' ('");

            if (shouldAssertMessage && expectedMessage is not null)
            {
                await assertEqualityAsync(expectedMessage, actualMessage).ConfigureAwait(false);
            }

            if (argExpected.ParamName is string expectedParamName)
            {
                await assertEqualityAsync(expectedParamName, actualParamName).ConfigureAwait(false);
            }
        }
        else if (expectedMessage is not null)
        {
            bool shouldAssertMessage =
                expected is not ObjectDisposedException ||
                !actualMessage.StartsWith(ObjectDisposedExceptionGuardMessageStart);

            if (shouldAssertMessage)
            {
                await assertEqualityAsync(expectedMessage, actualMessage).ConfigureAwait(false);
            }
        }
    }

    private static string GetNotExpectedExceptionMessage(Exception exception)
    => $"Did not expect exception to be thrown, " +
        $"but exception of type {GetTypeFullName(exception)} was thrown. " +
        $"Message: '{exception.Message}'";

    private static string GetExpectedExceptionOfTypeMessage(Type expectedType, string end)
    => $"{ExpectedExceptionMessageStart} of type {GetFullName(expectedType)}{end}";

    private static string GetExpectedExceptionOfTypeMessage(Exception expected, string end)
    => $"{ExpectedExceptionMessageStart} of type {GetTypeFullName(expected)}{end}";

    private static string GetNotExpectedExceptionOfTypeWasThrownMessageInsert(Type? actualType)
    => $", but exception of type {GetFullName(actualType)}{GetThrownMessageEnd(true)}";

    private static string GetThrownMessageEnd(bool thrown)
    {
        string thrownNotThrown = thrown ? string.Empty : "not ";
        return $" was {thrownNotThrown}thrown.";
    }

    private const string ExpectedExceptionMessageStart = "Expected exception";
    #endregion
    #endregion
}
