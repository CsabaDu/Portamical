// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Assertions;

/// <summary>
/// Provides framework-agnostic assertion helper methods for unit testing.
/// </summary>
/// <remarks>
/// <para>
/// This abstract base class defines reusable assertion logic that can be adapted to any testing
/// framework (MSTest, NUnit, xUnit, etc.) by passing framework-specific assertion delegates as parameters.
/// </para>
/// <para>
/// <strong>Design Pattern: Dependency Injection for Assertions</strong>
/// </para>
/// <para>
/// Rather than directly coupling to a specific testing framework, this class accepts assertion
/// delegates (e.g., <c>Action&lt;string&gt; assertFail</c>, <c>Action&lt;Type, Type&gt; assertEquality</c>)
/// that encapsulate framework-specific assertion behavior. This enables:
/// </para>
/// <list type="bullet">
///   <item>
///     <strong>Framework Independence:</strong> Core assertion logic works with MSTest, NUnit, xUnit,
///     or custom test frameworks without modification.
///   </item>
///   <item>
///     <strong>Extension Projects:</strong> Framework-specific projects (e.g., Portamical.MSTest)
///     derive from this class to provide convenience methods that pre-configure the assertion delegates.
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
///     <strong>Direct Usage:</strong> Call static methods directly, passing framework-specific
///     delegates explicitly.
///   </item>
///   <item>
///     <strong>Via Extension Projects:</strong> Use framework-specific derived classes that
///     provide simplified APIs (e.g., <c>Portamical.MSTest.Assertions.PortamicalAssert.DoesNotThrow(Action)</c>).
///   </item>
/// </list>
/// <para>
/// <strong>Inheritance:</strong> This class is abstract with a protected constructor to prevent
/// direct instantiation while enabling inheritance in framework-specific extension projects.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Direct Usage (Framework-Agnostic):</strong></para>
/// <code>
/// // With MSTest
/// PortamicalAssert.DoesNotThrow(
///     () => myService.DoWork(),
///     Assert.Fail);  // MSTest's Assert.Fail
/// 
/// // With NUnit
/// PortamicalAssert.DoesNotThrow(
///     () => myService.DoWork(),
///     Assert.Fail);  // NUnit's Assert.Fail
/// 
/// // With xUnit
/// PortamicalAssert.DoesNotThrow(
///     () => myService.DoWork(),
///     msg => Assert.True(false, msg));  // xUnit doesn't have Assert.Fail
/// 
/// // Custom handler
/// PortamicalAssert.DoesNotThrow(
///     () => myService.DoWork(),
///     msg => throw new CustomAssertionException(msg));
/// </code>
/// 
/// <para><strong>Via Extension Project (Simplified API):</strong></para>
/// <code>
/// // Portamical.MSTest extension project provides:
/// using Portamical.MSTest.Assertions;
/// 
/// // Simplified - no need to pass Assert.Fail
/// PortamicalAssert.DoesNotThrow(() => myService.DoWork());
/// 
/// // The extension class does:
/// // public static void DoesNotThrow(Action attempt)
/// //     => Portamical.Assertions.PortamicalAssert.DoesNotThrow(attempt, Assert.Fail);
/// </code>
/// 
/// <para><strong>Creating Framework-Specific Extensions:</strong></para>
/// <code>
/// namespace Portamical.MSTest.Assertions;
/// 
/// /// &lt;summary&gt;
/// /// MSTest-specific assertion helpers.
/// /// &lt;/summary&gt;
/// public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
/// {
///     // Simplify DoesNotThrow for MSTest users
///     public static void DoesNotThrow(Action attempt)
///         => DoesNotThrow(attempt, assertFail: Assert.Fail);
///     
///     // Simplify ThrowsDetails for MSTest users
///     public static TException ThrowsDetails&lt;TException&gt;(Action attempt, TException expected)
///     where TException : notnull, Exception
///         => ThrowsDetails(attempt, expected,
///             catchException: CatchException,
///             assertIsType: (e, a) => Assert.IsInstanceOfType(a, e),
///             assertEquality: (e, a) => Assert.AreEqual(e, a),
///             assertFail: Assert.Fail);
/// }
/// </code>
/// </example>
/// <seealso cref="DoesNotThrow(Action, Action{string})"/>
/// <seealso cref="CatchException(Action)"/>
/// <seealso cref="ThrowsDetails{TException}(Action, TException, Func{Action, Exception}, Action{Type, Exception}, Action{string, string}, Action{string})"/>
/// <seealso cref="ThrowsActualType{TException}(TException, Exception, Action{Type, Exception}, Action{string})"/>
/// <seealso cref="ThrowsMetadataEquality{TException}(TException, TException, Action{string, string})"/>
public abstract class PortamicalAssert
{
    /// <summary>
    /// Prevents external instantiation while allowing derived classes in extension projects.
    /// </summary>
    /// <remarks>
    /// This constructor is protected to enable inheritance in framework-specific extension projects
    /// (e.g., Portamical.MSTest, Portamical.NUnit, Portamical.xUnit) while preventing direct
    /// instantiation of this abstract base class.
    /// </remarks>
    protected PortamicalAssert()
    {
    }

