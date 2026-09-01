// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using Portamical.DataProviders.Models;
using System.Collections;

namespace Tests.Portamical.DataProviders.Models;

[TestClass]
public class DistinctDataProviderBaseTests
{
    private sealed class ConcreteProvider : TestDataProvider<ITestData, string>
    {
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
            => testData.TestCaseName;
    }

#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void AddRow_and_GetRow_useConvertedRow()
    {
        var item = CreateData("single");
        var provider = new ConcreteProvider();

        provider.AddRow(item);

        Assert.AreEqual(item.TestCaseName, provider.GetRow(item.TestCaseName));
        CollectionAssert.AreEqual([item.TestCaseName], provider.GetRows());
    }

    [TestMethod]
    public void Constructor_withCollection_populatesRowsAndNames()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);

        var provider = new ConcreteProvider([first, second]);

        CollectionAssert.AreEqual(
            [first.TestCaseName, second.TestCaseName],
            provider.GetRows());
        CollectionAssert.AreEqual(
            [first.TestCaseName, second.TestCaseName],
            provider.GetTestCaseNames());
    }

    [TestMethod]
    public void AddRow_duplicateTestCaseName_throwsArgumentException()
    {
        var provider = new ConcreteProvider(CreateData("dup", 1));
        var duplicate = CreateData("dup", 2);

        Assert.ThrowsExactly<ArgumentException>(() => provider.AddRow(duplicate));
    }

    [TestMethod]
    public void GetEnumerator_returnsConvertedRows()
    {
        var first = CreateData("enum-1", 1);
        var second = CreateData("enum-2", 2);
        var provider = new ConcreteProvider([first, second]);

        var genericRows = provider.ToArray();
        var nongenericRows = ((IEnumerable)provider).Cast<string>().ToArray();

        CollectionAssert.AreEqual([first.TestCaseName, second.TestCaseName], genericRows);
        CollectionAssert.AreEqual(genericRows, nongenericRows);
    }
}
