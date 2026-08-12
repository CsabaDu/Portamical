// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Portamical.Companion.Analysis;

/// <summary>
/// Signature information about a method under test, extracted from source.
/// </summary>
/// <param name="MethodName">The method name.</param>
/// <param name="ContainingType">The declaring type name, if resolvable.</param>
/// <param name="ReturnTypeName">The declared return type as written in source.</param>
/// <param name="Parameters">Parameter (name, type) pairs in declaration order.</param>
/// <param name="ThrownExceptionTypes">Exception type names found in <c>throw</c> statements/expressions of the method body.</param>
public sealed record TargetInfo(
    string MethodName,
    string? ContainingType,
    string ReturnTypeName,
    IReadOnlyList<(string Name, string TypeName)> Parameters,
    IReadOnlyList<string> ThrownExceptionTypes);

/// <summary>
/// Roslyn-based analyzer that extracts the signature and thrown exceptions of a
/// method under test from C# source text. This drives TestData family/arity selection
/// and test case proposal.
/// </summary>
public static class TargetAnalyzer
{
    /// <summary>
    /// Analyzes the given source text and returns info for every method matching
    /// <paramref name="methodName"/> (overloads produce multiple results).
    /// </summary>
    public static IReadOnlyList<TargetInfo> Analyze(string sourceText, string methodName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        var tree = CSharpSyntaxTree.ParseText(sourceText);
        var root = tree.GetRoot();

        return [.. root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.ValueText == methodName)
            .Select(ToTargetInfo)];
    }

    /// <summary>
    /// Analyzes the given source text and returns info for all public methods,
    /// useful for whole-class test case discovery.
    /// </summary>
    public static IReadOnlyList<TargetInfo> AnalyzeAll(string sourceText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);

        var tree = CSharpSyntaxTree.ParseText(sourceText);
        var root = tree.GetRoot();

        return [.. root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword))
            .Select(ToTargetInfo)];
    }

    private static TargetInfo ToTargetInfo(MethodDeclarationSyntax method)
    {
        string? containingType = method.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault()?.Identifier.ValueText;

        var parameters = method.ParameterList.Parameters
            .Select(p => (p.Identifier.ValueText, p.Type?.ToString() ?? "object"))
            .ToList();

        var thrown = method.DescendantNodes()
            .SelectMany(GetThrownTypeName)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new TargetInfo(
            method.Identifier.ValueText,
            containingType,
            method.ReturnType.ToString(),
            parameters,
            thrown);
    }

    private static IEnumerable<string> GetThrownTypeName(Microsoft.CodeAnalysis.SyntaxNode node)
    {
        var expression = node switch
        {
            ThrowStatementSyntax throwStatement => throwStatement.Expression,
            ThrowExpressionSyntax throwExpression => throwExpression.Expression,
            _ => null,
        };

        if (expression is ObjectCreationExpressionSyntax creation)
        {
            yield return creation.Type.ToString();
        }
        else if (node is InvocationExpressionSyntax invocation
            && invocation.Expression.ToString().Contains("ThrowIfNull", StringComparison.Ordinal))
        {
            yield return "ArgumentNullException";
        }
    }
}
