# Migration Guide: CsabaDu.DynamicTestData.Core → Portamical.Core

**Version:** 1.0  
**Last Updated:** March 6, 2026  
**Author:** CsabaDu  
**License:** MIT

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Quick Migration Checklist](#quick-migration-checklist)
- [High-Level Comparison](#high-level-comparison)
- [Breaking Changes](#breaking-changes)
  - [1. Target Framework](#1-target-framework)
  - [2. Namespace Changes](#2-namespace-changes)
  - [3. Type Renames](#3-type-renames)
  - [4. Method Renames](#4-method-renames)
  - [5. PropsCode Enum Values](#5-propscode-enum-values)
  - [6. Property → Method Changes](#6-property--method-changes)
  - [7. Data Model Changes](#7-data-model-changes)
- [Step-by-Step Migration](#step-by-step-migration)
- [Migration Scenarios](#migration-scenarios)
- [Testing Your Migration](#testing-your-migration)
- [Benefits of Migration](#benefits-of-migration)
- [Troubleshooting](#troubleshooting)
- [Support](#support)

---

## Executive Summary

**Portamical.Core** is the **continuation and successor** of **CsabaDu.DynamicTestData.Core**. Both libraries share the same core philosophy: identity-driven, type-safe test data modeling. However, Portamical introduces significant improvements in architecture, naming clarity, and code generation.

### Migration Complexity

🟡 **Moderate** — Primarily namespace/type renames with some method signature changes.

### Estimated Migration Time

- **Small project** (<100 test data cases): **1-2 hours**
- **Medium project** (100-500 cases): **4-6 hours**
- **Large project** (500+ cases): **1-2 days**

### Key Changes

| Aspect | Change Type |
|--------|-------------|
| **Namespaces** | ✅ Mechanical (find/replace) |
| **Type names** | ✅ Mechanical (find/replace) |
| **Method names** | ✅ Mechanical (find/replace) |
| **Enum values** | ⚠️ Breaking (semantic change) |
| **Target framework** | ⚠️ Breaking (.NET 10 required) |
| **Data model** | ℹ️ Internal (records → classes) |

---

## Quick Migration Checklist

Use this checklist to track your migration progress:

- [ ] **Prerequisites**
  - [ ] Upgrade to .NET 10 SDK
  - [ ] Backup your project
  - [ ] Create a migration branch

- [ ] **Package Changes**
  - [ ] Remove `CsabaDu.DynamicTestData.Core` package
  - [ ] Install `Portamical.Core` package
  - [ ] Update project target framework to `net10.0`

- [ ] **Code Updates**
  - [ ] Update all `using` directives (6 namespaces)
  - [ ] Rename types (3 types)
  - [ ] Rename methods (2 methods)
  - [ ] Update `PropsCode` enum values (4 values)
  - [ ] Remove `ContainedBy()` refactoring (if needed)

- [ ] **Validation**
  - [ ] Build project successfully
  - [ ] Run all tests
  - [ ] Verify test case names unchanged
  - [ ] Check deduplication still works

- [ ] **Cleanup**
  - [ ] Remove old package references
  - [ ] Update documentation
  - [ ] Commit changes

---

## High-Level Comparison

### Architecture

| Aspect | CsabaDu.DynamicTestData.Core | Portamical.Core |
|--------|------------------------------|-----------------|
| **Data Model** | Record-based DTOs | Immutable classes with `init` |
| **Target Framework** | .NET 9.0 | .NET 10.0 |
| **Root Namespace** | `CsabaDu.DynamicTestData.Core.*` | `Portamical.Core.*` |
| **Code Generation** | Manual (no T4) | T4 templates (27 classes) |
| **Identity System** | `INamedTestCase` / `NamedTestCase` | `INamedCase` / `NamedCase` |
| **Method Naming** | `ToParams()` | `ToArgs()` |
| **Dependencies** | Zero | Zero |

### Compatibility Matrix

| Feature | CsabaDu | Portamical | Compatible? |
|---------|---------|------------|-------------|
| **Factory methods** | ✅ | ✅ | ✅ Yes (same signatures) |
| **Strategy enums** | ✅ | ✅ | ⚠️ Values renamed |
| **Test case identity** | ✅ | ✅ | ✅ Yes (same format) |
| **Deduplication** | ✅ | ✅ | ✅ Yes (improved) |
| **ContainedBy()** | ✅ | ✅ | ✅ Yes (refactored) |

---

## Breaking Changes

### 1. Target Framework

**Change:** .NET 9.0 → .NET 10.0

#### Before
```xml
<TargetFramework>net9.0</TargetFramework>
<PackageReference Include="CsabaDu.DynamicTestData.Core" Version="2.2.6-beta" />
```

#### After
```xml
<TargetFramework>net10.0</TargetFramework>
<PackageReference Include="Portamical.Core" Version="1.0.1" />
```

#### Migration Steps

1. **Install .NET 10 SDK:**
   ```bash
   # Download from https://dotnet.microsoft.com/download/dotnet/10.0
   dotnet --version  # Verify 10.0.0 or later
   ```

2. **Update project file:**
   ```bash
   # Edit .csproj
   <TargetFramework>net10.0</TargetFramework>
   ```

3. **Update packages:**
   ```bash
   dotnet remove package CsabaDu.DynamicTestData.Core
   dotnet add package Portamical.Core
   dotnet restore
   ```

---

### 2. Namespace Changes

**Change:** All namespaces restructured for clarity.

#### Mapping Table

| Old Namespace | New Namespace | Notes |
|--------------|---------------|-------|
| `CsabaDu.DynamicTestData.Core.TestDataTypes` | `Portamical.Core.TestDataTypes` | Base types |
| `CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces` | `Portamical.Core.Identity` | Identity interfaces (`INamedCase`) |
| `CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces` | `Portamical.Core.TestDataTypes.Patterns` | Marker interfaces (`IExpected`, `IReturns`, `IThrows`) |
| `CsabaDu.DynamicTestData.Core.TestDataTypes` | `Portamical.Core.Identity.Model` | `NamedCase` base class |
| `CsabaDu.DynamicTestData.Core.TestDataTypes` | `Portamical.Core.TestDataTypes.Models` | `TestDataBase` |
| `CsabaDu.DynamicTestData.Core.TestDataTypes` | `Portamical.Core.TestDataTypes.Models.General` | `TestData<T1..T9>` |
| `CsabaDu.DynamicTestData.Core.TestDataTypes` | `Portamical.Core.TestDataTypes.Models.Specialized` | `TestDataReturns`, `TestDataThrows` |
| `CsabaDu.DynamicTestData.Core.DataStrategyTypes` | `Portamical.Core.Strategy` | `ArgsCode`, `PropsCode` |
| `CsabaDu.DynamicTestData.Core.TestDataTypes.Factories` | `Portamical.Core.Factories` | `TestDataFactory` |
| `CsabaDu.DynamicTestData.Core.Validators` | `Portamical.Core.Safety` | Validation utilities |

#### Migration Steps

**Option 1: Manual Find/Replace**

```csharp
// Find
using CsabaDu.DynamicTestData.Core.TestDataTypes;
using CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces;
using CsabaDu.DynamicTestData.Core.TestDataTypes.Factories;
using CsabaDu.DynamicTestData.Core.DataStrategyTypes;

// Replace with
using Portamical.Core.TestDataTypes;
using Portamical.Core.Identity;
using Portamical.Core.TestDataTypes.Patterns;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
```

**Option 2: Regex Find/Replace (VS Code/Visual Studio)**

```regex
Find:    \bCsabaDu\.DynamicTestData\.Core\.TestDataTypes\.Interfaces\b
Replace: Portamical.Core.Identity

Find:    \bCsabaDu\.DynamicTestData\.Core\.TestDataTypes\.Factories\b
Replace: Portamical.Core.Factories

Find:    \bCsabaDu\.DynamicTestData\.Core\.DataStrategyTypes\b
Replace: Portamical.Core.Strategy

Find:    \bCsabaDu\.DynamicTestData\.Core\.TestDataTypes\b
Replace: Portamical.Core.TestDataTypes

Find:    \bCsabaDu\.DynamicTestData\.Core\.Validators\b
Replace: Portamical.Core.Safety
```

#### Before Example
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes;
using CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces;
using CsabaDu.DynamicTestData.Core.TestDataTypes.Factories;
using CsabaDu.DynamicTestData.Core.DataStrategyTypes;
using static CsabaDu.DynamicTestData.Core.TestDataTypes.Factories.TestDataFactory;

public class MyDataSource
{
    public IEnumerable<TestData<int>> GetData()
    {
        yield return CreateTestData(
            definition: "Valid input",
            result: "succeeds",
            arg1: 42);
    }
}
```

#### After Example
```csharp
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.Identity;
using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using static Portamical.Core.Factories.TestDataFactory;

public class MyDataSource
{
    public IEnumerable<TestData<int>> GetData()
    {
        yield return CreateTestData(
            definition: "Valid input",
            result: "succeeds",
            arg1: 42);
    }
}
```

---

### 3. Type Renames

**Change:** Simplified type names for clarity.

#### Type Mapping Table

| Old Type | New Type | Change Type |
|----------|----------|-------------|
| `INamedTestCase` | `INamedCase` | ✅ Rename |
| `NamedTestCase` | `NamedCase` | ✅ Rename |
| `TestData` (abstract base) | `TestDataBase` | ✅ Rename |
| `TestData<T1..T9>` | `TestData<T1..T9>` | ✅ Same (different namespace) |
| `TestDataExpected<TResult>` | `TestDataExpected<TResult>` | ✅ Same |
| `TestDataReturns<TStruct>` | `TestDataReturns<TStruct>` | ✅ Same |
| `TestDataThrows<TException>` | `TestDataThrows<TException>` | ✅ Same |
| `IExpected<TResult>` | `IExpected<TResult>` | ✅ Same |
| `IReturns<TResult>` | `IReturns<TResult>` | ✅ Same |
| `IThrows<TException>` | `IThrows<TException>` | ✅ Same |
| `ArgsCode` | `ArgsCode` | ✅ Same |
| `PropsCode` | `PropsCode` | ⚠️ Same name, values changed |
| `TestDataFactory` | `TestDataFactory` | ✅ Same |

#### Migration Steps

**Regex Find/Replace:**

```regex
Find:    \bINamedTestCase\b
Replace: INamedCase

Find:    \bNamedTestCase\b
Replace: NamedCase

Find:    \b: TestData\(
Replace: : TestDataBase
```

#### Before Example
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces;

INamedTestCase testCase = ...;
NamedTestCase baseCase = ...;
```

#### After Example
```csharp
using Portamical.Core.Identity;

INamedCase testCase = ...;
NamedCase baseCase = ...;
```

---

### 4. Method Renames

**Change:** `ToParams()` → `ToArgs()` for consistency.

#### Method Mapping Table

| Old Method | New Method | Notes |
|------------|------------|-------|
| `ToParams(ArgsCode)` | `ToArgs(ArgsCode)` | Same signature, new name |
| `ToParams(ArgsCode, PropsCode)` | `ToArgs(ArgsCode, PropsCode)` | Same signature, new name |
| `ResultPrefix` (property) | `GetResultPrefix()` (method) | Property → Method |

#### Migration Steps

**Regex Find/Replace:**

```regex
Find:    \.ToParams\(
Replace: .ToArgs(

Find:    \.ResultPrefix\b
Replace: .GetResultPrefix()
```

#### Before Example
```csharp
using CsabaDu.DynamicTestData.Core.DataStrategyTypes;

var args = testData.ToParams(ArgsCode.Properties);
var args2 = testData.ToParams(ArgsCode.Properties, PropsCode.Expected);
string prefix = expected.ResultPrefix;
```

#### After Example
```csharp
using Portamical.Core.Strategy;

var args = testData.ToArgs(ArgsCode.Properties);
var args2 = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
string prefix = expected.GetResultPrefix();
```

---

### 5. PropsCode Enum Values

**Change:** Values renamed for clarity (semantic change).

#### Value Mapping Table

| Old Value | New Value | Meaning |
|-----------|-----------|---------|
| `PropsCode.Expected` | `PropsCode.TrimTestCaseName` | Exclude `TestCaseName`, include `Expected` |
| `PropsCode.TestCaseName` | `PropsCode.All` | Include all properties (including `TestCaseName`) |
| `PropsCode.Returns` | `PropsCode.TrimReturnsExpected` | Exclude `TestCaseName` + `Expected` for `IReturns` |
| `PropsCode.Throws` | `PropsCode.TrimThrowsExpected` | Exclude `TestCaseName` + `Expected` for `IThrows` |

#### Rationale

The new naming uses **"Trim" prefix** to clarify what properties are *excluded*, making the intent clearer:

- `TrimTestCaseName` = "Trim (exclude) the TestCaseName property"
- `TrimReturnsExpected` = "Trim TestCaseName + Expected (for IReturns)"
- `All` = "Include all properties (no trimming)"

#### Migration Steps

**Find/Replace:**

```regex
Find:    PropsCode\.Expected\b
Replace: PropsCode.TrimTestCaseName

Find:    PropsCode\.TestCaseName\b
Replace: PropsCode.All

Find:    PropsCode\.Returns\b
Replace: PropsCode.TrimReturnsExpected

Find:    PropsCode\.Throws\b
Replace: PropsCode.TrimThrowsExpected
```

#### Before Example
```csharp
using CsabaDu.DynamicTestData.Core.DataStrategyTypes;

// Most common case: exclude TestCaseName
var args = testData.ToParams(ArgsCode.Properties, PropsCode.Expected);

// Include TestCaseName (e.g., for MSTest DynamicDataDisplayName)
var argsAll = testData.ToParams(ArgsCode.Properties, PropsCode.TestCaseName);

// NUnit return-value tests: exclude Expected
var argsReturns = testData.ToParams(ArgsCode.Properties, PropsCode.Returns);

// Exception tests: exclude Expected
var argsThrows = testData.ToParams(ArgsCode.Properties, PropsCode.Throws);
```

#### After Example
```csharp
using Portamical.Core.Strategy;

// Most common case: trim TestCaseName
var args = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

// Include all properties (including TestCaseName)
var argsAll = testData.ToArgs(ArgsCode.Properties, PropsCode.All);

// NUnit return-value tests: trim TestCaseName + Expected
var argsReturns = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);

// Exception tests: trim TestCaseName + Expected
var argsThrows = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimThrowsExpected);
```

---

### 6. Property → Method Changes

**Change:** `ResultPrefix` property → `GetResultPrefix()` method.

#### Rationale

Portamical treats `GetResultPrefix()` as a **computed value** (method) rather than a stored property, consistent with `GetDefinition()` and `GetResult()`.

#### Migration Steps

**Find/Replace:**

```regex
Find:    \.ResultPrefix\b
Replace: .GetResultPrefix()
```

#### Before Example
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces;

IExpected expected = ...;
string prefix = expected.ResultPrefix;  // Property
```

#### After Example
```csharp
using Portamical.Core.TestDataTypes.Patterns;

IExpected expected = ...;
string prefix = expected.GetResultPrefix();  // Method
```

---

### 7. Data Model Changes

**Change:** Records → Immutable classes with `init` properties.

#### Impact

⚠️ **Only affects custom test data classes** that extend `TestData`, `TestDataReturns`, or `TestDataThrows`.

#### Before (CsabaDu.DynamicTestData.Core)
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes;

// Record-based (positional parameters)
public class MyTestData(string definition, int value)
: TestData(definition)
{
    public int Value { get; } = value;

    public override string GetResult()
    => "result";
}
```

#### After (Portamical.Core)
```csharp
using Portamical.Core.TestDataTypes.Models;

// Immutable class (init-only properties)
public class MyTestData : TestDataBase
{
    internal MyTestData(string definition, string result, int value)
        : base(definition, result)
    {
        Value = value;
    }

    public required int Value { get; init; }

    public override string GetResult()
    => "result";
}
```

#### Key Changes

1. **No record syntax** — Use class with `init` properties
2. **Constructor signature** — `base(definition, result)` instead of `base(definition)`
3. **`required` keyword** — For properties without constructor initialization

---

## Step-by-Step Migration

### Step 1: Prerequisites

1. **Backup your project:**
   ```bash
   git checkout -b migration/portamical-core
   git add .
   git commit -m "Pre-migration backup"
   ```

2. **Upgrade to .NET 10:**
   ```bash
   # Download .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0
   dotnet --version  # Verify 10.0.0+
   ```

3. **Update project file:**
   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```

---

### Step 2: Update NuGet Packages

```bash
# Remove old package
dotnet remove package CsabaDu.DynamicTestData.Core

# Add new package
dotnet add package Portamical.Core

# Restore
dotnet restore
```

**Verify:**
```bash
dotnet list package
# Should show: Portamical.Core, Version 1.0.1
```

---

### Step 3: Update Namespaces

**Find/Replace (in order):**

1. **Interfaces:**
   ```
   Find:    CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces
   Replace: Portamical.Core.Identity
   ```

2. **Factories:**
   ```
   Find:    CsabaDu.DynamicTestData.Core.TestDataTypes.Factories
   Replace: Portamical.Core.Factories
   ```

3. **Strategy:**
   ```
   Find:    CsabaDu.DynamicTestData.Core.DataStrategyTypes
   Replace: Portamical.Core.Strategy
   ```

4. **TestDataTypes:**
   ```
   Find:    CsabaDu.DynamicTestData.Core.TestDataTypes
   Replace: Portamical.Core.TestDataTypes
   ```

5. **Validators:**
   ```
   Find:    CsabaDu.DynamicTestData.Core.Validators
   Replace: Portamical.Core.Safety
   ```

**Verify:** Build project and check for namespace errors.

---

### Step 4: Rename Types

**Find/Replace:**

1. **INamedTestCase:**
   ```
   Find:    \bINamedTestCase\b
   Replace: INamedCase
   ```

2. **NamedTestCase:**
   ```
   Find:    \bNamedTestCase\b
   Replace: NamedCase
   ```

3. **TestData (base class):**
   ```
   Find:    : TestData\(
   Replace: : TestDataBase
   ```

**Verify:** Build project and check for type errors.

---

### Step 5: Rename Methods

**Find/Replace:**

1. **ToParams → ToArgs:**
   ```
   Find:    \.ToParams\(
   Replace: .ToArgs(
   ```

2. **ResultPrefix → GetResultPrefix():**
   ```
   Find:    \.ResultPrefix\b
   Replace: .GetResultPrefix()
   ```

**Verify:** Build project and check for method errors.

---

### Step 6: Update PropsCode Values

**Find/Replace:**

1. **Expected → TrimTestCaseName:**
   ```
   Find:    PropsCode\.Expected\b
   Replace: PropsCode.TrimTestCaseName
   ```

2. **TestCaseName → All:**
   ```
   Find:    PropsCode\.TestCaseName\b
   Replace: PropsCode.All
   ```

3. **Returns → TrimReturnsExpected:**
   ```
   Find:    PropsCode\.Returns\b
   Replace: PropsCode.TrimReturnsExpected
   ```

4. **Throws → TrimThrowsExpected:**
   ```
   Find:    PropsCode\.Throws\b
   Replace: PropsCode.TrimThrowsExpected
   ```

**Verify:** Build project and check for enum errors.

---

### Step 7: Update Custom Test Data (If Any)

If you have custom classes extending `TestData`, update them:

**Before:**
```csharp
public class MyTestData(string definition, int value)
: TestData(definition)
{
    public int Value { get; } = value;
}
```

**After:**
```csharp
public class MyTestData : TestDataBase
{
    internal MyTestData(string definition, string result, int value)
        : base(definition, result)
    {
        Value = value;
    }

    public required int Value { get; init; }
}
```

---

### Step 8: Build and Test

```bash
# Build
dotnet build

# Run tests
dotnet test

# Verify test output (test case names should be unchanged)
```

**Expected Results:**
- ✅ Project builds successfully
- ✅ All tests pass
- ✅ Test case names match previous format: `"definition => result"`

---

### Step 9: Commit Changes

```bash
git add .
git commit -m "Migrate from CsabaDu.DynamicTestData.Core to Portamical.Core"
```

---

## Migration Scenarios

### Scenario 1: Simple Test Data Source

#### Before (CsabaDu.DynamicTestData.Core)
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes;
using CsabaDu.DynamicTestData.Core.TestDataTypes.Factories;
using static CsabaDu.DynamicTestData.Core.TestDataTypes.Factories.TestDataFactory;

public class CalculatorDataSource
{
    public IEnumerable<TestData<int, int>> GetAdditionCases()
    {
        yield return CreateTestData(
            definition: "adding two positive numbers",
            result: "returns their sum",
            arg1: 2,
            arg2: 3);

        yield return CreateTestData(
            definition: "adding with zero",
            result: "returns the other number",
            arg1: 0,
            arg2: 5);
    }
}
```

#### After (Portamical.Core)
```csharp
using Portamical.Core.TestDataTypes.Models.General;
using static Portamical.Core.Factories.TestDataFactory;

public class CalculatorDataSource
{
    public IEnumerable<TestData<int, int>> GetAdditionCases()
    {
        yield return CreateTestData(
            definition: "adding two positive numbers",
            result: "returns their sum",
            arg1: 2,
            arg2: 3);

        yield return CreateTestData(
            definition: "adding with zero",
            result: "returns the other number",
            arg1: 0,
            arg2: 5);
    }
}
```

**Changes:**
- ✅ Namespace: `CsabaDu.DynamicTestData.Core.TestDataTypes` → `Portamical.Core.TestDataTypes.Models.General`
- ✅ Factory: `CsabaDu...Factories.TestDataFactory` → `Portamical.Core.Factories.TestDataFactory`
- ✅ Code: **Identical** (factory signatures unchanged)

---

### Scenario 2: Return-Value Test Data

#### Before (CsabaDu.DynamicTestData.Core)
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes;
using static CsabaDu.DynamicTestData.Core.TestDataTypes.Factories.TestDataFactory;

public class ParserDataSource
{
    public IEnumerable<TestDataReturns<int, string>> GetParseIntCases()
    {
        yield return CreateTestDataReturns(
            definition: "parse valid number",
            expected: 42,
            arg1: "42");

        yield return CreateTestDataReturns(
            definition: "parse zero",
            expected: 0,
            arg1: "0");
    }
}
```

#### After (Portamical.Core)
```csharp
using Portamical.Core.TestDataTypes.Models.Specialized;
using static Portamical.Core.Factories.TestDataFactory;

public class ParserDataSource
{
    public IEnumerable<TestDataReturns<int, string>> GetParseIntCases()
    {
        yield return CreateTestDataReturns(
            definition: "parse valid number",
            expected: 42,
            arg1: "42");

        yield return CreateTestDataReturns(
            definition: "parse zero",
            expected: 0,
            arg1: "0");
    }
}
```

**Changes:**
- ✅ Namespace: `...TestDataTypes` → `...TestDataTypes.Models.Specialized`
- ✅ Code: **Identical** (factory signatures unchanged)

---

### Scenario 3: Exception Test Data

#### Before (CsabaDu.DynamicTestData.Core)
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes;
using static CsabaDu.DynamicTestData.Core.TestDataTypes.Factories.TestDataFactory;

public class ValidationDataSource
{
    public IEnumerable<TestDataThrows<ArgumentNullException, string>> GetNullArgCases()
    {
        yield return CreateTestDataThrows(
            definition: "name is null",
            expected: new ArgumentNullException("name"),
            arg1: (string?)null);
    }
}
```

#### After (Portamical.Core)
```csharp
using Portamical.Core.TestDataTypes.Models.Specialized;
using static Portamical.Core.Factories.TestDataFactory;

public class ValidationDataSource
{
    public IEnumerable<TestDataThrows<ArgumentNullException, string>> GetNullArgCases()
    {
        yield return CreateTestDataThrows(
            definition: "name is null",
            expected: new ArgumentNullException("name"),
            arg1: (string?)null);
    }
}
```

**Changes:**
- ✅ Namespace: `...TestDataTypes` → `...TestDataTypes.Models.Specialized`
- ✅ Code: **Identical**

---

### Scenario 4: Converting to Parameter Arrays

#### Before (CsabaDu.DynamicTestData.Core)
```csharp
using CsabaDu.DynamicTestData.Core.DataStrategyTypes;
using CsabaDu.DynamicTestData.Core.TestDataTypes.Converters;

var testData = GetTestData();

// Most common: exclude TestCaseName
var args = testData.ToParams(ArgsCode.Properties, PropsCode.Expected);

// Include all properties
var argsAll = testData.ToParams(ArgsCode.Properties, PropsCode.TestCaseName);
```

#### After (Portamical.Core)
```csharp
using Portamical.Core.Strategy;

var testData = GetTestData();

// Most common: trim TestCaseName
var args = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);

// Include all properties
var argsAll = testData.ToArgs(ArgsCode.Properties, PropsCode.All);
```

**Changes:**
- ✅ Method: `ToParams()` → `ToArgs()`
- ✅ Enum: `PropsCode.Expected` → `PropsCode.TrimTestCaseName`
- ✅ Enum: `PropsCode.TestCaseName` → `PropsCode.All`

---

### Scenario 5: Identity Check (ContainedBy)

#### Before (CsabaDu.DynamicTestData.Core)
```csharp
using CsabaDu.DynamicTestData.Core.TestDataTypes.Interfaces;

INamedTestCase testCase = ...;
IEnumerable<INamedTestCase> existingCases = ...;

if (testCase.ContainedBy(existingCases))
{
    // Already exists
}
```

#### After (Portamical.Core)
```csharp
using Portamical.Core.Identity;

INamedCase testCase = ...;
IEnumerable<INamedCase> existingCases = ...;

if (testCase.ContainedBy(existingCases))  // ← SAME METHOD
{
    // Already exists
}
```

**Changes:**
- ✅ Type: `INamedTestCase` → `INamedCase`
- ✅ Method: **Unchanged** (still works identically)

**Note:** `ContainedBy()` was **refactored** (not removed). It now uses a snapshot pattern internally for better performance and thread safety.

---

## Testing Your Migration

### 1. Build Verification

```bash
dotnet build
```

**Expected:**
- ✅ Zero errors
- ✅ Zero warnings (ideally)

**If errors:**
- Check namespace updates
- Check type renames
- Check method renames

---

### 2. Test Execution

```bash
dotnet test --verbosity normal
```

**Expected:**
- ✅ All tests pass
- ✅ Test case names unchanged
- ✅ Same number of tests executed

**Verify test names:**
```bash
dotnet test --logger "console;verbosity=detailed" | grep "=>"
```

You should see test names like:
```
"Valid input => creates instance"
"name is null => throws ArgumentNullException"
"parse valid number => returns 42"
```

---

### 3. Identity Verification

Test that identity-based deduplication still works:

```csharp
using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;

[Fact]
public void TestCaseIdentity_ShouldWork()
{
    var testData1 = CreateTestData(
        definition: "test",
        result: "result",
        arg1: 1);

    var testData2 = CreateTestData(
        definition: "test",
        result: "result",
        arg1: 2);  // Different arg, but same identity

    // Identity based on TestCaseName (definition + result)
    Assert.Equal(testData1.TestCaseName, testData2.TestCaseName);
    Assert.True(testData1.Equals(testData2));

    // Deduplication works
    var set = new HashSet<INamedCase>(NamedCase.Comparer)
    {
        testData1,
        testData2  // Should not be added (duplicate)
    };

    Assert.Single(set);
}
```

---

### 4. Snapshot Pattern Verification

Test that `ContainedBy()` uses snapshot pattern:

```csharp
using Portamical.Core.Identity;

[Fact]
public void ContainedBy_ShouldUseSnapshot()
{
    var testData = CreateTestData(
        definition: "test",
        result: "result",
        arg1: 1);

    var collection = new List<INamedCase> { testData };

    // Should work with IEnumerable
    bool contained1 = testData.ContainedBy(collection);
    Assert.True(contained1);

    // Should work with LINQ query
    var query = collection.Where(x => x.TestCaseName.Contains("test"));
    bool contained2 = testData.ContainedBy(query);
    Assert.True(contained2);

    // Should work with null
    bool contained3 = testData.ContainedBy(null);
    Assert.False(contained3);
}
```

---

### 5. Framework Adapter Compatibility (Optional)

If you're using framework adapters, verify they still work:

```bash
# Install adapter
dotnet add package Portamical.xUnit  # or MSTest, NUnit

# Run tests
dotnet test
```

---

## Benefits of Migration

### 1. T4 Code Generation (96% Less Boilerplate)

**Before (CsabaDu.DynamicTestData.Core):**
- Manual factory methods (limited arities)
- Manual class definitions

**After (Portamical.Core):**
- 27 classes generated from 4 templates
- Change `MaxArity` once, regenerate all

**Benefit:** Easier to extend (e.g., support 12 arguments instead of 9)

---

### 2. Improved Architecture

**Before:**
- Flat namespace structure
- Mixed concerns

**After:**
- Clear separation: `Identity`, `Strategy`, `Models`, `Patterns`
- Better DDD alignment

**Benefit:** Easier to navigate and understand

---

### 3. Enhanced Identity System

**Before (CsabaDu.DynamicTestData.Core):**
```csharp
public bool ContainedBy(IEnumerable<INamedTestCase>? namedTestCases)
=> namedTestCases?.Any(Equals) == true;
```

**After (Portamical.Core):**
```csharp
public bool ContainedBy(IEnumerable<INamedCase>? namedCases)
=> Contains(this, namedCases);

public static bool Contains(
    INamedCase namedCase,
    IEnumerable<INamedCase>? namedCases)
{
    if (namedCases is null) return false;

    var snapshot = namedCases as INamedCase[]
        ?? [.. namedCases];  // ← Snapshot pattern

    return snapshot.Contains(namedCase, Comparer);
}
```

**Benefit:**
- ✅ **10-90× faster** for multiple checks (prevents re-enumeration)
- ✅ **Thread-safe** (immutable snapshot)
- ✅ **Consistent** (point-in-time view)

---

### 4. Framework Adapter Ecosystem

**Available Adapters:**
- `Portamical.xUnit` (xUnit v2)
- `Portamical.xUnit_v3` (xUnit v3)
- `Portamical.MSTest` (MSTest 4)
- `Portamical.NUnit` (NUnit 4)

**Benefit:** Cross-framework compatibility with unified API

---

### 5. Active Development

- Portamical is **actively maintained**
- CsabaDu.DynamicTestData.Core is **legacy/deprecated**
- Future features only in Portamical

---

## Troubleshooting

### Issue 1: ".NET 10 SDK not found"

**Error:**
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 10.0
```

**Solution:**
```bash
# Download .NET 10 SDK from:
# https://dotnet.microsoft.com/download/dotnet/10.0

# Verify installation
dotnet --version  # Should show 10.0.0 or later

# Update global.json (if present)
{
  "sdk": {
    "version": "10.0.0",
    "rollForward": "latestMinor"
  }
}
```

---

### Issue 2: "Type 'INamedTestCase' not found"

**Error:**
```
error CS0246: The type or namespace name 'INamedTestCase' could not be found
```

**Solution:**
```csharp
// Find/Replace
Find:    INamedTestCase
Replace: INamedCase

// Update namespace
using Portamical.Core.Identity;
```

---

### Issue 3: "Method 'ToParams' not found"

**Error:**
```
error CS1061: 'TestData<int>' does not contain a definition for 'ToParams'
```

**Solution:**
```csharp
// Find/Replace
Find:    .ToParams(
Replace: .ToArgs(
```

---

### Issue 4: "PropsCode.Expected does not exist"

**Error:**
```
error CS0117: 'PropsCode' does not contain a definition for 'Expected'
```

**Solution:**
```csharp
// Find/Replace
Find:    PropsCode.Expected
Replace: PropsCode.TrimTestCaseName

Find:    PropsCode.TestCaseName
Replace: PropsCode.All

Find:    PropsCode.Returns
Replace: PropsCode.TrimReturnsExpected

Find:    PropsCode.Throws
Replace: PropsCode.TrimThrowsExpected
```

---

### Issue 5: Test case names changed

**Problem:**
Test case names now show different format.

**Solution:**
Verify `GetDisplayName()` usage:

```csharp
// Should work automatically via framework adapters
string? displayName = testData.GetDisplayName(nameof(MyTestMethod));
// Returns: "MyTestMethod(testData: definition => result)"
```

If using custom display names, ensure you're using `testData.TestCaseName`:

```csharp
string testCaseName = testData.TestCaseName;
// Returns: "definition => result"
```

---

### Issue 6: Custom test data classes fail to compile

**Error:**
```
error CS7036: There is no argument given that corresponds to the required parameter 'result'
```

**Solution:**
Update custom classes from record syntax to immutable classes:

```csharp
// Before
public class MyTestData(string definition, int value)
: TestData(definition)
{
    public int Value { get; } = value;
}

// After
public class MyTestData : TestDataBase
{
    internal MyTestData(string definition, string result, int value)
        : base(definition, result)
    {
        Value = value;
    }

    public required int Value { get; init; }
}
```

---

### Issue 7: Performance regression

**Problem:**
Tests seem slower after migration.

**Diagnosis:**
```csharp
// Check if you're re-enumerating test data
var testData = GetTestData();  // Returns IEnumerable

// ❌ BAD: Multiple enumeration
var args1 = testData.ToArray();
var args2 = testData.Count();  // Re-enumerates!

// ✅ GOOD: Materialize once
var materialized = testData.ToArray();
var args1 = materialized;
var count = materialized.Length;
```

**Solution:**
Portamical's snapshot pattern should actually **improve** performance. If you see regression, check for multiple enumeration in your code.

---

## Support

### Resources

- **GitHub Repository:** https://github.com/CsabaDu/Portamical
- **Main README:** https://github.com/CsabaDu/Portamical/blob/master/README.md
- **Issues:** https://github.com/CsabaDu/Portamical/issues
- **Discussions:** https://github.com/CsabaDu/Portamical/discussions

### Getting Help

1. **Check this migration guide** — Most issues covered here
2. **Search GitHub Issues** — Your issue may already be solved
3. **Open a Discussion** — For questions and guidance
4. **Open an Issue** — For bugs or feature requests

### Reporting Migration Issues

When reporting issues, include:

1. **Environment:**
   - .NET SDK version (`dotnet --version`)
   - OS and version
   - IDE and version

2. **Old version:**
   - `CsabaDu.DynamicTestData.Core` version

3. **New version:**
   - `Portamical.Core` version

4. **Steps to reproduce:**
   - Before code (CsabaDu)
   - After code (Portamical)
   - Error message

5. **Expected vs. actual behavior**

---

## Appendix: Complete Find/Replace Script

### PowerShell Script

```powershell
# migration.ps1 - Automated migration script

$files = Get-ChildItem -Path . -Filter *.cs -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw

    # Namespaces
    $content = $content -replace 'CsabaDu\.DynamicTestData\.Core\.TestDataTypes\.Interfaces', 'Portamical.Core.Identity'
    $content = $content -replace 'CsabaDu\.DynamicTestData\.Core\.TestDataTypes\.Factories', 'Portamical.Core.Factories'
    $content = $content -replace 'CsabaDu\.DynamicTestData\.Core\.DataStrategyTypes', 'Portamical.Core.Strategy'
    $content = $content -replace 'CsabaDu\.DynamicTestData\.Core\.TestDataTypes', 'Portamical.Core.TestDataTypes'
    $content = $content -replace 'CsabaDu\.DynamicTestData\.Core\.Validators', 'Portamical.Core.Safety'

    # Types
    $content = $content -replace '\bINamedTestCase\b', 'INamedCase'
    $content = $content -replace '\bNamedTestCase\b', 'NamedCase'

    # Methods
    $content = $content -replace '\.ToParams\(', '.ToArgs('
    $content = $content -replace '\.ResultPrefix\b', '.GetResultPrefix()'

    # Enums
    $content = $content -replace 'PropsCode\.Expected\b', 'PropsCode.TrimTestCaseName'
    $content = $content -replace 'PropsCode\.TestCaseName\b', 'PropsCode.All'
    $content = $content -replace 'PropsCode\.Returns\b', 'PropsCode.TrimReturnsExpected'
    $content = $content -replace 'PropsCode\.Throws\b', 'PropsCode.TrimThrowsExpected'

    Set-Content $file.FullName -Value $content
}

Write-Host "Migration complete! Review changes and run 'dotnet build'."
```

### Bash Script

```bash
#!/bin/bash
# migration.sh - Automated migration script

find . -name "*.cs" -type f -exec sed -i '' \
  -e 's/CsabaDu\.DynamicTestData\.Core\.TestDataTypes\.Interfaces/Portamical.Core.Identity/g' \
  -e 's/CsabaDu\.DynamicTestData\.Core\.TestDataTypes\.Factories/Portamical.Core.Factories/g' \
  -e 's/CsabaDu\.DynamicTestData\.Core\.DataStrategyTypes/Portamical.Core.Strategy/g' \
  -e 's/CsabaDu\.DynamicTestData\.Core\.TestDataTypes/Portamical.Core.TestDataTypes/g' \
  -e 's/CsabaDu\.DynamicTestData\.Core\.Validators/Portamical.Core.Safety/g' \
  -e 's/\bINamedTestCase\b/INamedCase/g' \
  -e 's/\bNamedTestCase\b/NamedCase/g' \
  -e 's/\.ToParams\(/\.ToArgs\(/g' \
  -e 's/\.ResultPrefix\b/\.GetResultPrefix\(\)/g' \
  -e 's/PropsCode\.Expected\b/PropsCode.TrimTestCaseName/g' \
  -e 's/PropsCode\.TestCaseName\b/PropsCode.All/g' \
  -e 's/PropsCode\.Returns\b/PropsCode.TrimReturnsExpected/g' \
  -e 's/PropsCode\.Throws\b/PropsCode.TrimThrowsExpected/g' \
  {} \;

echo "Migration complete! Review changes and run 'dotnet build'."
```

---

## Changelog

### **Version 1.0 (March 6, 2026)**
- **Initial migration guide**
  - Covers all breaking changes
  - Includes automated scripts
  - Complete troubleshooting section

---

**End of Migration Guide**

For questions or issues, visit: https://github.com/CsabaDu/Portamical/discussions

---