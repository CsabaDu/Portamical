// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestHelpers;

namespace Portamical.MSTest.TestHelpers;

public class FramedAssert : FramedAssertBase
{
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    {
        try
        {
            attempt();

            Assert.Fail(GwetExpectedTypeExceptionNotThrownMessage(expected));

            throw UnreachableCodePathException;
        }
        catch (Exception actual)
        {
            return AssertThrowsDetails(
                expected,
                actual,
                assertIsType,
                assertEquality,
                Assert.Fail);
        }

        #region Local methods
        static void assertEquality(string expectedString, string? actualString)
        => Assert.AreEqual(expectedString, actualString);

        static void assertIsType(Type expectedType, Exception actual)
        => Assert.AreEqual(expectedType, actual.GetType());
        #endregion
    }
}
