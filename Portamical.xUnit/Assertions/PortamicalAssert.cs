// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.xUnit.Assertions;

public abstract class PortamicalAssert : Portamical.Assertions.PortamicalAssert
{
    public static void AssertMultiple(Action assertions)
    => assertions();

    public static Task AssertMultipleAsync(Func<Task> assertions)
    => assertions();

    public static void DoesNotThrow(Action attempt)
    => DoesNotThrow(attempt,
        assertFail: Assert.Fail);

    public static void IsTypeOf(Type expected, object actual)
    => IsTypeOf(expected, actual,
        assertEquality: (e, a) => Assert.Equal(e, a));

    public static TException ThrowsDetails<TException>(Action attempt, TException expected)
    where TException : notnull, Exception
    => ThrowsDetails(attempt, expected,
        catchException: Record.Exception,
        assertIsType: Assert.IsType,
        assertEquality: Assert.Equal,
        assertFail: Assert.Fail);
}