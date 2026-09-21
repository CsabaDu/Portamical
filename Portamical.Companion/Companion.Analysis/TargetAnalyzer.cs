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
/// <param name="ThrownExceptionTypes">
/// Exception type names that can surface from the method: those thrown directly in its body,
/// those thrown by BCL guard-clause helpers it calls (e.g. <c>ArgumentNullException.ThrowIfNull</c>),
/// and those thrown transitively by other methods it calls that are declared in the analyzed source.
/// </param>
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
/// <remarks>
/// Detection is semantic where possible: a real <see cref="Compilation"/> is built so
/// <c>throw</c> targets and called methods are resolved to symbols rather than guessed from
/// text. Two consequences follow. First, well-known BCL guard-clause helpers (the
/// <c>Throw*</c> statics on <see cref="ArgumentNullException"/>, <see cref="ArgumentException"/>,
/// <see cref="ArgumentOutOfRangeException"/> and <see cref="ObjectDisposedException"/>) are
/// recognized by resolved symbol instead of a substring match on the call text. Second, when
/// the analyzed method calls another method that is itself declared in the supplied source,
/// that callee's own thrown/propagated exceptions are collected transitively (with a cycle
/// guard for recursive/mutually recursive calls), so exceptions raised by in-source helper
/// methods are no longer missed. A plain substring fallback is kept only for calls whose
/// symbol cannot be resolved (e.g. snippets without the needed <c>using</c> directives), and
/// exceptions thrown by unresolved external calls (BCL methods outside the guard-clause table,
/// or calls into assemblies not supplied via <c>referencePaths</c>) are still not detected —
/// full whole-program interprocedural analysis is out of scope for a source-text analyzer.
/// </remarks>
public static class TargetAnalyzer
{
    /// <summary>
    /// Analyzes the given source text and returns info for every method matching
    /// <paramref name="methodName"/> (overloads produce multiple results).
    /// </summary>
    /// <param name="sourceText">C# source text containing the target method.</param>
    /// <param name="methodName">The method name to look up.</param>
    /// <param name="referencePaths">
    /// Optional extra assembly paths (e.g. a domain library) to resolve calls into types
    /// beyond the core runtime surface when analyzing thrown exceptions.
    /// </param>
    public static IReadOnlyList<TargetInfo> Analyze(
        string sourceText, string methodName, IEnumerable<string>? referencePaths = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        return Analyze([sourceText], methodName, referencePaths);
    }