    #region Assert Methods
    /// <summary>
    /// Verifies that the specified action does not throw an exception, and invokes a failure callback if an exception
    /// is thrown.
    /// </summary>
    /// <remarks>Use this method to assert that a given operation completes without throwing any exceptions.
    /// If an exception is thrown, the provided failure callback is called with a descriptive message.</remarks>
    /// <param name="attempt">The action to execute and verify for the absence of exceptions. Cannot be null.</param>
    /// <param name="assertFail">A callback to invoke with an error message if the action throws an exception. Cannot be null.</param>
    /// <example>
    /// <code>
    /// // xUnit usage:
    /// PortamicalAssert.DoesNotThrow(
    ///     () => myService.DoWork(),
    ///     Assert.Fail);
    /// 
    /// // Custom handler:
    /// PortamicalAssert.DoesNotThrow(
    ///     () => myService.DoWork(),
    ///     msg => _logger.Error(msg));
    /// </code>
    /// </example>
    public static void DoesNotThrow(Action attempt, Action<string> assertFail)
    {
        var exception = CatchException(attempt);
        _ = NotNull(assertFail, nameof(assertFail));

        if (exception is not null)
        {
            assertFail(getNotExpectedExceptionMessage(exception));
        }

        #region Local methods
        static string getNotExpectedExceptionMessage(Exception exception)
        => $"Did not expect exception to be thrown, " +
            $"but exception of type {GetTypeFullName(exception)} was thrown. " +
            $"Message: '{exception.Message}'";
        #endregion
    }

    /// <summary>
    /// Invokes the specified assertion action to verify that the type of the actual object matches the expected type.
    /// </summary>
    /// <remarks>This method does not perform the assertion itself, but delegates the comparison to the
    /// provided assertion action. This allows for custom assertion logic or integration with different testing
    /// frameworks.</remarks>
    /// <param name="expected">The expected type to compare against the actual object's type. Cannot be null.</param>
    /// <param name="actual">The object whose runtime type is to be compared with the expected type. Cannot be null.</param>
    /// <param name="assertEquality">An action that receives the expected type and the actual object's type, and performs the equality assertion.
    /// Cannot be null.</param>
    public static void IsTypeOf(
        Type expected,
        object actual,
        Action<Type, Type> assertEquality)
    => NotNull(assertEquality, nameof(assertEquality))(
        NotNull(expected, nameof(expected)),
        NotNull(actual, nameof(actual)).GetType());

    /// <summary>
    /// Invokes the specified action and returns any exception that is thrown, or null if the action completes
    /// successfully.
    /// </summary>
    /// <remarks>This method allows exception handling logic to be centralized or deferred by capturing any
    /// exception thrown by the action rather than propagating it. The method does not rethrow exceptions.</remarks>
    /// <param name="attempt">The action to execute. Cannot be null.</param>
    /// <returns>The exception thrown by the action, or null if no exception is thrown.</returns>
    public static Exception? CatchException(Action attempt)
    {
        _ = NotNull(attempt, nameof(attempt));

        try
        {
            attempt();
        }
        catch (Exception exception) when (isNotFatal(exception))
        {
            return exception;
        }

        return null;

        #region Local methods
        static bool isNotFatal(Exception exception)
        => exception is not (
            OutOfMemoryException or
            AccessViolationException or
            StackOverflowException or
            ThreadAbortException);
        #endregion
    }

