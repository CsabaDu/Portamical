// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.ObjectArray;

namespace Portamical.DataProviders.Models.ObjectArray;

public class TestDataProvider<TTestData>
: DistinctDataProvider<TTestData, object?[]>,
ITestDataProvider<TTestData>
where TTestData : notnull, ITestData
{
    public TestDataProvider(ArgsCode argsCode, PropsCode propsCode) : base()
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        PropsCode = propsCode.Defined(nameof(propsCode));
    }

    public TestDataProvider(TTestData testData, ArgsCode argsCode, PropsCode propsCode)
    : base(testData)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        PropsCode = propsCode.Defined(nameof(propsCode));
    }

    public TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, PropsCode propsCode)
    : base(testDataCollection)
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        PropsCode = propsCode.Defined(nameof(propsCode));
    }

    public ArgsCode ArgsCode { get; init; }
    public PropsCode PropsCode { get; init; }

    public override object?[] ConvertRow(TTestData testData)
    => testData.ToArgs(ArgsCode, PropsCode);
}
