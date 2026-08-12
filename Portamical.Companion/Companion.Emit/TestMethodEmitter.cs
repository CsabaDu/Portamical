// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Text;
using Portamical.Companion.Core;

namespace Portamical.Companion.Emit;

/// <summary>
/// Supported test frameworks for wiring emission.
/// </summary>
public enum TestFramework
{
    /// <summary>xUnit v3 (Theory + MemberData).</summary>
    XUnitV3,

    /// <summary>MSTest (TestMethod + DynamicData).</summary>
    MSTest,

    /// <summary>NUnit (TestCaseSource).</summary>
    NUnit
}

/// <summary>
/// Emits the framework wiring around an emitted TestData array: a data source member
/// (rows produced via <c>ToArgs(ArgsCode.Instance)</c>) plus a test method skeleton
/// consuming the strongly-typed TestData instance.
/// </summary>
public static class TestMethodEmitter
{
    /// <summary>
    /// Emits a data source property yielding <c>object?[]</c> rows from a TestData array field,
    /// compatible with all supported frameworks:
    /// <code>
    /// public static IEnumerable&lt;object?[]&gt; AddTestCases
    /// =&gt; addCases.Select(td =&gt; td.ToArgs(ArgsCode.Instance));
    /// </code>
    /// </summary>
    public static string EmitDataSource(string fieldName, string memberName, string indent = "    ")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);

        return $"{indent}public static IEnumerable<object?[]> {memberName}\n"
            + $"{indent}=> {fieldName}.Select(td => td.ToArgs(ArgsCode.Instance));";
    }

    /// <summary>
    /// Emits a test method skeleton for the given framework, consuming the strongly-typed
    /// TestData parameter with an Arrange/Act/Assert body scaffold.
    /// </summary>
    /// <param name="spec">A representative spec (defines family, arity, and target method).</param>
    /// <param name="dataSourceMember">The member name emitted by <see cref="EmitDataSource"/>.</param>
    /// <param name="framework">Target framework.</param>
    /// <param name="testMethodName">Test method name; derived from the target method when omitted.</param>
    public static string EmitTestMethod(
        TestCaseSpec spec,
        string dataSourceMember,
        TestFramework framework,
        string? testMethodName = null,
        string indent = "    ")
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataSourceMember);

        var selection = AritySelector.Select(spec);
        string methodName = testMethodName
            ?? $"{spec.TargetMethod ?? "Target"}_validArgs_{ExpectedSuffix(spec.Kind)}";

        var builder = new StringBuilder();

        foreach (string attribute in GetAttributes(framework, dataSourceMember))
        {
            builder.Append(indent).AppendLine(attribute);
        }

        builder.Append(indent)
            .Append("public void ")
            .Append(methodName)
            .Append('(')
            .Append(selection.ConstructedTypeName)
            .AppendLine(" testData)")
            .Append(indent).AppendLine("{");

        string inner = indent + "    ";
        string argList = string.Join(", ", Enumerable.Range(1, spec.Args.Count).Select(i => $"testData.Arg{i}"));
        string call = $"{spec.TargetMethod ?? "TODO_TargetMethod"}({argList})";

        switch (spec.Kind)
        {
            case ResultKind.Returns:
                builder.Append(inner).AppendLine("// Act");
                builder.Append(inner).AppendLine($"var actual = {call};");
                builder.AppendLine();
                builder.Append(inner).AppendLine("// Assert");
                builder.Append(inner).AppendLine(GetEqualAssert(framework));
                break;
            case ResultKind.Throws:
                builder.Append(inner).AppendLine("// Act & Assert");
                builder.Append(inner).AppendLine(GetThrowsAssert(framework, spec.ExpectedTypeName!, call));
                break;
            default:
                builder.Append(inner).AppendLine("// Act");
                builder.Append(inner).AppendLine($"var actual = {call};");
                builder.AppendLine();
                builder.Append(inner).AppendLine("// Assert (custom result — complete manually)");
                builder.Append(inner).AppendLine($"// {NamingSemantics.RenderResult(spec)}");
                break;
        }

        builder.Append(indent).Append('}');

        return builder.ToString();
    }

    private static string ExpectedSuffix(ResultKind kind)
    => kind switch
    {
        ResultKind.Returns => "returnsExpected",
        ResultKind.Throws => "throwsExpected",
        _ => "behavesAsExpected",
    };

    private static IReadOnlyList<string> GetAttributes(TestFramework framework, string dataSourceMember)
    => framework switch
    {
        TestFramework.XUnitV3 => ["[Theory]", $"[MemberData(nameof({dataSourceMember}))]"],
        TestFramework.MSTest => ["[TestMethod]", $"[DynamicData(nameof({dataSourceMember}))]"],
        _ => [$"[TestCaseSource(nameof({dataSourceMember}))]"],
    };

    private static string GetEqualAssert(TestFramework framework)
    => framework switch
    {
        TestFramework.XUnitV3 => "Assert.Equal(testData.Expected, actual);",
        TestFramework.MSTest => "Assert.AreEqual(testData.Expected, actual);",
        _ => "Assert.That(actual, Is.EqualTo(testData.Expected));",
    };

    private static string GetThrowsAssert(TestFramework framework, string exceptionType, string call)
    => framework switch
    {
        TestFramework.XUnitV3 => $"Assert.Throws<{exceptionType}>(() => {call});",
        TestFramework.MSTest => $"Assert.ThrowsExactly<{exceptionType}>(() => {call});",
        _ => $"Assert.Throws<{exceptionType}>(() => {call});",
    };
}
