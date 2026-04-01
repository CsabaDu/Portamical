// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Reflection;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit.Attributes;

namespace Tests.Portamical.xUnit.Attributes;

[TestClass]
public class PortamicalDataAttributeTests
{
    private sealed class TestableAttribute : PortamicalBaseDataAttribute
    {
        public TestableAttribute() : base("DummyMember", null) { }

        public object?[]? ExposedConvertDataItem(MethodInfo method, object item)
            => ConvertDataItem(method, item);
    }

    public static void AnchorMethod() { }
    private static MethodInfo AnchorMethodInfo
        => typeof(PortamicalDataAttributeTests).GetMethod(nameof(AnchorMethod))!;

    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void ConvertDataItem_null_returnsNull()
    {
        var attr = new TestableAttribute();
        var result = attr.ExposedConvertDataItem(AnchorMethodInfo, null!);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ConvertDataItem_objectArray_passesThroughUnchanged()
    {
        var attr = new TestableAttribute();
        var array = new object?[] { 1, "two", 3.0 };
        var result = attr.ExposedConvertDataItem(AnchorMethodInfo, array);
        Assert.AreSame(array, result);
    }

    [TestMethod]
    public void ConvertDataItem_iTestData_convertsToArgsInstance()
    {
        var attr = new TestableAttribute();
        var item = CreateData("def");
        var result = attr.ExposedConvertDataItem(AnchorMethodInfo, item);
        var expectedRow = item.ToArgs(ArgsCode.Instance);
        CollectionAssert.AreEqual(expectedRow, result);
    }

    [TestMethod]
    public void ConvertDataItem_unsupportedType_throwsArgumentException()
    {
        var attr = new TestableAttribute();
        Assert.ThrowsExactly<ArgumentException>(
            () => attr.ExposedConvertDataItem(AnchorMethodInfo, 42));
    }

    [TestMethod]
    public void PortamicalDataAttribute_isSealed()
        => Assert.IsTrue(typeof(PortamicalDataAttribute).IsSealed);

    [TestMethod]
    public void PortamicalDataAttribute_inheritsFromPortamicalBaseDataAttribute()
        => Assert.IsTrue(
            typeof(PortamicalDataAttribute).IsSubclassOf(typeof(PortamicalBaseDataAttribute)));

    [TestMethod]
    public void PortamicalDataAttribute_canBeConstructed()
    {
        var attr = new PortamicalDataAttribute("TestMember");
        Assert.IsNotNull(attr);
    }
}
