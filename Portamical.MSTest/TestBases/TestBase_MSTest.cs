// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Strategy;
using Portamical.TestBases;

namespace Portamical.MSTest.TestBases;

public abstract class TestBase_MSTest(ArgsCode argsCode = ArgsCode.Instance)
: TestBase(argsCode)
{
    protected static TException AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var expectedType =
            Validator.NotNull(expected, nameof(expected))
            .GetType();

        try
        {
            attempt();

            Assert.Fail(ExpectedTypeExceptionNotThrownMessage(expectedType));

            throw new InvalidOperationException("Unreachable code path.");
        }
        catch (Exception actual)
        {
            var typedActual = AssertActualType(
                actual,
                expected,
                assertIsType,
                Assert.Fail);

            return AssertMetadataEquality(
                expected,
                typedActual,
                assertEquality);
        }

        #region Local methods
        static void assertEquality(string expectedString, string? actualString)
        => Assert.AreEqual(expectedString, actualString);

        static void assertIsType(Type expectedType, Exception actual)
        => Assert.AreEqual(expectedType, actual.GetType());
        #endregion
    }
}