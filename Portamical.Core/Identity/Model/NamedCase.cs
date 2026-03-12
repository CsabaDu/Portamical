// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static Portamical.Core.Safety.Validator;

namespace Portamical.Core.Identity.Model;

/// <summary>
/// Provides an abstract base implementation of <see cref="INamedCase"/> with built-in equality comparison 
/// and display name generation.
/// </summary>
/// <remarks>
/// <para>
/// This class implements the Identity Object pattern where equality is based on the <see cref="TestCaseName"/> property.
/// </para>
/// <para>
/// <strong>Immutability:</strong> Derived classes should initialize <see cref="TestCaseName"/> using the 
/// <see langword="init"/> accessor to ensure immutability.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> This class is thread-safe when derived types maintain immutability.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Automatic equality comparison via <see cref="Comparer"/></item>
///   <item>Display name generation for test frameworks</item>
///   <item>Implicit string conversion operator</item>
///   <item>Sealed equality and hash code implementations</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class TestData : NamedCase
/// {
///     public override string TestCaseName { get; init; } = "default";
///     public int Value { get; init; }
/// }
/// 
/// var test = new TestData 
/// { 
///     TestCaseName = "Adding positives =&gt; returns sum",
///     Value = 5 
/// };
/// string name = test; // Implicit conversion to string
/// </code>
/// </example>
[SuppressMessage("SonarLint", "S4035:Classes implementing 'IEqualityComparer<T>' should be sealed",
    Justification = "This abstract base class implements IEquatable<T>, not IEqualityComparer<T>. The nested NamedCaseEqualityComparer that implements IEqualityComparer<T> is properly sealed.")]
public abstract class NamedCase : INamedCase
{    /// <summary>
     /// Gets the unique name identifying this test case.
     /// </summary>
     /// <value>
     /// A non-null string representing the test case identity, typically formatted as 
     /// <c>"scenario description =&gt; expected outcome"</c>.
     /// </value>
     /// <remarks>
     /// <para>
     /// This property serves as the primary identity for equality comparison and is used for:
     /// <list type="bullet">
     ///   <item>Deduplication via <see cref="Equals(INamedCase)"/></item>
     ///   <item>Hash code generation via <see cref="GetHashCode()"/></item>
     ///   <item>Display name generation via <see cref="GetDisplayName(string)"/></item>
     ///   <item>String representation via <see cref="ToString()"/></item>
     /// </list>
     /// </para>
     /// <para>
     /// <strong>Note:</strong> Derived classes must override this property and should use the 
     /// <see langword="init"/> accessor to ensure immutability.
     /// </para>
     /// </remarks>
    public abstract string TestCaseName { get; init; }

    /// <summary>
    /// Gets a singleton equality comparer for <see cref="INamedCase"/> instances based on 
    /// <see cref="TestCaseName"/> comparison.
    /// </summary>
    /// <value>
    /// A <see cref="IEqualityComparer{T}"/> that performs ordinal (case-sensitive) comparison 
    /// of <see cref="TestCaseName"/> properties.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Comparison Logic:</strong>
    /// <list type="bullet">
    ///   <item>Uses <see cref="StringComparer.Ordinal"/> for case-sensitive comparison</item>
    ///   <item>Reference equality (<see cref="ReferenceEquals"/>) returns <see langword="true"/></item>
    ///   <item>Null instances are considered equal to each other but not to non-null instances</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> This comparer is used internally by <see cref="Equals(INamedCase)"/> 
    /// and <see cref="GetHashCode()"/> methods. It can also be used explicitly with LINQ methods 
    /// like <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCases = new[] { test1, test2, test1 }; // test1 appears twice
    /// var unique = testCases.Distinct(NamedCase.Comparer); // Returns test1, test2
    /// </code>
    /// </example>
    public static IEqualityComparer<INamedCase> Comparer { get; } =
        new NamedCaseEqualityComparer();

    private sealed class NamedCaseEqualityComparer : IEqualityComparer<INamedCase>
    {
        /// <summary>
        /// Determines whether two INamedCase instances have the same test case name using an ordinal string comparison.
        /// </summary>
        /// <remarks>
        /// This method performs a case-sensitive, ordinal comparison of the TestCaseName
        /// properties. If both parameters refer to the same object or are both null, the method returns true.
        /// </remarks>
        /// <param name="x">The first INamedCase instance to compare, or null.</param>
        /// <param name="y">The second INamedCase instance to compare, or null.</param>
        /// <returns>
        /// true if both instances are non-null and their TestCaseName properties are equal using ordinal comparison, or
        /// if both are null; otherwise, false.
        /// </returns>
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
        /// <remarks>
        /// The hash code is computed using an ordinal string comparison of the test case name.
        /// If the test case name is null, an empty string is used instead.
        /// </remarks>
        /// <param name="obj">
        /// The <see cref="INamedCase"/> instance for which to compute the hash code. Cannot be <see langword="null"/>.
        /// </param>
        /// <returns>A 32-bit signed integer hash code for the test case name of the specified object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public int GetHashCode(INamedCase obj)
        {
            var testCaseName =
                NotNull(obj, nameof(obj)).TestCaseName
                ?? string.Empty;

            return StringComparer.Ordinal
                .GetHashCode(testCaseName);
        }
    }

