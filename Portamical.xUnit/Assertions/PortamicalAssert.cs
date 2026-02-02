// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Assertions;

namespace Portamical.xUnit.Assertions;

public abstract class PortamicalAssert : PortamicalAssertBase
{
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(
        attempt,
        expected,
        assertIsType: Assert.IsType,
        assertEquality: Assert.Equal,
        assertFail: Assert.Fail,
        catchException: Record.Exception);
}
