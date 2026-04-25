// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Reflection;
using Portamical.MSTest.Attributes;

namespace Tests.Portamical.MSTest.Attributes;

[TestClass]
public class PortamicalDataAttributeTests
{
    [TestMethod]
    public void Constructor_withSourceName_createsInstance()
    {
        var attr = new PortamicalDataAttribute("MyMethod");
        Assert.IsNotNull(attr);
    }

    [TestMethod]
    public void Constructor_withNullSourceName_throwsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new PortamicalDataAttribute(null!));
    }

    [TestMethod]
    public void Constructor_withEmptySourceName_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => _ = new PortamicalDataAttribute(string.Empty));
    }

    [TestMethod]
    public void Constructor_withSourceType_createsInstance()
    {
        var attr = new PortamicalDataAttribute("MyMethod", DynamicDataSourceType.Method);
        Assert.IsNotNull(attr);
    }

    [TestMethod]
    public void Constructor_withSourceArgs_createsInstance()
    {
        var attr = new PortamicalDataAttribute("MyMethod", "arg1", 42);
        Assert.IsNotNull(attr);
    }

    [TestMethod]
    public void Constructor_withDeclaringType_createsInstance()
    {
        var attr = new PortamicalDataAttribute("MyMethod", typeof(PortamicalDataAttributeTests));
        Assert.IsNotNull(attr);
    }

    [TestMethod]
    public void Constructor_withDeclaringTypeAndSourceArgs_createsInstance()
    {
        var attr = new PortamicalDataAttribute("MyMethod", typeof(PortamicalDataAttributeTests), "arg1");
        Assert.IsNotNull(attr);
    }

    [TestMethod]
    public void Constructor_withDeclaringTypeAndSourceType_createsInstance()
    {
        var attr = new PortamicalDataAttribute("MyMethod", typeof(PortamicalDataAttributeTests), DynamicDataSourceType.Method);
        Assert.IsNotNull(attr);
    }

    [TestMethod]
    public void IgnoreMessage_getSet_roundtrips()
    {
        var attr = new PortamicalDataAttribute("MyMethod")
        {
            IgnoreMessage = "skip reason"
        };
        Assert.AreEqual("skip reason", attr.IgnoreMessage);
    }

    [TestMethod]
    public void DynamicDataDisplayName_getSet_roundtrips()
    {
        var attr = new PortamicalDataAttribute("MyMethod")
        {
            DynamicDataDisplayName = "MyDisplay"
        };
        Assert.AreEqual("MyDisplay", attr.DynamicDataDisplayName);
    }

    [TestMethod]
    public void DynamicDataDisplayNameDeclaringType_getSet_roundtrips()
    {
        var attr = new PortamicalDataAttribute("MyMethod")
        {
            DynamicDataDisplayNameDeclaringType = typeof(PortamicalDataAttributeTests)
        };
        Assert.AreEqual(typeof(PortamicalDataAttributeTests), attr.DynamicDataDisplayNameDeclaringType);
    }

    [TestMethod]
    public void GetDisplayName_nullMethodInfo_throwsArgumentNullException()
    {
        var attr = new PortamicalDataAttribute("MyMethod");
        Assert.ThrowsExactly<ArgumentNullException>(
            () => attr.GetDisplayName(null!, "caseName"));
    }

    [TestMethod]
    public void GetDisplayName_withStringFirstArg_returnsEnhancedDisplayName()
    {
        var attr = new PortamicalDataAttribute("MyMethod");
        MethodInfo methodInfo = typeof(PortamicalDataAttributeTests)
            .GetMethod(nameof(GetDisplayName_withStringFirstArg_returnsEnhancedDisplayName))!;

        string? displayName = attr.GetDisplayName(methodInfo, "MyTestCase");

        Assert.IsNotNull(displayName);
        Assert.Contains("MyTestCase", displayName,
            $"Expected display name to contain 'MyTestCase' but was '{displayName}'");
    }

    [TestMethod]
    public void GetDisplayName_withNonStringFirstArg_returnsFallback()
    {
        var attr = new PortamicalDataAttribute("MyMethod");
        MethodInfo methodInfo = typeof(PortamicalDataAttributeTests)
            .GetMethod(nameof(GetDisplayName_withNonStringFirstArg_returnsFallback))!;

        string? displayName = attr.GetDisplayName(methodInfo, 42, "other");

        Assert.IsNotNull(displayName);
    }

    [TestMethod]
    public void GetDisplayName_withNullDisplayName_returnsFallback()
    {
        var attr = new PortamicalDataAttribute("MyMethod");
        MethodInfo methodInfo = typeof(PortamicalDataAttributeTests)
            .GetMethod(nameof(GetDisplayName_withNullDisplayName_returnsFallback))!;
        string? displayName = attr.GetDisplayName(methodInfo, []);

        Assert.IsNotNull(displayName);
    }

    [TestMethod]
    public void GetDisplayName_withNullData_returnsFallback()
    {
        var attr = new PortamicalDataAttribute("MyMethod");
        MethodInfo methodInfo = typeof(PortamicalDataAttributeTests)
            .GetMethod(nameof(GetDisplayName_withNullData_returnsFallback))!;

        string? displayName = attr.GetDisplayName(methodInfo, null);

        Assert.IsNull(displayName);
    }
}
