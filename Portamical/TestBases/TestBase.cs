// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Strategy;
using Portamical.TestDataTypes;

namespace Portamical.TestBases;

public abstract class TestBase(ArgsCode argsCode = ArgsCode.Instance)
{
    protected ArgsCode ArgsCode { get; init; } = argsCode.Defined(nameof(argsCode));

    protected static readonly ArgsCode AsInstance = ArgsCode.Instance;
    protected static readonly ArgsCode AsProperties = ArgsCode.Properties;

    protected static void AssertMetadataEquality<TException>(
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
    }

    protected static TException AssertActualType<TException>(
        Exception? actual,
        Type expectedType,
        Action<string> assertFail)
    where TException : Exception
    {
        if (actual is null)
        {
            assertFail(ExpectedTypeExceptionNotThrownMessage(expectedType));
        }

        if (actual is not TException)
        {
            assertFail(UnexpectedTypeExceptionThrownMessage<TException>(actual!.GetType()));
        }

        return (TException)actual;
    }

    protected static string ExpectedTypeExceptionNotThrownMessage(Type expectedType)
    {
        return $"Expected {expectedType.Name} was not thrown.";
    }

    protected static string UnexpectedTypeExceptionThrownMessage<TException>(Type actualType)
    where TException : Exception
    {
        return $"Expected exception of type {typeof(TException).Name}, " +
            $"but exception of type {actualType.Name} was thrown.";
    }

    protected IReadOnlyCollection<object?[]> ConvertToArgs<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertToArgs(ArgsCode);
}