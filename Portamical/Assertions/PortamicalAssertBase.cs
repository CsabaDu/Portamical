// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Assertions;

public abstract class PortamicalAssertBase
{
    #region Assert Methods
    public static void DoesNotThrow(Action attempt, Action<string> assertFail)
    {
        var exception = CatchException(attempt);
        _ = NotNull(assertFail, nameof(assertFail));

        if (exception is not null)
        {
            assertFail(GetNotExpectedExceptionMessage(exception));
        }
    }

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

    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected,
        Action<Type, Exception> assertIsType,
        Action<string, string?> assertEquality,
        Action<string> assertFail,
        Func<Action, Exception?>? catchException = null)
    where TException : notnull, Exception
    {
        var actual = catchException is null ?
            CatchException(attempt)
            : catchException(attempt);

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

    public static TException ThrowsActualType<TException>(
        TException expected,
        Exception? actual,
        Action<Type, Exception> assertIsType,
        Action<string> assertFail)
    where TException : notnull, Exception
    {
        NotNull(assertFail, nameof(assertFail));
        NotNull(assertIsType, nameof(assertIsType));

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

    public static TException ThrowsMetadataEquality<TException>(
        TException expected,
        TException actual,
        Action<string, string?> assertEquality)
    where TException : notnull, Exception
    {
        NotNull(assertEquality, nameof(assertEquality));

        if (expected.Message is string expectedMessage)
        {
            assertEquality(expectedMessage, actual.Message);
        }

        if (expected is ArgumentException argExpected &&
            argExpected.ParamName is string argExpectedParamName &&
            actual is ArgumentException argActual)
        {
            assertEquality(argExpectedParamName, argActual.ParamName);
        }

        return actual;
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
