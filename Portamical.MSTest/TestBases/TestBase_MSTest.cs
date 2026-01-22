// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestBases;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase_MSTest : TestBase
{
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
            Assert.IsInstanceOfType(actual, expectedType);

            AssertMetadataEquality(
                expected,
                actual,
                assertEquality);

            return actual;
        }
        catch (Exception actual)
        {
            Assert.Fail(UnexpectedTypeExceptionThrownMessage<TException>(actual.GetType()));
        }

        throw new InvalidOperationException("Unreachable code path.");

        #region Local methods
        static void assertEquality(string expectedString, string? actualString)
        => Assert.AreEqual(expectedString, actualString);
        #endregion
    }
}