    /// <summary>
    /// Executes the specified action and verifies that it throws an exception of the expected type and with matching
    /// details, using the provided assertion and comparison delegates.
    /// </summary>
    /// <remarks>This method is typically used in unit testing scenarios to assert that an action throws a
    /// specific exception with expected details. The assertion and comparison delegates allow for custom verification
    /// logic and integration with various test frameworks.</remarks>
    /// <typeparam name="TException">The type of exception expected to be thrown by the action. Must be a non-null reference type derived from
    /// Exception.</typeparam>
    /// <param name="attempt">The action to execute, which is expected to throw an exception of type TException.</param>
    /// <param name="expected">The expected exception instance, used as a reference for type and detail comparisons.</param>
    /// <param name="catchException">A delegate that executes the action and returns the exception thrown, or null if no exception is thrown.</param>
    /// <param name="assertIsType">A delegate that asserts the actual exception is of the expected type. Receives the expected type and the actual
    /// exception as parameters.</param>
    /// <param name="assertEquality">A delegate that asserts equality between the expected and actual exception details. Receives the expected and
    /// actual detail strings as parameters.</param>
    /// <param name="assertFail">A delegate that is called to indicate a failed assertion, receiving a message describing the failure.</param>
    /// <returns>The actual exception of type TException that was thrown by the action and verified to match the expected
    /// details.</returns>
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected,
        Func<Action, Exception?> catchException,
        Action<Type, Exception> assertIsType,
        Action<string, string?> assertEquality,
        Action<string> assertFail)
    where TException : notnull, Exception
    {
        var actual = NotNull(
            catchException,
            nameof(catchException))(
                attempt);
        var typedActual = ThrowsActualType(
            expected,
            actual,
            assertIsType,
            assertFail);

        return ThrowsMetadataEquality(
            expected,
            typedActual,
            assertEquality);
    }

    /// <summary>
    /// Asserts that the actual exception is of the same type as the expected exception and returns it as the specified
    /// exception type.
    /// </summary>
    /// <remarks>This method is typically used in test assertions to verify that an operation throws an
    /// exception of the expected type. If the actual exception is null or of a different type, the provided assertFail
    /// action is invoked and an assertion failure exception is thrown.</remarks>
    /// <typeparam name="TException">The type of exception expected and asserted. Must be a non-null exception type.</typeparam>
    /// <param name="expected">The expected exception instance whose type is used for comparison.</param>
    /// <param name="actual">The actual exception instance to verify. Can be null if no exception was thrown.</param>
    /// <param name="assertIsType">An action to perform when the actual exception type matches the expected type. Receives the expected type and
    /// the actual exception as arguments.</param>
    /// <param name="assertFail">An action to perform when the assertion fails. Receives a failure message describing the mismatch.</param>
    /// <returns>The actual exception cast to the specified exception type if its type matches the expected exception type.</returns>
    public static TException ThrowsActualType<TException>(
        TException expected,
        Exception? actual,
        Action<Type, Exception> assertIsType,
        Action<string> assertFail)
    where TException : notnull, Exception
    {
        _ = NotNull(assertIsType, nameof(assertIsType));
        _ = NotNull(assertFail, nameof(assertFail));

        const string expectedExceptionMessageStart = "Expected exception";
        const string wasNotThrownMessageEnd = " was not thrown.";
        const string wasThrownMessageEnd = " was thrown.";

        if (actual is null)
        {
            assertFail(getExpectedExceptionOfTypeMessage(
                expected,
                wasNotThrownMessageEnd));

            const string expectedExceptionNotThrownMessage =
                $"{expectedExceptionMessageStart}{wasNotThrownMessageEnd}";

            // throws when custom assertFail does not throw,
            // or to ensure method exits after framework-specific assertFail
            throw GetAssertionFailedException(expectedExceptionNotThrownMessage);
        }

        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        if (actualType == expectedType)
        {
            var typedActual = (TException)actual;
            assertIsType(expectedType, typedActual);
            return typedActual;
        }

        assertFail(getExpectedExceptionOfTypeMessage(
            expected,
            getNotExpectedExceptionOfTypeWasThrownMessageInsert(actual)));

        const string unexpectedExceptionThrownMessage =
            $"Unexpected exception type{wasThrownMessageEnd}";

        throw GetAssertionFailedException(unexpectedExceptionThrownMessage);

        #region Local methods
        static string getExpectedExceptionOfTypeMessage(TException expected, string end)
        => $"{expectedExceptionMessageStart} of type {GetTypeFullName(expected)}{end}";

        static string getNotExpectedExceptionOfTypeWasThrownMessageInsert(Exception actual)
        => $", but exception of type {GetTypeFullName(actual)}{wasThrownMessageEnd}";
        #endregion
    }

    /// <summary>
    /// Asserts exception metadata equality with selective assertion control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Selective Assertion Pattern:</strong> Uses null as a sentinel value to indicate
    /// which exception properties should be asserted:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <strong>ParamName (ArgumentException):</strong>
    ///     - Set (non-null): Asserts parameter name equality
    ///     - null: Skips parameter name assertion (test doesn't care about param name)
    ///   </item>
    ///   <item>
    ///     <strong>Message:</strong>
    ///     - Set (non-null): Asserts message equality
    ///     - null: Skips message assertion (test only validates exception type)
    ///   </item>
    /// </list>
    /// <para>
    /// This pattern enables focused, less brittle tests by allowing developers to specify
    /// exactly which exception properties matter for their test scenario.
    /// </para>
    /// </remarks>
    /// <remarks>
    /// <para>
    /// <strong>Selective Assertion:</strong> Properties are only asserted if set (non-null).
    /// Use null to skip assertions for properties that are implementation details.
    /// </para>
    /// </remarks>
    public static TException ThrowsMetadataEquality<TException>(
        TException expected,
        TException actual,
        Action<string, string?> assertEquality)
    where TException : notnull, Exception
    {
        _ = NotNull(assertEquality, nameof(assertEquality));

        // initialized for better readability
        // and to ensure it's assigned before use
        bool shouldAssertMessage = false;
        var expectedMessage = expected.Message;
        var actualMessage = actual.Message;

        if (expected is ArgumentException argExpected &&
            actual is ArgumentException argActual)
        {

        const string argumentExceptionGuardMessageStart =
            "The value cannot be an empty string";

        var actualParamName = argActual.ParamName;
            shouldAssertMessage =
                actualMessageDoesNotStartWith(argumentExceptionGuardMessageStart) &&
                actualMessageDoesNotStartWith(
                    getArgumentOutOfRangeExceptionGuardMessageStart(actualParamName));

            assertMessage();

            if (argExpected.ParamName is string expectedParamName)
            {
                assertEquality(expectedParamName, actualParamName);
            }
        }
        else if (expectedMessage is not null)
        {
            const string objectDisposedExceptionGuardMessageStart =
                "Cannot access a disposed object.\nObject name: '";

            shouldAssertMessage =
                expected is not ObjectDisposedException ||
                actualMessageDoesNotStartWith(objectDisposedExceptionGuardMessageStart);

            assertMessage();
        }

        return actual;

        #region Local methods
        void assertMessage()
        {
            if (shouldAssertMessage)
            {
                assertEquality(expectedMessage, actualMessage);
            }
        }

        bool actualMessageDoesNotStartWith(string start)
        => !actualMessage.StartsWith(start);

        static string getArgumentOutOfRangeExceptionGuardMessageStart(string? actualParamName)
        => $"'{actualParamName}' ('";
        #endregion
    }
    #endregion

    #region Helpers
    protected static string? GetTypeFullName(object? obj)
    => obj?.GetType().FullName;

    protected static InvalidOperationException GetAssertionFailedException(string message)
    => new($"Assertion failed: {message}");
    #endregion
}
