// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.MSTest.TestBases;

namespace Tests.Portamical.MSTest.TestBases;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteTestBase : TestBase
    {
        public static IReadOnlyCollection<object?[]> CallConvert<T>(IEnumerable<T> data, ArgsCode argsCode)
            where T : notnull, ITestData
            => Convert(data, argsCode);

        public static IReadOnlyCollection<object?[]> CallConvert<T>(IEnumerable<T> data)
            where T : notnull, ITestData
            => Convert(data);
    }

    private static TestData<int> MakeData(string definition, int arg = 0)
        => TestDataFactory.CreateTestData<int>(definition, "ok", arg);

    [TestMethod]
    public void Convert_withArgsCode_returnsRowsWithTestDataObject()
    {
        var td = MakeData("Case1", 42);
        var data = new[] { td };

        var result = ConcreteTestBase.CallConvert(data, ArgsCode.Instance);

        Assert.HasCount(1, result);
        Assert.IsInstanceOfType<TestData<int>>(result.First()[0]);
    }

    [TestMethod]
    public void Convert_withArgsCode_multipleRows_returnsAll()
    {
        var data = new[] { MakeData("A"), MakeData("B") };

        var result = ConcreteTestBase.CallConvert(data, ArgsCode.Instance);

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void Convert_withArgsCodeProperties_firstElementIsTestCaseName()
    {
        var td = MakeData("CaseName");
        var data = new[] { td };

        var result = ConcreteTestBase.CallConvert(data, ArgsCode.Properties);

        Assert.AreEqual(td.TestCaseName, result.First()[0]);
    }

    [TestMethod]
    public void Convert_withoutArgsCode_returnsRowWithTestDataObject()
    {
        var td = MakeData("Case1");
        var data = new[] { td };

        var result = ConcreteTestBase.CallConvert(data);

        Assert.HasCount(1, result);
        Assert.IsInstanceOfType<TestData<int>>(result.First()[0]);
    }

    [TestMethod]
    public void Convert_withDuplicates_deduplicates()
    {
        // "Dup" + "ok" produces the same TestCaseName for different arg values
        var data = new[]
        {
            MakeData("Dup", 1),
            MakeData("Dup", 2),
            MakeData("Unique")
        };

        var result = ConcreteTestBase.CallConvert(data, ArgsCode.Instance);

        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void Convert_returnsIReadOnlyCollection()
    {
        var td = MakeData("C");
        var data = new[] { td };

        var result = ConcreteTestBase.CallConvert(data, ArgsCode.Instance);

        Assert.IsInstanceOfType<IReadOnlyCollection<object?[]>>(result);
    }
}
