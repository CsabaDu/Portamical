// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestBases;

namespace Portamical.xUnit_v3.TestBases;

public abstract class TestBase_xUnit_v3 : TestBase
{
    protected static TException? AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var expectedType = expected.GetType();
        var actual = Record.Exception(attempt);

        if (actual is null)
        {
            Assert.Fail(ExpectedTypeExceptionNotThrownMessage(expectedType));
        }

        if (actual is not TException)
        {
            Assert.Fail(UnexpectedTypeExceptionThrownMessage<TException>(actual.GetType()));

            return null;
        }

        Assert.IsType(expectedType, actual);

        if (expected.Message is string expectedMessage)
        {
            Assert.Equal(expectedMessage, actual!.Message);
        }

        if (AreArgumentExceptionsWithParamNames(
            expected,
            actual,
            out string? expectedParamName,
            out string? actualParamName))
        {
            Assert.Equal(expectedParamName, actualParamName);
        }

        return (TException)actual;
    }
}
