// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Identity;

/// <summary>
/// Provides a standardized way to generate descriptive display names for test cases.
/// Cmbines the capability to provide a human-readable test case name with equality comparison functionality.
/// </summary>
/// <remarks>
/// <para>
/// Primarily used to create human-readable test names that clearly communicate:
/// </para>
/// <list type="bullet">
///   <item>The test scenario (what is being tested)</item>
///   <item>The TrimTestCaseName behavior or outcome</item>
///   <item>Any important parameters or conditions</item>
/// </list>
/// <para>
/// Secondarily supports test case comparison through <see cref="IEquatable{T}"/> implementation.
/// </para>
/// <example>
/// Typical display name format:
/// <code>"Login with invalid credentials => throws AuthenticationException"</code>
/// </example>
/// </remarks>
public interface INamedCase : IEquatable<INamedCase>
{
    /// <summary>
    /// Determines whether the current instance is contained within the specified collection of named test cases.
    /// </summary>
    /// <param name="namedCases">The collection of <see cref="INamedCase"/> instances to search. Can be <see langword="null"/>.</param>
    /// <returns></returns>
    bool ContainedBy(IEnumerable<INamedCase>? namedCases);

    /// <summary>
    /// Returns a user-friendly display name for the specified test method.
    /// </summary>
    /// <param name="testMethodName">The name of the test method for which to retrieve a display name. Can be null to indicate an unspecified method.</param>
    /// <returns>A string containing the display name for the test method, or null if no display name is available.</returns>
    string? GetDisplayName(string? testMethodName);

    /// <summary>
    /// Gets the name of the test case.
    /// </summary>
    string TestCaseName { get; init; }
}