# Portamical.Companion — Quick Start

Get from zero to AI-assisted, compile-checked Portamical tests in about five minutes.

**What you get:** an MCP server that lets any AI host (GitHub Copilot, VS Code, Visual Studio, Claude,
Cursor) propose test cases as human-readable `"definition => result"` one-liners, verify expected values by
actually executing your code, and emit compilable [Portamical.Core](../Portamical.Core) test classes — plus
a CLI that gates CI builds on test coverage gaps.

---

## 1. Prerequisites

- **.NET 10 SDK** or later
- The Portamical repository cloned locally (Companion lives in `Portamical.Companion/`)
- An MCP-capable AI host (optional — the CLI works standalone)

## 2. Build

```bash
dotnet build Portamical.Companion/Portamical.Companion.slnx
```

Optionally verify with the test suite:

```bash
dotnet test _Tests/Portamical.Companion
```

## 3. Hook Up the MCP Server

Add the server to your MCP host configuration (stdio transport, no extra dependencies):

```jsonc
// e.g. .vscode/mcp.json, claude_desktop_config.json, or your host's equivalent
{
  "mcpServers": {
    "portamical-companion": {
      "command": "dotnet",
      "args": ["run", "--project", "Portamical.Companion/Companion.Mcp"]
    }
  }
}
```

Restart your host. Six tools appear:

| Tool | One-line purpose |
|---|---|
| `analyze_target` | Read a method's signature, parameters, and thrown exceptions from C# source |
| `propose_cases` | Validate `"definition => result"` lines; flag duplicates and unparsable lines |
| `characterize_case` | Execute the method under test sandboxed; report the *observed* result/exception |
| `generate_test_data` | Emit a compile-checked test class (TestData array + data source + test method) |
| `analyze_gaps` | Diff proposals against existing test sources; report covered vs. missing |
| `generate_formatter` | Scaffold a `Formatter<T>` for types that render poorly in test case names |

## 4. Your First Workflow

Say you have this method:

```csharp
public static int Add(int a, int b) => a + b;
```

Ask your AI host something like:

> *"Use portamical-companion to analyze `Calculator.cs`, propose test cases for `Add`, and generate an
> xUnit v3 test class."*

Behind the scenes the host walks the pipeline:

**Step 1 — Discover.** `analyze_target` with `sourceFile: "Calculator.cs"`, `methodName: "Add"` returns the
signature and any exceptions the method throws.

**Step 2 — Propose & review.** The AI drafts one-liners; `propose_cases` validates them. **This is your
moment:** review the list like a checklist — it is the specification.

```text
Adding two positives => returns 5          ✅ valid (returns)
Null input => throws ArgumentNullException ✅ valid (throws)
bad line                                   ⚠️ unparsable
Adding two positives => returns 5          ⚠️ duplicate (collapsed)
```

**Step 3 — Verify (optional but recommended).** `characterize_case` runs the real method with each case's
arguments in an isolated, timeout-guarded load context:

```jsonc
{ "assemblyPath": "bin/Debug/net10.0/Calculator.dll",
  "typeName": "Calculator", "methodName": "Divide", "args": ["1", "0"] }
// → { "Threw": true, "ExceptionTypeName": "DivideByZeroException",
//     "suggestedResult": "throws DivideByZeroException" }
```

Observed behavior replaces AI assumptions — wrong-but-compiling expected values are caught here.

**Step 4 — Generate.** `generate_test_data` emits a complete test class for the accepted specs
(frameworks: `xunit_v3`, `mstest`, `nunit`), optionally compile-checked in memory against
`Portamical.Core.dll`:

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
    var actual = Add(testData.Arg1, testData.Arg2);
    Assert.Equal(testData.Expected, actual);
}
```

Paste it into your test project, run `dotnet test`, done.

## 5. CLI: Gate Your CI on Coverage Gaps

The CLI (`portamical-companion`, a packable dotnet tool) needs no AI at all.

```bash
# Run from source
dotnet run --project Portamical.Companion/Companion.Cli -- scan _Tests/

# Or install as a dotnet tool
dotnet pack Portamical.Companion/Companion.Cli
dotnet tool install --global --add-source Portamical.Companion/Companion.Cli/bin/Release portamical-companion
```

**`scan`** — list every `"definition => result"` test case name found in test sources:

```bash
portamical-companion scan _Tests/
```

**`gaps`** — CI gate. Put your reviewed proposals (one per line) in `proposals.txt`:

```text
Adding two positives => returns 5
Null input => throws ArgumentNullException
```

```bash
portamical-companion gaps proposals.txt _Tests/
# Existing test cases: 42
# Covered proposals:   1
# Missing proposals:   1
#   MISSING: Null input => throws ArgumentNullException
```

Exit code is `1` when any proposal is missing — wire it straight into your pipeline:

```yaml
# GitHub Actions
- name: Test coverage gap gate
  run: portamical-companion gaps proposals.txt _Tests/
```

## 6. Cheat Sheet

| I want to… | Use |
|---|---|
| Know what a method can do/throw | `analyze_target` |
| Turn AI brainstorming into a reviewable spec | `propose_cases` |
| Trust the expected values | `characterize_case` |
| Get compilable test code | `generate_test_data` |
| Check what's already tested | `analyze_gaps` / CLI `scan` |
| Fail the build on untested proposals | CLI `gaps` (exit code 1) |
| Make a custom type readable in test names | `generate_formatter` |

## 7. Tips

- **Review the one-liners, not the code.** The `"definition => result"` list is the contract; the emitted
  C# is mechanical. Human attention belongs on step 2.
- **Characterize when vibe coding.** If the AI wrote both the implementation and the tests, only observed
  execution separates "what it does" from "what you meant".
- **Duplicates are free.** Identity is the `TestCaseName` (ordinal comparison, mirroring `INamedCase`), so
  repeated AI proposals collapse silently.
- **One family/arity per generated class.** `generate_test_data` specs must share one TestData family
  (`TestData` / `TestDataReturns` / `TestDataThrows`) and arity (1–9 arguments); split mixed sets into
  multiple calls.
- **No LLM inside.** The companion is fully deterministic — every tool result is reproducible and testable;
  your MCP host brings the model.

## Learn More

- [Portamical.Companion README](README.md) — architecture, design principles, full tool reference
- [Portamical.Core](../Portamical.Core) — TestData types, identity, `ArgsCode`/`PropsCode`
- [Portamical.Core.Formatting](../Portamical.Core.Formatting) — `Formatter<T>` registry
