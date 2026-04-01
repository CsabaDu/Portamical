// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;

namespace Tests.Portamical.Core.Strategy;

[TestClass]
public sealed class PropsCodeTests
{
    // Member count

    [TestMethod]
    public void PropsCode_hasFourMembers()
    {
        Assert.HasCount(4, Enum.GetValues<PropsCode>());
    }

    // Member names and underlying integer values (via TryParse — runtime value)

    [TestMethod]
    [DataRow("All",                  0)]
    [DataRow("TrimTestCaseName",     1)]
    [DataRow("TrimReturnsExpected",  2)]
    [DataRow("TrimThrowsExpected",   3)]
    public void PropsCode_memberName_hasExpectedValue(string name, int expectedValue)
    {
        Assert.IsTrue(Enum.TryParse(name, out PropsCode parsed), $"Member '{name}' not found in PropsCode.");
        Assert.AreEqual(expectedValue, (int)parsed);
    }

    // Undefined value

    [TestMethod]
    public void PropsCode_undefinedValue_isNotDefined()
    {
        Assert.IsFalse(Enum.IsDefined((PropsCode)99));
    }
}
