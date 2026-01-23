// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit.TestBases;

public abstract class TestBase_xUnit(ArgsCode argsCode = ArgsCode.Instance)
: TestBase(argsCode)
{
    public static TException AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var actual = Record.Exception(attempt);

        var typedActual = AssertActualType(
            actual,
            expected,
            Assert.IsType,
            Assert.Fail);

        return AssertMetadataEquality(
            expected,
            typedActual,
            Assert.Equal);
    }
}