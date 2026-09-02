// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.DataProviders.TestData;
using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using TestDataProvider = global::Portamical.DataProviders.Models.TestData.TestDataProvider<global::Portamical.Core.TestDataTypes.ITestData>;

namespace Tests.Portamical.Converters.DataProviders;

[TestClass]
public class CollectionConverter_ToDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region ToDataProvider with initializer function

    [TestMethod]
    public void ToDataProvider_withInitializer_singleElement_initializesWithFirstElement()
    {
        var item = CreateData("init");
        ITestData[] collection = [item];
        var provider = collection.ToDataProvider(
            first => new TestDataProvider(first));
        var rows = provider.GetRows();

        Assert.IsNotNull(provider);
        Assert.HasCount(1, rows);
        Assert.AreSame(item, rows[0]);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_multipleElements_addsRemainingViaAddRow()
    {
        var item1 = CreateData("p1");
        var item2 = CreateData("p2");
        var item3 = CreateData("p3");
        ITestData[] collection = [item1, item2, item3];
        var provider = collection.ToDataProvider(
            first => new TestDataProvider(first));
        var rows = provider.GetRows();

        Assert.HasCount(3, rows);
        Assert.AreSame(item1, rows[0]);
        Assert.AreSame(item2, rows[1]);
        Assert.AreSame(item3, rows[2]);
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_deduplicatesBeforeInitializing()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("prov-dup", "result", 77);
        ITestData[] collection = [CreateData("prov-dup"), duplicate];
        var provider = collection.ToDistinctDataProvider(
            first => new TestDataProvider(first));

        Assert.HasCount(1, provider.GetRows());
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_multipleItemsWithDuplicates_filtersCorrectly()
    {
        var item1 = CreateData("unique1", 1);
        var item2 = CreateData("dup-name", 2);
        var item3 = CreateData("unique2", 3);
        var duplicate = TestDataFactory.CreateTestData<int>("dup-name", "result", 99);
        var item4 = CreateData("unique3", 4);
        ITestData[] collection = [item1, item2, item3, duplicate, item4];

        var provider = collection.ToDistinctDataProvider(
            first => new TestDataProvider(first));
        var rows = provider.GetRows();

        Assert.HasCount(4, rows);
        Assert.AreSame(item1, rows[0]);
        Assert.AreSame(item2, rows[1]);
        Assert.AreSame(item3, rows[2]);
        Assert.AreSame(item4, rows[3]);
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_multipleDuplicatesInMiddle_keepsFirstOccurrence()
    {
        var item1 = CreateData("keep1", 1);
        var item2 = CreateData("keep2", 2);
        var dup1 = TestDataFactory.CreateTestData<int>("keep1", "result", 10);
        var dup2 = TestDataFactory.CreateTestData<int>("keep2", "result", 20);
        var item3 = CreateData("keep3", 3);
        ITestData[] collection = [item1, item2, dup1, dup2, item3];

        var provider = collection.ToDistinctDataProvider(
            first => new TestDataProvider(first));
        var rows = provider.GetRows();

        Assert.HasCount(3, rows);
        Assert.AreSame(item1, rows[0]);
        Assert.AreSame(item2, rows[1]);
        Assert.AreSame(item3, rows[2]);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, TestDataProvider> nullInit = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDataProvider(nullInit));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider(
                first => new TestDataProvider(first)));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider(
                first => new TestDataProvider(first)));
    }

    #endregion

    #region ToDataProvider with new() constraint

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_singleElement_createsProviderAndAddsItem()
    {
        var item = CreateData("default");
        ITestData[] collection = [item];
        var provider = collection.ToDataProvider<ITestData, TestDataProvider>();
        var rows = provider.GetRows();

        Assert.IsNotNull(provider);
        Assert.HasCount(1, rows);
        Assert.AreSame(item, rows[0]);
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_multipleElements_addsAllViaAddRow()
    {
        var item1 = CreateData("d1");
        var item2 = CreateData("d2");
        var item3 = CreateData("d3");
        ITestData[] collection = [item1, item2, item3];
        var provider = collection.ToDataProvider<ITestData, TestDataProvider>();
        var rows = provider.GetRows();

        Assert.HasCount(3, rows);
        Assert.AreSame(item1, rows[0]);
        Assert.AreSame(item2, rows[1]);
        Assert.AreSame(item3, rows[2]);
    }

    [TestMethod]
    public void ToDistinctDataProvider_withDefaultConstructor_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData("default-dup", "result", 88);
        ITestData[] collection = [CreateData("default-dup"), duplicate];
        var provider = collection.ToDistinctDataProvider<ITestData, TestDataProvider>();

        Assert.HasCount(1, provider.GetRows());
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider<ITestData, TestDataProvider>());
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider<ITestData, TestDataProvider>());
    }

    #endregion

    #region Comparison between overloads

    [TestMethod]
    public void ToDataProvider_bothOverloads_produceEquivalentResults()
    {
        var item1 = CreateData("compare1");
        var item2 = CreateData("compare2");
        ITestData[] collection = [item1, item2];

        // With initializer
        var providerWithInit = collection.ToDataProvider(
            first => new TestDataProvider(first));

        // With default constructor
        var providerWithDefault = collection.ToDataProvider<ITestData, TestDataProvider>();

        var initRows = providerWithInit.GetRows();
        var defaultRows = providerWithDefault.GetRows();

        Assert.HasCount(defaultRows.Length, initRows);
        Assert.AreSame(item1, initRows[0]);
        Assert.AreSame(item1, defaultRows[0]);
        Assert.AreSame(item2, initRows[1]);
        Assert.AreSame(item2, defaultRows[1]);
    }

    #endregion
}
