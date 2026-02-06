// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Assertions;

namespace Portamical.MSTest.Assertions;

public abstract class PortamicalAssert : PortamicalAssertBase
{
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(
        attempt,
        expected,
        catchException: CatchException,
        assertIsType: (e, a) => Assert.AreEqual(e, a.GetType()),
        assertEquality: (e, a) => Assert.AreEqual(e, a),
        assertFail: Assert.Fail);
}
