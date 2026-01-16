// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.xUnit_v3.TestDataTypes;
using Adatamiq.xUnit_v3.TestDataTypes.Model;

namespace Adatamiq.xUnit_v3.DataProviders.Model;

public sealed class TheoryTestData<TTestData>
: TheoryDataBase<ITheoryTestDataRow, TTestData>,
ITheoryTestData<TTestData>
where TTestData : notnull, ITestData
{
    #region Constructors
    internal TheoryTestData(
        TheoryTestDataRow<TTestData> theoryTestDataRow,
        ArgsCode argsCode,
        string? testMethodName)
    {
        Validator.ThrowIfNull(theoryTestDataRow, nameof(theoryTestDataRow));
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;

        Add(theoryTestDataRow);
    }
    #endregion

    #region Fields
    private readonly HashSet<INamedCase> _namedCase =
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

        if (_namedCase.Add(row))
        {
            base.Add(row);
        }
    }

    public ITheoryTestDataRow Convert(
        TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    => new TheoryTestDataRow<TTestData>(
        testData,
        argsCode,
        testMethodName);

    protected override ITheoryTestDataRow Convert(TTestData row)
    => Convert(row, ArgsCode, TestMethodName);

    internal static void AddRow(
        TheoryTestData<TTestData> theoryTestData,
        TheoryTestDataRow<TTestData> theoryTestDataRow)
    => theoryTestData.Add(theoryTestDataRow);
    #endregion
}
