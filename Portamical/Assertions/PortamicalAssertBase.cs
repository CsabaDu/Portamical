// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Assertions;

public abstract class PortamicalAssertBase
{
    #region Helpers
    #region Message Getters
    private static string? GetFullName(Exception exception)
    => exception.GetType().FullName;

    protected const string ExpectedExceptionNotThrownMessage =
        "Expected exception was not thrown.";

    public static string GetExpectedTypeExceptionNotThrownMessage<TException>(TException expected)
    where TException : notnull, Exception
    => $"Expected exception of type {GetFullName(expected)} was not thrown.";

    public static string GetNotExpectedTypeExceptionThrownMessage<TException>(TException expected, Exception actual)
    where TException : notnull, Exception
    => $"Expected exception of type {GetFullName(expected)}, " +
        $"but exception of type {GetFullName(actual)} was thrown.";
    #endregion

    #region Exceptions
    protected static InvalidOperationException UnreachableCodePathException
    => new("Unreachable code path.");

    protected static InvalidOperationException GetAssertionFailedException(string message)
    => new($"Assertion failed: {message}");
    #endregion
    #endregion

    #region Assert Methods
    public static TException ThrowsDetails<TException>(
        TException expected,
        Action attempt,
        Action<Type, Exception> assertIsType,
        Action<string, string?> assertEquality,
        Action<string> assertFail,
        Func<Action, Exception?>? catchException = null)
    where TException : notnull, Exception
    {
        var actual = catchException is null ?
            CatchException(attempt)
            : catchException(attempt);

        var typedActual = AssertActualType(
            expected,
            actual,
            assertIsType,
            assertFail);

        return AssertMetadataEquality(
            expected,
            typedActual,
            assertEquality);
    }

    protected static TException AssertActualType<TException>(
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
        throw GetAssertionFailedException("Unexpected exception type thrown.");
    }

    protected static TException AssertMetadataEquality<TException>(
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
            argExpected.ParamName is string expectedParamName &&
            actual is ArgumentException argActual)
        {
            var actualParamName = argActual.ParamName;

            assertEquality(expectedParamName, actualParamName);
        }

        return actual;
    }

    public static void DoesNotThrow(Action attempt, Action<string> assertFail)
    {
        var exception = CatchException(attempt);
        NotNull(assertFail, nameof(assertFail));

        if (exception is not null)
        {
            assertFail(
                $"Did not expect exception to be thrown, " +
                $"but exception of type {GetFullName(exception)} was thrown. " +
                $"Message: '{exception.Message}'");
        }
    }

    public static Exception? CatchException(Action attempt)
    {
        _ = NotNull(attempt, nameof(attempt));

        try
        {
            attempt();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
    #endregion
}
