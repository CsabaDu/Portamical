// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;

namespace Portamical.MSTest.DataProviders;

public interface ITestDataProvider : IDataProvider<object[]>, ITestDataSource
{
    string[] TestCategories { get; }
}