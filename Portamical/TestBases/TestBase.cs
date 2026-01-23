// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Strategy;
using Portamical.TestDataTypes;
using static Portamical.Strategy.Validator;

namespace Portamical.TestBases;

public abstract class TestBase(ArgsCode argsCode = ArgsCode.Instance)
{
    protected ArgsCode ArgsCode { get; init; } = argsCode.Defined(nameof(argsCode));

    public static readonly ArgsCode AsInstance = ArgsCode.Instance;
    public static readonly ArgsCode AsProperties = ArgsCode.Properties;

    protected static TException AssertActualType<TException>(
        Exception? actual,
        TException expected,
        Action<Type, Exception> assertIsType,
        Action<string> assertFail)
        where TException : Exception
    {
        var expectedType = NotNull(expected, nameof(expected)).GetType();
        _ = NotNull(assertIsType, nameof(assertIsType));
        _ = NotNull(assertFail, nameof(assertFail));

        if (actual is null)
        {
            assertFail(ExpectedTypeExceptionNotThrownMessage(expectedType));

            throw getAssertionFailedException("expected exception was not thrown.");
        }

        if (actual.GetType() == expectedType && actual is TException typedActual)
        {
            assertIsType(expectedType, typedActual);

            return typedActual;
        }

        assertFail($"Expected exception of type {typeof(TException).Name}, " +
            $"but exception of type {actual.GetType().Name} was thrown.");

        throw getAssertionFailedException("unexpected exception type thrown.");

        #region Local methods
        static InvalidOperationException getAssertionFailedException(string message)
        => new($"Assertion failed: {message}");
        #endregion
    }

    protected static TException AssertMetadataEquality<TException>(
        TException expected,
        TException actual,
        Action<string, string?> assertEquality)
    where TException : Exception
    {
        if (expected.Message is string expectedMessage)
        {
            var actualMessage = actual.Message;
            assertEquality(expectedMessage, actualMessage);
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

    protected static string ExpectedTypeExceptionNotThrownMessage(Type expectedType)
        => $"Expected exception of type {expectedType.Name} was not thrown.";

    protected IReadOnlyCollection<object?[]> ConvertToArgs<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertToArgs(ArgsCode);
}