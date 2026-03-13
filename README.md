# Portamical

**A Universal, Identity-Driven Test Data Modeling Framework for .NET**

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-2.0.0-orange.svg)](https://github.com/CsabaDu/Portamical/releases)
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

## ⚠️ Version 2.0 Breaking Changes

**Version 2.0.0** introduces architectural improvements for **thread safety** and **API clarity**.

### **Thread Safety Enhancement**

| v1.x (DEPRECATED) | v2.0 (CURRENT) |
|-------------------|----------------|
| ❌ `TestBase.ArgsCode` (static property, not thread-safe) | ✅ `Convert()` method overloads (fully thread-safe) |

**Why the change?** In v1.x, the static `TestBase.ArgsCode` property created **race conditions** when multiple tests ran in parallel:

```csharp
// v1.x ❌ RACE CONDITION EXAMPLE
// Thread 1: Set ArgsCode to Properties
TestBase.ArgsCode = AsProperties;

// Thread 2: Simultaneously sets ArgsCode to Instance (OVERWRITES Thread 1)
TestBase.ArgsCode = AsInstance;

// Thread 1: Convert reads ArgsCode.Instance (WRONG VALUE)
var args = Convert(dataSource.GetArgs());  // Uses Instance instead of Properties
```

**Migration:**

```csharp
// v1.x ❌ (DEPRECATED - potential race condition)
TestBase.ArgsCode = AsProperties;
var args = Convert(dataSource.GetArgs());

// v2.0 ✅ (RECOMMENDED - thread-safe)
var args = Convert(dataSource.GetArgs(), AsProperties);
```

### **New ConvertAsInstance Method**

In v2.0, `ConvertAsInstance` is a convenience helper for **instance-mode** conversion that avoids the v1.x static `ArgsCode` state and keeps conversions **thread-safe**.

```csharp
// v2.0
var args = ConvertAsInstance(convert, testDataCollection, testMethodName);
```

**Equivalent to:** invoking the adapter-supplied conversion delegate with `ArgsCode.Instance`:

- `convert(testDataCollection, ArgsCode.Instance, testMethodName)` (overload with `testMethodName`), or
- `convert(testDataCollection, ArgsCode.Instance)` (overload without `testMethodName`).

**GoF note:** This is a small **Template Method–style** helper: it fixes the invariant (`ArgsCode.Instance`) and delegates the framework-specific conversion step to the adapter (a strategy delegate).

### **Enhanced Documentation**

- Comprehensive, detailed XML documentation added with samples
- All public APIs now fully documented with examples
- Namespace dependency diagram updated
- Design patterns catalog with evidence

**Full migration guide:** [MIGRATION.md](https://github.com/CsabaDu/Portamical/blob/master/MIGRATION.md)

---

## Quick Start

### **1. Installation**

**For new projects (NuGet package):**
```bash
# Install the core library (v2.0.0)
dotnet add package Portamical.Core --version 2.0.0

# Install your framework adapter
dotnet add package Portamical.xUnit --version 2.0.0     # for xUnit v2
dotnet add package Portamical.MSTest --version 2.0.0    # for MSTest 4
dotnet add package Portamical.NUnit --version 2.0.0     # for NUnit 4
dotnet add package Portamical.xUnit.v3 --version 2.0.0  # for xUnit v3
```

**Note:** Version 2.0.0 is currently in beta. Packages are available on NuGet for early adopters.

**For contributors (clone and build):**
```bash
git clone https://github.com/CsabaDu/Portamical.git
cd Portamical
dotnet build
dotnet test
```

### **2. Choose Your Framework Solution**

| Framework | Solution File | Use Case |
|-----------|---------------|----------|
| **Core Library** | `Portamical.Core.slnx` | Framework-agnostic development |
| **Shared Layer** | `Portamical.slnx` | Converters, assertions, base classes |
| **xUnit v2** | `Portamical.xUnit.slnx` | xUnit 2.x integration |
| **xUnit v3** | `Portamical.xUnit_v3.slnx` | xUnit 3.2.2+ integration (fully documented) |
| **MSTest 4** | `Portamical.MSTest.slnx` | MSTest 4.0.2+ integration |
| **NUnit 4** | `Portamical.NUnit.slnx` | NUnit 4.4.0+ integration |

### **3. Create Your First Data Source**

```csharp
using static Portamical.Core.Factories.TestDataFactory;

// Identity-driven test cases with deterministic naming
public class EmailValidationCases
{
    public IEnumerable<TestData<string>> GetValidArgs()
    {
        // Each test case defines:
        // - a human-readable identity ("definition")
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

### **4. Consume Across All Frameworks**

**The same data source works everywhere:**

```csharp
// xUnit v2 / v3
[Theory, PortamicalData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) 
{
    var actual = Validate(testData.Arg1);
    Assert.True(actual);
}

// MSTest
[TestMethod, PortamicalData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) 
{
    var actual = Validate(testData.Arg1);
    Assert.IsTrue(actual);
}

// NUnit
[Test, PortamicalData(nameof(Args))]
public void Validate_validInput_returnsTrue(TestData<string> testData) 
{
    var actual = Validate(testData.Arg1);
    Assert.That(actual, Is.True);
}
```

---

## What's New in Version 2.0

### **Enhanced Thread Safety**

- ❌ Removed static `TestBase.ArgsCode` property (potential race condition in parallel tests)
- ✅ All conversions now use method parameters
- ✅ Safe for parallel test execution across all frameworks

### **New ConvertAsInstance Method**

Convenience method for instance-mode conversion:

```csharp
// v2.0
var args = ConvertAsInstance(dataSource.GetArgs());

// Equivalent to:
var args = Convert(dataSource.GetArgs(), ArgsCode.Instance);
```

Internally delegates to `Convert(collection, ArgsCode.Instance, PropsCode.TrimTestCaseName)`.

### **Comprehensive Documentation**

- ✅ **~7,000 lines** of XML documentation added to **Portamical.xUnit_v3**
- ✅ All public APIs fully documented with:
  - Detailed `<summary>` tags
  - Rich `<remarks>` sections explaining design patterns
  - Multiple `<example>` blocks with real-world usage
  - Cross-references using `<see>` and `<seealso>` tags
- ✅ Comparison tables (xUnit v2 vs v3)
- ✅ Architecture diagrams embedded in documentation

### **Architecture Refinements**

- ✅ Updated namespace dependency diagram
- ✅ Clarified adapter complexity comparison
- ✅ Added evidence-based design patterns catalog (16 patterns)

### **Migration Support**

- ✅ Backward-compatible API where possible
- ✅ Clear deprecation warnings for `TestBase.ArgsCode`
- ✅ Detailed migration path from v1.x
- ✅ Comprehensive [MIGRATION.md](https://github.com/CsabaDu/Portamical/blob/master/MIGRATION.md) guide

**Full changelog:** See [Changelog](#changelog) section below

---

## V2.0 Migration Checklist ✅

### **Step 1: Update Package References**

```bash
dotnet add package Portamical.Core --version 2.0.0-beta
dotnet add package Portamical.xUnit --version 2.0.0-beta  # or your framework
```

### **Step 2: Replace TestBase.ArgsCode Usage**

```csharp
// v1.x ❌ (DEPRECATED)
TestBase.ArgsCode = AsProperties;
var args = Convert(dataSource.GetArgs());

// v2.0 ✅ (RECOMMENDED)
var args = Convert(dataSource.GetArgs(), AsProperties);
```

### **Step 3: Adopt ConvertAsInstance (Optional)**

```csharp
// v1.x / v2.0 (verbose)
var args = Convert(dataSource.GetArgs(), ArgsCode.Instance);

// v2.0 ✅ (concise - recommended)
var args = ConvertAsInstance(dataSource.GetArgs());
```

### **Step 4: Rebuild and Test**

```bash
dotnet clean
dotnet build
dotnet test
```

### **Step 5: Review Breaking Changes**

- ✅ No more `TestBase.ArgsCode` static property assignments
- ✅ All `Convert()` calls now thread-safe by default
- ✅ Documentation updated with v2.0 examples
- ✅ xUnit_v3 adapter fully documented (~7,000 lines)

**Full guide:** [MIGRATION.md](https://github.com/CsabaDu/Portamical/blob/master/MIGRATION.md)

---

## Architecture

### Layered Design: Onion Architecture & Hexagonal Architecture

Portamical applies **two compatible views** of the same design goal: keep the domain stable and push framework details to the edge.

#### Onion Architecture (layering view)

Onion architecture describes **where code lives** and **how dependencies flow**: outer layers depend on inner layers.

#### Architectural Diagram

```
┌────────────────────────────────────────────────┐
│            _SampleCodes                        │  ← Outer Layer: Application/Reference implementations
│  (Testables, DataSources, UnitTests)           │     (Consumers of the framework)
└───────────────────────┬────────────────────────┘
                        │ depends on
┌───────────────────────▼────────────────────────┐
│  Portamical.xUnit | xUnit_v3 | MSTest | NUnit  │  ← Adapter Layer: Framework-specific ports
│          (Thin adapter layer)                  │     (Translates domain → framework APIs)
└───────────────────────┬────────────────────────┘
                        │ depends on
┌───────────────────────▼────────────────────────┐
│              Portamical                        │  ← Application Layer: Shared utilities
│  (Converters, Assertions, TestBases)           │     (Framework-agnostic implementations)
└───────────────────────┬────────────────────────┘
                        │ depends on
┌───────────────────────▼────────────────────────┐
│          Portamical.Core                       │  ← Domain Layer: Pure abstractions (THE CORE)
│  (Interfaces, Models, Factory — ZERO DEPS)     │     (Business logic, zero external dependencies)
└────────────────────────────────────────────────┘
```

#### Hexagonal Architecture (ports & adapters view)

Hexagonal architecture describes **integration points**: the domain exposes **ports** (contracts) and outer layers provide **adapters** (implementations) for specific frameworks.

- **Ports (in the domain):** core contracts such as `INamedCase`, `ITestData`, `IExpected`, `IReturns`, `IThrows`.
- **Adapters (at the edge):** `Portamical.xUnit`, `Portamical.xUnit_v3`, `Portamical.MSTest`, `Portamical.NUnit` translate the domain model into runner-specific concepts (attributes, theory/testcase sources, display names, etc.).
- **Shared adapter surface:** `Portamical` provides common building blocks (`Converters`, `TestBases`, `Assertions`, `DataProviders`) used by all adapters.

In short: **Onion = layers**, **Hexagonal = ports/adapters**; both ensure `Portamical.Core` stays framework-agnostic.

**Key Principle:** Portamical.Core has zero external dependencies, ensuring maximum portability and future-proofing. This is the essence of **Hexagonal Architecture**: the domain is a closed hexagon with **ports** (interfaces) that **adapters** plug into from the outside.

**Screaming Architecture (Structure Reveals Intent)**

The folder/namespace structure reveals the domain concepts:

```
Portamical.Core/
├── Identity/           ← Identity concerns (INamedCase, deduplication)
├── TestDataTypes/      ← Core domain models (ITestData, TestDataBase)
├── Factories/          ← Creation logic (TestDataFactory)
├── Strategy/           ← Behavioral strategies (ArgsCode, PropsCode)
└── Safety/             ← Validation (EnumValidator, parameter checks)
```

**Not:**
```
src/Interfaces/
src/Models/
src/Utilities/
```

The names tell you **what the system does**, not **how** it's organized technically.

**Thin Adapter Layer (Minimal Translation Code)**

Framework adapters are thin wrappers (typically <200 lines per adapter):

| Adapter | Lines of Code | Complexity | Reason |
|---------|---------------|------------|--------|
| **MSTest** | ~150 | Simple | Direct `IEnumerable<object?[]>` mapping |
| **xUnit v2** | ~180 | Moderate | Adds `TestDataProvider<T>` wrapper |
| **NUnit** | ~190 | Moderate | Adds `TestCaseTestData` wrapper with NUnit metadata |
| **xUnit v3** | ~250 | Complex | Implements `ITheoryTestDataRow` + `TheoryTestData<T>` container |

---

#### Benefits of Onion/Hexagonal Architecture in Portamical

| Benefit | Example in Portamical |
|---------|------------------------|
| **Framework Independence** | Swap xUnit for NUnit by changing 1 adapter—domain untouched |
| **Testability** | Test `TestDataFactory` without needing xUnit/MSTest/NUnit |
| **Evolvability** | Add new test framework support without modifying core |
| **Clarity** | Dependencies flow inward—no circular references possible |
| **Maintainability** | Changes to xUnit v3 API don't affect MSTest/NUnit adapters |

---

#### Namespace Dependency Diagram

The following diagram shows the complete namespace structure and dependency flow across all 6 packages. Understanding this architecture is key to grasping how Portamical achieves cross-framework portability.

![Portamical_Namespaces_Hierarchy](https://raw.githubusercontent.com/CsabaDu/Portamical/refs/heads/master/_Images/Portamical_Namespaces_Hierarchy.svg)

#### Reading the Diagram

**Color Coding:**
- 🟢 ***Green (contract)*** — Interfaces defining contracts (`INamedCase`, `ITestData`, `IExpected`)
- 🔵 ***Blue (abstract)*** — Abstract base classes (`NamedCase`, `TestDataBase`, `PortamicalAssert`)
- 🔵 **Blue (concrete)** — Concrete implementations (`TestDataThrows<T>`, attributes)
- 🔵 <u>**Blue (static)**</u> — Static utility classes (`TestDataFactory`, `Converters`, `Strategy`)
- 📦 **Package** — External framework dependencies (`xunit.core`, `MSTest.TestFramework`, `NUnitLite`)

**Dependency Flow Rules:**
1. **All arrows point inward** toward `Portamical.Core` (Dependency Inversion Principle)
2. **No backward dependencies** — Framework adapters never influence the core
3. **T4 appears as a namespace** because generated code depends on `SharedHelpers.ttinclude`

#### Key Architectural Insights

**1. Framework Adapter Complexity:**

| Adapter | Namespaces | Complexity | Reason | v2.0 Status |
|---------|------------|------------|--------|-------------|
| **MSTest** | 4 | Simple | Direct converter → test base pattern | Stable |
| **xUnit v2** | 6 | Moderate | Adds `DataProviders` + `TheoryData` support | Stable |
| **NUnit** | 6 | Moderate | Adds `TestDataTypes` + `TestCaseDataCollection` wrapper | Stable |
| **xUnit v3** | 9 | Complex | Full contract/model separation + `ITheoryTestDataRow` implementation | Fully documented |

**Note on NUnit:** NUnit returns `IReadOnlyCollection<TestCaseData>` instead of raw `object?[]` arrays. This requires an adapter layer (`TestCaseTestData`) to wrap test data and provide NUnit-specific metadata (test names, categories, etc.).

**Note on xUnit v3:** xUnit v3 requires implementing `ITheoryTestDataRow` interface with explicit `TestDisplayName` property, plus separate `TheoryTestData<T>` container type for strongly-typed theory data provisioning.

**2. Core Namespace Structure:**

```
Portamical.Core:
├── Strategy → ArgsCode & PropsCode enums (controls serialization)
├── Identity → INamedCase interface (enables deduplication)
├── Safety → Validator utilities (parameter validation)
├── TestDataTypes → ITestData interface (core abstraction)
├── Factories → TestDataFactory (T4-generated)
└── T4 → SharedHelpers.ttinclude (MaxArity = 9)
```

**3. Adapter Pattern Consistency:**

All 4 framework adapters follow the same template:

```
Converters (static) → TestBases (abstract) → Assertions (abstract)
```

This standardization makes adding new framework adapters straightforward.

**4. Identity-Driven Type Hierarchy:**

The diagram reveals how identity flows through the type system:

```
Identity.INamedCase
    ↓ implemented by
Identity.Model.NamedCase (abstract)
    ↓ inherited by
TestDataTypes.Models.TestDataBase
    ↓ inherited by
TestDataTypes.Models.General.TestData<T1..T9>
```

This is why every test data object has a `TestCaseName` property and identity-based equality.

---

### **Core Test Data Model**

### Four-layered Data Model

The type system of `Portamical.Core` forms a coherent four-layer architecture that progressively specializes from foundational identity concerns to fully typed, framework-consumable data transfer objects (DTOs). This layered design embodies the Separation of Concerns principle while maintaining a discoverable, intuitive API surface.

#### Architectural Layers

Each concrete `TestData` instance can be accessed through one of four progressively specialized entry points, each serving a distinct architectural role:

```
┌─────────────────────────────────────────────────────────────┐
│ Layer 1: Identity (INamedCase)                              │  ← Semantic identification
│ Purpose: Equality, deduplication, test case naming          │     & deduplication
└──────────────────────────┬──────────────────────────────────┘
                           │ extends
┌──────────────────────────▼──────────────────────────────────┐
│ Layer 2: Core Abstraction (ITestData)                       │  ← Non-generic contract
│ Purpose: Reflection-based, dynamically typed handling       │     for framework adapters
└──────────────────────────┬──────────────────────────────────┘
                           │ extends
┌──────────────────────────▼──────────────────────────────────┐
│ Layer 3: Semantic Specialization (IExpected<T>)             │  ← Expected outcome
│ - IReturns<TStruct> → Return-value assertions               │     categorization
│ - IThrows<TException> → Exception assertions                │
└──────────────────────────┬──────────────────────────────────┘
                           │ implemented by
┌──────────────────────────▼──────────────────────────────────┐
│ Layer 4: Concrete DTOs (TestData<T1..T9>)                   │  ← Fully typed
│ - TestData<T1..T9> (general test data)                      │     implementations
│ - TestDataReturns<TStruct, T1..T9> (return-value tests)     │     with compile-time
│ - TestDataThrows<TException, T1..T9> (exception tests)      │     type safety
└─────────────────────────────────────────────────────────────┘
```

#### Layer Responsibilities

**Layer 1: Identity (`INamedCase`)**

- **Purpose:** Provides the foundational identity mechanism for all test data
- **Key Members:**
  - `TestCaseName` property: Returns `"<definition> => <result>"`
  - `Comparer` property: Enables identity-based deduplication via `HashSet<INamedCase>`
- **Design Pattern:** Value Object pattern—identity is determined by value, not reference
- **Use Case:** Framework-agnostic deduplication and test case naming

```csharp
namespace Portamical.Core.Identity
{
    public interface INamedCase : IEquatable<INamedCase>
    {
        string TestCaseName { get; init; }  // Identity value
        static abstract IEqualityComparer<INamedCase> Comparer { get; }
    }
}
```

**Layer 2: Core Abstraction (`ITestData`)**

- **Purpose:** Non-generic contract enabling reflection-based and dynamically typed operations
- **Key Members:**
  - `ToArgs(ArgsCode, PropsCode, testMethodName)`: Materializes test data into `object?[]` arrays
  - Extends `INamedCase` to inherit identity behavior
- **Design Pattern:** Template Method pattern—defines algorithm skeleton, subclasses fill in details
- **Use Case:** Framework adapters that need to work with test data without knowing concrete generic types

```csharp
namespace Portamical.Core.TestDataTypes
{
    public interface ITestData : INamedCase
    {
        object?[] ToArgs(ArgsCode argsCode, PropsCode propsCode = default, string? testMethodName = null);
        string GetDefinition();
        string GetResult();
    }
}
```

**Layer 3: Semantic Specialization (`IExpected<TResult>`, `IReturns<TStruct>`, `IThrows<TException>`)**

- **Purpose:** Marker interfaces that expose the semantic category of the expected test outcome
- **Key Members:**
  - `Expected` property: Strongly typed expected value or exception
- **Design Pattern:** Strategy pattern—different assertion strategies based on specialization type
- **Use Case:** Enables type-safe expected value handling and specialized assertion logic

```csharp
namespace Portamical.Core.TestDataTypes
{
    // Base specialization marker
    public interface IExpected<out TResult> : ITestData
    {
        TResult Expected { get; }
    }

    // Return-value test data (value type constraint)
    public interface IReturns<out TStruct> : IExpected<TStruct>
        where TStruct : struct
    {
        // Inherits Expected property from IExpected<TStruct>
    }

    // Exception test data (exception constraint)
    public interface IThrows<out TException> : IExpected<TException>
        where TException : Exception
    {
        // Inherits Expected property from IExpected<TException>
    }
}
```

**Layer 4: Concrete DTOs (`TestData<T1..T9>`, `TestDataReturns<TStruct, T1..T9>`, `TestDataThrows<TException, T1..T9>`)**

- **Purpose:** Fully typed, immutable data transfer objects with compile-time safety
- **Key Members:**
  - `Arg1..Arg9` properties: Strongly typed test arguments
  - `Expected` property (for `Returns`/`Throws` variants): Strongly typed expected outcome
- **Design Pattern:** Factory pattern—created via `TestDataFactory.CreateTestData<T>()`
- **Use Case:** Test method parameters and strongly typed test data manipulation

```csharp
// T4-generated concrete implementations
namespace Portamical.Core.TestDataTypes.Models.General
{
    public sealed class TestData<T1> : TestData  // Layer 4 DTO
    {
        public T1 Arg1 { get; init; }  // Strongly typed argument
    }
}

namespace Portamical.Core.TestDataTypes.Models.Specialized
{
    public sealed class TestDataReturns<TStruct, T1> : TestDataReturns<TStruct>  // Layer 4 DTO
        where TStruct : struct
    {
        public T1 Arg1 { get; init; }
        // Inherits: public TStruct Expected { get; init; }
    }

    public sealed class TestDataThrows<TException, T1> : TestDataThrows<TException>  // Layer 4 DTO
        where TException : Exception
    {
        public T1 Arg1 { get; init; }
        // Inherits: public TException Expected { get; init; }
    }
}
```

---

#### Visual Type Diagram

![Portamical_Core_Datamodel_ClassDiagram_Simplified](https://raw.githubusercontent.com/CsabaDu/Portamical/refs/heads/master/_Images/Portamical_Core_Datamodel_ClassDiagram_Simplified.svg)

| Element Color | Type | Purpose | Examples |
|---------------|------------|--------------------|--------------------|
| 🟢 ***Green*** | [contract] | Interface definitions | `INamedCase`, `ITestData`, `IExpected` |
| 🔵 ***Blue*** | [abstract] | Abstract base classes | `NamedCase`, `TestDataBase`, `TestData`, `TestDataReturns<TStruct>` |
| 🔵 **Blue** | [concrete] | Concrete implementations (T4-generated) | `TestData<T1>`, `TestDataReturns<TStruct, T1, T2>` |

---

#### Design Rationale


| Layer | Solves | Example |
|-------|--------|---------|
| **Identity** | Test case deduplication | `HashSet<INamedCase>.Add(testData)` removes duplicates automatically |
| **Core Abstraction** | Framework adapter integration | Converters work with `ITestData` without knowing if it's `TestData<int>` or `TestDataReturns<bool, string>` |
| **Semantic Specialization** | Assertion strategy selection | `if (testData is IReturns<int> returns) Assert.AreEqual(returns.Expected, actual);` |
| **Concrete DTOs** | Compile-time type safety | `void Test(TestData<DateOnly> testData) => var date = testData.Arg1;  // No casting` |

---

#### Class Hierarchy (Template Method + Composite Patterns)

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

**Pattern Application:**

- **Template Method:** `TestDataBase.ToArgs()` defines the algorithm skeleton; subclasses override `ToObjectArray(ArgsCode)` to customize behavior
- **Composite:** All test data types implement `ITestData`, enabling uniform treatment across collections
- **T4 Code Generation:** Eliminates 27 classes worth of boilerplate while maintaining compile-time type safety

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

### Strategy Modes (v2.0)

#### 1. **TestData Mode** (Direct Instance Flow)

```csharp
// Data source (v2.0 - thread-safe)
public static IEnumerable<TTestData> Data 
    => Convert(dataSource.GetArgs());

// Test signature
void Test(TestData<DateOnly> testData) { ... }
```

**Best for:** Test methods needing access to `TestCaseName` or full test data object.

---

#### 2. **Instance Mode** (`ArgsCode.Instance` or `ConvertAsInstance`)

```csharp
// v2.0 - Uses the new ConvertAsInstance method
public static IEnumerable<object?[]> Data 
    => Convert(dataSource.GetArgs());

// Alternative: Explicit ArgsCode
public static IEnumerable<object?[]> Data 
    => Convert(dataSource.GetArgs(), ArgsCode.Instance);

// Test signature (same as TestData mode)
void Test(TestData<DateOnly> testData) { ... }
```

**Best for:** Frameworks requiring `object?[]` collections (MSTest, NUnit).

---

#### 3. **Properties Mode** (`ArgsCode.Properties`)

```csharp
// Data source (v2.0 - thread-safe)
public static IEnumerable<object?[]> Data 
    => Convert(dataSource.GetArgs(), AsProperties);

// Test signature (flattened parameters)
void Test(DateOnly dateOfBirth) { ... }
```

**Best for:** Test methods preferring flattened parameter signatures.

### PropsCode Options

| `PropsCode` | Includes | Use Case |
|-------------|----------|----------|
| `All` | `TestCaseName` + all properties | MSTest with `DynamicDataDisplayName` |
| `TrimTestCaseName` | All properties except `TestCaseName` | **Default** — test runner provides naming |
| `TrimReturnsExpected` | Also excludes `Expected` if `IReturns` | NUnit return-value tests |
| `TrimThrowsExpected` | Also excludes `Expected` if `IThrows` | Exception tests where assertion extracts exception |

---

## Design Patterns Catalog

Portamical implements **16 GoF and architectural patterns** to achieve portability and maintainability.

| Pattern | Implementation | Purpose | Evidence |
|---------|----------------|---------|----------|
| **Factory** | `TestDataFactory` (T4-generated) | Centralized test data creation | Static `CreateTestData<T>()`, `CreateTestDataReturns<T>()`, `CreateTestDataThrows<T>()` methods |
| **Builder** | Named parameters in factory methods | Fluent, self-documenting API | All factory methods use named params: `definition:`, `result:`, `expected:`, `arg1:` |
| **Adapter** | Framework-specific adapters (xUnit, MSTest, NUnit, xUnit v3) | Translate `ITestData` to framework types | Each adapter converts via `Convert()` methods to framework-native types |
| **Strategy** | `ArgsCode` + `PropsCode` enums | Configurable data serialization | `ToArgs(ArgsCode, PropsCode)` switches behavior: Instance vs Properties |
| **Template Method** | `TestDataBase.ToArgs()` | Skeleton algorithm with hooks | Public `ToArgs()` calls protected `ToObjectArray(ArgsCode)` (hook method) |
| **Composite** | Test data hierarchy via `ITestData` | Uniform treatment of all test data | All types implement `ITestData`, enabling polymorphic processing |
| **Command** | Delegate injection in `PortamicalAssert` | Framework-agnostic assertions | `ThrowsDetails()` accepts `catchException`, `assertIsType`, `assertEquality`, `assertFail` delegates |
| **Iterator** | `IEnumerable<TTestData>` | Lazy test data evaluation | All data sources return `IEnumerable<TTestData>`, enabling `yield return` |
| **Null Object** | Optional `testMethodName` parameter | Eliminate null checks | Defaults to `null`, allows safe operation without explicit null handling |
| **Value Object** | `NamedCase.Comparer` | Deterministic identity via `TestCaseName` | `IEquatable<INamedCase>`, identity-based `Equals()`, `HashSet<INamedCase>` deduplication |
| **Snapshot** | Array materialization before operations | Prevent multiple enumeration and ensure consistency | `NamedCase.Contains()`: `var snapshot = namedCases as INamedCase[] ?? [.. namedCases];` — immutable point-in-time view |
| **Dependency Inversion** | Layered architecture | Core has zero dependencies | `Portamical.Core` (abstractions) ← `Portamical` (utilities) ← Framework adapters |
| **Repository** | Data sources | Centralized test data storage | `BirthDayDataSource` with `Get*Args()` methods returns test data collections |
| **Code Generation** | T4 templates (`MaxArity = 9`) | Single source of truth | `TestDataFactory.tt`, `TestData.tt`, `TestDataReturns.tt`, `TestDataThrows.tt` |
| **Local Function Pattern** | `static` local functions in methods | Encapsulated helpers, zero closure cost | Used throughout: `static string createIdentity()`, `static bool isNotFatal()` |
| **Bridge** | Core ↔ Adapters separation | Decouple abstraction from implementation | `ITestData` (abstraction) implemented by concrete types, adapted by framework layers |


### Pattern Categories

**Creational Patterns (2)**

    ✅ Factory — Centralized test data object creation
    ✅ Builder — Fluent API via named parameters

**Structural Patterns (3)**

    ✅ Adapter — Framework integration layer
    ✅ Composite — Uniform test data treatment
    ✅ Bridge — Abstraction/implementation separation

**Behavioral Patterns (5)**

    ✅ Strategy — Configurable serialization
    ✅ Template Method — Algorithm skeleton with hooks
    ✅ Command — Encapsulated assertion operations
    ✅ Iterator — Lazy test data evaluation
    ✅ Null Object — Default behavior without null checks

**Architectural Patterns (6)**

    ✅ Value Object — Identity via immutable value
    ✅ Snapshot — Immutable point-in-time collection view
    ✅ Dependency Inversion — Depend on abstractions
    ✅ Repository — Centralized data access
    ✅ Code Generation — Metaprogramming via T4
    ✅ Local Function — Modern C# optimization pattern

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

**Key:** v2.0 introduces thread-safe `Convert()` overloads. Each framework uses its native return type:
- **MSTest:** `IEnumerable<object?[]>`
- **NUnit:** `IEnumerable<TestCaseTestData>`
- **xUnit v2:** `TestDataProvider<T>`
- **xUnit v3:** `TheoryTestData<T>`

| Project | Framework | Version | Key Integration | Package Reference |
|---------|-----------|---------|-----------------|-------------------|
| `Portamical.xUnit` | xUnit v2 | **2.0.0** | `PortamicalDataAttribute`, `TestDataProvider<T>`, `TheoryData<T>` support | `xunit.core` 2.9.3 |
| `Portamical.xUnit_v3` | xUnit v3 (3.2.2+) | **2.0.0** | `PortamicalDataAttribute`, `TheoryTestData<T>`, `ITheoryTestDataRow` | `xunit.v3` 3.2.2 |
| `Portamical.MSTest` | MSTest 4 (4.0.2+) | **2.0.0** | `PortamicalDataAttribute` | `MSTest.TestFramework` 4.0.2 |
| `Portamical.NUnit` | NUnit 4 (4.4.0+) | **2.0.0** | `PortamicalDataAttribute`, `TestCaseTestData` | `NUnit` 4.4.0 |

---

### Same Data Source, Four Frameworks (v2.0 Syntax)

```csharp
### Same Data Source, Four Frameworks (v2.0 Syntax)

```csharp
// Shared — works everywhere
private static readonly BirthDayDataSource _dataSource = new();

// MSTest (v2.0 - thread-safe)
private static IEnumerable<object?[]> Args 
    => Convert(_dataSource.GetConstructorValidArgs());
[TestMethod, PortamicalData(nameof(Args))]

// NUnit (v2.0)
public static IEnumerable<TestCaseTestData> Args 
    => Convert(_dataSource.GetConstructorValidArgs());
[Test, PortamicalData(nameof(Args))]

// xUnit v2 (v2.0)
public static TestDataProvider<TestData<DateOnly>> Args 
    => Convert(_dataSource.GetConstructorValidArgs());
[Theory, PortamicalData(nameof(Args))]

// xUnit v3 (v2.0 - fully documented!)
public static TheoryTestData<TestData<DateOnly>> Args 
    => Convert(_dataSource.GetConstructorValidArgs());
[Theory, PortamicalData(nameof(Args))]
```

**Key differences across frameworks:**

| Framework | Return Type | Method Visibility | Attribute |
|-----------|-------------|-------------------|-----------|
| MSTest | `IEnumerable<object?[]>` | `private static` | `[TestMethod, PortamicalData]` |
| NUnit | `IEnumerable<TestCaseTestData>` | `public static` | `[Test, PortamicalData]` |
| xUnit v2 | `TestDataProvider<T>` | `public static` | `[Theory, PortamicalData]` |
| xUnit v3 | `TheoryTestData<T>` | `public static` | `[Theory, PortamicalData]` |

All Portamical-specific `TestBase` classes use the `[PortamicalData]` attribute, while framework-agnostic patterns use standard attributes (`[MemberData]`, `[DynamicData]`, `[TestCaseSource]`).

---

## Unified Exception Assertions

`PortamicalAssert.ThrowsDetails` validates exception **type**, **message**, and **parameter name** using **delegate injection** (Command Pattern):

```csharp
// xUnit v2 / v3
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
| MSTest 4 | `_UnitTests/MSTest/Shared` | `_UnitTests/MSTest/Native/Native` |
| NUnit 4 | `_UnitTests/NUnit/Shared` | `_UnitTests/NUnit/Native/Native` |

```bash
# Run the MSTest sample
dotnet test _SampleCodes/_UnitTests/MSTest/

# Run the NUnit sample
dotnet test _SampleCodes/_UnitTests/NUnit/

# Run the xUnit v2 sample
dotnet test _SampleCodes/_UnitTests/xUnit/

# Run the xUnit v3 sample
dotnet test _SampleCodes/_UnitTests/xUnit_v3/
```

---

## Prerequisites

### **Runtime Requirements**

- **.NET 10 SDK** (Preview or later) — [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
  - ⚠️ **Production note:** .NET 10 is in preview; production apps should wait for stable release
  - ✅ **Future support:** .NET 8+ planned for v2.1

### **Development Tools**

- **For T4 regeneration only:**
  - [Visual Studio 2022 17.14+](https://visualstudio.microsoft.com/) with **Text Template Transformation** component
  - Alternative: [T4 Command-Line Tool](https://github.com/mono/t4)

- **Framework requirements (pick one or more):**
  - **xUnit v2:** `xunit` 2.9.3+
  - **xUnit v3:** `xunit.v3` 3.2.2+
  - **MSTest 4:** `MSTest.TestFramework` 4.0.2+
  - **NUnit 4:** `NUnit` 4.4.0+

### **Package Versions**

| Package | Version | NoteStatuss |
|---------|---------|--------|-------|
| `Portamical.Core` | 2.0.0 | Core abstractions |
| `Portamical` | 2.0.0 | Shared utilities |
| `Portamical.MSTest` | 2.0.0 | MSTest adapter |
| `Portamical.NUnit` | 2.0.0 | NUnit adapter |
| `Portamical.xUnit` | 2.0.0 | xUnit v2 adapter |
| `Portamical.xUnit_v3` | 2.0.0 |  xUnit v3 adapter |

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
| `master` | Stable, production-ready code (current: v2.0.0) |
| `Without_tt` | Pre-T4 baseline (manual generic classes) |
| `T4` | T4 template development |
| `v2_LocalDependencies` | v2.0 local dependencies development |

### Reporting Issues

Use [GitHub Issues](https://github.com/CsabaDu/Portamical/issues) with:
- Steps to reproduce
- Expected vs. actual behavior
- .NET SDK version and test framework
- Portamical version (e.g., 2.0.0)

---

## Repository Statistics

- **Created:** January 16, 2026 (56 days ago)
- **Current Version:** 2.0.0 (released March 12, 2026)
- **Language:** C# (99.6%)
- **Size:** ~8,500 KB (after PR #7 merge: +15,716 additions, -434 deletions)
- **Stars:** ⭐ 1
- **Forks:** 0
- **Commits:** 100+
- **Contributors:** 1 (CsabaDu)
- **Latest Release:** v2.0.0 (March 12, 2026)
- **Open Issues:** 0
- **License:** MIT
- **Visibility:** Public

[View Recent Commits](https://github.com/CsabaDu/Portamical/commits/master) | [View All Activity](https://github.com/CsabaDu/Portamical/events) | [View PR #7](https://github.com/CsabaDu/Portamical/pull/7)

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
- ✅ **Thread Safety:** v2.0 eliminates race conditions in parallel test execution

### Ideal For

- ✅ Large test suites (500+ parameterized tests)
- ✅ Multi-framework environments
- ✅ Domain-heavy logic with many edge cases
- ✅ Projects needing human-readable test reports
- ✅ Teams prioritizing consistency and maintainability
- ✅ Parallel test execution scenarios (v2.0+)

### Not Ideal For

- ⚠️ Simple test suites (<100 tests)
- ⚠️ Projects restricted to .NET 8 or earlier (support planned for v2.1)
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
- **Thread safety** (v2.0): Eliminated static `ArgsCode` property, introduced thread-safe `Convert()` overloads.

### Migration Guidance (High Level)

If you are using `CsabaDu.DynamicTestData.Core`:
- Prefer migrating to `Portamical.Core` for continued support and improvements.
- Expect mostly mechanical renames, restructured namespaces, plus updates where the API surface changed due to the move from records to immutable classes.
- v2.0 requires updating `TestBase.ArgsCode` usage to method parameters.

---

## Changelog

### **Version 2.0.0 (2026-03-12)** 🎉

**Breaking Changes:**
- ❌ **Removed:** `TestBase.ArgsCode` static property (thread safety issue)
- ✅ **Added:** `Convert()` overloads with `ArgsCode` parameter (thread-safe)
- ✅ **Added:** `ConvertAsInstance()` convenience method

**New Features:**
- ✨ **Comprehensive XML documentation** (~7,000 lines in Portamical.xUnit_v3)
- ✨ **Enhanced architecture diagrams** (namespace dependencies)
- ✨ **Design patterns catalog** (16 patterns with evidence)
- ✨ **Improved thread safety** across all converters and test bases

**Documentation:**
- 📖 All public APIs now fully documented with examples
- 📖 Enhanced remarks sections with design pattern explanations
- 📖 Multiple real-world usage examples per component
- 📖 Updated migration guide from v1.x
- 📖 Comparison tables (xUnit v2 vs v3)

**Bug Fixes:**
- 🐛 Fixed potential race condition in `TestBase.ArgsCode` (static property)
- 🐛 Corrected documentation typos in `ITestData.GetResult()`
- 🐛 Enhanced null-safety with explicit `#nullable enable` directives

**Package Updates:**
- 📦 `Portamical.Core` 2.0.0
- 📦 `Portamical` 2.0.0
- 📦 `Portamical.MSTest` 2.0.0
- 📦 `Portamical.NUnit` 2.0.0
- 📦 `Portamical.xUnit` 2.0.0
- 📦 `Portamical.xUnit_v3` 2.0.0

**Known Issues:**
- ⚠️ .NET 10 is in preview; production apps should wait for stable release
- ⚠️ Migration from v1.x requires updating `TestBase.ArgsCode` usage

**Pull Requests:**
- [#6: V2 local dependencies](https://github.com/CsabaDu/Portamical/pull/6) (merged March 12, 2026)
- [#7: V2](https://github.com/CsabaDu/Portamical/pull/7) (merged March 12, 2026)

**Commit Highlights:**
- `bf90a27`: TestBase.ArgsCode cancelled for thread safety; ConvertAsInstance introduced; Documentation enhanced
- `5716927`: Static GetValidResultPrefix; enhanced documentation
- `7bb10f5`: V2 local dependencies merged (project references, ConvertAsInstance)
- `fe883dd`: 2.0.0 release

---

### **Version 1.0.3 (2026-03-08)**

- T4 Generated Code: Explicit `#nullable enable` directive added to all generated files

### **Version 1.0.2 (2026-03-07)**

- Migration guide (MIGRATION.md) added to repository

### **Version 1.0.1 (2026-03-06)**

- `EnumValidator.Defined`: Parameter type corrected (`string paramName` → not nullable)
- README.md replaced with enhanced version

### **Version 1.0.0 (2026-03-04)**

- **Initial release** of Portamical
- Framework-agnostic core (`Portamical.Core`)
- Four framework adapters (xUnit v2, xUnit v3, MSTest, NUnit)
- Identity-driven test data model
- T4 code generation (MaxArity = 9)
- Cross-framework portability

---

## Links

- [GitHub Repository](https://github.com/CsabaDu/Portamical)
- [Discussions](https://github.com/CsabaDu/Portamical/discussions)
- [Issues](https://github.com/CsabaDu/Portamical/issues)
- [Migration Guide (v1.x → v2.0)](https://github.com/CsabaDu/Portamical/blob/master/MIGRATION.md)
- [Pull Request #7 (V2)](https://github.com/CsabaDu/Portamical/pull/7)
- [Release v2.0.0](https://github.com/CsabaDu/Portamical/releases/tag/v2.0.0)

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*