// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Reflection;
using Portamical.Core.Strategy;
using Portamical.xUnit_v3.Attributes;

namespace Tests.Portamical.xUnit_v3.Attributes;

[TestClass]
public class PortamicalDataAttributeTests
{
    [TestMethod]
    public void Constructor_validMemberName_setsMemberName()
    {
        var attr = new PortamicalDataAttribute("GetData");
        Assert.AreEqual("GetData", attr.MemberName);
    }

    [TestMethod]
    public void Constructor_nullMemberName_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => new PortamicalDataAttribute(null!));

    [TestMethod]
    public void Constructor_emptyMemberName_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => new PortamicalDataAttribute(string.Empty));

    [TestMethod]
    public void Constructor_withArguments_constructsSuccessfully()
    {
        var attr = new PortamicalDataAttribute("GetData", ArgsCode.Instance);
        Assert.IsNotNull(attr);
        Assert.AreEqual("GetData", attr.MemberName);
    }

    [TestMethod]
    public void PortamicalDataAttribute_isSealed()
        => Assert.IsTrue(typeof(PortamicalDataAttribute).IsSealed);

    [TestMethod]
    public void PortamicalDataAttribute_inheritsFromPortamicalBaseDataAttribute()
        => Assert.IsTrue(
            typeof(PortamicalDataAttribute).IsSubclassOf(typeof(PortamicalBaseDataAttribute)));

    [TestMethod]
    public void PortamicalDataAttribute_attributeUsage_allowsMultiple()
    {
        var usage = typeof(PortamicalDataAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();
        Assert.IsNotNull(usage);
        Assert.IsTrue(usage.AllowMultiple);
    }

    [TestMethod]
    public void PortamicalDataAttribute_attributeUsage_targetsMethod()
    {
        var usage = typeof(PortamicalDataAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();
        Assert.IsNotNull(usage);
        Assert.IsTrue(usage.ValidOn.HasFlag(AttributeTargets.Method));
    }
}
