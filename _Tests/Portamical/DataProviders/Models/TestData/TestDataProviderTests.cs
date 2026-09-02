// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using Sut = global::Portamical.DataProviders.Models.TestData.TestDataProvider<global::Portamical.Core.TestDataTypes.ITestData>;

namespace Tests.Portamical.DataProviders.Models.TestData;

[TestClass]
public class TestDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void Constructor_withCollection_addsAllItemsAsRows()
    {
        var item1 = CreateData("first", 1);
        var item2 = CreateData("second", 2);
        var item3 = CreateData("third", 3);
        ITestData[] collection = [item1, item2, item3];

        var provider = new Sut(collection);
        var rows = provider.GetRows();

        Assert.IsNotNull(provider);
        Assert.HasCount(3, rows);
        Assert.AreSame(item1, rows[0]);
        Assert.AreSame(item2, rows[1]);
        Assert.AreSame(item3, rows[2]);
    }

    [TestMethod]
    public void Constructor_withCollection_registersRowsByTestCaseName()
    {
        var item1 = CreateData("lookup-1", 1);
        var item2 = CreateData("lookup-2", 2);
        ITestData[] collection = [item1, item2];

        var provider = new Sut(collection);

        Assert.AreSame(item1, provider.GetRow(item1.TestCaseName));
        Assert.AreSame(item2, provider.GetRow(item2.TestCaseName));
    }

    [TestMethod]
    public void Constructor_withCollection_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new Sut(nullCollection));
    }

    [TestMethod]
    public void Constructor_withCollection_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        Assert.ThrowsExactly<ArgumentException>(
            () => _ = new Sut(empty));
    }

    [TestMethod]
    public void Constructor_withCollection_duplicateTestCaseNames_throwsArgumentException()
    {
        var first = CreateData("duplicate-name", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("duplicate-name", "result", 99);
        ITestData[] collection = [first, duplicate];

        Assert.ThrowsExactly<ArgumentException>(
            () => _ = new Sut(collection));
    }
}
