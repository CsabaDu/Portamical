// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.Assertions;

public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    public static void AssertMultiple(Action assertions)
    => assertions();

    public static Task AssertMultipleAsync(Func<Task> assertions)
    => assertions();

    private static Action<T, T?> AssertEquality<T>()
    => (e, a) => Assert.AreEqual(e, a);

    public static void DoesNotThrow(Action attempt)
    => DoesNotThrow(attempt,
        assertFail: Assert.Fail);

    public static void IsTypeOf(Type expected, object actual)
    => IsTypeOf(expected, actual,
        assertEquality: AssertEquality<object>());

    public static TException ThrowsDetails<TException>(Action attempt, TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(attempt, expected,
        catchException: CatchException,
        assertIsType: IsTypeOf,
        assertEquality: AssertEquality<string>(),
        assertFail: Assert.Fail);
}