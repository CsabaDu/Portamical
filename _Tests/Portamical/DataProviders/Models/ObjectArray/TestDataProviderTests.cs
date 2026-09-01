// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using ObjectArrayProvider = Portamical.DataProviders.Models.ObjectArray.TestDataProvider<Portamical.Core.TestDataTypes.ITestData>;

namespace Tests.Portamical.DataProviders.Models.ObjectArray;

[TestClass]
public class TestDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void Constructor_withArgsCodeAndPropsCode_setsConfiguration()
    {
        var provider = new ObjectArrayProvider(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual(PropsCode.TrimTestCaseName, provider.PropsCode);
    }

    [TestMethod]
    public void ConvertRow_usesConfiguredCodes()
    {
        var item = CreateData("props", 3);
        var provider = new ObjectArrayProvider(ArgsCode.Properties, PropsCode.TrimTestCaseName);

        var row = provider.ConvertRow(item);

        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName), row);
    }

    [TestMethod]
    public void Constructor_withSingleItem_populatesInitialRow()
    {
        var item = CreateData("single", 4);
        var provider = new ObjectArrayProvider(item, ArgsCode.Instance, PropsCode.TrimTestCaseName);

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual(PropsCode.TrimTestCaseName, provider.PropsCode);
        CollectionAssert.AreEqual(item.ToArgs(ArgsCode.Instance), provider.GetRow(item.TestCaseName));
    }

    [TestMethod]
    public void AddRow_duplicateTestCaseName_throwsArgumentException()
    {
        var provider = new ObjectArrayProvider(ArgsCode.Instance, PropsCode.All);
        provider.AddRow(CreateData("dup", 1));

        Assert.ThrowsExactly<ArgumentException>(() => provider.AddRow(CreateData("dup", 2)));
    }
}
