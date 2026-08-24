// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.ObjectArray;

public interface ITestDataProvider<TTestData>
: ITestDataRegistry<TTestData>, IDataProvider<object?[]>
where TTestData : notnull, ITestData
{
    ArgsCode ArgsCode { get; init; }
    PropsCode PropsCode { get; init; }
}
