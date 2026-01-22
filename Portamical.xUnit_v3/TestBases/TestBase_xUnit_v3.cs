// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestBases;

namespace Portamical.xUnit_v3.TestBases;

public abstract class TestBase_xUnit_v3(ArgsCode argsCode = ArgsCode.Instance)
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