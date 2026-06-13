// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;
using Portamical.Core.Formatting.Model;

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
        Assert.AreEqual("null", Formatter.NullString);
    }

    [TestMethod]
    public void MaxCount_hasCorrectValue()
    {
        Assert.AreEqual(3, Formatter.MaxCount);
    }
#pragma warning restore MSTEST0032
    #endregion

    #region FallbackIfNull
    [TestMethod]
    public void FallbackIfNull_withNull_returnsNullString()
    {
        var result = Formatter.FallbackIfNull(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FallbackIfNull_withEmptyString_returnsEmptyString()
    {
        var result = Formatter.FallbackIfNull("");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void FallbackIfNull_withNonNullString_returnsOriginal()
    {
        var input = "test";
        var result = Formatter.FallbackIfNull(input);
        Assert.AreEqual("test", result);
        Assert.AreSame(input, result);
    }

    [TestMethod]
    public void FallbackIfNull_withWhitespace_returnsOriginal()
    {
        var result = Formatter.FallbackIfNull("   ");
        Assert.AreEqual("   ", result);
    }
    #endregion

    #region CopyAsSpan
    [TestMethod]
    public void CopyAsSpan_copiesStringToSpan()
    {
        var buffer = new char[10];
        var span = new Span<char>(buffer);

        Formatter.CopyAsSpan("hello", span, 0);

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

        Formatter.CopyAsSpan("abc", span, 0);
        Formatter.CopyAsSpan("xyz", span, 5);

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

        Formatter.CopyAsSpan("", span, 2);

        Assert.AreEqual('x', buffer[0]);
        Assert.AreEqual('x', buffer[1]);
        Assert.AreEqual('x', buffer[2]);
    }
    #endregion

    #region CreateSeparatedString
    [TestMethod]
    public void CreateSeparatedString_concatenatesThreeParts()
    {
        var result = Formatter.CreateSeparatedString(11, "Hello", " ", "World");
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptyBase_concatenatesCorrectly()
    {
        // "" (0) + ", " (2) + "test" (4) = 6 total
        var result = Formatter.CreateSeparatedString(6, "", ", ", "test");
        Assert.AreEqual(", test", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptyAppendix_concatenatesCorrectly()
    {
        // "base" (4) + ": " (2) + "" (0) = 6 total
        var result = Formatter.CreateSeparatedString(6, "base", ": ", "");
        Assert.AreEqual("base: ", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptySeparator_concatenatesCorrectly()
    {
        // "test" (4) + "" (0) + "case" (4) = 8 total
        var result = Formatter.CreateSeparatedString(8, "test", "", "case");
        Assert.AreEqual("testcase", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withAllEmptyStrings_returnsEmptyString()
    {
        // "" (0) + "" (0) + "" (0) = 0 total
        var result = Formatter.CreateSeparatedString(0, "", "", "");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withComplexSeparator_concatenatesCorrectly()
    {
        // "Method" (6) + " - " (3) + "param1" (6) = 15 total
        var result = Formatter.CreateSeparatedString(15, "Method", " - ", "param1");
        Assert.AreEqual("Method - param1", result);
    }
    #endregion

    #region JoinWithComma - Empty and Single Item
    [TestMethod]
    public void JoinWithComma_withEmptyList_returnsEmptyString()
    {
        var items = new List<string?>();
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void JoinWithComma_withEmptyArray_returnsEmptyString()
    {
        var items = Array.Empty<string?>();
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void JoinWithComma_withSingleItem_returnsItem()
    {
        var items = new List<string?> { "test" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("test", result);
    }

    [TestMethod]
    public void JoinWithComma_withSingleNull_returnsNull()
    {
        var items = new List<string?> { null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("null", result);
    }
    #endregion

    #region JoinWithComma - Two Items
    [TestMethod]
    public void JoinWithComma_withTwoItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "first", "second" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("first, second", result);
    }

    [TestMethod]
    public void JoinWithComma_withTwoItemsFirstNull_returnsCommaSeparated()
    {
        var items = new List<string?> { null, "second" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("null, second", result);
    }

    [TestMethod]
    public void JoinWithComma_withTwoItemsSecondNull_returnsCommaSeparated()
    {
        var items = new List<string?> { "first", null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("first, null", result);
    }

    [TestMethod]
    public void JoinWithComma_withTwoNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { null, null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("null, null", result);
    }
    #endregion

    #region JoinWithComma - Three Items
    [TestMethod]
    public void JoinWithComma_withThreeItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "one", "two", "three" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("one, two, three", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsFirstNull_returnsCommaSeparated()
    {
        var items = new List<string?> { null, "two", "three" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("null, two, three", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsSecondNull_returnsCommaSeparated()
    {
        var items = new List<string?> { "one", null, "three" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("one, null, three", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsThirdNull_returnsCommaSeparated()
    {
        var items = new List<string?> { "one", "two", null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("one, two, null", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { null, null, null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("null, null, null", result);
    }

    [TestMethod]
    public void JoinWithComma_withThreeItemsMixedNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { null, "middle", null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("null, middle, null", result);
    }
    #endregion

    #region JoinWithComma - Four or More Items
    [TestMethod]
    public void JoinWithComma_withFourItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "a", "b", "c", "d" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("a, b, c, d", result);
    }

    [TestMethod]
    public void JoinWithComma_withManyItems_returnsCommaSeparated()
    {
        var items = new List<string?> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("1, 2, 3, 4, 5, 6, 7, 8, 9, 10", result);
    }

    [TestMethod]
    public void JoinWithComma_withFourItemsContainingNulls_returnsCommaSeparated()
    {
        var items = new List<string?> { "a", null, "c", null };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("a, null, c, null", result);
    }
    #endregion

    #region JoinWithComma - Non-List Collections
    [TestMethod]
    public void JoinWithComma_withArray_returnsCommaSeparated()
    {
        var items = new[] { "x", "y", "z" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("x, y, z", result);
    }

    [TestMethod]
    public void JoinWithComma_withEnumerable_returnsCommaSeparated()
    {
        var items = Enumerable.Range(1, 5).Select(i => i.ToString());
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("1, 2, 3, 4, 5", result);
    }

    [TestMethod]
    public void JoinWithComma_withHashSet_returnsCommaSeparated()
    {
        var items = new HashSet<string?> { "alpha", "beta", "gamma" };
        var result = Formatter.JoinWithComma(items);
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

        var result1 = Formatter.JoinWithComma(emptyCollection);
        var result2 = Formatter.JoinWithComma(singleEmpty);
        var result3 = Formatter.JoinWithComma(twoEmpties);

        Assert.AreEqual("", result1);
        Assert.AreEqual("", result2);
        Assert.AreEqual(", ", result3);
    }

    [TestMethod]
    public void JoinWithComma_withQuotedStrings_preservesQuotes()
    {
        var items = new List<string?> { "\"hello\"", "'a'", "42" };
        var result = Formatter.JoinWithComma(items);
        Assert.AreEqual("\"hello\", 'a', 42", result);
    }

    [TestMethod]
    public void JoinWithComma_withLongStrings_joinsCorrectly()
    {
        var long1 = new string('a', 100);
        var long2 = new string('b', 100);
        var items = new List<string?> { long1, long2 };
        var result = Formatter.JoinWithComma(items);

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
    private class TestFormatter : Formatter
    {
        public override string? Format(object value)
        {
            if (value == null) return null;
            return value.ToString()?.ToUpper();
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

    [TestMethod]
    public void FormatterT_FormatObject_withMatchingType_callsTypeSafeMethod()
    {
        var formatter = new TestFormatterInt();
        object value = 42;
        var result = formatter.Format(value);
        Assert.AreEqual("INT:42", result);
    }

    [TestMethod]
    public void FormatterT_FormatObject_withNonMatchingType_returnsNull()
    {
        var formatter = new TestFormatterInt();
        object value = "not an int";
        var result = formatter.Format(value);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FormatterT_FormatObject_withNullForValueType_returnsNull()
    {
        var formatter = new TestFormatterInt();
        var result = formatter.Format(null!);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void FormatterT_FormatObject_withNullForReferenceType_callsTypeSafeMethod()
    {
        var formatter = new TestFormatterString();
        var result = formatter.Format(null!);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_FormatObject_withBoxedValueType_unboxesAndFormats()
    {
        var formatter = new TestFormatterInt();
        object boxed = 123;
        var result = formatter.Format(boxed);
        Assert.AreEqual("INT:123", result);
    }

    [TestMethod]
    public void FormatterT_Format_withDerivedType_worksCorrectly()
    {
        var formatter = new TestFormatterException();
        var exception = new ArgumentException("test");
        var result = formatter.Format(exception);
        Assert.AreEqual("EX:System.ArgumentException", result);
    }

    [TestMethod]
    public void FormatterT_FormatObject_withDerivedType_worksCorrectly()
    {
        var formatter = new TestFormatterException();
        object exception = new InvalidOperationException("test");
        var result = formatter.Format(exception);
        Assert.AreEqual("EX:System.InvalidOperationException", result);
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

    [TestMethod]
    public void FormatterT_withNullableValueType_objectOverloadHandlesNull()
    {
        // When null is passed as object?, the pattern matching fails for nullable value types
        // because null (object?) is not the same as null (int?)
        var formatter = new TestFormatterNullableInt();
        var result = formatter.Format((object?)null!);
        Assert.IsNull(result); // Returns null due to type mismatch, not "null" string
    }

    [TestMethod]
    public void FormatterT_withNullableValueType_objectOverloadHandlesBoxedValue()
    {
        var formatter = new TestFormatterNullableInt();
        object? value = 99;
        var result = formatter.Format(value);
        Assert.AreEqual("NULLABLE:99", result);
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

    [TestMethod]
    public void FormatterT_formatMethodNotOverridable_isSealed()
    {
        // This test verifies at compile-time that Format(object) is sealed
        // and cannot be overridden by derived classes
        var formatter = new TestFormatterInt();
        var method = formatter.GetType().GetMethod("Format", [typeof(object)]);
        Assert.IsNotNull(method);
        Assert.IsTrue(method!.IsFinal); // Sealed methods are marked as Final in reflection
    }
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
            var totalLength = key.Length + 4 + val.Length; // " => "
            return CreateSeparatedString(totalLength, key, " => ", val);
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
}
