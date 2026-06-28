# Nullable Value Type Tests for DefaultFormatter.Format()

## Overview
Added comprehensive tests to verify that `DefaultFormatter.Format(object? obj)` correctly handles null values from various nullable value types.

## Test Cases Added

### 1. **Primitive Nullable Types**

#### `Format_withNullableNoValue_returnsNull` (existing)
```csharp
int? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null `int?` returns null.

#### `Format_withNullableDoubleNoValue_returnsNull`
```csharp
double? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null `double?` returns null.

#### `Format_withNullableDecimalNoValue_returnsNull`
```csharp
decimal? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null `decimal?` returns null.

#### `Format_withNullableBoolNoValue_returnsNull`
```csharp
bool? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null `bool?` returns null.

---

### 2. **DateTime and Guid Nullable Types**

#### `Format_withNullableDateTimeNoValue_returnsNull`
```csharp
DateTime? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null `DateTime?` returns null (important for timestamp formatting).

#### `Format_withNullableGuidNoValue_returnsNull`
```csharp
Guid? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null `Guid?` returns null (important for ID formatting).

---

### 3. **Enum Nullable Types**

#### `Format_withNullableEnumNoValue_returnsNull`
```csharp
DayOfWeek? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null enum values return null.

---

### 4. **Boxed Nullable Types**

#### `Format_withBoxedNullableIntNoValue_returnsNull`
```csharp
int? nullable = null;
object? boxed = nullable; // Boxing null Nullable<int>
var result = DefaultFormatter.Format(boxed);
Assert.IsNull(result);
```
**Purpose**: Verifies that boxing a null `Nullable<T>` and passing it as `object?` returns null.
**Key Behavior**: When `Nullable<T>` with no value is boxed, it becomes `null` reference.

#### `Format_withBoxedNullableIntHasValue_returnsValue`
```csharp
int? nullable = 42;
object boxed = nullable; // Boxing Nullable<int> with value
var result = DefaultFormatter.Format(boxed);
Assert.AreEqual("42", result);
```
**Purpose**: Verifies that boxing a `Nullable<T>` with a value boxes the underlying value directly.
**Key Behavior**: When `Nullable<T>` has a value, boxing extracts and boxes the value (as `int`, not `int?`).

---

### 5. **Custom Struct Nullable Types**

#### `Format_withNullableStructNoValue_returnsNull`
```csharp
TestStruct? nullable = null;
var result = DefaultFormatter.Format(nullable);
Assert.IsNull(result);
```
**Purpose**: Verifies that null custom struct types return null.

**Helper Struct**:
```csharp
private struct TestStruct
{
	public int Value { get; set; }
	public override string ToString() => $"TestStruct:{Value}";
}
```

---

## Key Behaviors Verified

### 1. **Null Nullable Value Types ? `null` Return**
The switch expression in `DefaultFormatter.Format(object? obj)`:
```csharp
public static string? Format(object? obj)
=> obj switch
{
	null => null,  // ? This handles all null nullable value types
	// ... other patterns
};
```

When a nullable value type has no value:
- Passing it directly: `Format(nullable)` where `nullable` is `int?` with no value ? `obj` is `null` ? returns `null`
- Boxing it: `object? boxed = nullable` ? `boxed` is `null` ? returns `null`

### 2. **Boxing Behavior of `Nullable<T>`**

| Scenario | Type | Boxed Type | Result |
|----------|------|------------|--------|
| `int? nullable = null;` | `Nullable<int>` (no value) | `null` | Returns `null` |
| `int? nullable = 42;` | `Nullable<int>` (has value) | `int` (boxed value) | Returns `"42"` |

**Important**: When `Nullable<T>` with a value is boxed, it boxes the **underlying value** (`T`), not the `Nullable<T>` itself.

---

## Coverage Summary

? **Primitive types**: `int?`, `double?`, `decimal?`, `bool?`  
? **DateTime types**: `DateTime?`  
? **Guid types**: `Guid?`  
? **Enum types**: `DayOfWeek?`  
? **Custom struct types**: `TestStruct?`  
? **Boxed nullable types**: Both null and non-null values  

---

## Test Results

**Total Tests**: 10 new tests added (1 already existed, 9 newly added)  
**Pass Rate**: 100% (10/10 pass)  
**Total Suite**: 145 tests (all pass)  
**Build Status**: ? Successful  

---

## Implementation Notes

### Why These Tests Matter

1. **Type Safety**: Ensures nullable value types are handled correctly across all primitive and custom types
2. **Boxing Semantics**: Validates understanding of how `Nullable<T>` boxes to either `null` or `T`
3. **Edge Cases**: Covers less common types like `Guid?`, `DateTime?`, and custom structs
4. **Consistency**: Demonstrates consistent behavior across all nullable value type scenarios

### DefaultFormatter Implementation

The formatter's switch expression correctly handles all these cases via the first pattern:
```csharp
null => null,
```

This works because:
- Nullable value types without values become `null` when passed as `object?`
- Boxing a `Nullable<T>` with no value results in `null` reference
- The pattern matching short-circuits and returns `null` immediately

---

## Related Code

**Source**: `Portamical.Core.Formatting\DefaultFormatter.cs`  
**Test File**: `_Tests\Portamical.Core.Formatting\DefaultFormatterTests.cs`  
**Lines**: Added tests around line 1333-1413  

---

## Conclusion

All nullable value type scenarios are now thoroughly tested and verified to work correctly. The `DefaultFormatter.Format()` method handles null values from nullable value types consistently, returning `null` to signal formatting failure as documented in the XML comments.
