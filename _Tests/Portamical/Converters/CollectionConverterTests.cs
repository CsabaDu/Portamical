// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using CollectionConverter = global::Portamical.Converters.CollectionConverter;
using Portamical.Core.Factories;
using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;
using Portamical.Core.TestDataTypes;

namespace Tests.Portamical.Converters;

[TestClass]
public class CollectionConverterTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);
#pragma warning restore CA1859

    #region AddConvertedIfDistinct

    [TestMethod]
    public void AddConvertedIfDistinct_newTestCaseName_addsToNamedCasesAndInvokesAddConverted()
    {
        var testData = CreateData("new-case");
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var added = new List<ITestData>();

        CollectionConverter.AddConvertedIfDistinct(
            testData,
            namedCases,
            added.Add);

        Assert.Contains(testData, namedCases);
        Assert.HasCount(1, added);
        Assert.AreSame(testData, added[0]);
    }

    [TestMethod]
    public void AddConvertedIfDistinct_duplicateTestCaseName_doesNotInvokeAddConverted()
    {
        var first = CreateData("dup-case", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("dup-case", "result", 2);
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer) { first };
        var added = new List<ITestData>();

        CollectionConverter.AddConvertedIfDistinct(
            duplicate,
            namedCases,
            added.Add);

        Assert.HasCount(0, added);
        Assert.HasCount(1, namedCases);
    }

    [TestMethod]
    public void AddConvertedIfDistinct_multipleDistinctItems_addsAllInOrder()
    {
        var first = CreateData("multi1", 1);
        var second = CreateData("multi2", 2);
        var third = CreateData("multi3", 3);
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var added = new List<ITestData>();

        foreach (var item in new[] { first, second, third })
        {
            CollectionConverter.AddConvertedIfDistinct(item, namedCases, added.Add);
        }

        Assert.HasCount(3, added);
        Assert.AreSame(first, added[0]);
        Assert.AreSame(second, added[1]);
        Assert.AreSame(third, added[2]);
    }

    [TestMethod]
    public void AddConvertedIfDistinct_mixOfDistinctAndDuplicateItems_addsOnlyFirstOccurrences()
    {
        var first = CreateData("mixed", 1);
        var duplicate = TestDataFactory.CreateTestData<int>("mixed", "result", 2);
        var other = CreateData("other", 3);
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var added = new List<ITestData>();

        foreach (var item in new[] { first, duplicate, other })
        {
            CollectionConverter.AddConvertedIfDistinct(item, namedCases, added.Add);
        }

        Assert.HasCount(2, added);
        Assert.AreSame(first, added[0]);
        Assert.AreSame(other, added[1]);
    }

    [TestMethod]
    public void AddConvertedIfDistinct_convertsToDifferentType_invokesAddConvertedWithTestData()
    {
        var testData = CreateData("converted-case");
        var namedCases = new HashSet<INamedCase>(NamedCase.Comparer);
        var names = new List<string>();

        CollectionConverter.AddConvertedIfDistinct(
            testData,
            namedCases,
            td => names.Add(td.TestCaseName));

        Assert.HasCount(1, names);
        Assert.AreEqual(testData.TestCaseName, names[0]);
    }

    #endregion
}
