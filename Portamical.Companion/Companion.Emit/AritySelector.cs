// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Core;

namespace Portamical.Companion.Emit;

/// <summary>
/// Selects the Portamical TestData family, factory method, and generic type arguments
/// for a <see cref="TestCaseSpec"/> based on its <see cref="ResultKind"/> and argument count.
/// </summary>
public static class AritySelector
{
    /// <summary>Maximum supported argument arity of the Portamical TestData families.</summary>
    public const int MaxArity = 9;

    /// <summary>
    /// Resolves the TestData type name, factory method name, and generic type argument list
    /// for the given spec.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Argument count is 0 or exceeds <see cref="MaxArity"/>.</exception>
    /// <exception cref="ArgumentException">Returns/Throws spec lacks an expected type name.</exception>
    public static AritySelection Select(TestCaseSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        if (spec.Args.Count is 0 or > MaxArity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spec),
                spec.Args.Count,
                $"Argument count must be between 1 and {MaxArity}.");
        }

        var argTypes = spec.Args.Select(a => a.TypeName);

        return spec.Kind switch
        {
            ResultKind.Returns => new AritySelection(
                "TestDataReturns",
                "CreateTestDataReturns",
                [RequireExpectedType(spec), .. argTypes]),
            ResultKind.Throws => new AritySelection(
                "TestDataThrows",
                "CreateTestDataThrows",
                [RequireExpectedType(spec), .. argTypes]),
            _ => new AritySelection(
                "TestData",
                "CreateTestData",
                [.. argTypes]),
        };
    }

    private static string RequireExpectedType(TestCaseSpec spec)
    => !string.IsNullOrWhiteSpace(spec.ExpectedTypeName)
        ? spec.ExpectedTypeName
        : throw new ArgumentException(
            $"{spec.Kind} spec '{spec.Definition}' requires ExpectedTypeName.",
            nameof(spec));
}

/// <summary>
/// Resolved TestData family selection for emission.
/// </summary>
/// <param name="TestDataTypeName">Base type name, e.g. "TestDataReturns".</param>
/// <param name="FactoryMethodName">TestDataFactory method name, e.g. "CreateTestDataReturns".</param>
/// <param name="GenericTypeArguments">Generic type arguments in declaration order.</param>
public sealed record AritySelection(
    string TestDataTypeName,
    string FactoryMethodName,
    IReadOnlyList<string> GenericTypeArguments)
{
    /// <summary>The full constructed type, e.g. "TestDataReturns&lt;int, int, int&gt;".</summary>
    public string ConstructedTypeName
    => $"{TestDataTypeName}<{string.Join(", ", GenericTypeArguments)}>";
}
