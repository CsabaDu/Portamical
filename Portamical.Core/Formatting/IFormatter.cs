// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Formatting;

public interface IFormatter
{
    string? Format(object value);
}

public interface IFormatter<in T> : IFormatter
{
    string? Format(T value);
}
