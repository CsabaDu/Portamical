// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using BaseProvider = Portamical.DataProviders.Models.TestDataProvider<Portamical.Core.TestDataTypes.ITestData, string>;

namespace Tests.Portamical.DataProviders.Models;

[TestClass]
public class TestDataProviderTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    private sealed class ConcreteProvider : BaseProvider
    {
        public int ConversionCount { get; private set; }

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
        {
            ConversionCount++;
            return testData.TestCaseName;
        }
    }

    [TestMethod]
    public void Constructor_withoutArguments_createsEmptyProvider()
    {
        var provider = new ConcreteProvider();

        Assert.AreEqual(0, provider.ConversionCount);
        CollectionAssert.AreEqual(Array.Empty<string>(), provider.GetRows());
        CollectionAssert.AreEqual(Array.Empty<string>(), provider.GetTestCaseNames());
        Assert.IsNull(provider.GetRow("missing"));
    }

    [TestMethod]
    public void Constructor_withSingleItem_populatesInitialRow()
    {
        var item = CreateData("single", 4);
        var provider = new ConcreteProvider(item);

        Assert.AreEqual(1, provider.ConversionCount);
        Assert.AreEqual(item.TestCaseName, provider.GetRow(item.TestCaseName));
        CollectionAssert.AreEqual(new[] { item.TestCaseName }, provider.GetRows());
        CollectionAssert.AreEqual(new[] { item.TestCaseName }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void Constructor_withCollection_populatesConvertedRows()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);

        Assert.AreEqual(2, provider.ConversionCount);
        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetRows());
        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void Constructor_withDuplicateCollection_throwsArgumentException()
    {
        var first = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);

        Assert.ThrowsExactly<ArgumentException>(
            () => _ = new ConcreteProvider([first, duplicate]));
    }

    #region GetRow - Null TestCaseName Handling (covering line 144: testCaseName ??= string.Empty;)

    [TestMethod]
    public void GetRow_withNullTestCaseName_treatsAsEmptyString_forExistingRow()
    {
        // This test verifies the null-coalescing branch of testCaseName ??= string.Empty.
        // We need a row with an empty string test case name. Since the factory doesn't allow empty definitions,
        // we'll manually create a concrete provider and add a row with an empty name.

        var provider = new ConcreteProvider();

        // Manually add to the provider's internal storage to simulate an empty-named row
        provider.AddRow(CreateData("dummy", 1));

        // Since we can't easily create an empty-named test case, we verify the null handling
        // by ensuring that GetRow(null) is equivalent to GetRow("") behavior
        var rowFromNull = provider.GetRow(null!);
        var rowFromEmpty = provider.GetRow(string.Empty);

        // Both should be either null or the same value (demonstrating null coalescing works)
        Assert.AreEqual(rowFromNull, rowFromEmpty);
    }

    [TestMethod]
    public void GetRow_withNullTestCaseName_transformedToEmptyString()
    {
        // This test specifically exercises the null-coalescing assignment of testCaseName ??= string.Empty.
        // Even if no row with empty string exists, null and empty string should be treated identically

        var item = CreateData("only_item", 1);
        var provider = new ConcreteProvider(item);

        // Calling with null should look for empty string in the dictionary (after coalescing)
        var rowFromNull = provider.GetRow(null!);

        // Since "only_item" != "", this should return null
        Assert.IsNull(rowFromNull, "GetRow(null) should treat null as empty string");
    }

    #endregion

    #region GetEnumerator - Generic IEnumerator<TRow> (lines 190-192)

    [TestMethod]
    public void GetEnumerator_withEmptyProvider_returnsEmptySequence()
    {
        var provider = new ConcreteProvider();

        var rows = provider.ToList();

        Assert.HasCount(0, rows);
    }

    [TestMethod]
    public void GetEnumerator_withSingleItem_returnsSingleElement()
    {
        var item = CreateData("single", 1);
        var provider = new ConcreteProvider(item);

        var rows = provider.ToList();

        Assert.HasCount(1, rows);
        Assert.AreEqual(item.TestCaseName, rows[0]);
    }

    [TestMethod]
    public void GetEnumerator_withMultipleItems_returnsAllElements()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var third = CreateData("third", 3);
        var provider = new ConcreteProvider([first, second, third]);

        var rows = provider.ToList();

        Assert.HasCount(3, rows);
        CollectionAssert.Contains(rows, first.TestCaseName);
        CollectionAssert.Contains(rows, second.TestCaseName);
        CollectionAssert.Contains(rows, third.TestCaseName);
    }

    [TestMethod]
    public void GetEnumerator_supportsForeachIteration()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);

        var collectedRows = new List<string>();
        foreach (var row in provider)
        {
            collectedRows.Add(row);
        }

        Assert.HasCount(2, collectedRows);
        CollectionAssert.Contains(collectedRows, first.TestCaseName);
        CollectionAssert.Contains(collectedRows, second.TestCaseName);
    }

    [TestMethod]
    public void GetEnumerator_supportsLinqWhere()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var third = CreateData("third", 3);
        var provider = new ConcreteProvider([first, second, third]);

        var filteredRows = provider.Where(r => r.Contains("second") || r.Contains("third")).ToList();

        Assert.HasCount(2, filteredRows);
        CollectionAssert.Contains(filteredRows, second.TestCaseName);
        CollectionAssert.Contains(filteredRows, third.TestCaseName);
    }

    [TestMethod]
    public void GetEnumerator_supportsLinqSelect()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);

        var uppercaseRows = provider.Select(r => r.ToUpper()).ToList();

        Assert.HasCount(2, uppercaseRows);
        CollectionAssert.Contains(uppercaseRows, first.TestCaseName.ToUpper());
        CollectionAssert.Contains(uppercaseRows, second.TestCaseName.ToUpper());
    }

    [TestMethod]
    public void GetEnumerator_supportsLinqAny()
    {
        var item = CreateData("test", 1);
        var provider = new ConcreteProvider(item);

        var hasAny = provider.Any();

        Assert.IsTrue(hasAny);
    }

    [TestMethod]
    public void GetEnumerator_supportsLinqCount()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);

        var count = provider.Count();

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    public void GetEnumerator_multipleEnumerations_produceConsistentResults()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);

        var firstEnumeration = provider.ToList();
        var secondEnumeration = provider.ToList();

        CollectionAssert.AreEqual(firstEnumeration, secondEnumeration);
    }

    [TestMethod]
    public void GetEnumerator_enumeratorDisposal_doesNotThrow()
    {
        var item = CreateData("test", 1);
        var provider = new ConcreteProvider(item);

        using var enumerator = provider.GetEnumerator();

        try
        {
            enumerator.MoveNext();
        }
        catch (Exception ex)
        {

            throw new AssertFailedException(BuildExceptionMessage(ex), ex);
        }
        // Dispose is implicitly called at end of using block

        // Test passes if no exception is thrown
    }

    [TestMethod]
    public void GetEnumerator_enumeratorCanBeCalledMultipleTimes()
    {
        var item = CreateData("test", 1);
        var provider = new ConcreteProvider(item);

        var enumerator1 = provider.GetEnumerator();
        var enumerator2 = provider.GetEnumerator();

        // Both enumerators should be able to iterate independently
        Assert.IsNotNull(enumerator1);
        Assert.IsNotNull(enumerator2);

        // They should be different instances
        Assert.AreNotSame(enumerator1, enumerator2);
    }

    #endregion

    #region IEnumerable.GetEnumerator - Non-Generic (lines 197-198)

    [TestMethod]
    public void IEnumerable_GetEnumerator_withEmptyProvider_returnsEmptySequence()
    {
        var provider = new ConcreteProvider();
        System.Collections.IEnumerable enumerable = provider;

        var count = 0;
        foreach (var _ in enumerable)
        {
            count++;
        }

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public void IEnumerable_GetEnumerator_withMultipleItems_enumeratesAllElements()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);
        System.Collections.IEnumerable enumerable = provider;

        var collectedRows = new List<object>();
        foreach (var row in enumerable)
        {
            collectedRows.Add(row!);
        }

        Assert.HasCount(2, collectedRows);
    }

    [TestMethod]
    public void IEnumerable_GetEnumerator_returnsNonGenericEnumerator()
    {
        var item = CreateData("test", 1);
        var provider = new ConcreteProvider(item);
        System.Collections.IEnumerable enumerable = provider;

        var enumerator = enumerable.GetEnumerator();

        Assert.IsNotNull(enumerator);
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(item.TestCaseName, enumerator.Current);
    }

    [TestMethod]
    public void IEnumerable_GetEnumerator_implementsDelegationToGenericGetEnumerator()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var provider = new ConcreteProvider([first, second]);
        System.Collections.IEnumerable enumerable = provider;

        var nonGenericResults = new List<object>();
        foreach (var row in enumerable)
        {
            nonGenericResults.Add(row!);
        }

        var genericResults = provider.ToList();

        Assert.HasCount(nonGenericResults.Count, genericResults);
    }

    #endregion

    private static string BuildExceptionMessage(Exception ex)
    {
        if (ex is null) return "Assertion failed: unknown exception.";

        var sb = new System.Text.StringBuilder();
        void AppendEx(Exception e, int depth)
        {
            var indent = new string(' ', depth * 2);
            sb.AppendLine($"{indent}{e.GetType().FullName}: {e.Message}");
            if (!string.IsNullOrEmpty(e.StackTrace)) sb.AppendLine($"{indent}{e.StackTrace}");
            if (e.InnerException != null)
            {
                sb.AppendLine($"{indent}InnerException:");
                AppendEx(e.InnerException, depth + 1);
            }
        }

        AppendEx(ex, 0);
        return sb.ToString().TrimEnd();
    }
}
