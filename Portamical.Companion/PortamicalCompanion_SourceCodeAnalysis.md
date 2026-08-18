## Portamical.Companion — Source Code Analysis

The provided code implements **Portamical.Companion**, a companion tool for the Portamical data-driven testing framework. It provides:

- A **CLI** (`Program.cs`) with `scan` and `gaps` commands.
- An **MCP server** (`McpServer` + `CompanionTools`) exposing the same functionality to AI assistants.
- **Analysis** services using Roslyn to inspect test source and method targets.
- **Emission** services that generate compilable C# test classes and formatter scaffolds.
- **Execution** via an isolated `AssemblyLoadContext` for characterization.
- **Core** types that model test case specs and naming semantics.

Overall the code is clean, well-structured, and follows modern C# practices (records, nullable reference types, collection expressions, DI-friendly design). However, several functional gaps and potential bugs exist, especially around compile-check references and edge-case handling.

---

## 1. High-Level Architecture

| Namespace / File | Responsibility |
|------------------|----------------|
| `Core` | Test case spec model, naming semantics, set operations |
| `Analysis` | Roslyn-based source analysis: compile checker, target analyzer, test scanner |
| `Emit` | Code generation for TestData arrays, test methods, formatters |
| `Execution` | In-memory assembly execution for characterization |
| `Mcp` | Minimal JSON-RPC MCP server and tool registration |
| `Program.cs` (CLI) | Command-line interface for `scan` and `gaps` |
| `Program.cs` (MCP) | MCP server entry point |

The separation of concerns is good. `Core` has no dependencies on Roslyn or code generation, making it unit-testable in isolation.

---

## 2. Component Analysis

### 2.1 Core (`NamingSemantics`, `SpecSet`, `TestCaseSpec`)

**Strengths**
- `NamingSemantics` centralizes the `definition => result` format, mirroring the Portamical core.
- `SpecSet.Distinct` uses ordinal `TestCaseName` equality, consistent with the framework’s identity semantics.
- Records and `required` init properties enforce valid object construction.

**Potential Issues**
- `NamingSemantics.RenderResult` for `Returns`:
  ```csharp
  $"{ReturnsPrefix} {spec.ExpectedDisplay ?? spec.ExpectedValueLiteral}"
  ```
  If both `ExpectedDisplay` and `ExpectedValueLiteral` are `null`, the result string becomes `"returns "` (trailing space). This may be acceptable, but could lead to unintended test names.
- `TryParse` for `Returns` only sets `ExpectedDisplay`, not `ExpectedValueLiteral`. Later code (e.g., `TestDataEmitter.EmitFactoryCall`) requires `ExpectedValueLiteral` for `Returns` and `Throws`. This means parsed proposals are incomplete until enriched by an AI or another step. That is intentional but should be documented clearly.

### 2.2 Analysis

#### `CompileChecker`
- **Purpose**: In-memory compile check of emitted code using Roslyn.
- **Strengths**: Uses `TRUSTED_PLATFORM_ASSEMBLIES` to get runtime references; supports extra reference paths.
- **Issue (Important)**: The compile check in `GenerateTestData` passes only `Portamical.Core.dll` as an additional reference, but the generated source includes `using Xunit;` / `using Microsoft.VisualStudio.TestTools.UnitTesting;` / `using NUnit.Framework;` and corresponding attributes. **Those assemblies are not referenced**, so the compile check will almost certainly fail for any non-xUnit framework and likely for xUnit too (unless xUnit is in the trusted platform assemblies, which it is not).  
  **Recommendation**: Add a parameter to `CompileChecker.Check` to accept framework reference paths, or omit the test framework attributes from the generated code when performing a compile check against only Portamical.Core.

- **Minor**: `MetadataReference.CreateFromFile(p)` will throw if the file path does not exist. Consider validating paths and reporting a friendly error.

#### `TargetAnalyzer`
- Extracts method signature and thrown exceptions from C# source.
- **Exception detection** only handles:
  - Direct `throw new ExceptionType(...)` (any object creation)
  - `ThrowIfNull` invocation (assumed to be `ArgumentNullException`)
