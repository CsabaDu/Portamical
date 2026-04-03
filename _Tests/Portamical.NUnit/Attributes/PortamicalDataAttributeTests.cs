// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Attributes;

namespace Tests.Portamical.NUnit.Attributes;

[TestClass]
public class PortamicalDataAttributeTests
{
    private static class DataSourceHelper
    {
        public static IEnumerable<global::NUnit.Framework.TestCaseData> GetTestData()
            => [new global::NUnit.Framework.TestCaseData(1)];
    }

    [TestMethod]
    public void Constructor_sourceName_setsSourceName()
    {
        var attr = new PortamicalDataAttribute("GetData");
        Assert.AreEqual("GetData", attr.SourceName);
    }

    [TestMethod]
    public void Constructor_nullSourceName_throwsArgumentException()
        => Assert.Throws<ArgumentException>(
            () => _ = new PortamicalDataAttribute(null!));

    [TestMethod]
    public void Constructor_emptySourceName_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => _ = new PortamicalDataAttribute(string.Empty));

    [TestMethod]
    public void Constructor_withMethodParams_setsMethodParams()
    {
        var attr = new PortamicalDataAttribute("GetTestData", new object?[] { "p1" });
        Assert.IsNotNull(attr.MethodParams);
        Assert.AreEqual("p1", attr.MethodParams![0]);
    }

    [TestMethod]
    public void Constructor_withSourceType_setsSourceType()
    {
        var attr = new PortamicalDataAttribute(
            typeof(DataSourceHelper),
            nameof(DataSourceHelper.GetTestData));
        Assert.AreEqual(typeof(DataSourceHelper), attr.SourceType);
    }

    [TestMethod]
    public void Constructor_withSourceType_setsSourceName()
    {
        var attr = new PortamicalDataAttribute(
            typeof(DataSourceHelper),
            nameof(DataSourceHelper.GetTestData));
        Assert.AreEqual(nameof(DataSourceHelper.GetTestData), attr.SourceName);
    }

    [TestMethod]
    public void Constructor_structSourceType_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => _ = new PortamicalDataAttribute(typeof(int), "SomeName"));

    [TestMethod]
    public void Constructor_nonExistentMember_throwsArgumentException()
        => Assert.ThrowsExactly<ArgumentException>(
            () => _ = new PortamicalDataAttribute(typeof(DataSourceHelper), "NonExistentMember"));

    [TestMethod]
    public void PortamicalDataAttribute_isSealed()
        => Assert.IsTrue(typeof(PortamicalDataAttribute).IsSealed);

    [TestMethod]
    public void PortamicalDataAttribute_inheritsFromPortamicalBaseDataAttribute()
        => Assert.IsTrue(
            typeof(PortamicalDataAttribute).IsSubclassOf(typeof(PortamicalBaseDataAttribute)));

    [TestMethod]
    public void Constructor_sourceNameOnly_sourceTypeIsNull()
    {
        var attr = new PortamicalDataAttribute("GetData");
        Assert.IsNull(attr.SourceType);
    }

    [TestMethod]
    public void Constructor_sourceNameOnly_methodParamsIsNull()
    {
        var attr = new PortamicalDataAttribute("GetData");
        Assert.IsNull(attr.MethodParams);
    }
}
