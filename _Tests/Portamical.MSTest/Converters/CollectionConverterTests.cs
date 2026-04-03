// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.MSTest.Converters;

namespace Tests.Portamical.MSTest.Converters;

[TestClass]
public class CollectionConverterTests
{
    private static TestData<int> MakeData(string definition, int arg = 0)
        => TestDataFactory.CreateTestData<int>(definition, "ok", arg);

    [TestMethod]
    public void ToArgsWithTestCaseName_withArgsCodeInstance_firstElementIsTestDataObject()
    {
        var td = MakeData("TestCase1");
        var data = new[] { td };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Instance);

        Assert.HasCount(1, result);
        object?[] row = result.First();
        Assert.IsInstanceOfType<TestData<int>>(row[0]);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_withArgsCodeProperties_prependsTestCaseName()
    {
        var td = MakeData("TestCase1", 42);
        var data = new[] { td };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Properties);

        Assert.HasCount(1, result);
        object?[] row = result.First();
        Assert.AreEqual(td.TestCaseName, row[0]);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_withArgsCodeInstance_rowHasSingleElement()
    {
        var td = MakeData("TestCase1");
        var data = new[] { td };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Instance);

        object?[] row = result.First();
        Assert.AreEqual(1, row.Length);
        Assert.IsInstanceOfType<TestData<int>>(row[0]);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_withMultipleItems_returnsAllRows()
    {
        var data = new[]
        {
            MakeData("Case1"),
            MakeData("Case2"),
            MakeData("Case3")
        };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Instance);

        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_withDuplicateTestCaseNames_deduplicates()
    {
        // "Case1" + "ok" produces the same TestCaseName regardless of arg value
        var data = new[]
        {
            MakeData("Case1", 1),
            MakeData("Case1", 99),
            MakeData("Case2", 2)
        };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Instance);

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_deduplication_keepsFirstOccurrence()
    {
        var td1 = MakeData("Case1", 1);
        var td2 = MakeData("Case1", 99);
        var data = new[] { td1, td2 };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Properties);

        Assert.HasCount(1, result);
        object?[] row = result.First();
        Assert.AreEqual(td1.TestCaseName, row[0]);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_returnsReadOnlyCollection()
    {
        var data = new[] { MakeData("Case1") };

        var result = data.ToArgsWithTestCaseName(ArgsCode.Instance);

        Assert.IsInstanceOfType<IReadOnlyCollection<object?[]>>(result);
    }

    [TestMethod]
    public void ToArgsWithTestCaseName_emptyCollection_throwsArgumentException()
    {
        var data = Array.Empty<TestData<int>>();

        Assert.ThrowsExactly<ArgumentException>(
            () => data.ToArgsWithTestCaseName(ArgsCode.Instance));
    }
}
