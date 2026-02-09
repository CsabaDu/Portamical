# Portamical

**A universal, identity-driven test data modeling framework for .NET**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/language-C%23-239120.svg)](https://docs.microsoft.com/dotnet/csharp/)

Define test data **once**. Run it on **xUnit v2**, **xUnit v3**, **MSTest 4**, and **NUnit 4** — without rewriting tests or losing strong typing.

Portamical is the **test data abstraction layer** missing from the .NET testing ecosystem.

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
┌──────────────────────────▼───────���───────────────────────────┐
│                       Portamical                              │
│         (Converters, assertions, test bases)                  │
└──────────────────────────┬───────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼───────────────────────────────────┐
│                    Portamical.Core                             │
│       (Interfaces, models, enums, factory — zero deps)        │
└──────────────────────────────────────────────────────────────┘
```

### Class Hierarchy

```
NamedCase (abstract) : INamedCase
 └── TestDataBase (abstract) : ITestData
      ├── TestData (abstract)
      │    └── TestData<T1> → ... → TestData<T1,...,T9>
      └── TestDataExpected<TResult> (abstract) : IExpected<TResult>
           ���── TestDataReturns<TStruct> : IReturns<TStruct>
           │    └── TestDataReturns<TStruct,T1> → ... → <TStruct,T1,...,T9>
           └── TestDataThrows<TException> : IThrows<TException>
                └── TestDataThrows<TException,T1> → ... → <TException,T1,...,T9>
```

---

## Why Portamical?

| Problem | Portamical's Solution |
|---------|----------------------|
| Duplicate test data across frameworks | Write once, consume everywhere |
| `object[]` fragility | Strongly typed generics (up to 9 args) |
| Cryptic test names in runners | Automatic `"definition => result"` naming |
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

## Bottom Line

Portamical elevates test data from a **framework concern** to a **domain concern**.
It delivers identity, strong typing, deduplication, and cross-framework portability —
enabling scalable, maintainable, future-proof test suites across the .NET ecosystem.

---

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

© 2025 Csaba Dudas ([@CsabaDu](https://github.com/CsabaDu))