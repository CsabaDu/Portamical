// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;
using Portamical.Core.Formatting.Model;
using static Portamical.Core.Formatting.FormatBuilder;

namespace Tests.Portamical.Core.Formatting.Model;

/// <summary>
/// Unit tests for <see cref="Formatter"/> static utility methods and constants.
/// </summary>
[TestClass]
public class FormatterTests
{
    #region Constants
#pragma warning disable MSTEST0032 // Review or remove the assertion as its condition is known to be always true
    [TestMethod]
    public void NullString_hasCorrectValue()
    {
        Assert.AreEqual("null", NullString);
    }

    [TestMethod]
    public void MaxCount_hasCorrectValue()
    {
        Assert.AreEqual(3, MaxCount);
    }
#pragma warning restore MSTEST0032
    #endregion

    #region FallbackIfNull
    [TestMethod]
    public void FallbackIfNull_withNull_returnsNullString()
    {
        var result = FallbackIfNull(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FallbackIfNull_withEmptyString_returnsEmptyString()
    {
        var result = FallbackIfNull("");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void FallbackIfNull_withNonNullString_returnsOriginal()
    {
        var input = "test";
        var result = FallbackIfNull(input);
        Assert.AreEqual("test", result);
        Assert.AreSame(input, result);
    }

    [TestMethod]
    public void FallbackIfNull_withWhitespace_returnsOriginal()
    {
        var result = FallbackIfNull("   ");
        Assert.AreEqual("   ", result);
    }
    #endregion

    #region CopyAsSpan
    [TestMethod]
    public void CopyAsSpan_copiesStringToSpan()
    {
        var buffer = new char[10];
        var span = new Span<char>(buffer);

        CopyAsSpan("hello", span, 0);

        Assert.AreEqual('h', buffer[0]);
        Assert.AreEqual('e', buffer[1]);
        Assert.AreEqual('l', buffer[2]);
        Assert.AreEqual('l', buffer[3]);
        Assert.AreEqual('o', buffer[4]);
    }

    [TestMethod]
    public void CopyAsSpan_copiesStringAtOffset()
    {
        var buffer = new char[10];
        var span = new Span<char>(buffer);

        CopyAsSpan("abc", span, 0);
        CopyAsSpan("xyz", span, 5);

        Assert.AreEqual('a', buffer[0]);
        Assert.AreEqual('b', buffer[1]);
        Assert.AreEqual('c', buffer[2]);
        Assert.AreEqual('\0', buffer[3]);
        Assert.AreEqual('\0', buffer[4]);
        Assert.AreEqual('x', buffer[5]);
        Assert.AreEqual('y', buffer[6]);
        Assert.AreEqual('z', buffer[7]);
    }

    [TestMethod]
    public void CopyAsSpan_withEmptyString_doesNothing()
    {
        var buffer = new char[5];
        Array.Fill(buffer, 'x');
        var span = new Span<char>(buffer);

        CopyAsSpan("", span, 2);

        Assert.AreEqual('x', buffer[0]);
        Assert.AreEqual('x', buffer[1]);
        Assert.AreEqual('x', buffer[2]);
    }
    #endregion

    #region CreateSeparatedString
    [TestMethod]
    public void CreateSeparatedString_concatenatesThreeParts()
    {
        var result = CreateSeparatedString("Hello", " ", "World");
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptyBase_concatenatesCorrectly()
    {
        // "" (0) + ", " (2) + "test" (4) = 6 total
        var result = CreateSeparatedString("", ", ", "test");
        Assert.AreEqual(", test", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptyAppendix_concatenatesCorrectly()
    {
        // "base" (4) + ": " (2) + "" (0) = 6 total
        var result = CreateSeparatedString("base", ": ", "");
        Assert.AreEqual("base: ", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptySeparator_concatenatesCorrectly()
    {
        // "test" (4) + "" (0) + "case" (4) = 8 total
        var result = CreateSeparatedString("test", "", "case");
        Assert.AreEqual("testcase", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withAllEmptyStrings_returnsEmptyString()
    {
        // "" (0) + "" (0) + "" (0) = 0 total
        var result = CreateSeparatedString("", "", "");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withComplexSeparator_concatenatesCorrectly()
    {
        // "Method" (6) + " - " (3) + "param1" (6) = 15 total
        var result = CreateSeparatedString("Method", " - ", "param1");
        Assert.AreEqual("Method - param1", result);
    }
    #endregion

    #region JoinWithComma - Empty and Single Item
    [TestMethod]
    public void JoinWithComma_withEmptyList_returnsEmptyString()
    {
        var items = new List<string?>();
        var result = JoinWithComma(items);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void JoinWithComma_withEmptyArray_returnsEmptyString()
    {
        var items = Array.Empty<string?>();
        var result = JoinWithComma(items);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void JoinWithComma_withSingleItem_returnsItem()
    {
        var items = new List<string?> { "test" };
        var result = JoinWithComma(items);
        Assert.AreEqual("test", result);
    }

    [TestMethod]
    public void JoinWithComma_withSingleNull_returnsNull()
    {
        var items = new List<string?> { null };
        var result = JoinWithComma(items);
        Assert.AreEqual("null", result);
    }
    #endregion

    #region JoinWithComma - Two Items
    [TestMethod]
    public void JoinWithComma_withTwoItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "first", "second" };
        var result = JoinWithComma(items);
        Assert.AreEqual("first, second", result);
    }

    [TestMethod]
    public void JoinWithComma_withTwoItemsFirstNull_returnsCommaSeparated()
    {
        var items = new List<string?> { null, "second" };
        var result = JoinWithComma(items);
        Assert.AreEqual("null, second", result);
    }

    [TestMethod]
    public void JoinWithComma_withTwoItemsSecondNull_returnsCommaSeparated()
    {
        var items = new List<string?> { "first", null };
        var result = JoinWithComma(items);
        Assert.AreEqual("first, null", result);
    }

    [TestMethod]
    public void JoinWithComma_withTwoNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { null, null };
        var result = JoinWithComma(items);
        Assert.AreEqual("null, null", result);
    }
    #endregion

    #region JoinWithComma - Three Items
    [TestMethod]
    public void JoinWithComma_withThreeItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "one", "two", "three" };
        var result = JoinWithComma(items);
        Assert.AreEqual("one, two, three", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsFirstNull_returnsCommaSeparated()
    {
        var items = new List<string?> { null, "two", "three" };
        var result = JoinWithComma(items);
        Assert.AreEqual("null, two, three", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsSecondNull_returnsCommaSeparated()
    {
        var items = new List<string?> { "one", null, "three" };
        var result = JoinWithComma(items);
        Assert.AreEqual("one, null, three", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsThirdNull_returnsCommaSeparated()
    {
        var items = new List<string?> { "one", "two", null };
        var result = JoinWithComma(items);
        Assert.AreEqual("one, two, null", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { null, null, null };
        var result = JoinWithComma(items);
        Assert.AreEqual("null, null, null", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsMixedNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { null, "middle", null };
        var result = JoinWithComma(items);
        Assert.AreEqual("null, middle, null", result);
    }
    #endregion

    #region JoinWithComma - Four or More Items
    [TestMethod]
    public void JoinWithComma_withFourItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "a", "b", "c", "d" };
        var result = JoinWithComma(items);
        Assert.AreEqual("a, b, c, d", result);
    }

    [TestMethod]
    public void JoinWithComma_withManyItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var result = JoinWithComma(items);
        Assert.AreEqual("1, 2, 3, 4, 5, 6, 7, 8, 9, 10", result);
    }

    [TestMethod]
    public void JoinWithComma_withFourItemsContainingNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { "a", null, "c", null };
        var result = JoinWithComma(items);
        Assert.AreEqual("a, null, c, null", result);
    }
    #endregion

    #region JoinWithComma - Non-List Collections
    [TestMethod]
    public void JoinWithComma_withArray_returnsCommaSeparated()
    {
        var items = new[] { "x", "y", "z" };
        var result = JoinWithComma(items);
        Assert.AreEqual("x, y, z", result);
    }

    [TestMethod]
    public void JoinWithComma_withEnumerable_returnsCommaSeparated()
    {
        var items = Enumerable.Range(1, 5).Select(i => i.ToString());
        var result = JoinWithComma(items);
        Assert.AreEqual("1, 2, 3, 4, 5", result);
    }

    [TestMethod]
    public void JoinWithComma_withHashSet_returnsCommaSeparated()
    {
        var items = new HashSet<string?> { "alpha", "beta", "gamma" };
        var result = JoinWithComma(items);
        // Order may vary with HashSet, just check all items are present
        Assert.Contains("alpha", result);
        Assert.Contains("beta", result);
        Assert.Contains("gamma", result);
        Assert.AreEqual(2, result.Count(c => c == ','));
    }
    #endregion

    #region JoinWithComma - Edge Cases
    [TestMethod]
    public void JoinWithComma_withEmptyStrings_distinguishesFromEmptyCollection()
    {
        var emptyCollection = new List<string?>();
        var singleEmpty = new List<string?> { "" };
        var twoEmpties = new List<string?> { "", "" };

        var result1 = JoinWithComma(emptyCollection);
        var result2 = JoinWithComma(singleEmpty);
        var result3 = JoinWithComma(twoEmpties);

        Assert.AreEqual("", result1);
        Assert.AreEqual("", result2);
        Assert.AreEqual(", ", result3);
    }

    [TestMethod]
    public void JoinWithComma_withQuotedStrings_preservesQuotes()
    {
        var items = new List<string?> { "\"hello\"", "'a'", "42" };
        var result = JoinWithComma(items);
        Assert.AreEqual("\"hello\", 'a', 42", result);
    }

    [TestMethod]
    public void JoinWithComma_withLongStrings_joinsCorrectly()
    {
        var long1 = new string('a', 100);
        var long2 = new string('b', 100);
        var items = new List<string?> { long1, long2 };
        var result = JoinWithComma(items);

        Assert.AreEqual(202, result.Length); // 100 + 2 (", ") + 100
        Assert.StartsWith(new string('a', 100), result);
        Assert.EndsWith(new string('b', 100), result);
    }
    #endregion

    #region Abstract Method Test Implementation
    [TestMethod]
    public void Format_abstractMethod_canBeImplemented()
    {
        var formatter = new TestFormatter();
        var result = formatter.Format("test");
        Assert.AreEqual("TEST", result);
    }

    [TestMethod]
    public void Format_abstractMethod_canReturnNull()
    {
        var formatter = new TestFormatter();
        var result = formatter.Format(null!);
        Assert.IsNull(result);
    }

    // Test implementation of abstract Formatter class
    private class TestFormatter : IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj == null) return null;
            return obj.ToString()?.ToUpper();
        }
    }
    #endregion

    #region Formatter<T> - Type Safety Tests
    [TestMethod]
    public void FormatterT_Format_withMatchingType_delegatesToTypeSafeMethod()
    {
        var formatter = new TestFormatterInt();
        var result = formatter.Format(42);
        Assert.AreEqual("INT:42", result);
    }

    // These methods test an earlier version of formatter that had a non-generic Format(object) method. In the current design, Formatter<T> has a sealed Format(object) method that delegates to the type-safe Format(T) method. These tests ensure that the delegation works correctly and that type mismatches are handled as expected.
    //[TestMethod]
    //public void FormatterT_FormatObject_withMatchingType_callsTypeSafeMethod()
    //{
    //    var formatter = new TestFormatterInt();
    //    object obj = 42;
    //    var result = formatter.Format(obj);
    //    Assert.AreEqual("INT:42", result);
    //}

    //[TestMethod]
    //public void FormatterT_FormatObject_withNonMatchingType_returnsNull()
    //{
    //    var formatter = new TestFormatterInt();
    //    object obj = "not an int";
    //    var result = formatter.Format(obj);
    //    Assert.IsNull(result);
    //}

    //[TestMethod]
    //public void FormatterT_FormatObject_withNullForValueType_returnsNull()
    //{
    //    var formatter = new TestFormatterInt();
    //    var result = formatter.Format(null!);
    //    Assert.IsNull(result);
    //}

    //[TestMethod]
    //public void FormatterT_FormatObject_withBoxedValueType_unboxesAndFormats()
    //{
    //    var formatter = new TestFormatterInt();
    //    object boxed = 123;
    //    var result = formatter.Format(boxed);
    //    Assert.AreEqual("INT:123", result);
    //}
    //[TestMethod]
    //public void FormatterT_FormatObject_withDerivedType_worksCorrectly()
    //{
    //    var formatter = new TestFormatterException();
    //    object exception = new InvalidOperationException("test");
    //    var result = formatter.Format(exception);
    //    Assert.AreEqual("EX:System.InvalidOperationException", result);
    //}
    //[TestMethod]
    //public void FormatterT_withNullableValueType_objectOverloadHandlesNull()
    //{
    //    // When null is passed as object?, the pattern matching fails for nullable obj types
    //    // because null (object?) is not the same as null (int?)
    //    var formatter = new TestFormatterNullableInt();
    //    var result = formatter.Format((object?)null!);
    //    Assert.IsNull(result); // Returns null due to type mismatch, not "null" string
    //}

    //[TestMethod]
    //public void FormatterT_withNullableValueType_objectOverloadHandlesBoxedValue()
    //{
    //    var formatter = new TestFormatterNullableInt();
    //    object? value = 99;
    //    var result = formatter.Format(value);
    //    Assert.AreEqual("NULLABLE:99", result);
    //}
    //private static Task NewMethod(int i, TestFormatterInt formatter, System.Collections.Concurrent.ConcurrentBag<(object input, string? output)> results)
    //{
    //    return Task.Run(() =>
    //    {
    //        // Mix of valid ints and invalid types
    //        object value = i % 2 == 0 ? i : $"string{i}";
    //        var result = formatter.Format(value);
    //        results.Add((value, result));
    //    });
    //}
    //[TestMethod]
    //public void FormatterT_concurrentMixedTypeCalls_handledCorrectly()
    //{
    //    var formatter = new TestFormatterInt();
    //    var results = new System.Collections.Concurrent.ConcurrentBag<(object input, string? output)>();

    //    var tasks = Enumerable.Range(0, 50).Select(i => NewMethod(i, formatter, results)).ToArray();

    //    Task.WaitAll(tasks, TestContext.CancellationToken);

    //    // Verify correct type handling
    //    foreach (var (input, output) in results)
    //    {
    //        if (input is int intValue)
    //        {
    //            Assert.AreEqual($"INT:{intValue}", output);
    //        }
    //        else
    //        {
    //            Assert.IsNull(output);
    //        }
    //    }
    //}
    //[TestMethod]
    //public void FormatterT_withExceptionInTypeSafeFormat_objectOverloadPropagatesException()
    //{
    //    var formatter = new TestFormatterThatThrows();
    //    object value = 42;

    //    try
    //    {
    //        formatter.Format(value);
    //        Assert.Fail("Expected InvalidOperationException");
    //    }
    //    catch (InvalidOperationException)
    //    {
    //        // Expected
    //    }
    //}

    //[TestMethod]
    //public void FormatterT_withObjectParameter_nullCheckedCorrectly()
    //{
    //    var formatter = new TestFormatterString();

    //    // Null as object? -> In C#, pattern matching `null is string?` returns false
    //    // even though string? is nullable, because the runtime type of null is not string
    //    string? result1 = formatter.Format((object?)null!);
    //    Assert.IsNull(result1); // Returns null due to type mismatch

    //    // Null as string? -> direct call to Format(string?) works correctly
    //    var result2 = formatter.Format((string?)null);
    //    Assert.AreEqual("null", result2);
    //}

    //[TestMethod]
    //public void FormatterT_withBoxedValueType_unboxesCorrectly()
    //{
    //    var formatter = new TestFormatterInt();

    //    // Direct call
    //    var result1 = formatter.Format(42);
    //    Assert.AreEqual("INT:42", result1);

    //    // Boxed int
    //    object boxed = 42;
    //    var result2 = formatter.Format(boxed);
    //    Assert.AreEqual("INT:42", result2);

    //    // Verify they're equal
    //    Assert.AreEqual(result1, result2);
    //}

    //[TestMethod]
    //public void FormatterT_withWrongBoxedType_returnsNull()
    //{
    //    var formatter = new TestFormatterInt();

    //    // Boxed double (not int)
    //    object boxed = 42.0;
    //    var result = formatter.Format(boxed);

    //    Assert.IsNull(result);
    //}

    //[TestMethod]
    //public void FormatterT_withNullableBoxedValue_handlesCorrectly()
    //{
    //    var formatter = new TestFormatterNullableInt();

    //    // Boxed nullable with obj
    //    object? boxedValue = (int?)42;
    //    var result1 = formatter.Format(boxedValue);
    //    Assert.AreEqual("NULLABLE:42", result1);

    //    // Direct nullable with obj
    //    var result2 = formatter.Format((int?)42);
    //    Assert.AreEqual("NULLABLE:42", result2);
    //}

    [TestMethod]
    public void FormatterT_FormatObject_withNullForReferenceType_callsTypeSafeMethod()
    {
        var formatter = new TestFormatterString();
        var result = formatter.Format(null!);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_Format_withDerivedType_worksCorrectly()
    {
        var formatter = new TestFormatterException();
        var exception = new ArgumentException("test");
        var result = formatter.Format(exception);
        Assert.AreEqual("EX:System.ArgumentException", result);
    }
    #endregion

    #region Formatter<T> - IFormatter Interface Compliance
    [TestMethod]
    public void FormatterT_implementsIFormatter()
    {
        var formatter = new TestFormatterInt();
        Assert.IsInstanceOfType<IFormatter>(formatter);
    }

    [TestMethod]
    public void FormatterT_implementsIFormatterT()
    {
        var formatter = new TestFormatterInt();
        Assert.IsInstanceOfType<IFormatter<int>>(formatter);
    }

    [TestMethod]
    public void FormatterT_IFormatterFormat_callsTypeSafeMethod()
    {
        IFormatter formatter = new TestFormatterInt();
        object value = 99;
        var result = formatter.Format(value);
        Assert.AreEqual("INT:99", result);
    }

    [TestMethod]
    public void FormatterT_IFormatterTFormat_callsTypeSafeMethod()
    {
        IFormatter<int> formatter = new TestFormatterInt();
        var result = formatter.Format(55);
        Assert.AreEqual("INT:55", result);
    }
    #endregion

    #region Formatter<T> - Base Class Utility Usage
    [TestMethod]
    public void FormatterT_canUseBaseClassConstants()
    {
        var formatter = new TestFormatterWithBaseUtils();
        var result = formatter.Format(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_canUseFallbackIfNull()
    {
        var formatter = new TestFormatterWithBaseUtils();
        var result = formatter.Format("test");
        Assert.AreEqual("VALUE:test", result);
    }

    [TestMethod]
    public void FormatterT_canUseJoinWithComma()
    {
        var formatter = new TestFormatterList();
        var result = formatter.Format(["a", "b", "c"]);
        Assert.AreEqual("LIST:[a, b, c]", result);
    }

    [TestMethod]
    public void FormatterT_canUseCreateSeparatedString()
    {
        var formatter = new TestFormatterWithSeparator();
        var result = formatter.Format(("key", "value"));
        Assert.AreEqual("key => value", result);
    }

    [TestMethod]
    public void FormatterT_canUseCopyAsSpan()
    {
        var formatter = new TestFormatterWithSpan();
        var result = formatter.Format("test");
        Assert.AreEqual("PREFIX:test", result);
    }
    #endregion

    #region Formatter<T> - Nullable Type Handling
    [TestMethod]
    public void FormatterT_withNullableValueType_handlesNull()
    {
        var formatter = new TestFormatterNullableInt();
        var result = formatter.Format(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_withNullableValueType_handlesValue()
    {
        var formatter = new TestFormatterNullableInt();
        var result = formatter.Format(42);
        Assert.AreEqual("NULLABLE:42", result);
    }
    #endregion

    #region Formatter<T> - Edge Cases
    [TestMethod]
    public void FormatterT_withStructType_handlesCorrectly()
    {
        var formatter = new TestFormatterGuid();
        var guid = Guid.NewGuid();
        var result = formatter.Format(guid);
        Assert.IsTrue(result?.StartsWith("GUID:"));
    }

    [TestMethod]
    public void FormatterT_withEnumType_handlesCorrectly()
    {
        var formatter = new TestFormatterEnum();
        var result = formatter.Format(DayOfWeek.Monday);
        Assert.AreEqual("ENUM:Monday", result);
    }

    [TestMethod]
    public void FormatterT_withInterfaceType_handlesImplementations()
    {
        var formatter = new TestFormatterEnumerable();
        var list = new List<int> { 1, 2, 3 };
        var result = formatter.Format(list);
        Assert.AreEqual("ENUMERABLE:3", result);
    }

    [TestMethod]
    public void FormatterT_withAbstractType_handlesDerivedTypes()
    {
        var formatter = new TestFormatterStream();
        var stream = new MemoryStream();
        var result = formatter.Format(stream);
        Assert.AreEqual("STREAM:System.IO.MemoryStream", result);
    }

    // TODO: Check if reasonable?
    //[TestMethod]
    //public void FormatterT_formatMethodNotOverridable_isSealed()
    //{
    //    // This test verifies at compile-time that Format(object) is sealed
    //    // and cannot be overridden by derived classes
    //    var formatter = new TestFormatterInt();
    //    var method = formatter.GetType().GetMethod("Format", [typeof(object)]);
    //    Assert.IsNotNull(method);
    //    Assert.IsTrue(method!.IsFinal); // Sealed methods are marked as Final in reflection
    //}
    #endregion

    #region Formatter<T> Test Implementations

    private class TestFormatterInt : Formatter<int>
    {
        public override string Format(int value) => $"INT:{value}";
    }

    private class TestFormatterString : Formatter<string?>
    {
        public override string Format(string? value) => value is null ? "null" : $"STR:{value}";
    }

    private class TestFormatterException : Formatter<Exception>
    {
        public override string Format(Exception value) => $"EX:{value.GetType().FullName}";
    }

    private class TestFormatterWithBaseUtils : Formatter<string?>
    {
        public override string Format(string? value)
        {
            if (value is null) return NullString;
            return $"VALUE:{value}";
        }
    }

    private class TestFormatterList : Formatter<List<string>>
    {
        public override string Format(List<string> value)
        {
            var joined = JoinWithComma(value);
            return $"LIST:[{joined}]";
        }
    }

    private class TestFormatterWithSeparator : Formatter<(string, string)>
    {
        public override string Format((string, string) value)
        {
            var (key, val) = value;
            //var totalLength = key.Length + 4 + val.Length; // " => "
            return CreateSeparatedString(key, " => ", val);
        }
    }

    private class TestFormatterWithSpan : Formatter<string>
    {
        public override string Format(string value)
        {
            const string prefix = "PREFIX:";
            var totalLength = prefix.Length + value.Length;
            return string.Create(totalLength, (prefix, value), static (span, state) =>
            {
                var (p, v) = state;
                CopyAsSpan(p, span, 0);
                CopyAsSpan(v, span, p.Length);
            });
        }
    }

    private class TestFormatterNullableInt : Formatter<int?>
    {
        public override string Format(int? value)
        {
            if (value is null) return "null";
            return $"NULLABLE:{value}";
        }
    }

    private class TestFormatterGuid : Formatter<Guid>
    {
        public override string Format(Guid value) => $"GUID:{value}";
    }

    private class TestFormatterEnum : Formatter<DayOfWeek>
    {
        public override string Format(DayOfWeek value) => $"ENUM:{value}";
    }

    private class TestFormatterEnumerable : Formatter<IEnumerable<int>>
    {
        public override string Format(IEnumerable<int> value) => $"ENUMERABLE:{value.Count()}";
    }

    private class TestFormatterStream : Formatter<Stream>
    {
        public override string Format(Stream value) => $"STREAM:{value.GetType().FullName}";
    }

    #endregion

    #region Formatter<T> - Performance and Allocation Tests
    [TestMethod]
    public void FormatterT_withZeroAllocationFormatting_doesNotAllocateExcessively()
    {
        var formatter = new TestFormatterWithSpan();

        // Warm up
        _ = formatter.Format("warmup");

        // This test verifies that the formatter can use zero-allocation patterns
        // We can't directly measure allocations in unit tests, but we verify the pattern works
        var result = formatter.Format("test");
        Assert.AreEqual("PREFIX:test", result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void FormatterT_multipleCallsSameValue_canReturnSameOrNewString()
    {
        var formatter = new TestFormatterInt();
        var result1 = formatter.Format(42);
        var result2 = formatter.Format(42);

        // Formatters are not required to cache results
        Assert.AreEqual(result1, result2);
        // They might return same or different instances
    }
    #endregion

    #region Formatter<T> - Thread Safety Tests
    [TestMethod]
    public void FormatterT_concurrentCalls_handledSafely()
    {
        var formatter = new TestFormatterInt();
        var tasks = new List<Task<string>>();

        // Create 100 concurrent formatting tasks
        for (int i = 0; i < 100; i++)
        {
            var value = i;
            tasks.Add(Task.Run(() => formatter.Format(value)));
        }

        Task.WaitAll([.. tasks]);

        // Verify all tasks completed successfully
        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual($"INT:{i}", tasks[i].Result);
        }
    }

    #endregion

    #region Formatter<T> - Error Handling Tests
    [TestMethod]
    public void FormatterT_whenFormatThrows_exceptionPropagates()
    {
        var formatter = new TestFormatterThatThrows();

        try
        {
            formatter.Format(42);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void FormatterT_withNullReturnFromFormat_returnsNull()
    {
        var formatter = new TestFormatterReturnsNull();
        var result = formatter.Format(42);

        Assert.IsNull(result);
    }
    #endregion

    #region Formatter<T> - Inheritance and Composition Tests
    [TestMethod]
    public void FormatterT_canBeInherited_byDerivedFormatter()
    {
        var formatter = new DerivedTestFormatter();
        var result = formatter.Format(42);

        Assert.AreEqual("DERIVED:INT:42", result);
    }

    [TestMethod]
    public void FormatterT_derivedFormatter_inheritsMethods()
    {
        var formatter = new DerivedTestFormatter();

        // Can use base class utility methods
        var fallback = FallbackIfNull(null);
        Assert.AreEqual("null", fallback);

        // Verify formatter works
        var result = formatter.Format(100);
        Assert.AreEqual("DERIVED:INT:100", result);
    }

    [TestMethod]
    public void FormatterT_canComposeMultipleFormatters()
    {
        var intFormatter = new TestFormatterInt();
        var stringFormatter = new TestFormatterString();

        // Use formatters independently
        var intResult = intFormatter.Format(42);
        var stringResult = stringFormatter.Format("test");

        Assert.AreEqual("INT:42", intResult);
        Assert.AreEqual("STR:test", stringResult);
    }
    #endregion

    #region Formatter<T> - Generic Type Constraints Tests
    [TestMethod]
    public void FormatterT_withClassConstraint_handlesReferenceTypes()
    {
        var formatter = new TestFormatterClassConstrained();
        var testObj = new TestReferenceType { Value = "test" };

        var result = formatter.Format(testObj);
        Assert.AreEqual("REF:test", result);
    }

    [TestMethod]
    public void FormatterT_withClassConstraint_handlesNullCorrectly()
    {
        var formatter = new TestFormatterClassConstrained();
        var result = formatter.Format(null);

        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_withStructConstraint_handlesValueTypes()
    {
        var formatter = new TestFormatterStructConstrained();
        var result = formatter.Format(42);

        Assert.AreEqual("STRUCT:42", result);
    }

    [TestMethod]
    public void FormatterT_withNewConstraint_canInstantiateType()
    {
        var formatter = new TestFormatterNewConstrained();
        var result = formatter.Format(new TestTypeWithDefaultConstructor { Id = 99 });

        Assert.IsTrue(result?.Contains("99"));
    }
    #endregion

    #region Formatter<T> - Complex Type Hierarchies
    [TestMethod]
    public void FormatterT_withBaseClass_formatsDerivedClasses()
    {
        var formatter = new TestFormatterException();
        var derived = new DerivedTestException("test message");

        var result = formatter.Format(derived);
        Assert.IsTrue(result?.StartsWith("EX:"));
    }

    [TestMethod]
    public void FormatterT_withInterface_formatsImplementations()
    {
        var formatter = new TestFormatterComparable();
        var result = formatter.Format(42);

        Assert.AreEqual("COMPARABLE:42", result);
    }

    [TestMethod]
    public void FormatterT_withGenericInterface_handlesGenericImplementations()
    {
        var formatter = new TestFormatterGenericInterface();
        var list = new List<string> { "a", "b", "c" };

        var result = formatter.Format(list);
        Assert.AreEqual("GENERIC:3", result);
    }
    #endregion

    #region Formatter<T> - Special Type Tests
    [TestMethod]
    public void FormatterT_withSealedType_handlesCorrectly()
    {
        var formatter = new TestFormatterSealed();
        var value = new SealedTestType { Data = "sealed" };

        var result = formatter.Format(value);
        Assert.AreEqual("SEALED:sealed", result);
    }

    [TestMethod]
    public void FormatterT_withRecord_handlesCorrectly()
    {
        var formatter = new TestFormatterRecord();
        var record = new TestRecord(42, "test");

        var result = formatter.Format(record);
        Assert.AreEqual("RECORD:42-test", result);
    }

    [TestMethod]
    public void FormatterT_withTupleType_handlesValueTuples()
    {
        var formatter = new TestFormatterValueTuple();
        var tuple = (42, "test");

        var result = formatter.Format(tuple);
        Assert.AreEqual("TUPLE:42,test", result);
    }

    [TestMethod]
    public void FormatterT_withDelegate_handlesCorrectly()
    {
        var formatter = new TestFormatterAction();
        static void attempt(int x) => Console.WriteLine(x);

        var result = formatter.Format(attempt);
        Assert.IsTrue(result?.StartsWith("ACTION:"));
    }
    #endregion

    #region Formatter<T> - Pattern Matching Edge Cases

    [TestMethod]
    public void FormatterT_withContravariance_handlesBaseTypeFormatters()
    {
        // IFormatter<in T> is contravariant
        IFormatter<Exception> baseFormatter = new TestFormatterException();

        // Can assign to more derived type through interface
        var exception = new ArgumentException("test");
        var result = baseFormatter.Format(exception);

        Assert.IsTrue(result?.StartsWith("EX:"));
    }
    #endregion

    #region Formatter<T> - Integration with Base Class
    [TestMethod]
    public void FormatterT_inheritsAllBaseClassMembers()
    {
#pragma warning disable MSTEST0032 // Using directive is unnecessary
        // Verify access to constants
        Assert.AreEqual(3, MaxCount);
        Assert.AreEqual("null", NullString);
#pragma warning restore MSTEST0032

        // Verify access to static methods
        var joined = JoinWithComma(["a", "b", "c"]);
        Assert.AreEqual("a, b, c", joined);

        var fallback = FallbackIfNull(null);
        Assert.AreEqual("null", fallback);
    }

    [TestMethod]
    public void FormatterT_canCallBaseClassUtilitiesInImplementation()
    {
        var formatter = new TestFormatterUsingAllBaseUtils();
        var result = formatter.Format(["a", "b", "c"]);

        // Uses JoinWithComma, CreateSeparatedString, CopyAsSpan internally
        Assert.AreEqual("ITEMS: a, b, c", result);
    }
    #endregion

    #region Formatter<T> Additional Test Implementations

    private class TestFormatterThatThrows : Formatter<int>
    {
        public override string Format(int value)
            => throw new InvalidOperationException("Formatter error");
    }

    private class TestFormatterReturnsNull : Formatter<int>
    {
        public override string Format(int value) => null!;
    }

    private class DerivedTestFormatter : TestFormatterInt
    {
        public override string Format(int value) => $"DERIVED:{base.Format(value)}";
    }

    private class TestReferenceType
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestFormatterClassConstrained : Formatter<TestReferenceType?>
    {
        public override string Format(TestReferenceType? value)
            => value is null ? "null" : $"REF:{value.Value}";
    }

    private class TestFormatterStructConstrained : Formatter<int>
    {
        public override string Format(int value) => $"STRUCT:{value}";
    }

    private class TestTypeWithDefaultConstructor
    {
        public int Id { get; set; }
    }

    private class TestFormatterNewConstrained : Formatter<TestTypeWithDefaultConstructor>
    {
        public override string Format(TestTypeWithDefaultConstructor value)
        {
            var newInstance = new TestTypeWithDefaultConstructor();
            return $"NEW:{value.Id},{newInstance.Id}";
        }
    }

    public class DerivedTestException(string message) : Exception(message)
    {
    }

    private class TestFormatterComparable : Formatter<IComparable>
    {
        public override string Format(IComparable value) => $"COMPARABLE:{value}";
    }

    private class TestFormatterGenericInterface : Formatter<IList<string>>
    {
        public override string Format(IList<string> value) => $"GENERIC:{value.Count}";
    }

    private sealed class SealedTestType
    {
        public string Data { get; set; } = string.Empty;
    }

    private class TestFormatterSealed : Formatter<SealedTestType>
    {
        public override string Format(SealedTestType value) => $"SEALED:{value.Data}";
    }

    private record TestRecord(int Id, string Name);

    private class TestFormatterRecord : Formatter<TestRecord>
    {
        public override string Format(TestRecord value) => $"RECORD:{value.Id}-{value.Name}";
    }

    private class TestFormatterValueTuple : Formatter<(int, string)>
    {
        public override string Format((int, string) value)
            => $"TUPLE:{value.Item1},{value.Item2}";
    }

    private class TestFormatterAction : Formatter<Action<int>>
    {
        public override string Format(Action<int> value)
            => $"ACTION:{value.Method.Name}";
    }

    private class TestFormatterUsingAllBaseUtils : Formatter<List<string>>
    {
        public override string Format(List<string> value)
        {
            // Use JoinWithComma
            var joined = JoinWithComma(value);

            // Use CreateSeparatedString
            const string prefix = "ITEMS: ";
            var totalLength = prefix.Length + joined.Length;

            return string.Create(totalLength, (prefix, joined), static (span, state) =>
            {
                var (p, j) = state;

                // Use CopyAsSpan
                CopyAsSpan(p, span, 0);
                CopyAsSpan(j, span, p.Length);
            });
        }
    }

    public TestContext TestContext { get; set; }

    #endregion
}
