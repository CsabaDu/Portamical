// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestHelpers;

namespace Portamical.NUnit.TestHelpers;

public class PortamicalAssert : PortamicalAssertBase
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
    => ThrowsDetails(
        expected,
        attempt,
        assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
        assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
        assertFail: Assert.Fail,
        catchException: (attempt) => Assert.Catch(() => attempt()));
}
