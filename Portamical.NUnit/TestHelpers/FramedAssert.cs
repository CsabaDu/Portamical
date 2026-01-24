// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestHelpers;

namespace Portamical.NUnit.TestHelpers;

public class FramedAssert : FramedAssertBase
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
        var actual = Assert.Catch(() => attempt());

        return AssertThrowsDetails(
            expected,
            actual,
            assertIsType,
            assertEquality,
            Assert.Fail);

        #region Local methods
        static void assertEquality(string expectedString, string? actualString)
        => Assert.That(actualString, Is.EqualTo(expectedString));

        static void assertIsType(Type expectedType, Exception actual)
        => Assert.That(actual, Is.TypeOf(expectedType));
        #endregion
    }
}
