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
- Migration Guide from *CsabaDu.DynamicTestData.Core* to *Portamical.Core*: https://github.com/CsabaDu/Portamical/blob/master/Portamical.Core/MIGRATION.md

## License and Project Lineage

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

`Portamical.Core` is the **continuation and successor** of `CsabaDu.DynamicTestData.Core` (also MIT-licensed).  
`CsabaDu.DynamicTestData.Core` is considered **legacy** and is **no longer supported**; new development happens in Portamical.

---

## Changelog

### **Version 1.0.0 (2026-03-04)**

- **Initial release**

##### **Version 1.0.1 (2026-03-06)**

- `Portamical.Core.Safety.EnumValidator.Defined`: `string paramName` changed to not nullable type.
- README.md replaced.

##### **Version 1.0.2 (2026-03-07)**

- Link of migration guide (MIGRATION.md) added to README.md.

##### **Version 1.0.3 (2026-03-08)**

- T4 Generated Code: All generated files (`TestDataFactory.generated.cs`, `TestData.generated.cs`, `TestDataReturns.generated.cs`, `TestDataThrows.generated.cs`) include explicit `#nullable enable` directive for enhanced null-safety compliance with C# nullable reference types.

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---