// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using IdentityProvider = Portamical.DataProviders.Models.TestData.TestDataProvider<Portamical.Core.TestDataTypes.ITestData>;

namespace Tests.Portamical.DataProviders.Models.TestData;

[TestClass]
public class TestDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    [TestMethod]
    public void ConvertRow_returnsSameInstance()
    {
        var item = CreateData("identity", 3);
        var provider = new IdentityProvider();

        var row = provider.ConvertRow(item);

        Assert.AreSame(item, row);
    }

    [TestMethod]
    public void Constructor_withSingleItem_populatesInitialRow()
    {
        var item = CreateData("single", 4);
        var provider = new IdentityProvider(item);

        Assert.AreSame(item, provider.GetRow(item.TestCaseName));
        Assert.AreSame(item, provider.Single());
    }

    [TestMethod]
    public void Constructor_withCollection_populatesAllRows()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new IdentityProvider([first, second]);

        CollectionAssert.AreEqual([first, second], provider.GetRows());
    }

    [TestMethod]
    public void AddRange_duplicateTestCaseName_throwsArgumentException()
    {
        var provider = new IdentityProvider();

        Assert.ThrowsExactly<ArgumentException>(() => provider.AddRange([
            CreateData("dup", 1),
            CreateData("dup", 2)]));
    }
}