- This misses:
  - `throw new ArgumentException(...)` (handled, but no type extraction from helper calls like `throw new MyException(...)` where the expression is not an object creation)
  - `throw new Exception(...)` (handled)
  - Indirect throws (`throw new InvalidOperationException(...)` is handled, but `throw new MyCustomException()` is handled)
  - More importantly, **throws inside called methods** are not detected. That is acceptable for static analysis of a single method, but the tool’s description says “thrown exceptions of the method body”. It does not perform flow analysis to catch exceptions from nested calls.
- **Containing type** is resolved via the first ancestor `TypeDeclarationSyntax`. This works for classes, structs, records, and interfaces but does not distinguish between them. Fine for naming.

#### `TestScanner`
- Extracts string literals that match the `definition => result` naming pattern.
- Uses `NamingSemantics.TryParse`, which requires the separator and non-empty definition.
- **Issue**: If a string literal contains the separator but is not meant to be a test case name, it will be incorrectly parsed. This is a heuristic limitation; acceptable for now.

### 2.3 Emit

#### `TestClassEmitter`
- Generates a full test class with usings, namespace, TestData array, data source, and test method.
- **Strengths**: Clean separation into `TestDataEmitter` and `TestMethodEmitter`.
- **Issues**:
  - Assumes all test methods are **static**. The generated test method calls `TargetMethod(...)` without any instance creation. If the target is an instance method, the generated test will not compile (unless `TargetMethod` is a fully qualified static call like `Class.Method(...)`, which is not supported by the current `TargetMethod` property).  
    **Recommendation**: Extend `TestCaseSpec` to include `ContainingType` or a factory expression for instance creation.
  - The `GenerateTestData` tool passes `distinct[0]` as the representative spec. If multiple specs have different `TargetMethod`s, the generated test method will use only the first target for all test cases. This is not validated.  
  - The compile check issue described above also impacts this component.

#### `TestDataEmitter`
- Emits `TestDataFactory` call expressions.
- **Strengths**: Handles escaping of strings, validates that all specs share the same TestData family/arity via `AritySelector` (not shown but referenced).
- **Issues**:
  - The second parameter of the factory call is always `ExpectedValueLiteral` for `Returns` and `Throws`. For `Throws`, this assumes the factory expects an **exception instance** (e.g., `new ArgumentNullException()`). If the Portamical factory instead expects the **exception type** or a different representation, the generated code will be invalid. This depends on the actual API, which is not visible here.
  - No validation that `ExpectedValueLiteral` is provided for `Returns`; it throws at runtime with a clear message, which is acceptable but could be caught earlier in `ParseSpecs`.

#### `TestMethodEmitter`
- Emits framework-specific data source and test method.
- **Strengths**: Supports xUnit v3, MSTest, NUnit; uses appropriate attributes and assertions.
- **Issues**:
  - The test method parameter is always named `testData` and typed as the constructed TestData type. This works if the Portamical `ToArgs` method returns a one-element `object[]` containing the TestData instance. That is not verified here.
  - For `Throws`, the assertion uses `spec.ExpectedTypeName!`. If that is null (e.g., a malformed spec with `Kind=Throws` but no exception type), it will throw a `NullReferenceException` at emission time, not at parse time.

### 2.4 Execution

#### `Characterizer`
- Loads a target assembly into a collectible `AssemblyLoadContext` and executes methods.
- **Strengths**: Isolated execution, timeout guard, conversion of primitive/enum arguments.
- **Issues**:
  - **Method selection** by name and parameter count only. If multiple overloads have the same name and parameter count but different types, `FirstOrDefault` may pick the wrong overload.  
    **Recommendation**: Add parameter type resolution or allow the caller to provide parameter type names.
  - **Argument conversion** is limited to `Convert.ChangeType` for primitives and enums. It cannot handle complex types (e.g., domain objects, arrays, custom structs). This is acceptable for a first version, but should be documented.
  - **Null handling for non-nullable value types**: If `argValues` contains `null` for a parameter of type `int`, `ConvertArgs` sets the object array slot to `null`. When `MethodInfo.Invoke` is called, it will throw a `TargetParameterCountException` or `ArgumentException` (wrapped). The resulting characterization result will report an exception instead of a meaningful conversion error. Not ideal.
  - The timeout logic uses `Task.Run` and `Wait(timeout)`. If the timeout is reached, the task continues running in the background; no cancellation is possible. This could leak threads/resources. A `CancellationTokenSource` with `Task.WaitAsync` would be more robust.

### 2.5 MCP Server

