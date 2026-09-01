// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using BaseProvider = Portamical.DataProviders.Models.TypedRow.TestDataProvider<Portamical.Core.TestDataTypes.ITestData, string>;

namespace Tests.Portamical.DataProviders.Models.TypedRow;

[TestClass]
public class TestDataProviderTests
{
    private sealed class ConcreteProvider : BaseProvider
    {
        public ConcreteProvider(ArgsCode argsCode, string? testMethodName)
            : base(argsCode, testMethodName)
        {
        }

        public ConcreteProvider(ITestData testData, ArgsCode argsCode, string? testMethodName)
            : base(testData, argsCode, testMethodName)
        {
        }

        public ConcreteProvider(IEnumerable<ITestData> testDataCollection, ArgsCode argsCode, string? testMethodName)
            : base(testDataCollection, argsCode, testMethodName)
        {
        }

        public override string ConvertRow(ITestData testData)
            => string.Join(" | ", testData.ToArgs(ArgsCode));
    }

#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void Constructor_setsArgsCodeAndTestMethodName()
    {
        var provider = new ConcreteProvider(ArgsCode.Properties, "MyTestMethod");

        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual("MyTestMethod", provider.TestMethodName);
    }

    [TestMethod]
    public void ConvertRow_usesConfiguredArgsCode()
    {
        var item = CreateData("props", 3);
        var provider = new ConcreteProvider(ArgsCode.Properties, "Method");

        var row = provider.ConvertRow(item);

        Assert.AreEqual(string.Join(" | ", item.ToArgs(ArgsCode.Properties)), row);
    }

    [TestMethod]
    public void Constructor_withSingleItem_populatesInitialRow()
    {
        var item = CreateData("single", 4);
        var provider = new ConcreteProvider(item, ArgsCode.Instance, "SingleCtor");

        Assert.AreEqual(item.TestCaseName, provider.GetRow(item.TestCaseName));
        Assert.AreEqual("SingleCtor", provider.TestMethodName);
    }

    [TestMethod]
    public void Constructor_withCollection_populatesConvertedRows()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second], ArgsCode.Instance, null);

        CollectionAssert.AreEqual(
            [first.TestCaseName, second.TestCaseName],
            provider.GetRows());
    }
}
