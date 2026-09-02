// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Collections;
using Portamical.Core.Identity;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using BaseProvider = Portamical.DataProviders.Models.TestDataProvider<Portamical.Core.TestDataTypes.ITestData, string>;

namespace Tests.Portamical.DataProviders.Models;

[TestClass]
public class DistinctDataProviderBaseTests
{
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

    private sealed class StubTestData(string testCaseName) : ITestData
    {
        public string TestCaseName { get; init; } = testCaseName;

        public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
            => namedCases?.Any(namedCase => Equals(namedCase)) is true;

        public bool Equals(INamedCase? other)
            => string.Equals(TestCaseName, other?.TestCaseName, StringComparison.Ordinal);

        public string? GetDisplayName(string? testMethodName)
            => string.IsNullOrWhiteSpace(testMethodName)
                ? TestCaseName
                : $"{testMethodName}(testData: {TestCaseName})";

        public string GetDefinition()
            => TestCaseName;

        public string GetResult()
            => TestCaseName;

        public object?[] ToArgs(ArgsCode argsCode)
            => [this];

        public object?[] ToArgs(ArgsCode argsCode, PropsCode propsCode)
            => [this];
    }

    private static ITestData CreateData(string testCaseName)
        => new StubTestData(testCaseName);

    [TestMethod]
    public void Constructor_withoutArguments_createsEmptyProvider()
    {
        var provider = new ConcreteProvider();

        Assert.AreEqual(0, provider.ConversionCount);
        CollectionAssert.AreEqual(Array.Empty<string>(), provider.GetRows());
        CollectionAssert.AreEqual(Array.Empty<string>(), provider.GetTestCaseNames());
        Assert.IsNull(provider.GetRow("missing"));
        Assert.IsFalse(provider.GetEnumerator().MoveNext());
    }

    [TestMethod]
    public void Constructor_withSingleItem_addsInitialRow()
    {
        var testData = CreateData("single");
        var provider = new ConcreteProvider(testData);

        Assert.AreEqual(1, provider.ConversionCount);
        Assert.AreEqual(testData.TestCaseName, provider.GetRow(testData.TestCaseName));
        CollectionAssert.AreEqual(new[] { testData.TestCaseName }, provider.GetRows());
        CollectionAssert.AreEqual(new[] { testData.TestCaseName }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void Constructor_withCollection_addsInitialRows()
    {
        var first = CreateData("first");
        var second = CreateData("second");
        var provider = new ConcreteProvider([first, second]);

        Assert.AreEqual(2, provider.ConversionCount);
        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetRows());
        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void AddRow_duplicateName_throwsArgumentException()
    {
        var provider = new ConcreteProvider();
        provider.AddRow(CreateData("duplicate"));

        Assert.ThrowsExactly<ArgumentException>(
            () => provider.AddRow(CreateData("duplicate")));
    }

    [TestMethod]
    public void AddRow_usesOrdinalComparisonForCaseSensitivity()
    {
        var provider = new ConcreteProvider();

        provider.AddRow(CreateData("case"));
        provider.AddRow(CreateData("CASE"));

        CollectionAssert.AreEqual(new[] { "case", "CASE" }, provider.GetRows());
    }

    [TestMethod]
    public void AddRange_nullCollection_throwsArgumentNullException()
    {
        var provider = new ConcreteProvider();
        IEnumerable<ITestData> testDataCollection = null!;

        Assert.ThrowsExactly<ArgumentNullException>(
            () => provider.AddRange(testDataCollection));
    }

    [TestMethod]
    public void AddRange_emptyCollection_throwsArgumentException()
    {
        var provider = new ConcreteProvider();

        Assert.ThrowsExactly<ArgumentException>(
            () => provider.AddRange(Array.Empty<ITestData>()));
    }

    [TestMethod]
    public void AddRange_duplicateInBatch_keepsPreviouslyAddedRows()
    {
        var provider = new ConcreteProvider();
        var first = CreateData("first");
        var second = CreateData("second");
        var duplicate = CreateData("first");

        Assert.ThrowsExactly<ArgumentException>(
            () => provider.AddRange([first, second, duplicate]));

        CollectionAssert.AreEqual(new[] { first.TestCaseName, second.TestCaseName }, provider.GetRows());
        Assert.AreEqual(3, provider.ConversionCount);
        Assert.AreEqual(first.TestCaseName, provider.GetRow(first.TestCaseName));
        Assert.AreEqual(second.TestCaseName, provider.GetRow(second.TestCaseName));
    }

    [TestMethod]
    public void GetRow_withNullName_usesEmptyStringKey()
    {
        var provider = new ConcreteProvider();
        provider.AddRow(CreateData(string.Empty));

        Assert.AreEqual(string.Empty, provider.GetRow(null!));
    }

    [TestMethod]
    public void GetRows_returnsSnapshot()
    {
        var provider = new ConcreteProvider();
        provider.AddRow(CreateData("first"));
        var snapshot = provider.GetRows();

        provider.AddRow(CreateData("second"));

        CollectionAssert.AreEqual(new[] { "first" }, snapshot);
        CollectionAssert.AreEqual(new[] { "first", "second" }, provider.GetRows());
    }

    [TestMethod]
    public void GetTestCaseNames_returnsSnapshot()
    {
        var provider = new ConcreteProvider();
        provider.AddRow(CreateData("first"));
        var snapshot = provider.GetTestCaseNames();

        provider.AddRow(CreateData("second"));

        CollectionAssert.AreEqual(new[] { "first" }, snapshot);
        CollectionAssert.AreEqual(new[] { "first", "second" }, provider.GetTestCaseNames());
    }

    [TestMethod]
    public void GetEnumerator_returnsGenericEnumeratorOverRows()
    {
        var provider = new ConcreteProvider();
        provider.AddRow(CreateData("first"));
        provider.AddRow(CreateData("second"));

        var rows = provider.ToArray();

        CollectionAssert.AreEqual(new[] { "first", "second" }, rows);
    }

    [TestMethod]
    public void NonGenericGetEnumerator_returnsEnumeratorOverRows()
    {
        var provider = new ConcreteProvider();
        provider.AddRow(CreateData("first"));
        provider.AddRow(CreateData("second"));
        var enumerator = ((IEnumerable)provider).GetEnumerator();
        var rows = new List<string>();

        while (enumerator.MoveNext())
        {
            rows.Add((string)enumerator.Current!);
        }

        CollectionAssert.AreEqual(new[] { "first", "second" }, rows);
    }
}
