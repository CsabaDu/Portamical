// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestBases;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase_MSTest : TestBase
{
    protected static TException? AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var expectedType = expected.GetType();
        try
        {
            attempt();

            Assert.Fail(ExpectedTypeExceptionNotThrownMessage(expectedType));
        }
        catch (TException actual)
        {
            Assert.IsInstanceOfType(actual, expectedType);

            if (expected.Message is string expectedMessage)
            {
                Assert.AreEqual(expectedMessage, actual.Message);
            }

            if (AreArgumentExceptionsWithParamNames(
                expected,
                actual,
                out string? expectedParamName,
                out string? actualParamName))
            {
                Assert.AreEqual(expectedParamName, actualParamName);
            }

            return actual;
        }
        catch (Exception actual)
        {
            Assert.Fail(UnexpectedTypeExceptionThrownMessage<TException>(actual.GetType()));
        }

        return null;
    }
}