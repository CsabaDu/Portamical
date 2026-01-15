// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Adatamiq.Validators;

public static class Validator
{
    public static T ThrowIfNull<T>(T arg, string? paramName = null)
    where T : class
    {
        if (arg is not null) return arg;

        throw new ArgumentNullException(paramName);
    }

    public static IEnumerable<T> ThrowIfNullOrEmpty<T>(
        IEnumerable<T> enumerable,
        string? paramName = null)
    {
        var moveNext = ThrowIfNull(enumerable, paramName)
            .GetEnumerator()
            .MoveNext();

        if (moveNext) return enumerable;

        throw new ArgumentException(
            "The sequence must contain at least one element.",
            paramName);
    }
}