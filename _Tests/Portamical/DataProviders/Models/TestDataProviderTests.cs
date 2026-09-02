// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using BaseProvider = Portamical.DataProviders.Models.TestDataProvider<Portamical.Core.TestDataTypes.ITestData, string>;

namespace Tests.Portamical.DataProviders.Models;

[TestClass]
public class TestDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    private sealed class ConcreteProvider : BaseProvider
    {
        public int ConversionCount { get; private set; }

        public ConcreteProvider()
        {
        }

        public ConcreteProvider(ITestData testData)
            : base(testData)
        {
        }

        public ConcreteProvider(IEnumerable<ITestData> testDataCollection)
            : base(testDataCollection)
        {
        }

        public override string ConvertRow(ITestData testData)
        {
            ConversionCount++;
            return testData.TestCaseName;
        }
    }

    [TestMethod]
    public void Constructor_withoutArguments_createsEmptyProvider()
    {
        var provider = new ConcreteProvider();

        Assert.AreEqual(0, provider.ConversionCount);
        CollectionAssert.AreEqual(Array.Empty<string>(), provider.GetRows());
        CollectionAssert.AreEqual(Array.Empty<string>(), provider.GetTestCaseNames());
        Assert.IsNull(provider.GetRow("missing"));
    }

    [TestMethod]
    public void Constructor_withSingleItem_populatesInitialRow()
    {
        var item = CreateData("single", 4);
        var provider = new ConcreteProvider(item);

        Assert.AreEqual(1, provider.ConversionCount);
        Assert.AreEqual(item.TestCaseName, provider.GetRow(item.TestCaseName));
        CollectionAssert.AreEqual(new[] { item.TestCaseName }, provider.GetRows());
        CollectionAssert.AreEqual(new[] { item.TestCaseName }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void Constructor_withCollection_populatesConvertedRows()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);

        Assert.AreEqual(2, provider.ConversionCount);
        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetRows());
        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void Constructor_withDuplicateCollection_throwsArgumentException()
    {
        var first = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);

        Assert.ThrowsExactly<ArgumentException>(
            () => _ = new ConcreteProvider([first, duplicate]));
    }
}
