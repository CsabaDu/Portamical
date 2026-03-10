// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Identity;

/// <summary>
/// Provides a standardized way to generate descriptive display names for test cases.
/// Combines the capability to provide a human-readable test case name with equality comparison functionality.
/// </summary>
/// <remarks>
/// <para>
/// Primarily used to create human-readable test names that clearly communicate:
/// </para>
/// <list type="bullet">
///   <item>The test scenario (what is being tested)</item>
///   <item>The expected behavior or outcome</item>
///   <item>Any important parameters or conditions</item>
/// </list>
/// <para>
/// Secondarily supports test case comparison through <see cref="IEquatable{T}"/> implementation.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Identity Object - equality based on <see cref="TestCaseName"/> property.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be immutable to ensure thread safety.
/// </para>
/// <example>
/// Typical display name format:
/// <code>"Login with invalid credentials => throws AuthenticationException"</code>
/// </example>
/// </remarks>
/// <seealso cref="IEquatable{T}"/>
/// <seealso cref="TestCaseName"/>
public interface INamedCase : IEquatable<INamedCase>
{
    /// <summary>
    /// Determines whether the current instance is contained within the specified collection of named test cases.
    /// </summary>
    /// <param name="namedCases">
    /// The collection of <see cref="INamedCase"/> instances to search. Can be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this instance is found in the <paramref name="namedCases"/> collection
    /// based on <see cref="TestCaseName"/> equality; otherwise, <see langword="false"/>.
    /// Returns <see langword="false"/> if <paramref name="namedCases"/> is <see langword="null"/> or empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Comparison Logic:</strong> Uses <see cref="IEquatable{T}.Equals(T)"/> for comparison,
    /// which typically compares <see cref="TestCaseName"/> properties.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> O(n) where n is the number of elements in <paramref name="namedCases"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase = new TestData&lt;int&gt; { TestCaseName = "test1 =&gt; succeeds" };
    /// var collection = new[] { testCase };
    /// bool result = testCase.ContainedBy(collection); // returns true
    /// </code>
    /// </example>
    bool ContainedBy(IEnumerable<INamedCase>? namedCases);

    /// <summary>
    /// Returns a user-friendly display name for the specified test method, combining the method name with the test case identity.
    /// </summary>
    /// <param name="testMethodName">
    /// The name of the test method for which to retrieve a display name.
    /// If <see langword="null"/> or empty, only the <see cref="TestCaseName"/> is returned.
    /// </param>
    /// <returns>
    /// A formatted string combining <paramref name="testMethodName"/> and <see cref="TestCaseName"/>,
    /// or <see langword="null"/> if <see cref="TestCaseName"/> is <see langword="null"/> or empty.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Typical Format:</strong>
    /// <list type="bullet">
    ///   <item>With method name: <c>"MethodName(testData: scenario =&gt; outcome)"</c></item>
    ///   <item>Without method name: <c>"scenario =&gt; outcome"</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Framework Usage:</strong> Used by test framework adapters to generate test display names
    /// in test runners (e.g., Visual Studio Test Explorer, xUnit console output).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase = new TestData&lt;int&gt; { TestCaseName = "Adding positives =&gt; returns sum" };
    /// string displayName = testCase.GetDisplayName("Add_ValidInputs_ReturnsSum");
    /// // Result: "Add_ValidInputs_ReturnsSum(testData: Adding positives => returns sum)"
    /// </code>
    /// </example>
    string? GetDisplayName(string? testMethodName);

    /// <summary>
    /// Gets the unique name of the test case used for identification and display purposes.
    /// </summary>
    /// <value>
    /// A non-null string representing the test case identity. Typically formatted as:
    /// <c>"scenario description =&gt; expected outcome"</c>
    /// </value>
    /// <remarks>
    /// <para>
    /// This property serves as the primary identity for the test case and is used for:
    /// <list type="bullet">
    ///   <item>Deduplication of test cases</item>
    ///   <item>Test framework display names</item>
    ///   <item>Equality comparison via <see cref="IEquatable{T}"/></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Immutability:</strong> Uses <see langword="init"/> accessor to ensure immutability after construction.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var testCase = new TestData&lt;int&gt;
    /// {
    ///     TestCaseName = "Adding two positive numbers =&gt; returns their sum"
    /// };
    /// </code>
    /// </example>
    string TestCaseName { get; init; }
}