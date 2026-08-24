// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.TestData;

public interface ITestDataProvider<TTestData>
: ITestDataRegistry<TTestData>, IDataProvider<TTestData>
where TTestData : notnull, ITestData;
