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
        var actual = Assert.Catch(() => attempt());
        var typedActual = AssertActualType(
            actual,
            expected,
            assertIsType,
            Assert.Fail);

        return AssertMetadataEquality(
            expected,
            typedActual,
            assertEquality);

        #region Local methods
        static void assertEquality(string expectedString, string? actualString)
        => Assert.That(actualString, Is.EqualTo(expectedString));

        static void assertIsType(Type expectedType, Exception actual)
        => Assert.That(actual, Is.TypeOf(expectedType));
        #endregion
    }
}
