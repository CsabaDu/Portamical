// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Reflection;
using static Portamical.Core.Safety.Validator;

namespace Portamical.Core.Identity.Model;

public abstract class NamedCase : INamedCase
{
    /// <summary>
    /// Gets the unique name identifying this test case.
    /// </summary>
    public abstract string TestCaseName { get; init; }

    /// <summary>
    /// Gets an equality comparer that determines whether two INamedCase instances are equal.
    /// </summary>
    public static IEqualityComparer<INamedCase> Comparer { get; } =
        new NamedCaseEqualityComparer();

    private sealed class NamedCaseEqualityComparer
    : IEqualityComparer<INamedCase>
    {
        /// <summary>
        /// Determines whether two INamedCase instances have the same test case name using an ordinal string comparison.
        /// </summary>
        /// <remarks>This method performs a case-sensitive, ordinal comparison of the TestCaseName
        /// properties. If both parameters refer to the same object or are both null, the method returns true.</remarks>
        /// <param name="x">The first INamedCase instance to compare, or null.</param>
        /// <param name="y">The second INamedCase instance to compare, or null.</param>
        /// <returns>true if both instances are non-null and their TestCaseName properties are equal using ordinal comparison, or
        /// if both are null; otherwise, false.</returns>
        public bool Equals(INamedCase? x, INamedCase? y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null) return false;

            return StringComparer.Ordinal.Equals(
                x.TestCaseName,
                y.TestCaseName);
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="INamedCase"/> instance based on its test case name.
        /// </summary>
        /// <remarks>The hash code is computed using an ordinal string comparison of the test case name.
        /// If the test case name is null, an empty string is used instead.</remarks>
        /// <param name="obj">The <see cref="INamedCase"/> instance for which to compute the hash code. Cannot be null.</param>
        /// <returns>A 32-bit signed integer hash code for the test case name of the specified object.</returns>
        public int GetHashCode(INamedCase obj)
        {
            var testCaseName =
                NotNull(obj, null).TestCaseName 
                ?? string.Empty;

            return StringComparer.Ordinal
                .GetHashCode(testCaseName);
        }
    }

    /// <summary>
    /// Determines whether the current instance is contained within the specified collection of named test cases.
    /// </summary>
    /// <param name="namedCases">The collection of <see cref="INamedCase"/> instances to search. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the current instance is found in the specified collection; otherwise, <see
    /// langword="false"/>.  TrimReturnsExpected <see langword="false"/> if <paramref name="namedCases"/> is <see
    /// langword="null"/>.</returns>
    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    /// <summary>
    /// Generates a display-friendly name for a test method, combining the specified method name with the test case
    /// name.
    /// </summary>
    /// <param name="testMethodName">The name of the test method to include in the display name. Can be null to indicate an unnamed method.</param>
    /// <returns>A string containing the display name for the test method and test case; or null if a display name cannot be
    /// generated.</returns>
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

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance. Can be null.</param>
    /// <returns>true if the specified object is an instance of INamedCase and is equal to the current instance; otherwise,
    /// false.</returns>
    public override sealed bool Equals(object? obj)
    => Equals(obj as INamedCase);

    /// <summary>
    /// Generates a hash code derived from the return value of the <see cref="GetTestCaseName"/> method.
    /// </summary>
    /// <returns>A stable hash code for the test case.</returns>
    public override sealed int GetHashCode()
    => Comparer.GetHashCode(this);

    /// <summary>
    /// Overrides and seals the `ToString()` method to return the value of <see cref=TestCaseName"/> property.
    /// </summary>
    public sealed override string ToString()
    => TestCaseName;

    #region Static Methods
    /// <summary>
    /// Converts a nullable NamedCase instance to its associated test case name string, or null if the instance is null.
    /// </summary>
    /// <remarks>This implicit conversion enables seamless use of NamedCase objects in contexts where a string
    /// is expected, such as assignment or comparison operations.</remarks>
    /// <param name="namedCase">The NamedCase instance to convert. If null, the result is also null.</param>
    public static implicit operator string?(NamedCase? namedCase)
    => namedCase?.TestCaseName;

    /// <summary>
    /// Creates a display name for a test method using the method name and the first argument as test data.
    /// </summary>
    /// <param name="testMethodName">The name of the test method for which to create a display name. Can be null or empty.</param>
    /// <param name="args">An array of arguments passed to the test method. The first argument is used as the test data in the display name.
    /// Can be null.</param>
    /// <returns>A formatted display name in the form "{testMethodName}(testData: {firstArgument})" if both the method name and the
    /// first argument are not null or empty; otherwise, null.</returns>
    public static string? CreateDisplayName(string? testMethodName, params object?[]? args)
    {
        if (string.IsNullOrEmpty(testMethodName)) return null;
        if (args is not { Length: > 0 }) return null;

        var testCaseName = args[0];

        if (string.IsNullOrEmpty(testCaseName?.ToString())) return null;

        return $"{testMethodName}(testData: {testCaseName})";
    }

    /// <summary>
    /// Creates a display name for a test method using the specified method information and arguments.
    /// </summary>
    /// <param name="testMethod">The method information for the test method. Can be null if only the arguments are used to generate the display
    /// name.</param>
    /// <param name="args">An array of arguments to include in the display name. Can be null or empty if no arguments are provided.</param>
    /// <returns>A string representing the display name for the test method, or null if a display name cannot be generated.</returns>
    public static string? CreateDisplayName(MethodInfo? testMethod, params object?[]? args)
    => CreateDisplayName(testMethod?.Name, args);

    /// <summary>
    /// Determines whether the specified collection contains the given named case using a predefined comparer.
    /// </summary>
    /// <param name="namedCase">The named case to locate in the collection. Can be null if the comparer supports null values.</param>
    /// <param name="namedCases">The collection of named cases to search. If null, the method returns <see langword="false"/>.</param>
    /// <returns>true if the collection contains the specified named case; otherwise, false.</returns>
    public static bool Contains(
        INamedCase namedCase,
        IEnumerable<INamedCase>? namedCases)
    {
        if (namedCases is null) return false;

        var snapshot = namedCases as INamedCase[]
            ?? [.. namedCases];

        return snapshot.Contains(namedCase, Comparer);
    }
    #endregion Static Methods
}
