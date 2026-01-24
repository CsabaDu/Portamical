// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.TestHelpers;

namespace Portamical.xUnit.TestHelpers;

public abstract class FramedAssert : FramedAssertBase
{
    public static TException ThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : notnull, Exception
    {
        var actual = Record.Exception(attempt);

        return AssertThrowsDetails(
            expected,
            actual,
            Assert.IsType,
            Assert.Equal,
            Assert.Fail);
    }
}
