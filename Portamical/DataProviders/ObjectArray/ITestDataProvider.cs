// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.ObjectArray;

public interface ITestDataProvider<TTestData>
: IDataProvider<TTestData, object?[]>
where TTestData : notnull, ITestData
{
    ArgsCode ArgsCode { get; init; }
    PropsCode PropsCode { get; init; }
}
