// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.DataProviders;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using System.Collections;

namespace Tests.Portamical.Converters.DataProviders;

[TestClass]
public class CollectionConverter_ToDataProviderTests
{
    private sealed class TestProvider : ITestDataProvider<ITestData>
    {
        public ArgsCode ArgsCode { get; init; }
        public string? TestMethodName { get; init; }
        public List<ITestData> Rows { get; } = [];
        public void AddRow(ITestData testData) => Rows.Add(testData);
        public IEnumerator GetEnumerator() => Rows.GetEnumerator();
    }

    private sealed class TestProviderWithDefaultConstructor : ITestDataProvider<ITestData>
    {
        public ArgsCode ArgsCode { get; init; }
        public string? TestMethodName { get; init; }
        public List<ITestData> Rows { get; } = [];
        public void AddRow(ITestData testData) => Rows.Add(testData);
        public IEnumerator GetEnumerator() => Rows.GetEnumerator();

        public TestProviderWithDefaultConstructor()
        {
            ArgsCode = ArgsCode.Instance;
        }
    }

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
        var provider = collection.ToDataProvider<TestProvider, ITestData>(
            first => new TestProvider
            {
                ArgsCode = ArgsCode.Properties,
                TestMethodName = "TestMethod"
            }.Apply(p => p.Rows.Add(first)));

        Assert.IsNotNull(provider);
        Assert.AreEqual(ArgsCode.Properties, provider.ArgsCode);
        Assert.AreEqual("TestMethod", provider.TestMethodName);
        Assert.HasCount(1, provider.Rows);
        Assert.AreSame(item, provider.Rows[0]);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_multipleElements_addsRemainingViaAddRow()
    {
        var item1 = CreateData("p1");
        var item2 = CreateData("p2");
        var item3 = CreateData("p3");
        ITestData[] collection = [item1, item2, item3];
        var provider = collection.ToDataProvider<TestProvider, ITestData>(
            first => new TestProvider { ArgsCode = ArgsCode.Instance }
                .Apply(p => p.Rows.Add(first)));

        Assert.HasCount(3, provider.Rows);
        Assert.AreSame(item1, provider.Rows[0]);
        Assert.AreSame(item2, provider.Rows[1]);
        Assert.AreSame(item3, provider.Rows[2]);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_deduplicatesBeforeInitializing()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("prov-dup", "result", 77);
        ITestData[] collection = [CreateData("prov-dup"), duplicate];
        var provider = collection.ToDataProvider<TestProvider, ITestData>(
            first => new TestProvider().Apply(p => p.Rows.Add(first)));

        Assert.HasCount(1, provider.Rows);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, TestProvider> nullInit = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDataProvider<TestProvider, ITestData>(nullInit));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider<TestProvider, ITestData>(
                first => new TestProvider().Apply(p => p.Rows.Add(first))));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider<TestProvider, ITestData>(
                first => new TestProvider().Apply(p => p.Rows.Add(first))));
    }

    #endregion

    #region ToDataProvider with new() constraint

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_singleElement_createsProviderAndAddsItem()
    {
        var item = CreateData("default");
        ITestData[] collection = [item];
        var provider = collection.ToDataProvider<TestProviderWithDefaultConstructor, ITestData>();

        Assert.IsNotNull(provider);
        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode); // Set by constructor
        Assert.HasCount(1, provider.Rows);
        Assert.AreSame(item, provider.Rows[0]);
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_multipleElements_addsAllViaAddRow()
    {
        var item1 = CreateData("d1");
        var item2 = CreateData("d2");
        var item3 = CreateData("d3");
        ITestData[] collection = [item1, item2, item3];
        var provider = collection.ToDataProvider<TestProviderWithDefaultConstructor, ITestData>();

        Assert.HasCount(3, provider.Rows);
        Assert.AreSame(item1, provider.Rows[0]);
        Assert.AreSame(item2, provider.Rows[1]);
        Assert.AreSame(item3, provider.Rows[2]);
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_deduplicatesByTestCaseName()
    {
        var duplicate = TestDataFactory.CreateTestData<int>("default-dup", "result", 88);
        ITestData[] collection = [CreateData("default-dup"), duplicate];
        var provider = collection.ToDataProvider<TestProviderWithDefaultConstructor, ITestData>();

        Assert.HasCount(1, provider.Rows);
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider<TestProviderWithDefaultConstructor, ITestData>());
    }

    [TestMethod]
    public void ToDataProvider_withDefaultConstructor_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();
        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider<TestProviderWithDefaultConstructor, ITestData>());
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
        var providerWithInit = collection.ToDataProvider<TestProvider, ITestData>(
            first => new TestProvider().Apply(p => p.Rows.Add(first)));

        // With default constructor
        var providerWithDefault = collection.ToDataProvider<TestProviderWithDefaultConstructor, ITestData>();

        Assert.AreEqual(providerWithInit.Rows.Count, providerWithDefault.Rows.Count);
        Assert.AreSame(item1, providerWithInit.Rows[0]);
        Assert.AreSame(item1, providerWithDefault.Rows[0]);
        Assert.AreSame(item2, providerWithInit.Rows[1]);
        Assert.AreSame(item2, providerWithDefault.Rows[1]);
    }

    #endregion
}

internal static class TestExtensions
{
    public static T Apply<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
