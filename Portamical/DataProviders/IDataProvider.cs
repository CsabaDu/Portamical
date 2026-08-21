// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders;

public interface IDataProvider<out TRow> : IEnumerable<TRow>
{
    TRow[] GetRows();
}