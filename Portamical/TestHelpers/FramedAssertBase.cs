// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using static Portamical.Strategy.Validator;

namespace Portamical.TestHelpers;

public abstract class FramedAssertBase
{
    protected const string ExpectedExceptionNotThrownMessage =
        "Expected exception was not thrown.";

    protected static InvalidOperationException UnreachableCodePathException
    => new("Unreachable code path.");

    protected static InvalidOperationException GetAssertionFailedException(string message)
    => new($"Assertion failed: {message}");

    private static string? GetFullName(Exception exception)
    => exception.GetType().FullName;

    protected static string GwetExpectedTypeExceptionNotThrownMessage<TException>(TException expected)
    where TException : notnull, Exception
    => $"Expected exception of type {GetFullName(expected)} was not thrown.";

    protected static TException AssertThrowsDetails<TException>(
        TException expected,
        Exception? actual,
        Action<Type, Exception> assertIsType,
        Action<string, string?> assertEquality,
        Action<string> assertFail)
    where TException : notnull, Exception
    {
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
        if (actual is null)
        {
            NotNull(assertFail, nameof(assertFail))(
                GwetExpectedTypeExceptionNotThrownMessage(expected));

            throw GetAssertionFailedException(ExpectedExceptionNotThrownMessage);
        }

        var expectedType = expected.GetType();

        if (actual.GetType() == expectedType && actual is TException typedActual)
        {
            NotNull(assertIsType, nameof(assertIsType))(
                expectedType, typedActual);

            return typedActual;
        }

        assertFail(
            $"Expected exception of type {GetFullName(expected)}, " +
            $"but exception of type {GetFullName(actual)} was thrown.");

        throw GetAssertionFailedException("Unexpected exception type thrown.");
    }

    protected static TException AssertMetadataEquality<TException>(
        TException expected,
        TException actual,
        Action<string, string?> assertEquality)
    where TException : notnull, Exception
    {
        _ = NotNull(assertEquality, nameof(assertEquality));

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
        try
        {
            attempt();
        }
        catch (Exception actual)
        {
            NotNull(assertFail, nameof(assertFail))(
                $"Did not expect exception to be thrown, " +
                $"but exception of type {GetFullName(actual)} was thrown. " +
                $"Message: '{actual.Message}'");
        }
    }
}