    /// <summary>
    /// Analyzes multiple source files as one compilation and returns info for every method
    /// matching <paramref name="methodName"/>, so calls into helper methods declared in a
    /// different file are also resolved for transitive exception collection.
    /// </summary>
    public static IReadOnlyList<TargetInfo> Analyze(
        IEnumerable<string> sourceTexts, string methodName, IEnumerable<string>? referencePaths = null)
    {
        ArgumentNullException.ThrowIfNull(sourceTexts);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        var compilation = BuildCompilation(sourceTexts, referencePaths, out var trees);
        var cache = new Dictionary<IMethodSymbol, IReadOnlyList<string>>(SymbolEqualityComparer.Default);

        return [.. trees
            .SelectMany(t => t.GetRoot().DescendantNodes())
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.ValueText == methodName)
            .Select(m => ToTargetInfo(m, compilation, cache))];
    }

    /// <summary>
    /// Analyzes the given source text and returns info for all public methods,
    /// useful for whole-class test case discovery.
    /// </summary>
    public static IReadOnlyList<TargetInfo> AnalyzeAll(
        string sourceText, IEnumerable<string>? referencePaths = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);

        return AnalyzeAll([sourceText], referencePaths);
    }

    /// <summary>
    /// Analyzes multiple source files as one compilation and returns info for all public
    /// methods across them.
    /// </summary>
    public static IReadOnlyList<TargetInfo> AnalyzeAll(
        IEnumerable<string> sourceTexts, IEnumerable<string>? referencePaths = null)
    {
        ArgumentNullException.ThrowIfNull(sourceTexts);

        var compilation = BuildCompilation(sourceTexts, referencePaths, out var trees);
        var cache = new Dictionary<IMethodSymbol, IReadOnlyList<string>>(SymbolEqualityComparer.Default);

        return [.. trees
            .SelectMany(t => t.GetRoot().DescendantNodes())
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword))
            .Select(m => ToTargetInfo(m, compilation, cache))];
    }

    private static CSharpCompilation BuildCompilation(
        IEnumerable<string> sourceTexts, IEnumerable<string>? referencePaths, out IReadOnlyList<SyntaxTree> trees)
    {
        var treeList = sourceTexts.Select(s => CSharpSyntaxTree.ParseText(s)).ToList();
        trees = treeList;

        var references = RuntimeReferences.GetAll()
            .Concat((referencePaths ?? [])
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)))
            .ToList();

        return CSharpCompilation.Create(
            assemblyName: "Portamical.Companion.TargetAnalysis",
            syntaxTrees: treeList,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static TargetInfo ToTargetInfo(
        MethodDeclarationSyntax method,
        Compilation compilation,
        Dictionary<IMethodSymbol, IReadOnlyList<string>> cache)
    {
        string? containingType = method.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault()?.Identifier.ValueText;

        var parameters = method.ParameterList.Parameters
            .Select(p => (p.Identifier.ValueText, p.Type?.ToString() ?? "object"))
            .ToList();

        var thrown = GetExceptionsForMethod(method, compilation, cache, [])
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new TargetInfo(
            method.Identifier.ValueText,
            containingType,
            method.ReturnType.ToString(),
            parameters,
            thrown);
    }

    /// <summary>
    /// Computes the exception types thrown directly by <paramref name="method"/>, plus those
    /// thrown by resolved BCL guard-clause calls and by in-source callees (transitively).
    /// Results are cached per method symbol; <paramref name="inProgress"/> guards against
    /// infinite recursion on (mutually) recursive calls.
    /// </summary>
    private static IReadOnlyList<string> GetExceptionsForMethod(
        MethodDeclarationSyntax method,
        Compilation compilation,
        Dictionary<IMethodSymbol, IReadOnlyList<string>> cache,
        HashSet<IMethodSymbol> inProgress)
    {
        var model = compilation.GetSemanticModel(method.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(method);

        if (symbol is not null)
        {
            if (cache.TryGetValue(symbol, out var cached))
            {
                return cached;
            }

            if (!inProgress.Add(symbol))
            {
                // Recursive/mutually-recursive call already being analyzed higher up the stack.
                return [];
            }
        }

        var thrown = new List<string>();

        foreach (var node in method.DescendantNodes())
        {
            thrown.AddRange(GetThrownTypeNames(node, model, compilation, cache, inProgress));
        }

        var result = (IReadOnlyList<string>)[.. thrown.Distinct(StringComparer.Ordinal)];

        if (symbol is not null)
        {
            inProgress.Remove(symbol);
            cache[symbol] = result;
        }

        return result;
    }

    private static IEnumerable<string> GetThrownTypeNames(
        SyntaxNode node,
        SemanticModel model,
        Compilation compilation,
        Dictionary<IMethodSymbol, IReadOnlyList<string>> cache,
        HashSet<IMethodSymbol> inProgress)
    {
        var thrownExpression = node switch
        {
            ThrowStatementSyntax throwStatement => throwStatement.Expression,
            ThrowExpressionSyntax throwExpression => throwExpression.Expression,
            _ => null,
        };

        if (thrownExpression is ObjectCreationExpressionSyntax creation)
        {
            yield return ResolveTypeName(creation, model);
            yield break;
        }

        if (node is not InvocationExpressionSyntax invocation)
        {
            yield break;
        }

        var symbolInfo = model.GetSymbolInfo(invocation);
        var invoked = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (invoked is null)
        {
            // Symbol resolution failed (e.g. missing `using` in a bare snippet); fall back to
            // the previous text heuristic rather than silently losing the guard clause.
            foreach (string exception in GuardClauses.MatchByText(invocation.Expression.ToString()))
            {
                yield return exception;
            }

            yield break;
        }

        if (GuardClauses.MatchBySymbol(invoked) is { } bySymbol)
        {
            foreach (string exception in bySymbol)
            {
                yield return exception;
            }

            yield break;
        }

        // Interprocedural: if the callee is declared in the analyzed source, its own thrown/
        // propagated exceptions belong to the caller too.
        var declaration = invoked.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (declaration is not null)
        {
            foreach (string exception in GetExceptionsForMethod(declaration, compilation, cache, inProgress))
            {
                yield return exception;
            }
        }
    }

    private static string ResolveTypeName(ObjectCreationExpressionSyntax creation, SemanticModel model)
    {
        var type = model.GetTypeInfo(creation).Type;

        // Prefer the resolved symbol's simple name (consistent with the previous syntactic
        // output); fall back to the written syntax when the type can't be resolved (e.g. a
        // bare snippet analyzed without the relevant `using` directive).
        return type is null || type.TypeKind == TypeKind.Error
            ? creation.Type.ToString()
            : type.Name;
    }
}
