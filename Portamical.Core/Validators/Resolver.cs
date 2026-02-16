// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Diagnostics;
using static Portamical.Core.Validators.Validator;

namespace Portamical.Core.Validators;

public static class Resolver
{
    private static long LogCounter;

    /// <summary>
    /// Returns the preferred value if it is not null, empty, or consists only of white-space characters; otherwise,
    /// returns a fallback label with a unique index appended.
    /// </summary>
    /// <remarks>If <paramref name="preferredValue"/> is null, empty, or white space, a warning is logged and
    /// the fallback label is returned with a unique index to aid in identifying fallback occurrences in logs and
    /// reports. This method is intended for use in test data scenarios where a meaningful value is required for
    /// reporting or logging.</remarks>
    /// <param name="fallbackLabel">The fallback label to use if <paramref name="preferredValue"/> is null, empty, or white space. Cannot be null.</param>
    /// <param name="preferredValue">The value to return if it is not null, empty, or white space. May be null.</param>
    /// <param name="methodName">The name of the method requesting the value, used for logging purposes. Cannot be null.</param>
    /// <returns>The <paramref name="preferredValue"/> if it contains non-white-space characters; otherwise, the <paramref
    /// name="fallbackLabel"/> with a unique index appended.</returns>
    public static string FallbackIfNullOrWhiteSpace(
        this string fallbackLabel,
        string? preferredValue,
        string methodName)
    {
        _ = NotNull(fallbackLabel, nameof(fallbackLabel));
        _ = NotNull(methodName, nameof(methodName));

        if (string.IsNullOrWhiteSpace(preferredValue))
        {
            var logIndex = Interlocked.Increment(ref LogCounter);
            var indexedFallback = $"{fallbackLabel} ({logIndex})";

            Trace.WriteLine(
                $"Portamical log {logIndex}: The '{methodName}' method of the test data object " +
                $"returned a null, empty, or whitespace value. " +
                $"Using indexed fallback label '{indexedFallback}' in the test report.");

            return indexedFallback;
        }

        return preferredValue;
    }
}