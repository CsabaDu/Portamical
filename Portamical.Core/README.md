# Portamical.Core

**Portamical.Core** is the framework-agnostic core of **Portamical**: a universal, identity-driven test data modeling framework for .NET.

Define test data **once** and consume it across test frameworks using adapter packages—without rewriting the data or sacrificing strong typing.

---

## Install

```bash
dotnet add package Portamical.Core
```

> You will typically also install a Portamical adapter package for your test framework (xUnit / MSTest / NUnit).

---

## Example

Create reusable test data in a single place:

```csharp
using static Portamical.Core.Factories.TestDataFactory;

public sealed class CalculatorCases
{
    public static IEnumerable<TestData<int, int>> Add()
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

Then consume via your test framework’s adapter (usage varies by framework).

---

## Links

- GitHub: https://github.com/CsabaDu/Portamical
- Documentation: https://github.com/CsabaDu/Portamical/blob/master/README.md
- Issues: https://github.com/CsabaDu/Portamical/issues
- Migration Guide: https://github.com/CsabaDu/Portamical/blob/master/MIGRATION.md

## License

MIT

---

## Changelog

### **Version 1.0.0 (2026-03-04)**

- **Initial release**

##### **Version 1.0.1 (2026-03-06)**

- **Changed:**
  - `Portamical.Core.Safety.EnumValidator.Defined`: `string paramName` changed to not nullable type.
  - README.md replaced.

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---