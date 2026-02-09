# Portamical

**A universal, identity-driven test data modeling framework for .NET**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/language-C%23-239120.svg)](https://docs.microsoft.com/dotnet/csharp/)

Define test data **once**. Run it on **xUnit v2**, **xUnit v3**, **MSTest 4**, and **NUnit 4** — without rewriting tests or losing strong typing.

Portamical is the **test data abstraction layer** missing from the .NET testing ecosystem.

---

## Table of Contents

- [Getting Started](#getting-started)
- [Core Innovation: Identity-Driven Test Cases](#core-innovation-identity-driven-test-cases)
- [Universal Test Data Model](#universal-test-data-model)
- [Three Consumption Modes](#three-consumption-modes)
- [Framework Adapters](#framework-adapters)
- [Unified Exception Assertions](#unified-exception-assertions)
- [Architecture](#architecture)
- [T4 Code Generation](#t4-code-generation)
- [Sample Code Walkthrough](#sample-code-walkthrough)
- [Solution Files](#solution-files)
- [Prerequisites](#prerequisites)
- [Contributing](#contributing)
- [License](#license)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/CsabaDu/Portamical.git
cd Portamical
```

### 2. Open the solution for your test framework

| Your framework | Open this solution |
|----------------|--------------------|
| Core library only | `Portamical.Core.slnx` |
| Shared layer | `Portamical.slnx` |
| xUnit v2 | `Portamical.xUnit.slnx` |
| xUnit v3 | `Portamical.xUnit_v3.slnx` |
| MSTest 4 | `Portamical.MSTest.slnx` |
| NUnit 4 | `Portamical.NUnit.slnx` |

### 3. Build and run

```bash
dotnet build
dotnet test
```

### 4. Create your first test data source

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public class MyDataSource
{
    public IEnumerable<TestData<string>> GetValidArgs()
    {
        yield return CreateTestData(
            definition: "input is a valid email",
            result: "returns true",
            arg1: "user@example.com");

        yield return CreateTestData(
            definition: "input is a valid name",
            result: "returns true",
            arg1: "John Doe");
    }
}
```

### 5. Consume it in any test framework

```csharp
// xUnit
[Theory, MemberData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) { ... }

// MSTest
[TestMethod, DynamicData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) { ... }

// NUnit
[Test, TestCaseSource(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) { ... }
```

---

## Core Innovation: Identity-Driven Test Cases

Every test case is an immutable domain object with deterministic identity:

```
"<definition> => <result>"
```

**Examples from the codebase:**
- "Valid name and dateOfBirth is equal with the current day => creates BirthDay instance"
- "name is null => throws ArgumentNullException"
- "other is null => returns -1"

Identity enables:
- **Automatic deduplication** — via `NamedCase.Comparer` + `HashSet<INamedCase>`
- **Self-documenting test output** — human-readable names in every test runner
- **Cross-framework naming consistency** — same name regardless of runner
- **Traceability** — from requirement → test data → execution

---

## Universal Test Data Model

Portamical replaces `object[]`-based test data with strongly typed, immutable objects:

| Type | Purpose | Key Properties |
|------|---------|---------------|
| `TestData<T1..T9>` | General scenarios (definition + string result) | `Arg1..Arg9` |
| `TestDataReturns<TStruct, T1..T9>` | Return-value expectations (`TStruct : struct`) | `Expected`, `Arg1..Arg9` |
| `TestDataThrows<TException, T1..T9>` | Exception expectations (`TException : Exception`) | `Expected`, `Arg1..Arg9` |
| `ITestData` | Unified contract (extends `INamedCase`) | `TestCaseName`, `ToArgs()` |

All test data is created via `TestDataFactory`:

```csharp
using static Portamical.Core.Factories.TestDataFactory;

// General test data
yield return CreateTestData(
    definition: "Valid name and dateOfBirth is equal with the current day",
    result: "creates BirthDay instance",
    arg1: DateOnly.FromDateTime(DateTime.Now));

// Return-value test data
yield return CreateTestDataReturns(
    definition: "other is null",
    expected: -1,
    arg1: dateOfBirth,
    arg2: (BirthDay?)null);

// Exception test data
yield return CreateTestDataThrows(
    definition: "name is null",
    expected: new ArgumentNullException("name"),
    arg1: (string?)null);
```

---

## Three Consumption Modes

The **Data Strategy Pattern** (`ArgsCode` + `PropsCode`) controls how test data materializes into framework-consumable rows:

### 1. TestData Mode
The `ITestData` instance flows directly into the test method as a typed parameter.

```csharp
// Data source
public static IEnumerable<TTestData> Data => Convert(dataSource.GetArgs());

// Test signature
void Test(TestData<DateOnly> testData) { ... }
```

### 2. Instance Mode (`ArgsCode.Instance`)
An `object[]` containing the `ITestData` instance as its single element.

```csharp
// Data source
public static IEnumerable<object?[]> Data => Convert(dataSource.GetArgs()); // uses ArgsCode.Instance

// Test signature — same as TestData mode
void Test(TestData<DateOnly> testData) { ... }
```

### 3. Properties Mode (`ArgsCode.Properties`)
A flattened `object[]` of individual property values, controlled by `PropsCode`:

| `PropsCode` | Includes |
|-------------|----------|
| `All` | `TestCaseName` + all properties |
| `TrimTestCaseName` | All properties except `TestCaseName` *(default)* |
| `TrimReturnsExpected` | Also excludes `Expected` if `IReturns` |
| `TrimThrowsExpected` | Also excludes `Expected` if `IThrows` |

```csharp
// Data source
public static IEnumerable<object?[]> Data => Convert(dataSource.GetArgs(), AsProperties);

// Test signature — flat parameters
void Test(DateOnly dateOfBirth) { ... }
```

---

## Framework Adapters

Thin, optional adapters bridge Portamical to each test runner:

| Project | Framework | Key Integration |
|---------|-----------|----------------|
| `Portamical.xUnit` | xUnit v2 | `TheoryData<T>` via `ToTheoryData()` |
| `Portamical.xUnit_v3` | xUnit v3 (3.2.2) | `MemberTestDataAttribute`, `ITheoryTestDataRow`, `TheoryTestData<T>` |
| `Portamical.MSTest` | MSTest 4 (4.0.2) | `DynamicData` + `PortamicalAssert.CatchException` |
| `Portamical.NUnit` | NUnit 4 (4.4.0) | `TestCaseSource` + `Assert.Catch` |

**Same data source, four frameworks:**

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
public static IEnumerable<object?[]> Args => Convert(_dataSource.GetConstructorValidArgs(), AsInstance);
[Test, TestCaseSource(nameof(Args))]
```

---

## Unified Exception Assertions

`PortamicalAssert.ThrowsDetails` validates exception **type**, **message**, and **parameter name** — with any test runner:

```csharp
// xUnit
ThrowsDetails(attempt, expected,
    catchException: Record.Exception,
    assertIsType: Assert.IsType,
    assertEquality: Assert.Equal,
    assertFail: Assert.Fail);

// NUnit
ThrowsDetails(attempt, expected,
    catchException: att => Assert.Catch(() => att()),
    assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
    assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
    assertFail: Assert.Fail);

// MSTest
ThrowsDetails(attempt, expected,
    catchException: CatchException,
    assertIsType: (e, a) => Assert.AreEqual(e, a.GetType()),
    assertEquality: (e, a) => Assert.AreEqual(e, a),
    assertFail: Assert.Fail);
```

---

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                       _SampleCodes                           │
│        (Testables, DataSources, UnitTests per framework)     │
└──────────────────────────┬───────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────┐
│   Portamical.xUnit │ .xUnit_v3 │ .MSTest │ .NUnit            │
│               (Framework-specific adapters)                   │
└──────────────────────────┬───────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────┐
│                       Portamical                              │
│         (Converters, assertions, test bases)                  │
└──────────────────────────┬───────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────┐
│                    Portamical.Core                            │
│       (Interfaces, models, enums, factory — zero deps)       │
└──────────────────────────────────────────────────────────────┘
```

### Class Hierarchy

```
NamedCase (abstract) : INamedCase
 └── TestDataBase (abstract) : ITestData
      ├── TestData (abstract)
      │    └── TestData<T1> → ... → TestData<T1,...,T9>
      └── TestDataExpected<TResult> (abstract) : IExpected<TResult>
           ├── TestDataReturns<TStruct> : IReturns<TStruct>
           │    └── TestDataReturns<TStruct,T1> → ... → <TStruct,T1,...,T9>
           └── TestDataThrows<TException> : IThrows<TException>
                └── TestDataThrows<TException,T1> → ... → <TException,T1,...,T9>
```

---

## T4 Code Generation

The generic test data classes (`TestData<T1..T9>`, `TestDataReturns<TStruct, T1..T9>`, `TestDataThrows<TException, T1..T9>`) and the `TestDataFactory` are **generated** by T4 (Text Template Transformation Toolkit) templates at design time.

### How it works

```
Portamical.Core/
├── T4/
│   └── SharedHelpers.ttinclude        ← single source of truth (MaxArity, helpers)
├── Factories/
│   ├── TestDataFactory.tt             ← template (StringBuilder pattern)
│   └── TestDataFactory.generated.cs   ← auto-generated output
└── TestDataTypes/Models/
    ├── General/
    │   ├── TestData.tt                ← template (inline text output)
    │   └── TestData.generated.cs      ← auto-generated output
    └── Specialized/
        ├── TestDataReturns.tt         ← template
        ├── TestDataReturns.generated.cs
        ├── TestDataThrows.tt          ← template
        └── TestDataThrows.generated.cs
```

### Centralized configuration

All four templates share a single `SharedHelpers.ttinclude` file that defines:
- **`MaxArity`** — the maximum number of generic type arguments (currently `9`)
- **`Ordinals`** — ordinal names for XML doc comments
- **Helper methods** — `TypeParams()`, `CtorParams()`, `ArgsList()`, `BaseArgsList()`

To change the maximum arity, edit **one** constant in `SharedHelpers.ttinclude`, then regenerate:
```
1. Edit Portamical.Core/T4/SharedHelpers.ttinclude → change MaxArity
2. In Solution Explorer, select all 4 .tt files
3. Right-click → Run Custom Tool
4. Build
```
### Design decisions

| Template | Pattern | Include placement | Why |
|----------|---------|-------------------|-----|
| `TestData.tt` | Inline text output | End of file | Extra trailing newline is harmless |
| `TestDataReturns.tt` | Inline text output | End of file | Same |
| `TestDataThrows.tt` | Inline text output | End of file | Same |
| `TestDataFactory.tt` | StringBuilder (`<#= sb.ToString() #>`) | Joined before `<#` on line 5 | Any stray text creates a second compilation unit (CS8802) |

> **Note:** The `.generated.cs` files are checked into source control so the solution builds without running T4 transformations. They should never be edited manually.

---

## Sample Code Walkthrough

The `_SampleCodes` folder contains a complete end-to-end example using a `BirthDay` class:

### The testable class

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

### The data source (framework-agnostic)

```csharp
// _SampleCodes/DataSources/TestDataSources/BirthDayDataSource.cs
public class BirthDayDataSource
{
    // TestData<DateOnly> — general constructor scenarios
    public IEnumerable<TestData<DateOnly>> GetBirthDayConstructorValidArgs() { ... }

    // TestDataThrows<ArgumentException, string> — exception scenarios
    public IEnumerable<TestDataThrows<ArgumentException, string>> GetBirthDayConstructorInvalidArgs() { ... }

    // TestDataReturns<int, DateOnly, BirthDay> — return-value scenarios
    public IEnumerable<TestDataReturns<int, DateOnly, BirthDay>> GetCompareToArgs() { ... }
}
```

### Test classes (one data source → four frameworks)

| Framework | Instance mode | Properties mode |
|-----------|--------------|-----------------|
| xUnit v2 | `_UnitTests/xUnit/` | `_UnitTests/xUnit/` |
| xUnit v3 | `_UnitTests/xUnit_v3/` | `_UnitTests/xUnit_v3/Specific/` |
| MSTest 4 | `_UnitTests/MSTest/Native/..._Instance.cs` | `_UnitTests/MSTest/Native/..._Properties.cs` |
| NUnit 4 | `_UnitTests/NUnit/Native/..._Instance.cs` | `_UnitTests/NUnit/Native/..._Properties.cs` |

Each unit test solution is self-contained:

```bash
# Run the MSTest sample
dotnet test _SampleCodes/_UnitTests/MSTest/

# Run the NUnit sample
dotnet test _SampleCodes/_UnitTests/NUnit/

# Run the xUnit v3 sample
dotnet test _SampleCodes/_UnitTests/xUnit_v3/
```

---

## Why Portamical?

| Problem | Portamical's Solution |
|---------|----------------------|
| Duplicate test data across frameworks | Write once, consume everywhere |
| `object[]` fragility | Strongly typed generics (up to 9 args) |
| Cryptic test names in runners | Automatic "definition => result" naming |
| Duplicate test cases slip through | Built-in deduplication via identity |
| Exception assertions differ per framework | `PortamicalAssert` with delegate injection |
| Boilerplate test data setup | `TestDataFactory` with fluent creation |
| Mutable test state | `init`-only properties throughout |

### Ideal For
- Large test suites (500+ parameterized tests)
- Multi-framework environments
- Domain-heavy logic with many edge cases
- Teams needing clarity, consistency, and maintainability

---

## Solution Files

Each adapter has its own `.slnx` for independent builds:

| Solution | Contents |
|----------|----------|
| `Portamical.Core.slnx` | Core library only |
| `Portamical.slnx` | Core + shared layer |
| `Portamical.xUnit.slnx` | Core + shared + xUnit adapter |
| `Portamical.xUnit_v3.slnx` | Core + shared + xUnit v3 adapter |
| `Portamical.MSTest.slnx` | Core + shared + MSTest adapter |
| `Portamical.NUnit.slnx` | Core + shared + NUnit adapter |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Preview or later)
- [Visual Studio 2022 17.14+](https://visualstudio.microsoft.com/) with the **Text Template Transformation** component (for T4 regeneration)
- One or more test frameworks:
  - xUnit v2 (`xunit` 2.x)
  - xUnit v3 (`xunit.v3` 3.2.2+)
  - MSTest 4 (`MSTest.TestFramework` 4.0.2+)
  - NUnit 4 (`NUnit` 4.4.0+)

---

## Contributing

Contributions are welcome! Here's how to get started:

1. **Fork** the repository
2. **Create a feature branch** from `master`:
   ```bash
   git checkout -b feature/my-improvement
   ```
3. **Make your changes** — follow the existing code style and conventions:
   - SPDX license headers on all `.cs` files
   - `init`-only properties for immutability
   - XML doc comments on public APIs
4. **Regenerate T4 output** if you modified any `.tt` or `.ttinclude` files:
   - Right-click the `.tt` files → **Run Custom Tool**
   - Commit the updated `.generated.cs` files
5. **Build and test** across all frameworks:
   ```bash
   dotnet build
   dotnet test
   ```
6. **Open a pull request** against `master` with a clear description

### Branch conventions

| Branch | Purpose |
|--------|---------|
| `master` | Stable, production-ready code |
| `Without_tt` | Pre-T4 baseline (manual generic classes) |
| `T4` | T4 template development |

### Reporting issues

Please use [GitHub Issues](https://github.com/CsabaDu/Portamical/issues) to report bugs or request features. Include:
- Steps to reproduce
- Expected vs. actual behavior
- Your .NET SDK version and test framework

---

## Bottom Line

Portamical elevates test data from a **framework concern** to a **domain concern**.
It delivers identity, strong typing, deduplication, and cross-framework portability —
enabling scalable, maintainable, future-proof test suites across the .NET ecosystem.

---

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

© 2025 Csaba Dudas ([@CsabaDu](https://github.com/CsabaDu))