// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestBases;

namespace Portamical.NUnit.TestBases;

public abstract class TestBase_NUnit : TestBase
{
    protected static void AssertMultiple(Action assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            assertions();
        }
    }

    protected static async Task AssertMultipleAsync(Func<Task> assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            await assertions().ConfigureAwait(false);
        }
    }

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
            AssertMultiple(() =>
            {
                Assert.That(actual, Is.TypeOf(expectedType));

                if (expected.Message is string expectedMessage)
                {
                    Assert.That(actual.Message, Is.EqualTo(expectedMessage));
                }

                if (AreArgumentExceptionsWithParamNames(
                    expected,
                    actual,
                    out string? expectedParamName,
                    out string? actualParamName))
                {
                    Assert.That(actualParamName, Is.EqualTo(expectedParamName));
                }
            });

            return actual;
        }
        catch (Exception actual)
        {
            Assert.Fail(UnexpectedTypeExceptionThrownMessage<TException>(actual.GetType()));
        }

        return null;
    }
}
