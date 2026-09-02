// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Converters.DataProviders.ObjectArray;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using BaseProvider = global::Portamical.DataProviders.Models.ObjectArray.TestDataProvider<global::Portamical.Core.TestDataTypes.ITestData>;

namespace Tests.Portamical.Converters.DataProviders.ObjectArray;

[TestClass]
public class CollectionConverter_ToDataProviderTests
{
    private sealed class ConcreteProvider : BaseProvider
    {
        public ConcreteProvider(ArgsCode argsCode, PropsCode propsCode)
            : base(argsCode, propsCode)
        {
        }

        public ConcreteProvider(ITestData testData, ArgsCode argsCode, PropsCode propsCode)
            : base(testData, argsCode, propsCode)
        {
        }
    }

#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void ToDataProvider_withInitializer_passesArgsCodeAndPropsCodeToProvider()
    {
        var item = CreateData("init", 1);
        ITestData[] collection = [item];

        var provider = collection.ToDataProvider<ConcreteProvider, ITestData>(
            (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
            ArgsCode.Instance,
            PropsCode.All);

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual(PropsCode.All, provider.PropsCode);
        Assert.HasCount(1, provider.GetRows());
        CollectionAssert.AreEqual(item.ToArgs(default, default), provider.GetRows()[0]);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_multipleElements_addsRemainingViaAddRow()
    {
        var item1 = CreateData("p1", 1);
        var item2 = CreateData("p2", 2);
        var item3 = CreateData("p3", 3);
        ITestData[] collection = [item1, item2, item3];

        var provider = collection.ToDataProvider<ConcreteProvider, ITestData>(
            (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
            ArgsCode.Instance,
            PropsCode.All);

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual(PropsCode.All, provider.PropsCode);
        Assert.HasCount(3, provider.GetRows());
        CollectionAssert.AreEqual(item1.ToArgs(default, default), provider.GetRows()[0]);
        CollectionAssert.AreEqual(item2.ToArgs(default, default), provider.GetRows()[1]);
        CollectionAssert.AreEqual(item3.ToArgs(default, default), provider.GetRows()[2]);
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, PropsCode, ConcreteProvider> nullInit = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDataProvider<ConcreteProvider, ITestData>(
                nullInit,
                ArgsCode.Instance,
                PropsCode.All));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_nullCollection_throwsArgumentNullException()
    {
        IEnumerable<ITestData> nullCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => nullCollection.ToDataProvider<ConcreteProvider, ITestData>(
                (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
                ArgsCode.Instance,
                PropsCode.All));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_emptyCollection_throwsArgumentException()
    {
        var empty = Array.Empty<ITestData>();

        Assert.ThrowsExactly<ArgumentException>(
            () => empty.ToDataProvider<ConcreteProvider, ITestData>(
                (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
                ArgsCode.Instance,
                PropsCode.All));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDataProvider<ConcreteProvider, ITestData>(
                (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
                (ArgsCode)999,
                PropsCode.All));
    }

    [TestMethod]
    public void ToDataProvider_withInitializer_undefinedPropsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDataProvider<ConcreteProvider, ITestData>(
                (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
                ArgsCode.Instance,
                (PropsCode)999));
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_deduplicatesBeforeInitializing()
    {
        var first = CreateData("dup", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("dup", "result", 99);
        ITestData[] collection = [first, duplicate];

        var provider = collection.ToDistinctDataProvider<ConcreteProvider, ITestData>(
            (item, argsCode, propsCode) => new ConcreteProvider(item, argsCode, propsCode),
            ArgsCode.Instance,
            PropsCode.All);

        Assert.AreEqual(ArgsCode.Instance, provider.ArgsCode);
        Assert.AreEqual(PropsCode.All, provider.PropsCode);
        Assert.HasCount(1, provider.GetRows());
        CollectionAssert.AreEqual(first.ToArgs(default, default), provider.GetRows()[0]);
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

        var provider = collection.ToDistinctDataProvider<ConcreteProvider, ITestData>(
            (item, argsCode, propsCode) => new ConcreteProvider(item, argsCode, propsCode),
            ArgsCode.Instance,
            PropsCode.All);

        Assert.HasCount(4, provider.GetRows());
        CollectionAssert.AreEqual(item1.ToArgs(default, default), provider.GetRows()[0]);
        CollectionAssert.AreEqual(item2.ToArgs(default, default), provider.GetRows()[1]);
        CollectionAssert.AreEqual(item3.ToArgs(default, default), provider.GetRows()[2]);
        CollectionAssert.AreEqual(item4.ToArgs(default, default), provider.GetRows()[3]);
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_nullInitFunction_throwsArgumentNullException()
    {
        ITestData[] collection = [CreateData("v")];
        Func<ITestData, ArgsCode, PropsCode, ConcreteProvider> nullInit = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => collection.ToDistinctDataProvider<ConcreteProvider, ITestData>(
                nullInit,
                ArgsCode.Instance,
                PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_undefinedArgsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctDataProvider<ConcreteProvider, ITestData>(
                (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
                (ArgsCode)999,
                PropsCode.All));
    }

    [TestMethod]
    public void ToDistinctDataProvider_withInitializer_undefinedPropsCode_throwsInvalidEnumArgumentException()
    {
        ITestData[] collection = [CreateData("invalid")];

        Assert.ThrowsExactly<System.ComponentModel.InvalidEnumArgumentException>(
            () => collection.ToDistinctDataProvider<ConcreteProvider, ITestData>(
                (first, argsCode, propsCode) => new ConcreteProvider(first, argsCode, propsCode),
                ArgsCode.Instance,
                (PropsCode)999));
    }
}
