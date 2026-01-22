// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Strategy;
using Portamical.TestDataTypes;
using System.Diagnostics.CodeAnalysis;

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

    protected static string ExpectedTypeExceptionNotThrownMessage(Type expectedType)
    {
        return $"Expected {expectedType.Name} was not thrown.";
    }

    protected static string UnexpectedTypeExceptionThrownMessage<TException>(
        Type actualType)
    where TException : Exception
    {
        return $"Expected exception of type {typeof(TException).Name}, " +
            $"but exception of type {actualType.Name} was thrown.";
    }

    protected static bool AreArgumentExceptionsWithParamNames<TException>(
        TException expected,
        TException actual,
        [NotNullWhen(true)] out string? expectedParamName,
        out string? actualParamName)
    where TException : Exception
    {
        expectedParamName =
            actualParamName = null;

        if (expected is not ArgumentException argExpected
            || argExpected.ParamName is null)
        {
            return false;
        }

        expectedParamName = argExpected.ParamName;
        var argActual = actual as ArgumentException;
        actualParamName = argActual?.ParamName;

        return true;
    }

    protected IEnumerable<object?[]> ConvertToArgs<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(ArgsCode);
}