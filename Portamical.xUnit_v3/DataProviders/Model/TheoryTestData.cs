// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Portamical.xUnit_v3.DataProviders.Model;

public sealed class TheoryTestData<TTestData>
: TheoryDataBase<ITheoryTestDataRow, TTestData>,
ITheoryTestData<TTestData>
where TTestData : notnull, ITestData
{
    #region Constructors
    internal TheoryTestData(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;

        AddRow(testData);
    }
    #endregion

    #region Fields
    private readonly HashSet<INamedCase> _namedCases =
        new(NamedCase.Comparer);
    #endregion

    #region Properties
    public ArgsCode ArgsCode { get; init; }
    public string? TestMethodName { get; init; }
    #endregion

    #region Methods
    public override void Add(ITheoryTestDataRow row)
    {
        if (row is not TheoryTestDataRow<TTestData>)
        {
            Guard.ArgumentValid(
                "The provided test data row does not match the expected generic 'ITestData' type.",
                row is TheoryTestDataRow<TTestData>,
                nameof(row));
        }

        if (_namedCases.Add(row))
        {
            base.Add(row);
        }
    }

    public ITheoryTestDataRow ConvertRow(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    => new TheoryTestDataRow<TTestData>(
        testData,
        argsCode,
        testMethodName);

    protected override ITheoryTestDataRow Convert(TTestData row)
    => ConvertRow(testData: row, ArgsCode, TestMethodName);

    public void AddRow(TTestData testData)
    => Add(Convert(testData));
    #endregion
}
