// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit.TestBases;

public abstract class TestBase_xUnit(ArgsCode argsCode = ArgsCode.Instance)
: TestBase(argsCode)
{
    protected static TException AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var expectedType = expected.GetType();
        var actual = Record.Exception(attempt);

        AssertActualType<TException>(
            actual,
            expectedType,
            Assert.Fail);

        Assert.IsType(expectedType, actual);

        var typedActual = (TException)actual!;

        AssertMetadataEquality(
            expected,
            typedActual,
            Assert.Equal);

        return typedActual;
    }
}