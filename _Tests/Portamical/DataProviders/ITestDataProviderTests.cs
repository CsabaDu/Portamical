// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.DataProviders;

namespace Tests.Portamical.DataProviders;

[TestClass]
public class ITestDataProviderTests
{
    private sealed class StubProvider<TTestData> : ITestDataProvider<TTestData>
    where TTestData : notnull, ITestData
    {
        public ArgsCode ArgsCode { get; init; }
        public string? TestMethodName { get; init; }
        public List<TTestData> Rows { get; } = [];
        public void AddRow(TTestData testData) => Rows.Add(testData);
    }

    private static ITestData CreateData(string def = "def")
        => TestDataFactory.CreateTestData<int>(def, "result", 1);

    #region Properties

    [TestMethod]
    public void ArgsCode_init_storesValue()
    {
        var sut = new StubProvider<ITestData> { ArgsCode = ArgsCode.Properties };
        Assert.AreEqual(ArgsCode.Properties, sut.ArgsCode);
    }

    [TestMethod]
    public void ArgsCode_defaultsToInstance()
    {
        var sut = new StubProvider<ITestData>();
        Assert.AreEqual(ArgsCode.Instance, sut.ArgsCode);
    }

    [TestMethod]
    public void TestMethodName_init_storesValue()
    {
        var sut = new StubProvider<ITestData> { TestMethodName = "MyTest" };
        Assert.AreEqual("MyTest", sut.TestMethodName);
    }

    [TestMethod]
    public void TestMethodName_init_acceptsNull()
    {
        var sut = new StubProvider<ITestData> { TestMethodName = null };
        Assert.IsNull(sut.TestMethodName);
    }

    #endregion

    #region AddRow

    [TestMethod]
    public void AddRow_singleCall_storesItem()
    {
        var sut = new StubProvider<ITestData>();
        var item = CreateData();
        sut.AddRow(item);
        Assert.HasCount(1, sut.Rows);
        Assert.AreSame(item, sut.Rows[0]);
    }

    [TestMethod]
    public void AddRow_multipleCalls_storesAllItemsInOrder()
    {
        var sut = new StubProvider<ITestData>();
        var item1 = CreateData("a");
        var item2 = CreateData("b");
        var item3 = CreateData("c");
        sut.AddRow(item1);
        sut.AddRow(item2);
        sut.AddRow(item3);
        Assert.HasCount(3, sut.Rows);
        Assert.AreSame(item1, sut.Rows[0]);
        Assert.AreSame(item2, sut.Rows[1]);
        Assert.AreSame(item3, sut.Rows[2]);
    }

    #endregion

    #region Contravariance

    [TestMethod]
    public void Contravariance_baseProviderAssignableToSpecificType()
    {
        ITestDataProvider<ITestData> general = new StubProvider<ITestData>();
        ITestDataProvider<TestData<int>> specific = general;
        Assert.IsNotNull(specific);
    }

    [TestMethod]
    public void Contravariance_addRowOnSpecificType_callsBaseImplementation()
    {
        var stub = new StubProvider<ITestData>();
        ITestDataProvider<TestData<int>> specific = stub;
        var item = TestDataFactory.CreateTestData<int>("def", "result", 42);
        specific.AddRow(item);
        Assert.HasCount(1, stub.Rows);
        Assert.AreSame(item, stub.Rows[0]);
    }

    [TestMethod]
    public void Contravariance_propertiesAccessibleViaSpecificReference()
    {
        var stub = new StubProvider<ITestData>
        {
            ArgsCode = ArgsCode.Properties,
            TestMethodName = "Test"
        };
        ITestDataProvider<TestData<int>> specific = stub;
        Assert.AreEqual(ArgsCode.Properties, specific.ArgsCode);
        Assert.AreEqual("Test", specific.TestMethodName);
    }

    #endregion
}
