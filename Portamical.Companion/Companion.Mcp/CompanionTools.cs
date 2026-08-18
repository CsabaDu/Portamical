// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Text.Json;
using System.Text.Json.Nodes;
using Portamical.Companion.Analysis;
using Portamical.Companion.Core;
using Portamical.Companion.Emit;
using Portamical.Companion.Execution;

namespace Portamical.Companion.Mcp;

/// <summary>
/// Registers the Portamical Companion tool surface on an <see cref="McpServer"/>:
/// <c>analyze_target</c>, <c>propose_cases</c>, <c>generate_test_data</c>,
/// <c>characterize_case</c>, <c>analyze_gaps</c>, <c>generate_formatter</c>.
/// </summary>
public static class CompanionTools
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>Registers all companion tools on the server.</summary>
    public static void RegisterAll(McpServer server)
    {
        ArgumentNullException.ThrowIfNull(server);

        server.RegisterTool(
            "analyze_target",
            "Analyze a method under test in C# source: signature, parameters, and thrown exceptions. "
                + "Pass sourceFile (path) or sourceText, and optionally methodName (all public methods when omitted).",
            Schema(
                ("sourceFile", "string", "Path of the C# source file containing the target."),
                ("sourceText", "string", "C# source text (alternative to sourceFile)."),
                ("methodName", "string", "Name of the method to analyze; omit for all public methods.")),
            AnalyzeTarget);

        server.RegisterTool(
            "propose_cases",
            "Validate and normalize proposed test cases written as 'definition => result' lines. "
                + "Returns parsed kind (returns/throws/custom) per line and flags duplicates and unparsable lines.",
            Schema(
                ("lines", "array", "Proposed test case lines in 'definition => result' form.")),
            ProposeCases);

        server.RegisterTool(
            "generate_test_data",
            "Generate a compilable Portamical test class from accepted test case specs. "
                + "Specs share one TestData family/arity. Returns the C# source; optionally compile-checked "
                + "when portamicalCoreDll is provided.",
            Schema(
                ("specs", "array", "TestCaseSpec objects: {definition, kind: returns|throws|custom, expectedTypeName, expectedValueLiteral, expectedDisplay, args: [{name, typeName, valueLiteral}], targetMethod}."),
                ("framework", "string", "Target framework: xunit_v3 (default), mstest, or nunit."),
                ("namespaceName", "string", "Namespace for the generated class."),
                ("className", "string", "Name of the generated test class."),
                ("portamicalCoreDll", "string", "Optional path of Portamical.Core.dll to enable the compile check.")),
            GenerateTestData);

        server.RegisterTool(
            "characterize_case",
            "Characterization mode: execute the method under test with given argument values in an "
                + "isolated load context and return the observed result or exception, so expected values "
                + "are verified instead of assumed.",
            Schema(
                ("assemblyPath", "string", "Path of the assembly containing the method under test."),
                ("typeName", "string", "Declaring type (full or simple name)."),
                ("methodName", "string", "Method to invoke."),
                ("args", "array", "Argument values as strings ('null' for null), in parameter order.")),
            CharacterizeCase);

        server.RegisterTool(
            "analyze_gaps",
            "Diff proposed test cases against test case names found in existing test sources. "
                + "Returns which proposals are already covered and which are missing.",
            Schema(
                ("testSourceFiles", "array", "Paths of existing test source files to scan."),
                ("proposedLines", "array", "Proposed test case lines in 'definition => result' form.")),
            AnalyzeGaps);

        server.RegisterTool(
            "generate_formatter",
            "Scaffold a Portamical Formatter<T> for a domain type that renders poorly in test case names.",
            Schema(
                ("typeName", "string", "The domain type to format."),
                ("formatExpression", "string", "Optional C# format expression using parameter 'value'.")),
            GenerateFormatter);
    }

    private static string AnalyzeTarget(JsonObject? args)
    {
        string sourceText = GetSourceText(args);
        string? methodName = args?["methodName"]?.GetValue<string>();

        var targets = string.IsNullOrWhiteSpace(methodName)
            ? TargetAnalyzer.AnalyzeAll(sourceText)
            : TargetAnalyzer.Analyze(sourceText, methodName);

        return JsonSerializer.Serialize(targets.Select(t => new
        {
            t.MethodName,
            t.ContainingType,
            t.ReturnTypeName,
            Parameters = t.Parameters.Select(p => new { p.Name, p.TypeName }),
            t.ThrownExceptionTypes,
        }), SerializerOptions);
    }

    private static string ProposeCases(JsonObject? args)
    {
        var lines = GetStringArray(args, "lines");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var results = new List<object>();

        foreach (string line in lines)
        {
            if (!NamingSemantics.TryParse(line, out var spec))
            {
                results.Add(new { line, valid = false, error = "Missing ' => ' separator or empty definition." });
                continue;
            }

            bool duplicate = !seen.Add(spec!.TestCaseName);

            results.Add(new
            {
                line,
                valid = true,
                duplicate,
                kind = spec.Kind.ToString().ToLowerInvariant(),
                definition = spec.Definition,
                testCaseName = spec.TestCaseName,
            });
        }

        return JsonSerializer.Serialize(results, SerializerOptions);
    }

    private static string GenerateTestData(JsonObject? args)
    {
        var specs = ParseSpecs(args?["specs"] as JsonArray
            ?? throw new ArgumentException("'specs' is required."));

        var framework = args?["framework"]?.GetValue<string>()?.ToLowerInvariant() switch
        {
            "mstest" => TestFramework.MSTest,
            "nunit" => TestFramework.NUnit,
            _ => TestFramework.XUnitV3,
        };

        string source = TestClassEmitter.EmitTestClass(
            specs,
            framework,
            args?["namespaceName"]?.GetValue<string>() ?? "Tests.Generated",
            args?["className"]?.GetValue<string>() ?? "GeneratedTests");

        string? coreDll = args?["portamicalCoreDll"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(coreDll))
        {
            return source;
        }

        var check = CompileChecker.Check([source], [coreDll]);
        string status = check.Success
            ? "// Compile check: OK"
            : "// Compile check FAILED:\n" + string.Join('\n', check.Errors.Select(e => "// " + e));

        return status + "\n\n" + source;
    }

    private static string CharacterizeCase(JsonObject? args)
    {
        string assemblyPath = Require(args, "assemblyPath");
        string typeName = Require(args, "typeName");
        string methodName = Require(args, "methodName");
        var argValues = GetStringArray(args, "args");

        using var characterizer = new Characterizer(assemblyPath);
        var result = characterizer.Characterize(typeName, methodName, argValues);

        return JsonSerializer.Serialize(new
        {
            result.Succeeded,
            result.Threw,
            result.ReturnValue,
            result.ExceptionTypeName,
            result.ExceptionMessage,
            suggestedResult = result.Threw
                ? $"{NamingSemantics.ThrowsPrefix} {result.ExceptionTypeName}"
                : $"{NamingSemantics.ReturnsPrefix} {result.ReturnValue}",
        }, SerializerOptions);
    }

    private static string AnalyzeGaps(JsonObject? args)
    {
        var files = GetStringArray(args, "testSourceFiles");
        var proposedLines = GetStringArray(args, "proposedLines");

        var existing = TestScanner.ExtractTestCaseNames(
            files.Select(f => File.ReadAllText(f!)));

        var proposed = new List<TestCaseSpec>();

        foreach (string line in proposedLines)
        {
            if (NamingSemantics.TryParse(line, out var spec))
            {
                proposed.Add(spec!);
            }
        }

        var gaps = SpecSet.AnalyzeGaps(existing, proposed);

        return JsonSerializer.Serialize(new
        {
            existingCount = existing.Count,
            covered = gaps.Covered.Select(s => s.TestCaseName),
            missing = gaps.Missing.Select(s => s.TestCaseName),
        }, SerializerOptions);
    }

    private static string GenerateFormatter(JsonObject? args)
    => FormatterEmitter.EmitFormatter(
        Require(args, "typeName"),
        args?["formatExpression"]?.GetValue<string>());

    internal static List<TestCaseSpec> ParseSpecs(JsonArray specsJson)
    {
        var specs = new List<TestCaseSpec>();

        foreach (JsonNode? node in specsJson)
        {
            if (node is not JsonObject spec)
            {
                continue;
            }

            var argsList = new List<ArgSpec>();

            if (spec["args"] is JsonArray argsJson)
            {
                foreach (JsonNode? argNode in argsJson)
                {
                    if (argNode is JsonObject arg)
                    {
                        argsList.Add(new ArgSpec(
                            arg["name"]?.GetValue<string>() ?? $"arg{argsList.Count + 1}",
                            arg["typeName"]?.GetValue<string>() ?? "object",
                            arg["valueLiteral"]?.GetValue<string>() ?? "null"));
                    }
                }
            }

            specs.Add(new TestCaseSpec
            {
                Definition = spec["definition"]?.GetValue<string>()
                    ?? throw new ArgumentException("Spec requires 'definition'."),
                Kind = spec["kind"]?.GetValue<string>()?.ToLowerInvariant() switch
                {
                    "returns" => ResultKind.Returns,
                    "throws" => ResultKind.Throws,
                    _ => ResultKind.Custom,
                },
                ExpectedTypeName = spec["expectedTypeName"]?.GetValue<string>(),
                ExpectedValueLiteral = spec["expectedValueLiteral"]?.GetValue<string>(),
                ExpectedDisplay = spec["expectedDisplay"]?.GetValue<string>(),
                Args = argsList,
                TargetMethod = spec["targetMethod"]?.GetValue<string>(),
            });
        }

        return specs;
    }

    private static string GetSourceText(JsonObject? args)
    {
        string? sourceFile = args?["sourceFile"]?.GetValue<string>();

        if (!string.IsNullOrWhiteSpace(sourceFile))
        {
            return File.ReadAllText(sourceFile);
        }

        return args?["sourceText"]?.GetValue<string>()
            ?? throw new ArgumentException("Either 'sourceFile' or 'sourceText' is required.");
    }

    private static string Require(JsonObject? args, string name)
    => args?[name]?.GetValue<string>()
        ?? throw new ArgumentException($"'{name}' is required.");

    private static List<string> GetStringArray(JsonObject? args, string name)
    => (args?[name] as JsonArray)?
        .Select(n => n?.GetValue<string>() ?? string.Empty)
        .ToList() ?? [];

    private static JsonObject Schema(params (string Name, string Type, string Description)[] properties)
    {
        var props = new JsonObject();

        foreach (var (name, type, description) in properties)
        {
            props[name] = new JsonObject
            {
                ["type"] = type,
                ["description"] = description,
            };

            if (type == "array")
            {
                ((JsonObject)props[name]!)["items"] = new JsonObject();
            }
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = props,
        };
    }
}
