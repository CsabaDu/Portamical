// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Text;
using Portamical.Companion.Core;

namespace Portamical.Companion.Emit;

/// <summary>
/// Emits compilable C# for <c>TestDataFactory</c> calls from <see cref="TestCaseSpec"/> instances.
/// </summary>
public static class TestDataEmitter
{
    /// <summary>
    /// Emits a single factory call expression, e.g.:
    /// <code>
    /// TestDataFactory.CreateTestDataReturns&lt;int, int, int&gt;(
    ///     "Adding two positives",
    ///     5,
    ///     2, 3)
    /// </code>
    /// </summary>
    public static string EmitFactoryCall(TestCaseSpec spec, string indent = "")
    {
        ArgumentNullException.ThrowIfNull(spec);

        var selection = AritySelector.Select(spec);
        var builder = new StringBuilder();

        builder.Append(indent)
            .Append("TestDataFactory.")
            .Append(selection.FactoryMethodName)
            .Append('<')
            .Append(string.Join(", ", selection.GenericTypeArguments))
            .AppendLine(">(");

        string inner = indent + "    ";

        builder.Append(inner).Append(Quote(spec.Definition)).AppendLine(",");

        string secondParam = spec.Kind == ResultKind.Custom
            ? Quote(NamingSemantics.RenderResult(spec))
            : spec.ExpectedValueLiteral
                ?? throw new ArgumentException(
                    $"{spec.Kind} spec '{spec.Definition}' requires ExpectedValueLiteral.",
                    nameof(spec));

        builder.Append(inner).Append(secondParam).AppendLine(",");
        builder.Append(inner).Append(string.Join(", ", spec.Args.Select(a => a.ValueLiteral))).Append(')');

        return builder.ToString();
    }

    /// <summary>
    /// Emits a static readonly array field containing factory calls for all specs, e.g.
    /// <c>private static readonly TestDataReturns&lt;int, int, int&gt;[] addCases = [ ... ];</c>
    /// All specs must share the same family and arity.
    /// </summary>
    public static string EmitTestDataArray(
        IReadOnlyList<TestCaseSpec> specs,
        string fieldName,
        string indent = "    ")
    {
        ArgumentNullException.ThrowIfNull(specs);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        if (specs.Count == 0)
        {
            throw new ArgumentException("At least one spec is required.", nameof(specs));
        }

        var selections = specs.Select(AritySelector.Select).ToList();
        string constructedType = selections[0].ConstructedTypeName;

        if (selections.Any(s => s.ConstructedTypeName != constructedType))
        {
            throw new ArgumentException(
                "All specs must resolve to the same TestData family and generic arguments.",
                nameof(specs));
        }

        var builder = new StringBuilder();

        builder.Append(indent)
            .Append("private static readonly ")
            .Append(constructedType)
            .Append("[] ")
            .Append(fieldName)
            .AppendLine(" =")
            .Append(indent).AppendLine("[");

        for (int i = 0; i < specs.Count; i++)
        {
            builder.Append(EmitFactoryCall(specs[i], indent + "    "));
            builder.AppendLine(i < specs.Count - 1 ? "," : string.Empty);
        }

        builder.Append(indent).Append("];");

        return builder.ToString();
    }

    internal static string Quote(string value)
    => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
