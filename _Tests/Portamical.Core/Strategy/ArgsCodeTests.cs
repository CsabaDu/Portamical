// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.Strategy;

[TestClass]
public sealed class ArgsCodeTests
{
    // Member count

    [TestMethod]
    public void ArgsCode_hasTwoMembers()
    {
        Assert.HasCount(2, Enum.GetValues<ArgsCode>());
    }

    // Member names and underlying integer values (via TryParse — runtime value)

    [TestMethod]
    [DataRow("Instance",   0)]
    [DataRow("Properties", 1)]
    public void ArgsCode_memberName_hasExpectedValue(string name, int expectedValue)
    {
        Assert.IsTrue(Enum.TryParse(name, out ArgsCode parsed), $"Member '{name}' not found in ArgsCode.");
        Assert.AreEqual(expectedValue, (int)parsed);
    }

    // Undefined value

    [TestMethod]
    public void ArgsCode_undefinedValue_isNotDefined()
    {
        Assert.IsFalse(Enum.IsDefined((ArgsCode)99));
    }
}