#### `McpServer`
- Minimal JSON-RPC 2.0 over stdio.
- **Strengths**: Simple, straightforward dispatch loop.
- **Issues**:
  - Only catches `JsonException` during parsing. Other exceptions (e.g., `InvalidOperationException` when accessing wrong JSON node type) will crash the server loop.  
    **Recommendation**: Wrap the entire parsing/dispatch in a try-catch and return a JSON-RPC error.
  - Does not handle `notifications/initialized` or `cancelled`; those are just skipped (no `id`), which is acceptable for minimal MCP.
  - `tools/list` returns the same `inputSchema` object; if mutable, could be modified. `DeepClone()` is used in some places, but not consistently.

#### `CompanionTools`
- Registers six tools with clear descriptions and schemas.
- **Strengths**: Good separation of concerns; handlers delegate to core/analysis/emit/execution.
- **Issues**:
  - `GenerateTestData` compile check problem (described earlier).
  - `ParseSpecs` does not validate required fields (`ExpectedValueLiteral` for `Returns`/`Throws`, `ExpectedTypeName` for `Throws`). This leads to late failures in emitters.
  - `CharacterizeCase` does not allow specifying an instance factory, so it only works for static methods or types with a parameterless constructor. The same limitation as `TestClassEmitter`.

### 2.6 CLI (`Program.cs` for `scan`/`gaps`)

- **Strengths**: Clear command handling, recursive file resolution.
- **Issues**:
  - Unknown command returns exit code `0` (via `PrintUsage`). This is misleading for CI; unknown commands should return a non-zero code (e.g., `2`).
  - `ResolveSourceFiles` warns on missing paths but continues; if all paths are missing, `Scan` and `Gaps` will process zero files. No error is returned.
  - `File.ReadAllText`/`File.ReadAllLines` are called without exception handling. In CI, unreadable files will crash the tool.

---

## 3. Strengths Summary

- **Clear domain modeling** (`TestCaseSpec`, `ResultKind`, `ArgSpec`).
- **Roslyn integration** for source analysis and in-memory compilation.
- **Isolated execution** via collectible `AssemblyLoadContext` is a sound approach for safe characterization.
- **MCP server** is lightweight and easily extendable.
- **Modern C#** features used consistently (records, nullable annotations, collection expressions).
- **XML documentation** is thorough and helpful.

---

## 4. Critical Issues & Recommendations

1. **Compile check missing test framework references**  
   In `GenerateTestData`, when `portamicalCoreDll` is provided, the compile check runs against generated code that uses xUnit/MSTest/NUnit types. These assemblies are not referenced, causing false compile failures.  
   **Fix**: Add framework reference paths to `CompileChecker.Check`, or generate a compile-check variant without framework attributes.

2. **Instance method support**  
   Both `Characterizer` and `TestClassEmitter`/`TestMethodEmitter` only support static methods or types with parameterless constructors. Many test targets are instance methods.  
   **Fix**: Extend `TestCaseSpec` with `ContainingType` and optionally an instance factory expression. For `Characterizer`, allow the caller to provide an instance or a constructor signature.

3. **Method overload resolution in `Characterizer`**  
   Selecting by name and parameter count is fragile.  
   **Fix**: Accept parameter type names or use a best-match algorithm based on convertible types.

4. **MCP server robustness**  
   The server loop may crash on malformed JSON structures other than `JsonException`.  
   **Fix**: Wrap parsing and dispatch in a broader try-catch and return a JSON-RPC error.

5. **CLI exit codes**  
   Unknown command returns `0`; missing all paths results in `0`.  
   **Fix**: Return appropriate error codes (`2` for usage errors, `1` for operational failures).

6. **Validation timing**  
   Many errors (missing `ExpectedValueLiteral`, `ExpectedTypeName`) are only detected inside emitters, not at spec parsing time.  
   **Fix**: Validate `TestCaseSpec` in `ParseSpecs` and `NamingSemantics.TryParse` (or a dedicated validator) before use.

---

## 5. Conclusion

`Portamical.Companion` is a well-architected companion tool with a clear purpose. The use of Roslyn for analysis, emit, and compile checking is appropriate. The main risks are:

- Incomplete compile-check references causing false negatives.
- Lack of instance method support.
- Fragile overload resolution and argument conversion in characterization.
- Some error handling and validation gaps in the MCP/CLI surfaces.

With targeted improvements in those areas, the tool would be robust for CI and AI-assisted test generation workflows.