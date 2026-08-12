# Portamical.Companion

**AI-Assisted Interactive Test Creation Toolset for the Portamical Ecosystem**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-5.0.0-orange.svg)](https://github.com/CsabaDu/Portamical)
[![C#](https://img.shields.io/badge/language-C%23-239120.svg)](https://docs.microsoft.com/dotnet/csharp/)

> **AI proposes test cases as human-readable `"definition => result"` one-liners; the type system, in-memory compile checking, and identity-based deduplication provide mechanical safety nets; characterization mode verifies expected values by executing the code under test.**

`Portamical.Companion` turns the Portamical `"definition => result"` test case name semantics into a **specification language for AI-assisted test creation**. An AI agent (via any MCP host — GitHub Copilot, VS Code, Visual Studio, Claude, Cursor) brainstorms test cases as readable one-liners you review like a checklist, then the companion materializes accepted cases into compilable [Portamical.Core](../Portamical.Core) `TestData` types, provider wiring, and test method skeletons.

---

## Why This Works

The core insight: Portamical's naming semantics make AI test generation **auditable and verifiable**.

1. **Readable specification** — `"Adding two positives => returns 5"` is both a valid test case name and a human-reviewable spec. What to test (text) is separated from how to encode it (types).
2. **Types constrain hallucination** — `TestDataReturns<TExpected, TArgs...>` / `TestDataThrows<TException, TArgs...>` force outcomes and argument types to match. Generated code either compiles or it doesn't.
3. **Compile check as a gate** — emitted code is verified by an in-memory Roslyn compilation against `Portamical.Core` before you ever see it.
4. **Deduplication for free** — `TestCaseName`-based identity (mirroring `INamedCase`) silently collapses duplicate AI proposals.
5. **Verified, not assumed** — characterization mode executes the method under test in a sandbox and reports the *actual* result or exception, so wrong-but-compiling expected values are caught.

---

## Architecture

```
Companion.Mcp  ─┐                       ┌─ Companion.Analysis   (Roslyn: signatures, test scan, compile check)
                ├──  Companion.Core  ───┼─ Companion.Emit       (TestData / provider / test method emitters)
Companion.Cli  ─┘   (TestCaseSpec,      └─ Companion.Execution  (sandboxed characterization)
                     NamingSemantics,
                     SpecSet)
```

| Project | Purpose |
|---|---|
| **Companion.Core** | `TestCaseSpec` domain model; `NamingSemantics` parser/renderer — single source of truth for `" => "`, `"returns"`, `"throws"`; `SpecSet` deduplication and gap analysis |
| **Companion.Analysis** | `TargetAnalyzer` (method signatures + thrown exceptions via Roslyn), `TestScanner` (extracts existing test case names from test sources), `CompileChecker` (in-memory verification gate) |
| **Companion.Execution** | `Characterizer` — invokes the method under test in a collectible `AssemblyLoadContext` with a timeout guard and captures the observed result or exception |
| **Companion.Emit** | `AritySelector` (TestData family + generic arguments, arity 1–9), `TestDataEmitter`, `TestMethodEmitter` (xUnit v3 / MSTest / NUnit), `TestClassEmitter`, `FormatterEmitter` (`Formatter<T>` scaffolds) |
| **Companion.Mcp** | Dependency-free stdio MCP server (JSON-RPC 2.0) exposing the companion tool surface to any MCP host |
| **Companion.Cli** | Packable dotnet tool for CI: scan test sources and gate on coverage gaps |

---

## The Interactive Workflow

```
1. DISCOVER    AI host calls analyze_target        → method signature, thrown exceptions
2. PROPOSE     AI writes "definition => result"    → propose_cases validates, flags
               one-liners; you review the list       duplicates and unparsable lines
3. VERIFY      characterize_case executes the      → observed result/exception replaces
               target with each case's arguments     assumed expected values
4. GENERATE    generate_test_data emits the test   → compile-checked C# with TestData
               class for accepted cases              array, data source, test method
5. GATE (CI)   portamical-companion gaps           → exit code 1 when proposed cases
                                                     are missing from the test suite
```

---

## MCP Server

Run the server over stdio from any MCP host:

```jsonc
// MCP host configuration
{
  "mcpServers": {
    "portamical-companion": {
      "command": "dotnet",
      "args": ["run", "--project", "Portamical.Companion/Companion.Mcp"]
    }
  }
}
```

### Tool Surface

| Tool | Purpose |
|---|---|
| `analyze_target` | Analyze a method under test: signature, parameters, thrown exceptions. Accepts `sourceFile` or `sourceText`, optional `methodName` (all public methods when omitted). |
| `propose_cases` | Validate/normalize proposed `"definition => result"` lines; classify kind (`returns` / `throws` / `custom`), flag duplicates and unparsable lines. |
| `generate_test_data` | Emit a compilable test class (TestData array + data source + test method) for accepted specs; optional compile check against `Portamical.Core.dll`. |
| `characterize_case` | Execute the method under test with a case's arguments in an isolated load context; returns the observed result or exception and a suggested `result` clause. |
| `analyze_gaps` | Diff proposed cases against test case names found in existing test sources; reports covered vs. missing. |
| `generate_formatter` | Scaffold a `Formatter<T>` for a domain type that renders poorly in test case names. |

---

## Examples

### Example 1: Propose and Validate Cases

Input lines (AI-proposed, human-reviewed):

```text
Adding two positives => returns 5
Null input => throws ArgumentNullException
bad line
Adding two positives => returns 5
```

`propose_cases` output: line 1 valid (`returns`), line 2 valid (`throws`), line 3 flagged unparsable, line 4 flagged duplicate.

### Example 2: Generate a Test Class (xUnit v3)

`generate_test_data` with a spec:

```jsonc
{
  "specs": [{
    "definition": "Adding two positives",
    "kind": "returns",
    "expectedTypeName": "int",
    "expectedValueLiteral": "5",
    "expectedDisplay": "5",
    "args": [
      { "name": "a", "typeName": "int", "valueLiteral": "2" },
      { "name": "b", "typeName": "int", "valueLiteral": "3" }
    ],
    "targetMethod": "Add"
  }],
  "framework": "xunit_v3",
  "namespaceName": "Tests.Calculator",
  "className": "AddTests"
}
```

emits (abbreviated):

```csharp
private static readonly TestDataReturns<int, int, int>[] testCases =
[
    TestDataFactory.CreateTestDataReturns<int, int, int>(
        "Adding two positives",
        5,
        2, 3)
];

public static IEnumerable<object?[]> TestCases
=> testCases.Select(td => td.ToArgs(ArgsCode.Instance));

[Theory]
[MemberData(nameof(TestCases))]
public void Add_validArgs_returnsExpected(TestDataReturns<int, int, int> testData)
{
    // Act
    var actual = Add(testData.Arg1, testData.Arg2);

    // Assert
    Assert.Equal(testData.Expected, actual);
}
```

### Example 3: Characterize Before Asserting

```jsonc
{ "assemblyPath": "bin/Debug/net10.0/Calculator.dll",
  "typeName": "Calculator", "methodName": "Divide", "args": ["1", "0"] }
```

returns:

```jsonc
{
  "Succeeded": true,
  "Threw": true,
  "ExceptionTypeName": "DivideByZeroException",
  "ExceptionMessage": "b must not be zero",
  "suggestedResult": "throws DivideByZeroException"
}
```

The observed outcome replaces the AI's assumption — the generated test asserts real behavior.

---

## CLI (CI Gap Gate)

```bash
# List all "definition => result" test case names in a test tree
portamical-companion scan _Tests/

# Gate a build: exit code 1 when proposals in proposals.txt are not covered
portamical-companion gaps proposals.txt _Tests/
```

---

## Build & Test

```bash
dotnet build Portamical.Companion/Portamical.Companion.slnx
dotnet test _Tests/Portamical.Companion
```

Test suite: `Tests.Portamical.Companion` (MSTest) — includes a compile-check integration test that emits a test class and compiles it against the real `Portamical.Core.dll`.

---

## Design Principles

- **Clean architecture** — hosts (MCP, CLI) depend on application services, which depend on the `TestCaseSpec` domain model; Roslyn and reflection are infrastructure adapters.
- **Deterministic tools, AI reasoning at the host** — the companion contains no LLM calls; the MCP host brings the model. Everything the companion does is reproducible and testable.
- **Mirrors Portamical semantics exactly** — `NamingSemantics` is the single source of truth matching `TestDataBase` behavior (`" => "` separator, `returns` / `throws` prefixes, ordinal `TestCaseName` identity).
- **Framework-agnostic emission** — data sources are emitted via `ToArgs(ArgsCode.Instance)`, so the same pipeline serves xUnit v3, MSTest, and NUnit.

---

## Related Modules

- [Portamical.Core](../Portamical.Core) — TestData types, `INamedCase` identity, `ArgsCode`/`PropsCode` strategies
- [Portamical.Core.Formatting](../Portamical.Core.Formatting) — `Formatter<T>` registry for test case name rendering
- [Portamical.Converters](../Portamical.Converters) — collection conversion, deduplication, data provider interfaces

---

## License

MIT — Copyright (c) 2026. Csaba Dudas (CsabaDu)
