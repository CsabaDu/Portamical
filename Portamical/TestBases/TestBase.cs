// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Strategy;
using Portamical.TestDataTypes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace Portamical.TestBases;

public abstract class TestBase
{
    protected TestBase()
    {
    }

    protected TestBase(ArgsCode argsCode) : this()
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
    }

    protected ArgsCode ArgsCode { get; private set; } = AsInstance;

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
        string expectedTypeName = expectedType.Name;

        if (actual is null)
        {
            assertFail(ExpectedTypeExceptionNotThrownMessage(expectedType));
        }

        expectedTypeName = typeof(TException).Name;

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

    //protected static bool AreExceptionsWithMessages<TException>(
    //    TException expected,
    //    TException actual,
    //    [NotNullWhen(true)] out string? expectedMessage,
    //    out string? actualMessage)
    //where TException : Exception
    //{
    //    expectedMessage =
    //        actualMessage = null;

    //    if (expected.Message is null)
    //    {
    //        return false;
    //    }

    //    expectedMessage = expected.Message;
    //    actualMessage = actual.Message;

    //    return true;
    //}

    //protected static bool AreArgumentExceptionsWithParamNames<TException>(
    //    TException expected,
    //    TException actual,
    //    [NotNullWhen(true)] out string? expectedParamName,
    //    out string? actualParamName)
    //where TException : Exception
    //{
    //    expectedParamName =
    //        actualParamName = null;

    //    if (expected is not ArgumentException argExpected
    //        || argExpected.ParamName is null)
    //    {
    //        return false;
    //    }

    //    expectedParamName = argExpected.ParamName;
    //    var argActual = actual as ArgumentException;
    //    actualParamName = argActual?.ParamName;

    //    return true;
    //}

    protected IEnumerable<object?[]> ConvertToArgs<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ConvertToArgs(ArgsCode);
}