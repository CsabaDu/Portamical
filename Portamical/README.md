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

## License

MIT

---

## Changelog

### **Version 1.0.0 (2026-03-06)**

- **Initial release**

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---