    /// <summary>
    /// Determines whether the current instance is contained within the specified collection of named test cases.
    /// </summary>
    /// <param name="namedCases">
    /// The collection of <see cref="INamedCase"/> instances to search. Can be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the current instance is found in the specified collection; 
    /// otherwise, <see langword="false"/>. Returns <see langword="false"/> if 
    /// <paramref name="namedCases"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method delegates to <see cref="Contains(INamedCase, IEnumerable{INamedCase})"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Contains(INamedCase, IEnumerable{INamedCase})"/>
    public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
    => Contains(this, namedCases);

    /// <summary>
    /// Generates a display-friendly name for a test method, combining the specified method name with the test case
    /// name.
    /// </summary>
    /// <param name="testMethodName">
    /// The name of the test method to include in the display name. Can be null to indicate an unnamed method.
    /// </param>
    /// <returns>
    /// A string containing the display name for the test method and test case; or null if a display name cannot be
    /// generated.
    /// </returns>
    public string? GetDisplayName(string? testMethodName)
    => CreateDisplayName(testMethodName, TestCaseName);

    /// <summary>
    /// Determines equality with another <see cref="INamedCase"/> based on test case name comparison.
    /// </summary>
    /// <param name="other">The <see cref="INamedCase"/> to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if the test case names match; otherwise <see langword="false"/>.
    /// </returns>
    public bool Equals(INamedCase? other)
    => Comparer.Equals(this, other);

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance. Can be null.</param>
    /// <returns>
    /// true if the specified object is an instance of INamedCase and is equal to the current instance; otherwise,
    /// false.
    /// </returns>
    public override sealed bool Equals(object? obj)
    => Equals(obj as INamedCase);

    /// <summary>
    /// Generates a hash code based on the <see cref="TestCaseName"/> property.
    /// </summary>
    /// <returns>A stable hash code for the test case instance.</returns>
    public override sealed int GetHashCode()
    => Comparer.GetHashCode(this);

    /// <summary>
    /// Returns the test case name as the string representation of this instance.
    /// </summary>
    /// <returns>The value of <see cref="TestCaseName"/>.</returns>
    /// <remarks>
    /// This method is sealed to ensure consistent string representation across all derived types.
    /// </remarks>
    public sealed override string ToString()
    => TestCaseName;

    #region Static Methods
    /// <summary>
    /// Implicitly converts a <see cref="NamedCase"/> instance to its <see cref="TestCaseName"/> string.
    /// </summary>
    /// <param name="namedCase">
    /// The <see cref="NamedCase"/> instance to convert. Can be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// The <see cref="TestCaseName"/> of <paramref name="namedCase"/>, or <see langword="null"/> 
    /// if <paramref name="namedCase"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This implicit conversion enables seamless use of <see cref="NamedCase"/> objects in contexts 
    /// where a string is expected.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase = new TestData { TestCaseName = "test =&gt; succeeds" };
    /// string name = testCase; // Implicit conversion
    /// Console.WriteLine(name); // Outputs: "test => succeeds"
    /// </code>
    /// </example>
    public static implicit operator string?(NamedCase? namedCase)
    => namedCase?.TestCaseName;

    /// <summary>
    /// Creates a display name for a test method using the method name and the first argument as test data.
    /// </summary>
    /// <param name="testMethodName">
    /// The name of the test method for which to create a display name. Can be null or empty.
    /// </param>
    /// <param name="args">
    /// An array of arguments passed to the test method. The first element (<c>args[0]</c>) is 
    /// used as the test case identifier. Can be <see langword="null"/> or empty.
    /// </param>
    /// <returns>
    /// A formatted display name in the form <c>"{testMethodName}(testData: {firstArgument})"</c> if both the method name and the
    /// first argument are not null or empty; otherwise, null.
    /// </returns>
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
    /// <param name="testMethod">
    /// The method information for the test method. Can be null if only the arguments are used to generate the display
    /// name.
    /// </param>
    /// <param name="args">
    /// An array of arguments to include in the display name. Can be null or empty if no arguments are provided.
    /// </param>
    /// <returns>
    /// A string representing the display name for the test method, or null if a display name cannot be generated.
    /// </returns>
    public static string? CreateDisplayName(MethodInfo? testMethod, params object?[]? args)
    => CreateDisplayName(testMethod?.Name, args);

    /// <summary>
    /// Determines whether the specified collection contains the given named case using a predefined comparer.
    /// </summary>
    /// <param name="namedCase">
    /// The named case to locate in the collection. Can be null if the comparer supports null values.
    /// </param>
    /// <param name="namedCases">
    /// The collection of named cases to search. If null, the method returns <see langword="false"/>.
    /// </param>
    /// <returns>
    /// true if the collection contains the specified named case; otherwise, false.
    /// </returns>
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