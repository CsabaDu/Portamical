// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Assertions;

public abstract class PortamicalAssert
{
    #region Assert Methods
    /// <summary>
    /// Verifies that the specified action does not throw an exception, and invokes a failure callback if an exception
    /// is thrown.
    /// </summary>
    /// <remarks>Use this method to assert that a given operation completes without throwing any exceptions.
    /// If an exception is thrown, the provided failure callback is called with a descriptive message.</remarks>
    /// <param name="attempt">The action to execute and verify for the absence of exceptions. Cannot be null.</param>
    /// <param name="assertFail">A callback to invoke with an error message if the action throws an exception. Cannot be null.</param>
    public static void DoesNotThrow(Action attempt, Action<string> assertFail)
    {
        var exception = CatchException(attempt);
        _ = NotNull(assertFail, nameof(assertFail));

        if (exception is not null)
        {
            assertFail(GetNotExpectedExceptionMessage(exception));
        }
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
        catch (Exception exception)
        {
            return exception;
        }

        return null;
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
        var actual =
            NotNull(catchException, nameof(catchException))(attempt);
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

        if (actual is null)
        {
            assertFail(GetExpectedTypeExceptionNotThrownMessage(expected));
            throw GetAssertionFailedException(ExpectedExceptionNotThrownMessage);
        }

        var expectedType = expected.GetType();

        if (actual.GetType() == expectedType && actual is TException typedActual)
        {
            assertIsType(expectedType, typedActual);
            return typedActual;
        }

        assertFail(GetNotExpectedTypeExceptionThrownMessage(expected, actual));
        throw GetAssertionFailedException(UnexpectedExceptionMessage);
    }

    /// <summary>
    /// Asserts that the metadata of two exceptions is equal using a provided assertion delegate, and returns the actual
    /// exception.
    /// </summary>
    /// <remarks>This method is typically used in unit tests to verify that two exceptions have equivalent
    /// messages and, for argument exceptions, equivalent parameter names. The provided assertion delegate is
    /// responsible for performing the actual comparison and throwing an assertion failure if the values do not
    /// match.</remarks>
    /// <typeparam name="TException">The type of exception to compare. Must be a non-null exception type.</typeparam>
    /// <param name="expected">The expected exception whose metadata will be used as the reference for comparison.</param>
    /// <param name="actual">The actual exception whose metadata will be compared to the expected exception.</param>
    /// <param name="assertEquality">A delegate that asserts the equality of two string values, typically used to compare exception messages and
    /// parameter names. Cannot be null.</param>
    /// <returns>The actual exception instance after performing the metadata equality assertions.</returns>
    public static TException ThrowsMetadataEquality<TException>(
        TException expected,
        TException actual,
        Action<string, string?> assertEquality)
    where TException : notnull, Exception
    {
        _ = NotNull(assertEquality, nameof(assertEquality));

        var expectedMessage = expected.Message;
        var actualMessage = actual.Message;
        bool shouldAssertMessage;

        if (expected is ArgumentException argExpected &&
            argExpected.ParamName is string expectedParamName &&
            actual is ArgumentException argActual)
        {
            var actualParamName = argActual.ParamName;
            shouldAssertMessage =
                !actualMessage.StartsWith("The value cannot be an empty string") ||
                !actualMessage.StartsWith($"'{actualParamName}' ('");

            assertMessage();
            assertEquality(expectedParamName, actualParamName);

            return actual;
        }

        if (expectedMessage is not null)
        {
            shouldAssertMessage =
                expected is not ObjectDisposedException ||
                !actualMessage.StartsWith("Cannot access a disposed object");

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
        #endregion
    }
    #endregion

    #region Helpers
    #region Message Getters
    private static string? GetFullName(Exception exception)
    => exception.GetType().FullName;

    protected const string ExpectedExceptionNotThrownMessage =
        "Expected exception was not thrown.";

    private const string UnexpectedExceptionMessage =
        "Unexpected exception type thrown.";

    public static string GetExpectedTypeExceptionNotThrownMessage<TException>(TException expected)
    where TException : notnull, Exception
    => $"Expected exception of type {GetFullName(expected)} was not thrown.";

    public static string GetNotExpectedTypeExceptionThrownMessage<TException>(TException expected, Exception actual)
    where TException : notnull, Exception
    => $"Expected exception of type {GetFullName(expected)}, " +
        $"but exception of type {GetFullName(actual)} was thrown.";

    private static string GetNotExpectedExceptionMessage(Exception exception)
    => $"Did not expect exception to be thrown, " +
        $"but exception of type {GetFullName(exception)} was thrown. " +
        $"Message: '{exception.Message}'";
    #endregion

    #region Exceptions
    protected static InvalidOperationException UnreachableCodePathException
    => new("Unreachable code path.");

    protected static InvalidOperationException GetAssertionFailedException(string message)
    => new($"Assertion failed: {message}");
    #endregion
    #endregion
}
