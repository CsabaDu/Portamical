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