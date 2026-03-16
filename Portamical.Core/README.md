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

### **Version 2.0.0 (2026-03-13)**

**Note:** This version does not introduce breaking changes in `Portamical.Core` itself. The major version bump to 2.0.0 aligns with the Portamical extension packages, where new versions may introduce rare breaking changes. The version number synchronization ensures consistency across the Portamical ecosystem.

#### **Major Enhancements**

**Comprehensive XML Documentation**
- **Added extensive XML documentation comments** across the entire codebase (65 files updated)
- **Documented design patterns** (Template Method, Strategy, Adapter, Decorator, Factory Method)
- **Added detailed usage examples** with code samples for all public APIs
- **Included architecture diagrams** and inheritance chains in documentation

**Core Infrastructure Improvements**
- **`NamedCase`**:
  - Added `[SuppressMessage]` attributes with justifications for SonarLint rules
  - Improved `CreateTestCaseName()` performance using string.Create for zero-allocation concatenation
  - Enhanced `Equals` and `GetHashCode` implementations with detailed explanations
- **`EnumValidator`**:
  - Renamed `GetResultPrefix` → `GetValidResultPrefix` for clarity
- **`Resolver`**:
  - Improved `FallbackIfNullOrWhiteSpace` with `string.Create` and `CultureInfo.InvariantCulture`
  - Changed `Trace.WriteLine` → `Trace.TraceWarning` for better diagnostic categorization
  - Added thread-safety documentation for `ResetLogCounter` atomic operations
- **`TestDataBase`**:
  - Optimized CreateTestCaseName() using string.Create (zero-copy concatenation)

**T4 Template Improvements**
- **`SharedHelpers.ttinclude`**: Fixed encoding issue (BOM removed)
- Maintained single source of truth for `MaxArity = 9`

**Build Configuration**
- Updated version to 2.0.0
- Cleared `PackageReleaseNotes` (prepared for final release notes)
- Added `PackageOutputPath` configuration

**Code Quality Improvements**
- Fixed typo: "Cmbines" → "Combines" in `INamedCase`
- Removed unused "TrimTestCaseName" placeholders
- Enhanced null-safety compliance throughout
- Added `ArgumentOutOfRangeException` documentation where applicable
- Improved parameter name consistency (`paramName` validation)

**Documentation Themes**
- **Design Patterns**: Template Method, Strategy, Adapter, Decorator, Identity Object
- **Performance**: O(n) complexity notes, zero-allocation techniques, atomic operations
- **Thread Safety**: Immutability patterns, concurrent access documentation
- **Framework Integration**: xUnit v2/v3, NUnit 3/4, MSTest examples
- **Migration Support**: Comparisons with previous API versions

---

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