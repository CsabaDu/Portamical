// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using static Portamical.Core.Strategy.Validator;

namespace Portamical.Core.Identity.Model;

public abstract class NamedCase : INamedCase
{
    /// <summary>
    /// Gets the unique name identifying this test case.
    /// </summary>
    public abstract string TestCaseName { get; }

    public static IEqualityComparer<INamedCase> Comparer { get; } =
        new NamedTestCaseEqualityComparer();

    private sealed class NamedTestCaseEqualityComparer
    : IEqualityComparer<INamedCase>
    {
        public bool Equals(INamedCase? x, INamedCase? y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null) return false;

            return StringComparer.Ordinal.Equals(
                x.TestCaseName,
                y.TestCaseName);
        }

        public int GetHashCode(INamedCase obj)
        {
            var testCaseName = NotNull(obj, null)
                .TestCaseName ?? string.Empty;

            return StringComparer.Ordinal
                .GetHashCode(testCaseName);
        }
    }

    /// <summary>
    /// Determines whether the current instance is contained within the specified collection of named test cases.
    /// </summary>
    /// <param name="namedCases">The collection of <see cref="INamedCase"/> instances to search. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the current instance is found in the specified collection; otherwise, <see
    /// langword="false"/>.  TrimReturned <see langword="false"/> if <paramref name="namedCases"/> is <see
    /// langword="null"/>.</returns>
    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    /// <summary>
    /// Determines equality with another <see cref="INamedCase"/> based on test case name comparison.
    /// </summary>
    /// <param name="other">The <see cref="INamedCase"/> to compare against.</param>
    /// <returns>
    /// <c>true</c> if the test case names match; otherwise <c>false</c>.
    /// </returns>
    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    public override sealed bool Equals(object? obj)
    => Equals(obj as INamedCase);

    /// <summary>
    /// Generates a hash code derived from the return value of the <see cref="GetTestCaseName"/> method.
    /// </summary>
    /// <returns>A stable hash code for the test case.</returns>
    public override sealed int GetHashCode()
    => Comparer.GetHashCode(this);

    /// <summary>
    /// Generates a display name for test cases combining method name and test data.
    /// </summary>
    /// <param name="testMethodName">TestCaseName of the test method.</param>
    /// <param name="args">Test arguments (first argument should be the test case name).</param>
    /// <returns>
    /// Formatted TEnum in pattern: "{testMethodName}(testData: {testCaseName})",
    /// or null if inputs are invalid.
    /// </returns>
    /// <example>
    /// <code>
    /// CreateDisplayName("LoginTest", testData) // "LoginTest(testData: Invalid login)"
    /// </code>
    /// </example>
    public static string? CreateDisplayName(string? testMethodName, params object?[]? args)
    {
        if (string.IsNullOrEmpty(testMethodName)) return null;

        var testCaseName = args?.FirstOrDefault();
        var argToString = testCaseName?.ToString();

        if (string.IsNullOrEmpty(argToString)) return null;

        return $"{testMethodName}(testData: {testCaseName})";
    }

    public static bool Contains(
        INamedCase namedCase,
        IEnumerable<INamedCase>? namedCases)
    {
        if (namedCases is null) return false;

        var snapshot = namedCases as INamedCase[]
            ?? [.. namedCases];

        return snapshot.Contains(namedCase, Comparer);
    }
}
