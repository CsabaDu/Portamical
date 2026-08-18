// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Companion.Core;

/// <summary>
/// The kind of expected outcome of a test case, mapping to the Portamical TestData families.
/// </summary>
public enum ResultKind
{
    /// <summary>Maps to <c>TestDataReturns&lt;TExpected, ...&gt;</c> ("returns {value}").</summary>
    Returns,

    /// <summary>Maps to <c>TestDataThrows&lt;TException, ...&gt;</c> ("throws {exception}").</summary>
    Throws,

    /// <summary>Maps to <c>TestData&lt;...&gt;</c> with a custom result description.</summary>
    Custom
}

/// <summary>
/// Describes a single argument of a test case: its declared type and its C# value literal.
/// </summary>
/// <param name="Name">Parameter name of the method under test (informational).</param>
/// <param name="TypeName">C# type name as it should appear in generic arguments (e.g. "int", "string?").</param>
/// <param name="ValueLiteral">C# expression literal for the value (e.g. "2", "\"abc\"", "null").</param>
public sealed record ArgSpec(string Name, string TypeName, string ValueLiteral);

/// <summary>
/// The lingua franca between AI-proposed test cases and code emission.
/// Serializes to/from the one-line <c>"definition =&gt; result"</c> form
/// (see <see cref="NamingSemantics"/>).
/// </summary>
public sealed record TestCaseSpec
{
    /// <summary>The scenario description (left side of "=&gt;").</summary>
    public required string Definition { get; init; }

    /// <summary>The kind of expected outcome.</summary>
    public required ResultKind Kind { get; init; }

    /// <summary>
    /// C# type name of the expected value (<see cref="ResultKind.Returns"/>)
    /// or of the expected exception (<see cref="ResultKind.Throws"/>).
    /// </summary>
    public string? ExpectedTypeName { get; init; }

    /// <summary>
    /// C# expression literal of the expected value or exception instance
    /// (e.g. "5", "new ArgumentNullException()").
    /// </summary>
    public string? ExpectedValueLiteral { get; init; }

    /// <summary>
    /// Human-readable expected result used in the test case name
    /// (e.g. "5" for returns, custom text for <see cref="ResultKind.Custom"/>).
    /// </summary>
    public string? ExpectedDisplay { get; init; }

    /// <summary>The test arguments, in method parameter order (1..9 supported).</summary>
    public IReadOnlyList<ArgSpec> Args { get; init; } = [];

    /// <summary>Name of the method under test (informational, used by emitters).</summary>
    public string? TargetMethod { get; init; }

    /// <summary>
    /// Whether the expected outcome was verified by characterization (execution of the
    /// method under test) rather than assumed by the proposer.
    /// </summary>
    public bool Verified { get; init; }

    /// <summary>The full test case name in "definition =&gt; result" form.</summary>
    public string TestCaseName => NamingSemantics.Render(this);
}
