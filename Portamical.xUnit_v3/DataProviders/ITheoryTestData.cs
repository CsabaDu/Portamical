// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;
using Portamical.xUnit_v3.TestDataTypes;

namespace Portamical.xUnit_v3.DataProviders;

public interface ITheoryTestData<TTestData>
: IDataProvider<TTestData, ITheoryTestDataRow>,
ITestDataConverter<TTestData, ITheoryTestDataRow>
where TTestData : notnull, ITestData;
