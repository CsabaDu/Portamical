// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using static Portamical.Core.Strategy.Validator;

namespace Portamical.MSTest.Assertions;

public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    public static void DoesNotThrow(Action attempt)
    => DoesNotThrow(
        NotNull(attempt, nameof(attempt)),
        assertFail: Assert.Fail);

    public static void IsTypeOf(Type expected, object actual)
    => IsTypeOf(
        expected,
        actual,
        assertEquality: (e, a) => Assert.AreEqual(e, a));

    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(
        attempt,
        expected,
        catchException: CatchException,
        assertIsType: IsTypeOf,
        assertEquality: (e, a) => Assert.AreEqual(e, a),
        assertFail: Assert.Fail);
}
