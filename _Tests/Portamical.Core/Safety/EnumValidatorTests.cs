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

#pragma warning disable MSTEST0046 // Use StringAssert for string-based assertion clarity
        StringAssert.Contains(ex.Message, "99");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void GetInvalidEnumArgumentException_returnsException_withCorrectEnumType()
    {
        DayOfWeek invalidValue = (DayOfWeek)99;

        var ex = invalidValue.GetInvalidEnumArgumentException(nameof(invalidValue));

#pragma warning disable MSTEST0046 // Use StringAssert for string-based assertion clarity
        StringAssert.Contains(ex.Message, nameof(DayOfWeek));
#pragma warning restore MSTEST0046
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

#pragma warning disable MSTEST0046 // Use StringAssert for string-based assertion clarity
        StringAssert.Contains(ex.Message, "999");
#pragma warning restore MSTEST0046
    }
}
