# Portamical

**A Universal, Identity-Driven Test Data Modeling Framework for .NET**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/language-C%23-239120.svg)](https://docs.microsoft.com/dotnet/csharp/)
[![Stars](https://img.shields.io/github/stars/CsabaDu/Portamical?style=social)](https://github.com/CsabaDu/Portamical/stargazers)

> **Write test data once. Run it on xUnit v2, xUnit v3, MSTest 4, and NUnit 4—without rewriting tests or losing strong typing.**

Portamical is the **test data abstraction layer** missing from the .NET testing ecosystem. It treats test data as **first-class domain objects** with deterministic identity, enabling automatic deduplication, cross-framework portability, and self-documenting test output.

---

## The Problem It Solves

| Traditional Approach | Portamical Approach |
|---------------------|---------------------|
| 🔴 Duplicate test data per framework | ✅ Write once, consume everywhere |
| 🔴 Fragile `object[]` arrays | ✅ Strongly typed generics (up to 9 args) |
| 🔴 Cryptic test names in runners | ✅ Human-readable "definition => result" |
| 🔴 Duplicate test cases slip through | ✅ Built-in identity-based deduplication |
| 🔴 Exception assertions differ by framework | ✅ Unified `PortamicalAssert` with delegate injection |
| 🔴 Boilerplate test data setup | ✅ `TestDataFactory` with fluent creation |
| 🔴 Mutable test state | ✅ `init`-only properties throughout |

---

## Quick Start

### 1. Installation

**For new projects (NuGet package - coming soon):**
```bash
# Install the core library
dotnet add package Portamical.Core

# Install your framework adapter
dotnet add package Portamical.xUnit     # for xUnit v2
dotnet add package Portamical.MSTest    # for MSTest 4
dotnet add package Portamical.NUnit     # for NUnit 4
dotnet add package Portamical.xUnit.v3  # for xUnit v3
```

**For contributors (clone and build):**
```bash
git clone https://github.com/CsabaDu/Portamical.git
cd Portamical
dotnet build
dotnet test
```

### 2. Choose Your Framework Solution

| Framework | Solution File | Use Case |
|-----------|---------------|----------|
| **Core Library** | `Portamical.Core.slnx` | Framework-agnostic development |
| **Shared Layer** | `Portamical.slnx` | Converters, assertions, base classes |
| **xUnit v2** | `Portamical.xUnit.slnx` | xUnit 2.x integration |
| **xUnit v3** | `Portamical.xUnit_v3.slnx` | xUnit 3.2.2+ integration |
| **MSTest 4** | `Portamical.MSTest.slnx` | MSTest 4.0.2+ integration |
| **NUnit 4** | `Portamical.NUnit.slnx` | NUnit 4.4.0+ integration |

### 3. Create Your First Data Source

```csharp
using static Portamical.Core.Factories.TestDataFactory;

// Identity‑driven test cases with deterministic naming
public class EmailValidationCases
{
    public IEnumerable<TestData<string>> GetValidArgs()
    {
        // Each test case defines:
        // - a human‑readable identity ("definition")
        // - the expected outcome ("result")
        // - the argument sequence (arg1, arg2, ...)
        yield return CreateTestData(
            definition: "input is a valid email",
            result: "validates successfully",
            arg1: "user@example.com");

        yield return CreateTestData(
            definition: "input is a valid name",
            result: "validates successfully",
            arg1: "John Doe");
    }
}
```

**Power Pattern:** Use local helper methods to maintain consistency and safety:

```csharp
public class AdvancedEmailValidationCases
{
    public IEnumerable<TestDataReturns<bool, string>> ValidEmails()
    {
        // Local helper centralizes factory call shape
        // - Ensures argument ordering stays invariant across all yields
        // - Makes edits safer: update variables once, keep call sites unchanged
        TestDataReturns<bool, string> createTestData()
            => CreateTestDataReturns(
                definition: definition,
                expected: expected,
                arg1: email);

        // First case
        string definition = "input is a valid email";
        bool expected = true;
        string email = "user@example.com";
        yield return createTestData();

        // Reassign the same variables for the next identity
        definition = "input is a valid email with subdomain";
        expected = true;
        email = "john.doe@mail.example.com";
        yield return createTestData();
    }
}
```

### 4. Consume Across All Frameworks

**The same data source works everywhere:**

```csharp
// xUnit v2/v3
[Theory, MemberData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) 
{
    var actual = Validate(testData.Arg1);
    Assert.True(actual);
}

// MSTest
[TestMethod, DynamicData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) 
{
    var actual = Validate(testData.Arg1);
    Assert.IsTrue(actual);
}

// NUnit
[Test, TestCaseSource(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) 
{
    var actual = Validate(testData.Arg1);
    Assert.That(actual, Is.True);
}
```

---

## Architecture

### Layered Design (Zero-Dependency Core)

```
┌────────────────────────────────────────────────┐
│            _SampleCodes                        │  ← Reference implementations
│  (Testables, DataSources, UnitTests)           │
└───────────────────────┬────────────────────────┘
                        │ depends on
┌───────────────────────▼────────────────────────┐
│  Portamical.xUnit | xUnit_v3 | MSTest | NUnit  │  ← Framework adapters
│          (Thin adapter layer)                  │
└───────────────────────┬────────────────────────┘
                        │ depends on
┌───────────────────────▼────────────────────────┐
│              Portamical                        │  ← Shared utilities
│  (Converters, Assertions, TestBases)           │
└───────────────────────┬────────────────────────┘
                        │ depends on
┌───────────────────────▼────────────────────────┐
│          Portamical.Core                       │  ← Pure abstractions
│  (Interfaces, Models, Factory — ZERO DEPS)     │
└────────────────────────────────────────────────┘
```

**Key Principle:** Portamical.Core has **zero external dependencies**, ensuring maximum portability and future-proofing.

---

### Namespace Dependency Diagram

![Portamical_Namespaces_Hierarchy](https://raw.githubusercontent.com/CsabaDu/Portamical/refs/heads/master/_Images/Portamical_Namespaces_Hierarchy.svg)

---

### Class Hierarchy (Template Method + Composite)

```
NamedCase (abstract) : INamedCase : IEquatable<INamedCase>
 └── TestDataBase (abstract) : ITestData
      ├── TestData<T1..T9> (abstract)
      │    └── [T4-generated: TestData<T1> → ... → TestData<T1,...,T9>]
      └── TestDataExpected<TResult> (abstract) : IExpected<TResult>
           ├── TestDataReturns<TStruct> : IReturns<TStruct>
           │    └── [T4-generated: TestDataReturns<TStruct,T1> → ... → <TStruct,T1,...,T9>]
           └── TestDataThrows<TException> : IThrows<TException>
                └── [T4-generated: TestDataThrows<TException,T1> → ... → <TException,T1,...,T9>]
```

**Key:** T4 code generation eliminates 27 classes worth of boilerplate while maintaining type safety.

---

## Core Innovation: Identity-Driven Test Cases

Every test case is an **immutable value object** with deterministic identity:

```
"<definition> => <result>"
```

### Real Examples from the Codebase

| Test Case Identity |
|--------------------|
| `"Valid name and dateOfBirth is equal with the current day => creates BirthDay instance"` |
| `"name is null => throws ArgumentNullException"` |
| `"other is null => returns -1"` |

### What This Enables

1. **Automatic Deduplication**
   ```csharp
   // Built into test data providers
   private readonly HashSet<INamedCase> _namedCases = new(NamedCase.Comparer);
   
   public override void Add(ITheoryTestDataRow row)
   {
       if (_namedCases.Add(row))  // ← Identity-based add
       {
           base.Add(row);
       }
   }
   ```

2. **Self-Documenting Test Output**
   - Test runners display: `"input is valid email => validates successfully"`
   - No manual `[Trait]`, `[TestName]`, or `DisplayName` attributes needed

3. **Cross-Framework Consistency**
   - Same identity regardless of xUnit, MSTest, or NUnit
   - Enables traceability from requirement → test data → execution

4. **Value-Based Equality**
   ```csharp
   public static IEqualityComparer<INamedCase> Comparer { get; } =
       new NamedCaseEqualityComparer();
   
   public bool Equals(INamedCase? x, INamedCase? y)
   => StringComparer.Ordinal.Equals(x?.TestCaseName, y?.TestCaseName);
   ```

---

## Test Data Types

### Universal Test Data Model

| Type | Purpose | Constraints | Use Case |
|------|---------|-------------|----------|
| `TestData<T1..T9>` | General scenarios | Up to 9 arguments | Constructor tests, basic behavior |
| `TestDataReturns<TStruct, T1..T9>` | Return-value assertions | `TStruct : struct` | Method output validation |
| `TestDataThrows<TException, T1..T9>` | Exception assertions | `TException : Exception` | Error handling tests |

### Factory Pattern (T4-Generated)

```csharp
using static Portamical.Core.Factories.TestDataFactory;

// General test data
yield return CreateTestData(
    definition: "Valid input",
    result: "creates instance",
    arg1: "valid name",
    arg2: DateOnly.FromDateTime(DateTime.Now));

// Return-value test data
yield return CreateTestDataReturns(
    definition: "other is null",
    expected: -1,  // ← TStruct (value type)
    arg1: dateOfBirth,
    arg2: (BirthDay?)null);

// Exception test data
yield return CreateTestDataThrows(
    definition: "name is null",
    expected: new ArgumentNullException("name"),
    arg1: (string?)null);
```

**Key:** Factory methods are **T4-generated** from a single source (`SharedHelpers.ttinclude`). Change `MaxArity = 9` to support more arguments, regenerate, and build.

---

## Data Strategy Pattern

The **Strategy Pattern** (`ArgsCode` + `PropsCode`) controls how test data materializes into framework-consumable rows.

### Strategy Modes

#### 1. **TestData Mode** (Direct Instance Flow)

```csharp
// Data source
public static IEnumerable<TTestData> Data => Convert(dataSource.GetArgs());

// Test signature
void Test(TestData<DateOnly> testData) { ... }
```

**Best for:** Test methods that need access to the `TestCaseName` property or prefer working with the complete test data object.

#### 2. **Instance Mode** (`ArgsCode.Instance`)

```csharp
// Data source
public static IEnumerable<object?[]> Data => Convert(dataSource.GetArgs());

// Test signature (same as TestData mode)
void Test(TestData<DateOnly> testData) { ... }
```

**Best for:** Frameworks requiring `object?[]` collections (MSTest, NUnit).

#### 3. **Properties Mode** (`ArgsCode.Properties`)

```csharp
// Data source
public static IEnumerable<object?[]> Data 
    => Convert(dataSource.GetArgs(), AsProperties);

// Test signature (flattened parameters)
void Test(DateOnly dateOfBirth) { ... }
```

**Best for:** Test methods that prefer flattened parameter signatures.

### PropsCode Options

| `PropsCode` | Includes | Use Case |
|-------------|----------|----------|
| `All` | `TestCaseName` + all properties | MSTest with `DynamicDataDisplayName` |
| `TrimTestCaseName` | All properties except `TestCaseName` | **Default** — test runner provides naming |
| `TrimReturnsExpected` | Also excludes `Expected` if `IReturns` | NUnit return-value tests |
| `TrimThrowsExpected` | Also excludes `Expected` if `IThrows` | Exception tests where assertion extracts exception |

---

## Design Patterns Catalog

Portamical implements **15 GoF and architectural patterns** to achieve portability and maintainability.

| Pattern | Implementation | Purpose |
|---------|----------------|---------|
| **Factory** | `TestDataFactory` (T4-generated) | Centralized test data creation |
| **Builder** | Named parameters in factory methods | Fluent, self-documenting API |
| **Adapter** | Framework-specific adapters | Translate `ITestData` to framework types |
| **Strategy** | `ArgsCode` + `PropsCode` enums | Configurable data serialization |
| **Template Method** | `TestDataBase.ToArgs()` | Skeleton algorithm with hooks |
| **Composite** | Test data hierarchy | Uniform treatment of test data |
| **Command** | Delegate injection in `PortamicalAssert` | Framework-agnostic assertions |
| **Iterator** | `IEnumerable<TTestData>` | Lazy test data evaluation |
| **Null Object** | Optional `testMethodName` | Eliminate null checks |
| **Identity** | `NamedCase.Comparer` | Value object with deterministic identity |
| **Dependency Inversion** | Layered architecture | Core has zero dependencies |
| **Repository** | Data sources | Centralized test data storage |
| **Code Generation** | T4 templates | Single source of truth (MaxArity) |
| **Local Method Pattern** | `static` local functions | Encapsulated helpers, zero closure cost |
| **Bridge** | Core ↔ Adapters | Decouple abstraction from implementation |

[**Full Pattern Analysis**](https://github.com/CsabaDu/Portamical/discussions)

---

## T4 Code Generation

### How It Works

All generic test data classes and the factory are **generated at design time** by T4 templates.

```
Portamical.Core/
├── T4/
│   └── SharedHelpers.ttinclude        ← Single source of truth (MaxArity = 9)
├── Factories/
│   ├── TestDataFactory.tt             ← Template
│   └── TestDataFactory.generated.cs   ← Auto-generated output (736 lines)
└── TestDataTypes/Models/
    ├── General/
    │   ├── TestData.tt                ← Template
    │   └── TestData.generated.cs      ← Auto-generated output
    └── Specialized/
        ├── TestDataReturns.tt         ← Template
        ├── TestDataReturns.generated.cs
        ├── TestDataThrows.tt          ← Template
        └── TestDataThrows.generated.cs
```

### Centralized Configuration

**All four templates** share a single `SharedHelpers.ttinclude` file:

```csharp
// Portamical.Core/T4/SharedHelpers.ttinclude
const int MaxArity = 9;  // ← Change once, regenerate all
```

### Regeneration Process

```bash
# 1. Edit SharedHelpers.ttinclude → change MaxArity
vim Portamical.Core/T4/SharedHelpers.ttinclude

# 2. In Visual Studio: select all 4 .tt files
# 3. Right-click → Run Custom Tool

# 4. Build
dotnet build
```

### Design Decisions

| Template | Pattern | Include Placement | Rationale |
|----------|---------|-------------------|-----------|
| `TestData.tt` | Inline text output | End of file | Extra trailing newline is harmless |
| `TestDataReturns.tt` | Inline text output | End of file | Same |
| `TestDataThrows.tt` | Inline text output | End of file | Same |
| `TestDataFactory.tt` | `StringBuilder` | Joined before line 5 | Prevents CS8802 (second compilation unit) |

**Note:** The `.generated.cs` files are **checked into source control** so the solution builds without running T4 transformations.

---

## Framework Adapters

Thin, optional adapters bridge Portamical to each test runner:

| Project | Framework | Key Integration | Package Reference |
|---------|-----------|-----------------|-------------------|
| `Portamical.xUnit` | xUnit v2 | `MemberTestDataAttribute`, `TestDataProvider` | `xunit` 2.9.3 |
| `Portamical.xUnit_v3` | xUnit v3 (3.2.2+) | `MemberTestDataAttribute`, `TheoryTestData`, `ITheoryTestDataRow` | `xunit.v3` 3.2.2 |
| `Portamical.MSTest` | MSTest 4 (4.0.2+) | `DynamicTestDataAttribute` | `MSTest.TestFramework` 4.0.2 |
| `Portamical.NUnit` | NUnit 4 (4.4.0+) | `TestCaseDataSourceAttribute`, `TestCaseTestData` | `NUnit` 4.4.0 |

### Same Data Source, Four Frameworks

```csharp
// Shared — works everywhere
private static readonly BirthDayDataSource _dataSource = new();

// xUnit v2 / v3
public static IEnumerable<object?[]> Args => Convert(_dataSource.GetConstructorValidArgs());
[Theory, MemberData(nameof(Args))]

// MSTest
private static IEnumerable<object?[]> Args => Convert(_dataSource.GetConstructorValidArgs());
[TestMethod, DynamicData(nameof(Args))]

// NUnit
public static IEnumerable<object?[]> Args => Convert(_dataSource.GetConstructorValidArgs());
[Test, TestCaseSource(nameof(Args))]
```

---

## Unified Exception Assertions

`PortamicalAssert.ThrowsDetails` validates exception **type**, **message**, and **parameter name** using **delegate injection** (Command Pattern):

```csharp
// xUnit
PortamicalAssert.ThrowsDetails(attempt, expected,
    catchException: Record.Exception,
    assertIsType: Assert.IsType,
    assertEquality: Assert.Equal,
    assertFail: Assert.Fail);

// NUnit
PortamicalAssert.ThrowsDetails(attempt, expected,
    catchException: CatchException,
    assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
    assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
    assertFail: Assert.Fail);

// MSTest
PortamicalAssert.ThrowsDetails(attempt, expected,
    catchException: CatchException,
    assertIsType: (e, a) => Assert.AreEqual(e, a.GetType()),
    assertEquality: (e, a) => Assert.AreEqual(e, a),
    assertFail: Assert.Fail);
```

**Key:** Framework-specific assertion methods are injected as **delegates**, making the core logic framework-agnostic.

---

## Sample Code Walkthrough

The `_SampleCodes` folder contains a complete **BirthDay** class example:

### The Testable Class

```csharp
// _SampleCodes/Testables/SampleClasses/BirthDay.cs
public class BirthDay : IComparable<BirthDay>
{
    public string Name { get; init; }
    public DateOnly DateOfBirth { get; init; }

    public BirthDay(string name, DateOnly dateOfBirth) { ... }
    public int CompareTo(BirthDay? other) => ...;
}
```

### The Data Source (Framework-Agnostic)

```csharp
// _SampleCodes/DataSources/TestDataSources/BirthDayDataSource.cs
public class BirthDayDataSource
{
    // TestData<DateOnly> — general constructor scenarios
    public IEnumerable<TestData<DateOnly>> GetBirthDayConstructorValidArgs()
    {
        const string result = "creates BirthDay instance";
        
        string definition = "Valid name and dateOfBirth is equal with the current day";
        DateOnly dateOfBirth = Today;
        yield return createTestData();  // ← Local method pattern

        #region Local Methods
        TestData<DateOnly> createTestData()
        => CreateTestData(definition, result, dateOfBirth);
        #endregion
    }

    // TestDataThrows<ArgumentException, string> — exception scenarios
    public IEnumerable<TestDataThrows<ArgumentException, string>> GetBirthDayConstructorInvalidArgs() { ... }

    // TestDataReturns<int, DateOnly, BirthDay> — return-value scenarios
    public IEnumerable<TestDataReturns<int, DateOnly, BirthDay>> GetCompareToArgs() { ... }
}
```

### Test Classes (One Data Source → Four Frameworks)

| Framework | Shared Mode | Native Mode |
|-----------|--------------|-----------------|
| xUnit v2 | `_UnitTests/xUnit/Shared` | `_UnitTests/xUnit/Native` |
| xUnit v3 | `_UnitTests/xUnit_v3/Shared` | `_UnitTests/xUnit_v3/Native` |
| MSTest 4 | `_UnitTests/MSTestShared` | `_UnitTests/MSTest/Native/Native` |
| NUnit 4 | `_UnitTests/NUnit/Shared` | `_UnitTests/NUnit/Native/Native` |

```bash
# Run the MSTest sample
dotnet test _SampleCodes/_UnitTests/MSTest/

# Run the NUnit sample
dotnet test _SampleCodes/_UnitTests/NUnit/

# Run the xUnit v3 sample
dotnet test _SampleCodes/_UnitTests/xUnit_v3/
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview or later)
- **For T4 regeneration only:**
  - [Visual Studio 2022 17.14+](https://visualstudio.microsoft.com/) with **Text Template Transformation** component
- **Framework requirements (pick one or more):**
  - xUnit v2 (`xunit` 2.x)
  - xUnit v3 (`xunit.v3` 3.2.2+)
  - MSTest 4 (`MSTest.TestFramework` 4.0.2+)
  - NUnit 4 (`NUnit` 4.4.0+)

**Note:** .NET 10 is currently in preview. The framework will support .NET 8+ in future releases.

---

## Contributing

Contributions are welcome! Here's how to get started:

### 1. Fork and Branch

```bash
git checkout -b feature/my-improvement
```

### 2. Follow Code Conventions

- **SPDX license headers** on all `.cs` files
- **`init`-only properties** for immutability
- **XML doc comments** on public APIs
- **Local methods** with `#region Local methods` for encapsulation
- **Naming:** `camelCase` for local methods, `PascalCase` for public APIs

### 3. Regenerate T4 Output (If Applicable)

If you modified any `.tt` or `.ttinclude` files:

```bash
# In Visual Studio:
# Right-click the .tt files → Run Custom Tool
# Commit the updated .generated.cs files
```

### 4. Build and Test

```bash
dotnet build
dotnet test
```

### 5. Open a Pull Request

Submit against `master` with a clear description.

### Branch Conventions

| Branch | Purpose |
|--------|---------|
| `master` | Stable, production-ready code |
| `Without_tt` | Pre-T4 baseline (manual generic classes) |
| `T4` | T4 template development |

### Reporting Issues

Use [GitHub Issues](https://github.com/CsabaDu/Portamical/issues) with:
- Steps to reproduce
- Expected vs. actual behavior
- .NET SDK version and test framework

---

## Repository Statistics

- **Created:** January 16, 2026 (46 days ago)
- **Language:** C# (98.5%)
- **Size:** ~7,223 KB
- **Stars:** ⭐ 1
- **Forks:** 0
- **Open Issues:** 0
- **License:** MIT
- **Visibility:** Public

[View Recent Commits](https://github.com/CsabaDu/Portamical/commits/master) | [View All Activity](https://github.com/CsabaDu/Portamical/events)

---

## Why Portamical?

Portamical **elevates test data from a framework concern to a domain concern**. It treats test cases as **immutable, identity-driven value objects**, enabling:

- ✅ **Portability:** Write once, run on xUnit, MSTest, and NUnit
- ✅ **Strong Typing:** Generics up to 9 arguments (T4-generated)
- ✅ **Deduplication:** Automatic via identity-based `HashSet<INamedCase>`
- ✅ **Self-Documentation:** Test names read like specifications
- ✅ **Immutability:** `init`-only properties throughout
- ✅ **Zero Boilerplate:** Factory pattern + T4 code generation
- ✅ **Unified Assertions:** `PortamicalAssert` with delegate injection

### Ideal For

- ✅ Large test suites (500+ parameterized tests)
- ✅ Multi-framework environments
- ✅ Domain-heavy logic with many edge cases
- ✅ Projects needing human-readable test reports
- ✅ Teams prioritizing consistency and maintainability

### Not Ideal For

- ⚠️ Simple test suites (<100 tests)
- ⚠️ Projects restricted to .NET 8 or earlier
- ⚠️ Teams unfamiliar with design patterns
- ⚠️ Projects requiring framework-specific features (e.g., xUnit's `IClassFixture`)

---

## License and Project Lineage

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

`Portamical.Core` is the **continuation and successor** of `CsabaDu.DynamicTestData.Core` (also MIT-licensed).  
`CsabaDu.DynamicTestData.Core` is considered **legacy** and is **no longer supported**; new development happens in Portamical.

### What Changed Compared to CsabaDu.DynamicTestData.Core?

Portamical continues the original ideas, with important corrections and refinements:

- **Data model**: moved away from a record-based model (which proved to be a wrong fit) to **immutable classes**.
- **Identity**: improved test case name construction and identity handling:
  - More effective name construction (Span-based for performance)
  - Deduplication via a comparer
- **Naming/clarity**: several concepts were renamed for readability and long-term maintainability (e.g., `PropsCode` values and related terms).

### Migration Guidance (High Level)

If you are using `CsabaDu.DynamicTestData.Core`:
- Prefer migrating to `Portamical.Core` for continued support and improvements.
- Expect mostly mechanical renames, restructured namespaces, plus updates where the API surface changed due to the move from records to immutable classes.

If you need detailed migration guidance, please open an issue requesting a `MIGRATION.md` with:
- Package replacement mapping
- Namespace/type rename table
- Common before/after snippets

---

## Links

- [GitHub Repository](https://github.com/CsabaDu/Portamical)
- [Discussions](https://github.com/CsabaDu/Portamical/discussions)
- [Issues](https://github.com/CsabaDu/Portamical/issues)
- [Pull Requests](https://github.com/CsabaDu/Portamical/pulls)

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*