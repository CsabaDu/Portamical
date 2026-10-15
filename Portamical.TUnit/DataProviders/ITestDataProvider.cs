// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders;
using TUnit.Core.Interfaces;

namespace Portamical.TUnit.DataProviders;

public interface ITestDataProvider : IDataProvider, IDataSource
{
    string[]? Categories { get; init; }
}
