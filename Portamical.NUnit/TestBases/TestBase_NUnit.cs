// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.NUnit.TestBases;

public abstract class TestBase_NUnit(ArgsCode argsCode = ArgsCode.Instance)
: TestBase(argsCode)
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

    protected static TException AssertThrowsDetails<TException>(
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

                AssertMetadataEquality(
                    expected,
                    actual,
                    assertEquality);
            });

            return actual;
        }
        catch (Exception actual)
        {
            Assert.Fail(UnexpectedTypeExceptionThrownMessage<TException>(actual.GetType()));
        }

        throw new InvalidOperationException("Unreachable code path.");

        #region Local methods
        static void assertEquality(string expectedString, string? actualString)
        => Assert.That(actualString, Is.EqualTo(expectedString));
        #endregion
    }
}
