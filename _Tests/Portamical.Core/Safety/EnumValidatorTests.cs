// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.ComponentModel;
using Portamical.Core.Safety;

namespace Tests.Portamical.Core.Safety;

[TestClass]
public sealed class EnumValidatorTests
{
    // GetInvalidEnumArgumentException

    [TestMethod]
    public void GetInvalidEnumArgumentException_returnsException_withCorrectParamName()
    {
        DayOfWeek invalidValue = (DayOfWeek)99;
        string paramName = nameof(invalidValue);

        var ex = invalidValue.GetInvalidEnumArgumentException(paramName);

        Assert.AreEqual(paramName, ex.ParamName);
    }

    [TestMethod]
    public void GetInvalidEnumArgumentException_returnsException_withCorrectIntValue()
    {
        DayOfWeek invalidValue = (DayOfWeek)99;

        var ex = invalidValue.GetInvalidEnumArgumentException(nameof(invalidValue));

        Assert.Contains("99", ex.Message);
    }

    [TestMethod]
    public void GetInvalidEnumArgumentException_returnsException_withCorrectEnumType()
    {
        DayOfWeek invalidValue = (DayOfWeek)99;

        var ex = invalidValue.GetInvalidEnumArgumentException(nameof(invalidValue));

        Assert.Contains(nameof(DayOfWeek), ex.Message);
    }

    // Defined

    [TestMethod]
    public void Defined_definedValue_returnsValue()
    {
        DayOfWeek value = DayOfWeek.Wednesday;

        var result = value.Defined(nameof(value));

        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void Defined_undefinedValue_throwsInvalidEnumArgumentException()
    {
        DayOfWeek invalid = (DayOfWeek)999;

        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => invalid.Defined(nameof(invalid)));
    }

    [TestMethod]
    public void Defined_undefinedValue_exceptionHasCorrectParamName()
    {
        DayOfWeek invalid = (DayOfWeek)999;
        string paramName = nameof(invalid);

        var ex = Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => invalid.Defined(paramName));

        Assert.AreEqual(paramName, ex.ParamName);
    }

    [TestMethod]
    public void Defined_undefinedValue_exceptionHasCorrectIntValue()
    {
        DayOfWeek invalid = (DayOfWeek)999;

        var ex = Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => invalid.Defined(nameof(invalid)));

        Assert.Contains("999", ex.Message);
    }
}
