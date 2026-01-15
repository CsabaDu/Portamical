// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Adatamiq.Validators;

public static class StringValidator
{
    public static string FallbackIfNullOrEmpty(
        this string label,
        string? value)
    => string.IsNullOrEmpty(value) ?
        label
        : value;
}
