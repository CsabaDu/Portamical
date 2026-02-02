// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Assertions;

namespace Portamical.NUnit.Assertions;

public abstract class PortamicalAssert : PortamicalAssertBase
{
    public static void AssertMultiple(Action assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            assertions();
        }
    }

    public static async Task AssertMultipleAsync(Func<Task> assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            await assertions().ConfigureAwait(false);
        }
    }

    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    {
        TException actual = default!;

        AssertMultiple(() =>
        {
            actual = ThrowsDetails(
                attempt,
                expected,
                assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
                assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
                assertFail: Assert.Fail,
                catchException: att => Assert.Catch(() => att()));
        });

        return actual;
    }
}
