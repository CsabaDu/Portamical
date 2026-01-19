// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using static Adatamiq.Identity.Model.NamedCase;

namespace Adatamiq.xUnit_v3.TestDataTypes.Model;

internal class TheoryTestDataRow
: TheoryDataRowBase,
ITheoryTestDataRow
{
    #region Constructors
    private TheoryTestDataRow(
        INamedCase? namedCase,
        string paramName,
        Func<object?[]> getData,
        string? testMethodName)
    {
        namedCase = Guard.ArgumentNotNull(namedCase, paramName);
        TestCaseName = namedCase.TestCaseName;
        TestDisplayName = GetDisplayName(testMethodName);
        _data = getData();
    }

    internal TheoryTestDataRow(
        ITestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    : this(
        namedCase: testData,
        paramName: nameof(testData),
        getData: () => testData.ToArgs(argsCode),
        testMethodName: testMethodName)
    {
    }

    internal TheoryTestDataRow(
        ITheoryTestDataRow other,
        string? testMethodName)
    : this(
        namedCase: other,
        paramName: nameof(other),
        getData: other.GetData,
        testMethodName: testMethodName)
    {
        TestDisplayName ??= other.TestDisplayName;

        Explicit = other.Explicit;
        Skip = other.Skip;
        Label = other.Label;
        SkipType = other.SkipType;
        SkipUnless = other.SkipUnless;
        SkipWhen = other.SkipWhen;
        Timeout = other.Timeout;
        Traits = other.Traits?.ToDictionary(
            kvp => kvp.Key,
            kvp => new HashSet<string>(kvp.Value))
            ?? [];
    }
    #endregion

    #region Fields
    private readonly object?[] _data;
    #endregion

    #region Properties
    public string TestCaseName { get; init; }
    #endregion

    #region Methods
    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    public override bool Equals(object? obj)
    => Equals(obj as INamedCase);

    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    public override int GetHashCode()
    => Comparer.GetHashCode(this);

    #region Non-Public Methods
    protected override sealed object?[] GetData()
    => _data;
    #endregion
    #endregion
}

internal sealed class TheoryTestDataRow<TTestData>(
    TTestData testData,
    ArgsCode argsCode,
    string? testMethodName)
: TheoryTestDataRow(testData, argsCode, testMethodName)
where TTestData : notnull, ITestData;
