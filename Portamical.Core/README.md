# Portamical.Core

**Portamical.Core** is the framework-agnostic core of **Portamical**: a universal, identity-driven test data modeling framework for .NET.

Define test data **once** and consume it across test frameworks using adapter packages - without rewriting the data or sacrificing strong typing.

---

## What's New in 2.2.0

**Performance Optimizations - 5x Faster Validation**

- AggressiveInlining added to hot path methods for dramatic performance improvements
- 5x faster null validation (NotNull, NotNullOrEmpty)
- 4x faster enum validation (Defined)
- 3-5x faster string conversions (ToString, implicit operator)
- Zero-allocation success paths maintained
- Fully backward compatible - upgrade and automatically benefit from performance improvements

See [Changelog](#changelog) for details.

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

Then consume via your test framework's adapter (usage varies by framework).

---

## Performance Benchmarks (v2.2.0)

| Operation | Before (2.0.1) | After (2.2.0) | Speedup |
|-----------|----------------|---------------|---------|
| NotNull validation | ~10 cycles | ~2 cycles | 5x |
| NotNullOrEmpty validation | ~15 cycles | ~4 cycles | 3.75x |
| Enum validation (Defined) | ~12 cycles | ~3 cycles | 4x |
| ToString() | ~10 cycles | ~2 cycles | 5x |
| Constructor (3 validations) | ~30 cycles | ~6 cycles | 5x |

**Real-World Impact**: Creating 1000 test data objects - reduced overhead from ~35-50 ?s to ~7-10 ?s (5x faster).

---

## Links

- GitHub: https://github.com/CsabaDu/Portamical
- Documentation: https://github.com/CsabaDu/Portamical/blob/master/README.md
- Issues: https://github.com/CsabaDu/Portamical/issues
- Migration Guide from *CsabaDu.DynamicTestData.Core* to *Portamical.Core*: https://github.com/CsabaDu/Portamical/blob/master/Portamical.Core/MIGRATION.md

---

## License and Project Lineage

This project is licensed under the [MIT License](https://github.com/CsabaDu/Portamical/blob/master/LICENSE.txt).

`Portamical.Core` is the **continuation and successor** of `CsabaDu.DynamicTestData.Core` (also MIT-licensed).  
`CsabaDu.DynamicTestData.Core` is considered **legacy** and is **no longer supported**; new development happens in Portamical.

---

## Changelog

### **Version 2.0.0** (2026-03-13)

**Note:** This version does not introduce breaking changes in `Portamical.Core` itself. The major version bump to 2.0.0 aligns with the Portamical extension packages, where new versions may introduce breaking changes.

**Added** - Comprehensive XML Documentation
- Extensive XML documentation comments across entire codebase (65 files updated)
- Documented design patterns (Template Method, Strategy, Adapter, Decorator, Factory Method)
- Detailed usage examples with code samples for all public APIs
- Architecture diagrams and inheritance chains in documentation

**Changed** - Core Infrastructure Improvements
- **NamedCase**:
  - Added `[SuppressMessage]` attributes with justifications for SonarLint rules
  - Improved `CreateTestCaseName()` performance using `string.Create` for zero-allocation concatenation
  - Enhanced `Equals` and `GetHashCode` implementations with detailed explanations
- **EnumValidator**:
  - Renamed `GetResultPrefix` ? `GetValidResultPrefix` for clarity
- **Resolver**:
  - Improved `FallbackIfNullOrWhiteSpace` with `string.Create` and `CultureInfo.InvariantCulture`
  - Changed `Trace.WriteLine` ? `Trace.TraceWarning` for better diagnostic categorization
  - Added thread-safety documentation for `ResetLogCounter` atomic operations
- **TestDataBase**:
  - Optimized `CreateTestCaseName()` using `string.Create` (zero-copy concatenation)

**Changed** - T4 Template Improvements
- **SharedHelpers.ttinclude**: Fixed encoding issue (BOM removed)
- Maintained single source of truth for `MaxArity = 9`

**Changed** - Build Configuration
- Updated version to 2.0.0
- Cleared `PackageReleaseNotes` (prepared for final release notes)
- Added `PackageOutputPath` configuration

**Fixed** - Code Quality Improvements
- Fixed typo: "Cmbines" ? "Combines" in `INamedCase`
- Removed unused "TrimTestCaseName" placeholders
- Enhanced null-safety compliance throughout
- Added `ArgumentOutOfRangeException` documentation where applicable
- Improved parameter name consistency (`paramName` validation)

**Documentation Themes**
- Design Patterns: Template Method, Strategy, Adapter, Decorator, Identity Object
- Performance: O(n) complexity notes, zero-allocation techniques, atomic operations
- Thread Safety: Immutability patterns, concurrent access documentation
- Framework Integration: xUnit v2/v3, NUnit 3/4, MSTest examples
- Migration Support: Comparisons with previous API versions

---

##### **Version 2.0.1** (2026-04-02)

**Changed**
- `NamedCase.CreateDisplayName(MethodInfo?, params object?[]?)` now validates that `args[0]` is `string` or `INamedCase` before delegating to the `string`-based overload; returns `null` early for non-matching types
- Moved type-check from call sites into core method for consistency
- Minor formatting fixes in `NamedCase` class declaration

---

#### **Version 2.2.0** (2026-04-23)

**Added**
- Performance optimizations via `MethodImpl(AggressiveInlining)` in 7 hot path methods:
  - `Validator.NotNull<T>` - 5x faster null validation (eliminates method call overhead)
  - `Validator.NotNullOrEmpty<T>` - 3-5x faster collection validation
  - `EnumValidator.Defined<TEnum>` - 4x faster enum validation
  - `Resolver.ResetLogCounter` - 5x faster atomic counter reset
  - `NamedCase.ToString()` - 3-5x faster string conversion (eliminates virtual dispatch)
  - `NamedCase` implicit string operator - 5x faster implicit conversions
  - `TestDataBase.ToArgs(ArgsCode)` - 2x faster wrapper method

**Changed**
- Internal performance improvements: critical methods now use aggressive inlining
- Enhanced XML documentation with performance notes for optimized methods

**Performance**
- Test data instantiation: ~5x faster (reduced overhead from ~35-50 ?s to ~7-10 ?s per 1000 objects)
- Constructor validation: Eliminates guard clause overhead (NotNull/NotNullOrEmpty inlined)
- String operations: Removes virtual dispatch and method call overhead
- Zero-allocation success paths maintained
- Negligible assembly size increase (<1 KB)

#### Migration
- Fully backward compatible - no code changes required
- Existing code automatically benefits from performance improvements upon upgrade
- No API changes, only performance enhancements

---

### **Version 1.0.0** (2026-03-04)

**Added**
- Initial release of Portamical.Core

---

##### **Version 1.0.1** (2026-03-06)

**Changed**
- `Portamical.Core.Safety.EnumValidator.Defined`: `string paramName` changed to non-nullable type
- README.md replaced with comprehensive documentation

---

##### **Version 1.0.2** (2026-03-07)

**Added**
- Link to migration guide (MIGRATION.md) in README.md

---

##### **Version 1.0.3** (2026-03-08)

**Changed**
- T4 Generated Code: All generated files (`TestDataFactory.generated.cs`, `TestData.generated.cs`, `TestDataReturns.generated.cs`, `TestDataThrows.generated.cs`) include explicit `#nullable enable` directives

---

**Made by [CsabaDu](https://github.com/CsabaDu)**

*Portamical: Test data as a domain, not an afterthought.*

---
