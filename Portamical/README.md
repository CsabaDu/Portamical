# Portamical

**Shared utilities and base classes for cross-framework test data solutions in .NET.**

Portamical provides framework-agnostic converters, assertions, and test base classes that bridge between **Portamical.Core** and framework-specific adapters.

---

## Install

```bash
dotnet add package Portamical
```

> **Note:** Most users should install a framework adapter instead:
> - `Portamical.xUnit` for xUnit v2
> - `Portamical.xUnit_v3` for xUnit v3
> - `Portamical.MSTest` for MSTest 4
> - `Portamical.NUnit` for NUnit 4

---

## What's Included

### Converters
Transform test data collections into framework-consumable formats:

```csharp
using Portamical.Converters;

IEnumerable<object?[]> args = testDataCollection.ToObjectArrayCollection(ArgsCode.Instance);
```

### Assertions
Framework-agnostic assertion helpers with delegate injection:

```csharp
using Portamical.Assertions;

PortamicalAssert.ThrowsDetails(
    attempt: () => MethodUnderTest(),
    expected: new ArgumentNullException("paramName"),
    catchException: Record.Exception,
    assertIsType: Assert.IsType,
    assertEquality: Assert.Equal,
    assertFail: Assert.Fail);
```

### Test Bases
Abstract base classes for common test patterns:

```csharp
using Portamical.TestBases.ObjectArrayCollection;

public class MyTestClass : TestBase
{
    protected static IEnumerable<object?[]> Args
        => Convert(dataSource.GetArgs());
}
```

---

## Use Cases

Install **Portamical** directly if you are:

- Building a custom test framework adapter
- Creating shared test utilities across multiple frameworks
- Need direct access to converters without framework-specific wrappers

For typical testing scenarios, use a framework adapter instead.

---

## Links

- GitHub: https://github.com/CsabaDu/Portamical
- Documentation: https://github.com/CsabaDu/Portamical/blob/master/README.md
- Issues: https://github.com/CsabaDu/Portamical/issues

---


## License and Project Lineage

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

`Portamical` is the **continuation and successor** of `CsabaDu.DynamicTestData.Light` and `CsabaDu.DynamicTestData` (also MIT-licensed).  
`CsabaDu.DynamicTestData.Light` and `CsabaDu.DynamicTestData` are considered **legacy** and is **no longer supported**; new development happens in Portamical.

---

## Changelog

### **Version 1.0.0 (2026-03-06)**

- **Initial release**

#### **Version 1.0.1 (2026-03-07)**

- **xunit.runner.json relocation**
  - Moved from Portamical package to xUnit adapter packages
  - Improves package separation and reduces unnecessary files

- **Global usings consolidation**
  - Improved GlobalUsings.cs organization
  - Better namespace management across the solution

#### **Version 1.0.2 (2026-03-08)**

- **`Portamical.TestBases.TestBase`**: Implemented standard `IDisposable` pattern with `Dispose(bool disposing)` method that resets `ArgsCode` to default (`AsInstance`) and clears log counter, enabling proper cleanup in test fixture teardown scenarios.
- License and Project Lineage added to README.md.

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---