// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Microsoft.CodeAnalysis;

namespace Portamical.Companion.Analysis;

/// <summary>
/// Recognition of well-known BCL guard-clause "Throw*" static helpers (the modern
/// <c>ArgumentNullException.ThrowIfNull</c>-style APIs), by resolved symbol when possible and
/// by a substring fallback otherwise.
/// </summary>
internal static class GuardClauses
{
    /// <summary>
    /// Exception type(s) raised by each known guard-clause helper, keyed by
    /// (declaring type simple name, method name). Some helpers can raise more than one
    /// exception type depending on the input (e.g. <c>ArgumentException.ThrowIfNullOrEmpty</c>
    /// throws <see cref="ArgumentNullException"/> for <c>null</c> and <see cref="ArgumentException"/>
    /// for an empty value), so both are reported.
    /// </summary>
    private static readonly Dictionary<(string Type, string Method), string[]> BySymbolTable = new()
    {
        [("ArgumentNullException", "ThrowIfNull")] = ["ArgumentNullException"],
        [("ArgumentException", "ThrowIfNullOrEmpty")] = ["ArgumentNullException", "ArgumentException"],
        [("ArgumentException", "ThrowIfNullOrWhiteSpace")] = ["ArgumentNullException", "ArgumentException"],
        [("ObjectDisposedException", "ThrowIf")] = ["ObjectDisposedException"],
        [("ArgumentOutOfRangeException", "ThrowIfZero")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfNegative")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfNegativeOrZero")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfGreaterThan")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfGreaterThanOrEqual")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfLessThan")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfLessThanOrEqual")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfEqual")] = ["ArgumentOutOfRangeException"],
        [("ArgumentOutOfRangeException", "ThrowIfNotEqual")] = ["ArgumentOutOfRangeException"],
    };

    /// <summary>
    /// Text markers checked in longest-first order so a more specific helper (e.g.
    /// <c>ThrowIfNullOrEmpty</c>) is matched before a shorter one it contains (<c>ThrowIfNull</c>).
    /// Used only when the invocation's symbol cannot be resolved.
    /// </summary>
    private static readonly (string Marker, string[] Exceptions)[] ByTextTable =
    [
        .. BySymbolTable
            .Select(kv => (kv.Key.Method, kv.Value))
            .Distinct()
            .OrderByDescending(t => t.Method.Length),
    ];

    /// <summary>Resolves the exception type(s) raised by a resolved guard-clause symbol, or <c>null</c> if it isn't one.</summary>
    public static string[]? MatchBySymbol(IMethodSymbol method)
    {
        string typeName = method.ContainingType?.Name ?? string.Empty;

        return BySymbolTable.TryGetValue((typeName, method.Name), out var exceptions) ? exceptions : null;
    }

    /// <summary>Best-effort text fallback for guard-clause calls whose symbol couldn't be resolved.</summary>
    public static IEnumerable<string> MatchByText(string invocationExpressionText)
    {
        foreach (var (marker, exceptions) in ByTextTable)
        {
            if (invocationExpressionText.Contains(marker, StringComparison.Ordinal))
            {
                return exceptions;
            }
        }

        return [];
    }
}
