// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using System.ComponentModel;
using DataProvider = Portamical.DataProviders.Models.ObjectArray.TestDataProvider<Portamical.Core.TestDataTypes.ITestData>;

namespace Tests.Portamical.DataProviders.Models.ObjectArray;

[TestClass]
public class TestDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void Constructor_setsArgsCodeAndPropsCode()
    {
        var provider = new DataProvider(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual(PropsCode.TrimTestCaseName, provider.PropsCode);
    }

    [TestMethod]
    public void ConvertRow_usesConfiguredArgsCodeAndPropsCode()
    {
        var item = CreateData("props", 3);
        var provider = new DataProvider(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        var row = provider.ConvertRow(item);

        CollectionAssert.AreEqual(
            item.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName),
            row);
    }

    [TestMethod]
    public void Constructor_withSingleItem_populatesInitialRow_usingDefaultCodesDuringBaseConstruction()
    {
        var item = CreateData("single", 4);
        var provider = new DataProvider(item, ArgsCode.Properties, PropsCode.TrimTestCaseName);

        var row = provider.GetRow(item.TestCaseName);

        Assert.IsNotNull(row);
        CollectionAssert.AreEqual(
            item.ToArgs(default, default),
            row);

        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual(PropsCode.TrimTestCaseName, provider.PropsCode);
    }

    [TestMethod]
    public void Constructor_withCollection_populatesConvertedRows_usingDefaultCodesDuringBaseConstruction()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new DataProvider(
            [first, second],
            ArgsCode.Properties,
            PropsCode.TrimTestCaseName);

        var firstRow = provider.GetRow(first.TestCaseName);
        var secondRow = provider.GetRow(second.TestCaseName);

        Assert.IsNotNull(firstRow);
        Assert.IsNotNull(secondRow);
        Assert.AreEqual(2, provider.GetRows().Length);

        CollectionAssert.AreEqual(
            first.ToArgs(default, default),
            firstRow);

        CollectionAssert.AreEqual(
            second.ToArgs(default, default),
            secondRow);

        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual(PropsCode.TrimTestCaseName, provider.PropsCode);
    }

    [TestMethod]
    public void Constructor_withUndefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => _ = new DataProvider((ArgsCode)99, PropsCode.All));
    }

    [TestMethod]
    public void Constructor_withUndefinedPropsCode_throwsInvalidEnumArgumentException()
    {
        Assert.ThrowsExactly<InvalidEnumArgumentException>(
            () => _ = new DataProvider(ArgsCode.Instance, (PropsCode)99));
    }
}