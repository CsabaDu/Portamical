// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit_v3.Assertions;

public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(
        attempt,
        expected,
        catchException: Record.Exception,
        assertIsType: Assert.IsType,
        assertEquality: Assert.Equal,
        assertFail: Assert.Fail);
}