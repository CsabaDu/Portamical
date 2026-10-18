// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.MSTest.DataProviders;

public interface ITestDataProvider : IDataProvider, ITestDataSource
{
    string[]? TestCategories { get; init; }
}

public interface ITestDataProvider<TTestData, TRow> : IDataProvider<TTestData, TRow>, ITestDataProvider
where TTestData : notnull, ITestData
{
}