// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Companion.Emit;

/// <summary>
/// Scaffolds <c>Portamical.Core.Formatting.Formatter&lt;T&gt;</c> implementations for domain
/// types that render poorly in auto-generated test case names — the AI improving the
/// readability of its own output.
/// </summary>
public static class FormatterEmitter
{
    /// <summary>
    /// Emits a <c>Formatter&lt;T&gt;</c> scaffold plus its registration call.
    /// </summary>
    /// <param name="typeName">The domain type to format (e.g. "ProductId").</param>
    /// <param name="formatExpression">
    /// Optional format expression using the parameter <c>value</c>
    /// (e.g. <c>$"PROD-{value.Id:D6}"</c>); a TODO body is emitted when omitted.
    /// </param>
    public static string EmitFormatter(string typeName, string? formatExpression = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        string body = formatExpression ?? $"value.ToString() ?? nameof({typeName}) /* TODO: refine */";

        return $$"""
            /// <summary>
            /// Formats <see cref="{{typeName}}"/> values for Portamical test case names.
            /// </summary>
            public class {{typeName}}Formatter : Formatter<{{typeName}}>
            {
                public override string Format({{typeName}} value)
                => {{body}};

                string? IFormatter.Format(object? value)
                => value is {{typeName}} typed ? Format(typed) : null;
            }

            // Register once at test assembly startup:
            // Formatter.RegisterFormatter<{{typeName}}>(new {{typeName}}Formatter());
            """;
    }
}
