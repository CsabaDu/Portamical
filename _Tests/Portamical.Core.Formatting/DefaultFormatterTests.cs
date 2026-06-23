// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;
using System.Collections;

namespace Tests.Portamical.Core.Formatting;

/// <summary>
/// Unit tests for <see cref="DefaultFormatter"/> static formatting methods.
/// </summary>
[TestClass]
[DoNotParallelize] // Registry is a shared static resource; tests must run sequentially
public class DefaultFormatterTests
{
    private sealed class NullToStringObject
    {
        public override string ToString() => null!;
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Ensure registry is clean after each test to prevent test interference
        Formatter.ClearFormatters();
    }

    #region Format(object?) - Null and Basic Types
    [TestMethod]
    public void Format_withNull_returnsNull()
    {
        var result = DefaultFormatter.Format(null);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_withNullToString_returnsNull()
    {
        var result = DefaultFormatter.Format(new NullToStringObject());
        Assert.IsNull(result);
    }


    [TestMethod]
    public void Format_withNotDefinedObject_returnsToString()
    {
        var result = DefaultFormatter.Format(new object());
        Assert.AreEqual("System.Object", result);
    }

    #endregion

    #region Format(object?) - Custom Registry Formatters
    [TestMethod]
    public void Format_withEmptyRegistry_usesDefaultFormatting()
    {
        // Arrange: Registry.Count should be 0 initially
        var obj = new CustomType { Value = 42 };

        // Act
        var result = DefaultFormatter.Format(obj);

        // Assert: Should use default ToString()
        Assert.AreEqual("CustomType:42", result);
    }

    [TestMethod]
    public void Format_withRegisteredFormatter_usesCustomFormatter()
    {
        // Arrange: Register a custom formatter using public API
        var customFormatter = new CustomTypeFormatter();

        try
        {
            Formatter.RegisterFormatter<CustomType>(customFormatter);

            var obj = new CustomType { Value = 42 };

            // Act
            var result = Formatter.Format(obj);

            // Assert: Should use custom formatter
            Assert.AreEqual("Custom:42", result);
        }
        finally
        {
            // Cleanup: Remove the registered formatter
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Format_withRegisteredFormatterReturningNull_returnsNull()
    {
        // Arrange
        var nullFormatter = new NullReturningFormatter();

        try
        {
            Formatter.RegisterFormatter<CustomType>(nullFormatter);
            var obj = new CustomType { Value = 42 };

            // Act
            var result = Formatter.Format(obj);

            // Assert: Custom formatter returns null
            Assert.IsNull(result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Format_withRegistryCountZero_skipsRegistryLookup()
    {
        // Arrange: Ensure registry is empty
        Formatter.ClearFormatters();

        var obj = new CustomType { Value = 99 };

        // Act
        var result = DefaultFormatter.Format(obj);

        // Assert: Should use default ToString() without registry lookup
        Assert.AreEqual("CustomType:99", result);
    }

    [TestMethod]
    public void Format_withUnregisteredType_fallsBackToDefaultFormatting()
    {
        // Arrange: Register a formatter for one type
        var customFormatter = new CustomTypeFormatter();

        try
        {
            Formatter.RegisterFormatter<CustomType>(customFormatter);

            var obj = new AnotherCustomType { Name = "Test" };

            // Act
            var result = DefaultFormatter.Format(obj);

            // Assert: Should fall back to default ToString()
            Assert.AreEqual("Another:Test", result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Format_withMultipleRegisteredTypes_usesCorrectFormatter()
    {
        // Arrange
        var customFormatter = new CustomTypeFormatter();
        var anotherFormatter = new AnotherTypeFormatter();

        try
        {
            Formatter.RegisterFormatter<CustomType>(customFormatter);
            Formatter.RegisterFormatter<AnotherCustomType>(anotherFormatter);

            var obj1 = new CustomType { Value = 42 };
            var obj2 = new AnotherCustomType { Name = "Test" };

            // Act
            var result1 = Formatter.Format(obj1);
            var result2 = Formatter.Format(obj2);

            // Assert
            Assert.AreEqual("Custom:42", result1);
            Assert.AreEqual("Another_Custom:Test", result2);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
            Formatter.UnregisterFormatter<AnotherCustomType>();
        }
    }

    [TestMethod]
    public void Format_withRegisteredFormatterForBuiltInType_overridesDefault()
    {
        // Arrange: Register a custom formatter for string
        var stringFormatter = new CustomStringFormatter();

        try
        {
            Formatter.RegisterFormatter<string>(stringFormatter);

            // Act
            var result = Formatter.Format("hello");

            // Assert: Should use custom formatter instead of default double-quoting
            Assert.AreEqual("[hello]", result);
        }
        finally
        {
            Formatter.UnregisterFormatter<string>();
        }
    }

    // Test helper types
    private class CustomType
    {
        public int Value { get; set; }
        public override string ToString() => $"CustomType:{Value}";
    }

    private class AnotherCustomType
    {
        public string Name { get; set; } = string.Empty;
        public override string ToString() => $"Another:{Name}";
    }

    private class CustomTypeFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is CustomType custom)
                return $"Custom:{custom.Value}";
            return null;
        }
    }

    private class AnotherTypeFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is AnotherCustomType another)
                return $"Another_Custom:{another.Name}";
            return null;
        }
    }

    private class NullReturningFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj) => null;
    }

    private class CustomStringFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is string str)
                return $"[{str}]";
            return null;
        }
    }
    #endregion

    #region Format(object?) - Basic Types
    [TestMethod]
    public void Format_withInt_returnsToString()
    {
        var result = DefaultFormatter.Format(42);
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Format_withBool_returnsToString()
    {
        var result = DefaultFormatter.Format(true);
        Assert.AreEqual("True", result);
    }

    [TestMethod]
    public void Format_withDouble_returnsToString()
    {
        var result = DefaultFormatter.Format(3.14);
        Assert.IsNotNull(result);
        Assert.Contains("3", result);
        Assert.Contains("14", result);
    }
    #endregion

    #region Format(char)
    [TestMethod]
    public void Format_withChar_returnsSingleQuoted()
    {
        var result = DefaultFormatter.Format('A');
        Assert.AreEqual("'A'", result);
    }

    [TestMethod]
    public void Format_withCharEscapeSequence_returnsSingleQuotedEscape()
    {
        var result = DefaultFormatter.Format('\n');
        Assert.AreEqual("'\n'", result);
    }

    [TestMethod]
    public void Format_withCharUnicode_returnsSingleQuoted()
    {
        var result = DefaultFormatter.Format('\u0041');
        Assert.AreEqual("'A'", result);
    }
    #endregion

    #region Format(string)
    [TestMethod]
    public void Format_withString_returnsDoubleQuoted()
    {
        var result = DefaultFormatter.Format("hello");
        Assert.AreEqual("\"hello\"", result);
    }

    [TestMethod]
    public void Format_withEmptyString_returnsDoubleQuotedEmpty()
    {
        var result = DefaultFormatter.Format("");
        Assert.AreEqual("\"\"", result);
    }

    [TestMethod]
    public void Format_withStringNull_returnsUnquotedNull()
    {
        var result = DefaultFormatter.Format("null");
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void Format_withStringWithSpaces_returnsDoubleQuoted()
    {
        var result = DefaultFormatter.Format("hello world");
        Assert.AreEqual("\"hello world\"", result);
    }
    #endregion

    #region Format(DateTime)
    [TestMethod]
    public void Format_withDateTimeUtc_returnsIso8601()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var result = DefaultFormatter.Format(dt);
        Assert.AreEqual("2026-01-15T10:30:45.0000000Z", result);
    }

    [TestMethod]
    public void Format_withDateTimeLocal_returnsIso8601WithLocalOffset()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 45, DateTimeKind.Local);
        var result = DefaultFormatter.Format(dt);
        Assert.IsNotNull(result);
        Assert.StartsWith("2026-01-15T10:30:45", result);
    }

    [TestMethod]
    public void Format_withDateTimeUnspecified_returnsIso8601()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 45, DateTimeKind.Unspecified);
        var result = DefaultFormatter.Format(dt);
        Assert.AreEqual("2026-01-15T10:30:45.0000000", result);
    }
    #endregion

    #region Format(DateTimeOffset)
    [TestMethod]
    public void Format_withDateTimeOffset_returnsIso8601WithOffset()
    {
        var dto = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.FromHours(5));
        var result = DefaultFormatter.Format(dto);
        Assert.AreEqual("2026-01-15T10:30:45.0000000+05:00", result);
    }

    [TestMethod]
    public void Format_withDateTimeOffsetNegativeOffset_returnsIso8601WithOffset()
    {
        var dto = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.FromHours(-5));
        var result = DefaultFormatter.Format(dto);
        Assert.AreEqual("2026-01-15T10:30:45.0000000-05:00", result);
    }

    [TestMethod]
    public void Format_withDateTimeOffsetZeroOffset_returnsIso8601WithZ()
    {
        var dto = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var result = DefaultFormatter.Format(dto);
        Assert.AreEqual("2026-01-15T10:30:45.0000000+00:00", result);
    }
    #endregion

    #region Format(Guid)
    [TestMethod]
    public void Format_withGuid_returnsHyphenatedFormat()
    {
        var guid = new Guid("12345678-1234-1234-1234-123456789012");
        var result = DefaultFormatter.Format(guid);
        Assert.AreEqual("12345678-1234-1234-1234-123456789012", result);
    }

    [TestMethod]
    public void Format_withGuidEmpty_returnsZeroGuid()
    {
        var guid = Guid.Empty;
        var result = DefaultFormatter.Format(guid);
        Assert.AreEqual("00000000-0000-0000-0000-000000000000", result);
    }
    #endregion

    #region Format(byte[])
    [TestMethod]
    public void Format_withByteArray_returnsHexString()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var result = DefaultFormatter.Format(bytes);
        Assert.AreEqual("01-02-03-FF", result);
    }

    [TestMethod]
    public void Format_withEmptyByteArray_returnsEmptyString()
    {
        var bytes = Array.Empty<byte>();
        var result = DefaultFormatter.Format(bytes);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void Format_withSingleByte_returnsHexString()
    {
        var bytes = new byte[] { 0xAB };
        var result = DefaultFormatter.Format(bytes);
        Assert.AreEqual("AB", result);
    }
    #endregion

    #region Format(Exception)
    [TestMethod]
    public void Format_withExceptionMessage_returnsTypeAndMessage()
    {
        var ex = new InvalidOperationException("Operation failed");
        var result = DefaultFormatter.Format(ex);
        Assert.AreEqual("InvalidOperationException: Operation failed", result);
    }

    [TestMethod]
    public void Format_withExceptionEmptyMessage_returnsTypeAndEmptyMessage()
    {
        var ex = new InvalidOperationException("");
        var result = DefaultFormatter.Format(ex);
        Assert.AreEqual("InvalidOperationException: ", result);
    }

    [TestMethod]
    public void Format_withArgumentException_returnsTypeAndMessage()
    {
        var paramName = "paramName";
        var ex = new ArgumentException("Value cannot be null", paramName);
        var result = DefaultFormatter.Format(ex);
        Assert.StartsWith("ArgumentException:", result);
        Assert.Contains("Value cannot be null", result);
    }
    #endregion

    #region Format(Delegate)
    [TestMethod]
    public void Format_withAnonymousLambda_returnsTypeAndAnonymous()
    {
        Func<int, string> func = x => x.ToString();
        var result = DefaultFormatter.Format(func);
        Assert.Contains("Func<int, string>", result!);
        Assert.Contains("anonymous", result!);
    }

    [TestMethod]
    public void Format_withNamedMethodReference_returnsTypeAndMethodName()
    {
        Action<string> action = Console.WriteLine;
        var result = DefaultFormatter.Format(action);
        Assert.Contains("Action<string>", result!);
        Assert.Contains("WriteLine", result!);
        Assert.DoesNotContain("anonymous", result!);
    }

    [TestMethod]
    public void Format_withSimpleActionLambda_returnsTypeAndAnonymous()
    {
        Action simple = () => Console.WriteLine("test");
        var result = DefaultFormatter.Format(simple);
        Assert.Contains("Action", result!);
        Assert.Contains("anonymous", result!);
    }

    [TestMethod]
    public void Format_withFuncNamedMethod_returnsTypeAndMethodName()
    {
        Func<int, string> func = ConvertIntToString;
        var result = DefaultFormatter.Format(func);
        Assert.Contains("Func<int, string>", result!);
        Assert.Contains("ConvertIntToString", result!);
        Assert.DoesNotContain("anonymous", result!);
    }

    [TestMethod]
    public void Format_withPredicateLambda_returnsTypeAndAnonymous()
    {
        Predicate<int> pred = x => x > 0;
        var result = DefaultFormatter.Format(pred);
        Assert.Contains("Predicate<int>", result!);
        Assert.Contains("anonymous", result!);
    }

    [TestMethod]
    public void Format_withPredicateNamedMethod_returnsTypeAndMethodName()
    {
        Predicate<int> pred = IsPositive;
        var result = DefaultFormatter.Format(pred);
        Assert.Contains("Predicate<int>", result!);
        Assert.Contains("IsPositive", result!);
        Assert.DoesNotContain("anonymous", result!);
    }

    [TestMethod]
    public void Format_withMulticastDelegate_returnsTypeAndMethodName()
    {
        Action action = TestMethod;
        var result = DefaultFormatter.Format(action);
        Assert.Contains("Action", result!);
        Assert.Contains("TestMethod", result!);
    }

    [TestMethod]
    public void Format_withActionOfTwoParams_returnsTypeAndAnonymous()
    {
        Action<int, string> action = (x, s) => Console.WriteLine($"{x}: {s}");
        var result = DefaultFormatter.Format(action);
        Assert.Contains("Action<int, string>", result!);
        Assert.Contains("anonymous", result!);
    }

    [TestMethod]
    public void Format_withFuncOfThreeParams_returnsTypeAndAnonymous()
    {
        Func<int, string, bool, string> func = (x, s, b) => $"{x}-{s}-{b}";
        var result = DefaultFormatter.Format(func);
        Assert.Contains("Func<int, string, bool, string>", result!);
        Assert.Contains("anonymous", result!);
    }

    [TestMethod]
    public void Format_withCustomDelegate_returnsTypeAndMethodName()
    {
        Comparison<int> comparison = CompareInts;
        var result = DefaultFormatter.Format(comparison);
        Assert.Contains("Comparison<int>", result!);
        Assert.Contains("CompareInts", result!);
    }

    [TestMethod]
    public void Format_withEventHandler_returnsTypeAndAnonymous()
    {
        EventHandler handler = (sender, e) => Console.WriteLine("Event fired");
        var result = DefaultFormatter.Format(handler);
        Assert.Contains("EventHandler", result!);
        Assert.Contains("anonymous", result!);
    }

    [TestMethod]
    public void Format_withLocalFunction_returnsTypeAndAnonymous()
    {
        static string LocalFunc(int x) => x.ToString();
        Func<int, string> func = LocalFunc;
        var result = DefaultFormatter.Format(func);
        Assert.Contains("Func<int, string>", result!);
        // Local functions are compiler-generated and appear as anonymous
        Assert.Contains("anonymous", result!);
    }


    // Helper methods for delegate tests
    private static bool IsPositive(int x) => x > 0;
    private static void TestMethod()
    {
        // Intentionally empty - used as a named method reference for delegate formatting tests
    }
    private static string ConvertIntToString(int x) => x.ToString();
    private static int CompareInts(int x, int y) => x.CompareTo(y);
    #endregion

    #region Format(KeyValuePair)
    [TestMethod]
    public void Format_withKeyValuePairStringInt_returnsFormattedPair()
    {
        var kvp = new KeyValuePair<string, int>("key1", 42);
        var result = DefaultFormatter.Format(kvp);
        Assert.AreEqual("{\"key1\": 42}", result);
    }

    [TestMethod]
    public void Format_withKeyValuePairIntString_returnsFormattedPair()
    {
        var kvp = new KeyValuePair<int, string>(1, "first");
        var result = DefaultFormatter.Format(kvp);
        Assert.AreEqual("{1: \"first\"}", result);
    }

    [TestMethod]
    public void Format_withKeyValuePairNullValue_returnsFormattedPairWithNull()
    {
        var kvp = new KeyValuePair<string, string?>("key", null);
        var result = DefaultFormatter.Format(kvp);
        Assert.AreEqual("{\"key\": null}", result);
    }

    [TestMethod]
    public void Format_withKeyValuePairBothNull_returnsFormattedPairWithNulls()
    {
        // Arrange: Create a KeyValuePair with both null key and value (nullable reference types)
        var kvp = new KeyValuePair<string?, string?>(null, null);

        // Act
        var result = DefaultFormatter.Format(kvp);

        // Assert: Should format both as null
        Assert.AreEqual("{null: null}", result);
    }

    [TestMethod]
    public void GetKvpPropValues_withTypeMissingProperties_returnsNulls()
    {
        // Arrange: Use reflection to invoke the private GetKvpPropValues method
        // with a type that doesn't have Key/Value properties to test the null-safety on line 825
        var method = typeof(DefaultFormatter).GetMethod(
            "GetKvpPropValues",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.IsNotNull(method, "GetKvpPropValues method should exist");

        // Create an object with a type that doesn't have Key or Value properties
        var testObject = new { SomeProperty = "test" };
        var testType = testObject.GetType();

        // Act: Invoke the private method
        var result = method.Invoke(null, new object[] { testType, testObject });

        // Assert: Should return (null, null) because GetProperty returns null for non-existent properties
        Assert.IsNotNull(result);
        var tuple = ((object? key, object? value))result;
        Assert.IsNull(tuple.key, "Key should be null when property doesn't exist");
        Assert.IsNull(tuple.value, "Value should be null when property doesn't exist");
    }
    #endregion

    #region Format(ITuple) - Tuple and ValueTuple
    [TestMethod]
    public void Format_withValueTupleTwoItems_returnsParenthesizedItems()
    {
        var tuple = (1, 2);
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(1, 2)", result);
    }

    [TestMethod]
    public void Format_withValueTupleThreeItems_returnsParenthesizedItems()
    {
        var tuple = (1, 2, 3);
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(1, 2, 3)", result);
    }

    [TestMethod]
    public void Format_withValueTupleMixedTypes_returnsParenthesizedFormatted()
    {
        var tuple = ("name", 42, true);
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(\"name\", 42, True)", result);
    }

    [TestMethod]
    public void Format_withTuple_returnsParenthesizedItems()
    {
        var tuple = Tuple.Create(1, "test");
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(1, \"test\")", result);
    }

    [TestMethod]
    public void Format_withTupleChar_returnsParenthesizedFormatted()
    {
        var tuple = Tuple.Create('a', "test");
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("('a', \"test\")", result);
    }

    [TestMethod]
    public void Format_withValueTupleSingleItem_returnsParenthesizedItem()
    {
        var tuple = ValueTuple.Create(42);
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(42)", result);
    }
    #endregion

    #region Format(IEnumerable) - Collections
    [TestMethod]
    public void Format_withEmptyCollection_returnsZeroCount()
    {
        var list = new List<int>();
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[0]: []", result);
    }

    [TestMethod]
    public void Format_withEmptyArray_returnsZeroCount()
    {
        var array = Array.Empty<int>();
        var result = DefaultFormatter.Format(array);
        Assert.AreEqual("[0]: []", result);
    }

    [TestMethod]
    public void Format_withSingleItemCollection_returnsCountAndItem()
    {
        var list = new List<int> { 42 };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[1]: [42]", result);
    }

    [TestMethod]
    public void Format_withSingleNullItemCollection_returnsCountAndNull()
    {
        var list = new List<string?> { null };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[1]: [null]", result);
    }

    [TestMethod]
    public void Format_withTwoItemCollection_returnsCountAndBothItems()
    {
        var list = new List<int> { 1, 2 };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[2]: [1, 2]", result);
    }

    [TestMethod]
    public void Format_withThreeItemCollection_returnsAllItems()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[3]: [1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_withFourItemCollection_returnsFirstThree()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[First 3 of 4+]: [1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_withManyItemCollection_returnsFirstThree()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[First 3 of 4+]: [1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_withStringCollection_returnsQuotedItems()
    {
        var list = new List<string> { "a", "b", "c" };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[3]: [\"a\", \"b\", \"c\"]", result);
    }

    [TestMethod]
    public void Format_withCharCollection_returnsSingleQuotedItems()
    {
        var list = new List<char> { 'x', 'y', 'z' };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[3]: ['x', 'y', 'z']", result);
    }

    [TestMethod]
    public void Format_withCollectionContainingNull_replacesWithNull()
    {
        var list = new List<string?> { "a", null, "c" };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[3]: [\"a\", null, \"c\"]", result);
    }

    [TestMethod]
    public void Format_withCollectionOfTwoNulls_formatsBothAsNull()
    {
        var list = new List<string?> { null, null };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[2]: [null, null]", result);
    }

    [TestMethod]
    public void Format_withCollectionOfThreeNulls_formatsAllAsNull()
    {
        var list = new List<string?> { null, null, null };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[3]: [null, null, null]", result);
    }
    #endregion

    #region JoinWithComma Edge Cases - Empty vs Single Null
    [TestMethod]
    public void Format_emptyCollection_distinguishedFromSingleNullElement()
    {
        var emptyList = new List<string?>();
        var singleNullList = new List<string?> { null };

        var emptyResult = DefaultFormatter.Format(emptyList);
        var nullResult = DefaultFormatter.Format(singleNullList);

        // Empty collection returns empty brackets
        Assert.AreEqual("[0]: []", emptyResult);

        // Single null element returns "null" inside brackets
        Assert.AreEqual("[1]: [null]", nullResult);

        // Verify they are different
        Assert.AreNotEqual(emptyResult, nullResult);
    }

    [TestMethod]
    public void Format_emptyTuple_returnsEmptyParens()
    {
        var tuple = ValueTuple.Create();
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("()", result);
    }

    [TestMethod]
    public void Format_tupleWithSingleNull_returnsNullInParens()
    {
        var tuple = ValueTuple.Create<string?>(null);
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(null)", result);
    }

    [TestMethod]
    public void Format_tupleWithTwoNulls_returnsNullsInParens()
    {
        var tuple = (default(string), default(string));
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(null, null)", result);
    }

    [TestMethod]
    public void Format_tupleWithThreeNulls_returnsNullsInParens()
    {
        var tuple = (default(string), default(string), default(string));
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(null, null, null)", result);
    }
    #endregion

    #region Format(IDictionary) - Dictionary
    [TestMethod]
    public void Format_withEmptyDictionary_returnsZeroCount()
    {
        var dict = new Dictionary<string, int>();
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[0]: {}", result);
    }

    [TestMethod]
    public void Format_withSingleItemDictionary_returnsKeyValuePair()
    {
        var dict = new Dictionary<string, int> { ["key1"] = 42 };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[1]: {{\"key1\": 42}}", result);
    }

    [TestMethod]
    public void Format_withTwoItemDictionary_returnsAllPairs()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[2]: {{\"a\": 1}, {\"b\": 2}}", result);
    }

    [TestMethod]
    public void Format_withThreeItemDictionary_returnsAllPairs()
    {
        var dict = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20, ["z"] = 30 };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[3]: {{\"x\": 10}, {\"y\": 20}, {\"z\": 30}}", result);
    }

    [TestMethod]
    public void Format_withFourItemDictionary_returnsFirstThreePairs()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3, ["d"] = 4 };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[First 3 of 4+]: {{\"a\": 1}, {\"b\": 2}, {\"c\": 3}}", result);
    }

    [TestMethod]
    public void Format_withDictionaryStringValues_formatsValuesWithQuotes()
    {
        var dict = new Dictionary<int, string> { [1] = "hello", [2] = "world" };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[2]: {{1: \"hello\"}, {2: \"world\"}}", result);
    }

    [TestMethod]
    public void Format_withDictionaryMixedStringValues_formatsCorrectly()
    {
        var dict = new Dictionary<int, string> { [1] = "value", [2] = "", [3] = "null" };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[3]: {{1: \"value\"}, {2: \"\"}, {3: null}}", result);
    }
    #endregion

    #region Format(IDictionary) - Hashtable
    [TestMethod]
    public void Format_withHashtable_formatsWithKeyValuePairs()
    {
        var hashtable = new Hashtable { ["key1"] = 100, ["key2"] = 200 };
        var result = DefaultFormatter.Format(hashtable);
        Assert.IsNotNull(result);
        var expected1 = "[2]: {{\"key1\": 100}, {\"key2\": 200}}";
        var expected2 = "[2]: {{\"key2\": 200}, {\"key1\": 100}}";
        Assert.IsTrue(result == expected1 || result == expected2,
            $"Expected one of '{expected1}' or '{expected2}', but got '{result}'");
    }

    [TestMethod]
    public void Format_withHashtableManyItems_formatsWithFirstThree()
    {
        var hashtable = new Hashtable { ["a"] = 1, ["b"] = 2, ["c"] = 3, ["d"] = 4, ["e"] = 5 };
        var result = DefaultFormatter.Format(hashtable);
        Assert.IsNotNull(result);
        Assert.StartsWith("[First 3 of 4+]:", result);
    }

    [TestMethod]
    public void Format_withHashtableNullValue_formatsWithNull()
    {
        var hashtable = new Hashtable { ["key1"] = null, ["key2"] = 42 };
        var result = DefaultFormatter.Format(hashtable);
        Assert.IsNotNull(result);
        Assert.Contains("null", result);
        Assert.Contains("42", result);
    }
    #endregion

    #region Format(Stream)
    [TestMethod]
    public void Format_withMemoryStream_formatsWithLengthAndPosition()
    {
        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var result = DefaultFormatter.Format(stream);
        Assert.AreEqual("MemoryStream (Length: 5, Position: 0)", result);
    }

    [TestMethod]
    public void Format_withMemoryStreamAtPosition_formatsWithLengthAndPosition()
    {
        var stream = new MemoryStream([1, 2, 3, 4, 5]) { Position = 3 };
        var result = DefaultFormatter.Format(stream);
        Assert.AreEqual("MemoryStream (Length: 5, Position: 3)", result);
    }

    [TestMethod]
    public void Format_withEmptyMemoryStream_formatsWithZeroLength()
    {
        var stream = new MemoryStream();
        var result = DefaultFormatter.Format(stream);
        Assert.AreEqual("MemoryStream (Length: 0, Position: 0)", result);
    }

    [TestMethod]
    public void Format_withDisposedStream_returnsNull()
    {
        var stream = new MemoryStream();
        stream.Dispose();
        var result = DefaultFormatter.Format(stream);
        Assert.IsNull(result);
    }
    #endregion

    #region Format(Type)
    [TestMethod]
    public void Format_withIntType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(int));
        Assert.AreEqual("int", result);
    }

    [TestMethod]
    public void Format_withStringType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(string));
        Assert.AreEqual("string", result);
    }

    [TestMethod]
    public void Format_withBoolType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(bool));
        Assert.AreEqual("bool", result);
    }

    [TestMethod]
    public void Format_withGenericList_returnsGenericNotation()
    {
        var result = DefaultFormatter.Format(typeof(List<string>));
        Assert.AreEqual("List<string>", result);
    }

    [TestMethod]
    public void Format_withGenericDictionary_returnsGenericNotation()
    {
        var result = DefaultFormatter.Format(typeof(Dictionary<string, int>));
        Assert.AreEqual("Dictionary<string, int>", result);
    }

    [TestMethod]
    public void Format_withNestedGeneric_returnsNestedNotation()
    {
        var result = DefaultFormatter.Format(typeof(Dictionary<string, List<int>>));
        Assert.AreEqual("Dictionary<string, List<int>>", result);
    }

    [TestMethod]
    public void Format_withNullableInt_returnsNullableSyntax()
    {
        var result = DefaultFormatter.Format(typeof(int?));
        Assert.AreEqual("int?", result);
    }

    [TestMethod]
    public void Format_withIntArray_returnsArrayNotation()
    {
        var result = DefaultFormatter.Format(typeof(int[]));
        Assert.AreEqual("int[]", result);
    }

    [TestMethod]
    public void Format_withMultiDimensionalArray_returnsArrayNotation()
    {
        var result = DefaultFormatter.Format(typeof(int[,]));
        Assert.AreEqual("int[,]", result);
    }

    [TestMethod]
    public void Format_withThreeDimensionalArray_returnsArrayNotation()
    {
        var result = DefaultFormatter.Format(typeof(int[,,]));
        Assert.AreEqual("int[,,]", result);
    }

    [TestMethod]
    public void Format_withGenericNullable_returnsNullableSyntax()
    {
        var result = DefaultFormatter.Format(typeof(List<int?>));
        Assert.AreEqual("List<int?>", result);
    }
    #endregion

    #region Complex Scenarios
    [TestMethod]
    public void Format_withNestedCollections_formatsRecursively()
    {
        var list = new List<List<int>> { new() { 1, 2 }, new() { 3, 4 } };
        var result = DefaultFormatter.Format(list);
        Assert.AreEqual("[2]: [[2]: [1, 2], [2]: [3, 4]]", result);
    }

    [TestMethod]
    public void Format_withDictionaryOfLists_formatsRecursively()
    {
        var dict = new Dictionary<string, List<int>>
        {
            ["a"] = [1, 2],
            ["b"] = [3, 4]
        };
        var result = DefaultFormatter.Format(dict);
        Assert.AreEqual("[2]: {{\"a\": [2]: [1, 2]}, {\"b\": [2]: [3, 4]}}", result);
    }

    [TestMethod]
    public void Format_withTupleContainingCollection_formatsRecursively()
    {
        var tuple = ("list", new List<int> { 1, 2, 3 });
        var result = DefaultFormatter.Format(tuple);
        Assert.AreEqual("(\"list\", [3]: [1, 2, 3])", result);
    }
    #endregion

    #region Additional Type Coverage
    [TestMethod]
    public void Format_withDecimalType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(decimal));
        Assert.AreEqual("decimal", result);
    }

    [TestMethod]
    public void Format_withFloatType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(float));
        Assert.AreEqual("float", result);
    }

    [TestMethod]
    public void Format_withLongType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(long));
        Assert.AreEqual("long", result);
    }

    [TestMethod]
    public void Format_withByteType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(byte));
        Assert.AreEqual("byte", result);
    }

    [TestMethod]
    public void Format_withSbyteType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(sbyte));
        Assert.AreEqual("sbyte", result);
    }

    [TestMethod]
    public void Format_withUintType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(uint));
        Assert.AreEqual("uint", result);
    }

    [TestMethod]
    public void Format_withUlongType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(ulong));
        Assert.AreEqual("ulong", result);
    }

    [TestMethod]
    public void Format_withUshortType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(ushort));
        Assert.AreEqual("ushort", result);
    }

    [TestMethod]
    public void Format_withShortType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(short));
        Assert.AreEqual("short", result);
    }

    [TestMethod]
    public void Format_withObjectType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(object));
        Assert.AreEqual("object", result);
    }

    [TestMethod]
    public void Format_withVoidType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(void));
        Assert.AreEqual("void", result);
    }

    [TestMethod]
    public void Format_withCharType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(char));
        Assert.AreEqual("char", result);
    }

    [TestMethod]
    public void Format_withDoubleType_returnsAlias()
    {
        var result = DefaultFormatter.Format(typeof(double));
        Assert.AreEqual("double", result);
    }

    [TestMethod]
    public void Format_withCustomType_returnsTypeName()
    {
        var result = DefaultFormatter.Format(typeof(DefaultFormatterTests));
        Assert.AreEqual("DefaultFormatterTests", result);
    }
    #endregion

    #region Format(Stream)
    [TestMethod]
    public void Format_withSeekableStream_returnsTypeNameLengthAndPosition()
    {
        // Arrange: Create a seekable stream (MemoryStream)
        using var stream = new MemoryStream(new byte[1024]);
        stream.Position = 0;

        // Act
        var result = DefaultFormatter.Format(stream);

        // Assert: Should include type name, length, and position
        Assert.AreEqual("MemoryStream (Length: 1024, Position: 0)", result);
    }

    [TestMethod]
    public void Format_withSeekableStreamNonZeroPosition_returnsTypeNameLengthAndPosition()
    {
        // Arrange: Create a seekable stream with non-zero position
        using var stream = new MemoryStream(new byte[2048]);
        stream.Position = 512;

        // Act
        var result = DefaultFormatter.Format(stream);

        // Assert: Should include type name, length, and current position
        Assert.AreEqual("MemoryStream (Length: 2048, Position: 512)", result);
    }

    [TestMethod]
    public void Format_withNonSeekableStream_returnsTypeNameAndPosition()
    {
        // Arrange: Create a non-seekable stream (NetworkStream simulation)
        using var nonSeekableStream = new NonSeekableStream();

        // Act
        var result = DefaultFormatter.Format(nonSeekableStream);

        // Assert: Should include only type name and position (no length)
        Assert.AreEqual("NonSeekableStream (Position: 0)", result);
    }
    #endregion

    #region Edge Cases
    [TestMethod]
    public void Format_withVeryLongString_returnsQuoted()
    {
        var longString = new string('a', 1000);
        var result = DefaultFormatter.Format(longString);
        Assert.StartsWith("\"", result);
        Assert.EndsWith("\"", result);
        Assert.AreEqual(1002, result!.Length); // 1000 chars + 2 quotes
    }

    [TestMethod]
    public void Format_withObjectWithCustomToString_returnsToStringResult()
    {
        var obj = new CustomObject();
        var result = DefaultFormatter.Format(obj);
        Assert.AreEqual("CustomObject", result);
    }

    [TestMethod]
    public void Format_withEnumValue_returnsEnumName()
    {
        var result = DefaultFormatter.Format(DayOfWeek.Monday);
        Assert.AreEqual("Monday", result);
    }

    [TestMethod]
    public void Format_withNullableHasValue_returnsValue()
    {
        int? nullable = 42;
        var result = DefaultFormatter.Format(nullable);
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Format_withNullableNoValue_returnsNull()
    {
        int? nullable = null;
        var result = DefaultFormatter.Format(nullable);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_withJaggedArray_returnsArrayNotation()
    {
        var result = DefaultFormatter.Format(typeof(int[][]));
        Assert.AreEqual("int[][]", result);
    }

    [TestMethod]
    public void Format_withComplexGenericType_returnsNestedGenerics()
    {
        var result = DefaultFormatter.Format(typeof(Dictionary<string, List<Dictionary<int, string>>>));
        Assert.AreEqual("Dictionary<string, List<Dictionary<int, string>>>", result);
    }

    [TestMethod]
    public void Format_withArrayOfNullable_returnsArrayNotation()
    {
        var result = DefaultFormatter.Format(typeof(int?[]));
        Assert.AreEqual("int?[]", result);
    }

    private class CustomObject
    {
        public override string ToString() => "CustomObject";
    }

    /// <summary>
    /// A test stream that simulates a non-seekable stream (e.g., NetworkStream).
    /// </summary>
    private sealed class NonSeekableStream : Stream
    {
        private long _position;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException("Stream does not support seeking.");

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException("Stream does not support seeking.");
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException("Stream does not support seeking.");

        public override void SetLength(long value) =>
            throw new NotSupportedException("Stream does not support seeking.");

        public override void Write(byte[] buffer, int offset, int count)
        {
            _position += count;
        }
    }
    #endregion
}
