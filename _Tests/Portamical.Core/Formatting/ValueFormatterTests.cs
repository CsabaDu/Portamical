// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Collections;
using Portamical.Core.Formatting;

namespace Tests.Portamical.Core.Formatting;

/// <summary>
/// Unit tests for <see cref="ValueFormatter"/> static formatting methods.
/// </summary>
[TestClass]
public class ValueFormatterTests
{
    #region Format(object?) - Null and Basic Types
    [TestMethod]
    public void Format_withNull_returnsNull()
    {
        var result = ValueFormatter.Format(null);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_withInt_returnsToString()
    {
        var result = ValueFormatter.Format(42);
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Format_withBool_returnsToString()
    {
        var result = ValueFormatter.Format(true);
        Assert.AreEqual("True", result);
    }

    [TestMethod]
    public void Format_withDouble_returnsToString()
    {
        var result = ValueFormatter.Format(3.14);
        Assert.IsNotNull(result);
        Assert.Contains("3", result);
        Assert.Contains("14", result);
    }
    #endregion

    #region Format(char)
    [TestMethod]
    public void Format_withChar_returnsSingleQuoted()
    {
        var result = ValueFormatter.Format('A');
        Assert.AreEqual("'A'", result);
    }

    [TestMethod]
    public void Format_withCharEscapeSequence_returnsSingleQuotedEscape()
    {
        var result = ValueFormatter.Format('\n');
        Assert.AreEqual("'\n'", result);
    }

    [TestMethod]
    public void Format_withCharUnicode_returnsSingleQuoted()
    {
        var result = ValueFormatter.Format('\u0041');
        Assert.AreEqual("'A'", result);
    }
    #endregion

    #region Format(string)
    [TestMethod]
    public void Format_withString_returnsDoubleQuoted()
    {
        var result = ValueFormatter.Format("hello");
        Assert.AreEqual("\"hello\"", result);
    }

    [TestMethod]
    public void Format_withEmptyString_returnsDoubleQuotedEmpty()
    {
        var result = ValueFormatter.Format("");
        Assert.AreEqual("\"\"", result);
    }

    [TestMethod]
    public void Format_withStringNull_returnsUnquotedNull()
    {
        var result = ValueFormatter.Format("null");
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void Format_withStringWithSpaces_returnsDoubleQuoted()
    {
        var result = ValueFormatter.Format("hello world");
        Assert.AreEqual("\"hello world\"", result);
    }
    #endregion

    #region Format(DateTime)
    [TestMethod]
    public void Format_withDateTimeUtc_returnsIso8601()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var result = ValueFormatter.Format(dt);
        Assert.AreEqual("2026-01-15T10:30:45.0000000Z", result);
    }

    [TestMethod]
    public void Format_withDateTimeLocal_returnsIso8601WithLocalOffset()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 45, DateTimeKind.Local);
        var result = ValueFormatter.Format(dt);
        Assert.IsNotNull(result);
        Assert.StartsWith("2026-01-15T10:30:45", result);
    }

    [TestMethod]
    public void Format_withDateTimeUnspecified_returnsIso8601()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 45, DateTimeKind.Unspecified);
        var result = ValueFormatter.Format(dt);
        Assert.AreEqual("2026-01-15T10:30:45.0000000", result);
    }
    #endregion

    #region Format(DateTimeOffset)
    [TestMethod]
    public void Format_withDateTimeOffset_returnsIso8601WithOffset()
    {
        var dto = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.FromHours(5));
        var result = ValueFormatter.Format(dto);
        Assert.AreEqual("2026-01-15T10:30:45.0000000+05:00", result);
    }

    [TestMethod]
    public void Format_withDateTimeOffsetNegativeOffset_returnsIso8601WithOffset()
    {
        var dto = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.FromHours(-5));
        var result = ValueFormatter.Format(dto);
        Assert.AreEqual("2026-01-15T10:30:45.0000000-05:00", result);
    }

    [TestMethod]
    public void Format_withDateTimeOffsetZeroOffset_returnsIso8601WithZ()
    {
        var dto = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var result = ValueFormatter.Format(dto);
        Assert.AreEqual("2026-01-15T10:30:45.0000000+00:00", result);
    }
    #endregion

    #region Format(Guid)
    [TestMethod]
    public void Format_withGuid_returnsHyphenatedFormat()
    {
        var guid = new Guid("12345678-1234-1234-1234-123456789012");
        var result = ValueFormatter.Format(guid);
        Assert.AreEqual("12345678-1234-1234-1234-123456789012", result);
    }

    [TestMethod]
    public void Format_withGuidEmpty_returnsZeroGuid()
    {
        var guid = Guid.Empty;
        var result = ValueFormatter.Format(guid);
        Assert.AreEqual("00000000-0000-0000-0000-000000000000", result);
    }
    #endregion

    #region Format(byte[])
    [TestMethod]
    public void Format_withByteArray_returnsHexString()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var result = ValueFormatter.Format(bytes);
        Assert.AreEqual("01-02-03-FF", result);
    }

    [TestMethod]
    public void Format_withEmptyByteArray_returnsEmptyString()
    {
        var bytes = Array.Empty<byte>();
        var result = ValueFormatter.Format(bytes);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void Format_withSingleByte_returnsHexString()
    {
        var bytes = new byte[] { 0xAB };
        var result = ValueFormatter.Format(bytes);
        Assert.AreEqual("AB", result);
    }
    #endregion

    #region Format(Exception)
    [TestMethod]
    public void Format_withExceptionMessage_returnsTypeAndMessage()
    {
        var ex = new InvalidOperationException("Operation failed");
        var result = ValueFormatter.Format(ex);
        Assert.AreEqual("InvalidOperationException: Operation failed", result);
    }

    [TestMethod]
    public void Format_withExceptionEmptyMessage_returnsTypeAndEmptyMessage()
    {
        var ex = new InvalidOperationException("");
        var result = ValueFormatter.Format(ex);
        Assert.AreEqual("InvalidOperationException: ", result);
    }

    [TestMethod]
    public void Format_withArgumentException_returnsTypeAndMessage()
    {
        var paramName = "paramName";
        var ex = new ArgumentException("Value cannot be null", paramName);
        var result = ValueFormatter.Format(ex);
        Assert.StartsWith("ArgumentException:", result);
        Assert.Contains("Value cannot be null", result);
    }
    #endregion

    #region Format(KeyValuePair)
    [TestMethod]
    public void Format_withKeyValuePairStringInt_returnsFormattedPair()
    {
        var kvp = new KeyValuePair<string, int>("key1", 42);
        var result = ValueFormatter.Format(kvp);
        Assert.AreEqual("{\"key1\": 42}", result);
    }

    [TestMethod]
    public void Format_withKeyValuePairIntString_returnsFormattedPair()
    {
        var kvp = new KeyValuePair<int, string>(1, "first");
        var result = ValueFormatter.Format(kvp);
        Assert.AreEqual("{1: \"first\"}", result);
    }

    [TestMethod]
    public void Format_withKeyValuePairNullValue_returnsFormattedPairWithNull()
    {
        var kvp = new KeyValuePair<string, string?>("key", null);
        var result = ValueFormatter.Format(kvp);
        Assert.AreEqual("{\"key\": null}", result);
    }
    #endregion

    #region Format(ITuple) - Tuple and ValueTuple
    [TestMethod]
    public void Format_withValueTupleTwoItems_returnsParenthesizedItems()
    {
        var tuple = (1, 2);
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(1, 2)", result);
    }

    [TestMethod]
    public void Format_withValueTupleThreeItems_returnsParenthesizedItems()
    {
        var tuple = (1, 2, 3);
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(1, 2, 3)", result);
    }

    [TestMethod]
    public void Format_withValueTupleMixedTypes_returnsParenthesizedFormatted()
    {
        var tuple = ("name", 42, true);
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(\"name\", 42, True)", result);
    }

    [TestMethod]
    public void Format_withTuple_returnsParenthesizedItems()
    {
        var tuple = Tuple.Create(1, "test");
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(1, \"test\")", result);
    }

    [TestMethod]
    public void Format_withTupleChar_returnsParenthesizedFormatted()
    {
        var tuple = Tuple.Create('a', "test");
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("('a', \"test\")", result);
    }

    [TestMethod]
    public void Format_withValueTupleSingleItem_returnsParenthesizedItem()
    {
        var tuple = ValueTuple.Create(42);
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(42)", result);
    }
    #endregion

    #region Format(IEnumerable) - Collections
    [TestMethod]
    public void Format_withEmptyCollection_returnsZeroCount()
    {
        var list = new List<int>();
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[0]: []", result);
    }

    [TestMethod]
    public void Format_withEmptyArray_returnsZeroCount()
    {
        var array = Array.Empty<int>();
        var result = ValueFormatter.Format(array);
        Assert.AreEqual("[0]: []", result);
    }

    [TestMethod]
    public void Format_withSingleItemCollection_returnsCountAndItem()
    {
        var list = new List<int> { 42 };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[1]: [42]", result);
    }

    [TestMethod]
    public void Format_withSingleNullItemCollection_returnsCountAndNull()
    {
        var list = new List<string?> { null };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[1]: [null]", result);
    }

    [TestMethod]
    public void Format_withTwoItemCollection_returnsCountAndBothItems()
    {
        var list = new List<int> { 1, 2 };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[2]: [1, 2]", result);
    }

    [TestMethod]
    public void Format_withThreeItemCollection_returnsAllItems()
    {
        var list = new List<int> { 1, 2, 3 };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[3]: [1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_withFourItemCollection_returnsFirstThree()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[First 3 of 4+]: [1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_withManyItemCollection_returnsFirstThree()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[First 3 of 4+]: [1, 2, 3]", result);
    }

    [TestMethod]
    public void Format_withStringCollection_returnsQuotedItems()
    {
        var list = new List<string> { "a", "b", "c" };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[3]: [\"a\", \"b\", \"c\"]", result);
    }

    [TestMethod]
    public void Format_withCharCollection_returnsSingleQuotedItems()
    {
        var list = new List<char> { 'x', 'y', 'z' };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[3]: ['x', 'y', 'z']", result);
    }

    [TestMethod]
    public void Format_withCollectionContainingNull_replacesWithNull()
    {
        var list = new List<string?> { "a", null, "c" };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[3]: [\"a\", null, \"c\"]", result);
    }

    [TestMethod]
    public void Format_withCollectionOfTwoNulls_formatsBothAsNull()
    {
        var list = new List<string?> { null, null };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[2]: [null, null]", result);
    }

    [TestMethod]
    public void Format_withCollectionOfThreeNulls_formatsAllAsNull()
    {
        var list = new List<string?> { null, null, null };
        var result = ValueFormatter.Format(list);
        Assert.AreEqual("[3]: [null, null, null]", result);
    }
    #endregion

    #region JoinWithComma Edge Cases - Empty vs Single Null
    [TestMethod]
    public void Format_emptyCollection_distinguishedFromSingleNullElement()
    {
        var emptyList = new List<string?>();
        var singleNullList = new List<string?> { null };

        var emptyResult = ValueFormatter.Format(emptyList);
        var nullResult = ValueFormatter.Format(singleNullList);

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
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("()", result);
    }

    [TestMethod]
    public void Format_tupleWithSingleNull_returnsNullInParens()
    {
        var tuple = ValueTuple.Create<string?>(null);
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(null)", result);
    }

    [TestMethod]
    public void Format_tupleWithTwoNulls_returnsNullsInParens()
    {
        var tuple = (default(string), default(string));
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(null, null)", result);
    }

    [TestMethod]
    public void Format_tupleWithThreeNulls_returnsNullsInParens()
    {
        var tuple = (default(string), default(string), default(string));
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(null, null, null)", result);
    }
    #endregion

    #region Format(IDictionary) - Dictionary
    [TestMethod]
    public void Format_withEmptyDictionary_returnsZeroCount()
    {
        var dict = new Dictionary<string, int>();
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[0]: {}", result);
    }

    [TestMethod]
    public void Format_withSingleItemDictionary_returnsKeyValuePair()
    {
        var dict = new Dictionary<string, int> { ["key1"] = 42 };
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[1]: {{\"key1\": 42}}", result);
    }

    [TestMethod]
    public void Format_withTwoItemDictionary_returnsAllPairs()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[2]: {{\"a\": 1}, {\"b\": 2}}", result);
    }

    [TestMethod]
    public void Format_withThreeItemDictionary_returnsAllPairs()
    {
        var dict = new Dictionary<string, int> { ["x"] = 10, ["y"] = 20, ["z"] = 30 };
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[3]: {{\"x\": 10}, {\"y\": 20}, {\"z\": 30}}", result);
    }

    [TestMethod]
    public void Format_withFourItemDictionary_returnsFirstThreePairs()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["c"] = 3, ["d"] = 4 };
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[First 3 of 4+]: {{\"a\": 1}, {\"b\": 2}, {\"c\": 3}}", result);
    }

    [TestMethod]
    public void Format_withDictionaryStringValues_formatsValuesWithQuotes()
    {
        var dict = new Dictionary<int, string> { [1] = "hello", [2] = "world" };
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[2]: {{1: \"hello\"}, {2: \"world\"}}", result);
    }

    [TestMethod]
    public void Format_withDictionaryMixedStringValues_formatsCorrectly()
    {
        var dict = new Dictionary<int, string> { [1] = "value", [2] = "", [3] = "null" };
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[3]: {{1: \"value\"}, {2: \"\"}, {3: null}}", result);
    }
    #endregion

    #region Format(IDictionary) - Hashtable
    [TestMethod]
    public void Format_withHashtable_formatsWithKeyValuePairs()
    {
        var hashtable = new Hashtable { ["key1"] = 100, ["key2"] = 200 };
        var result = ValueFormatter.Format(hashtable);
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
        var result = ValueFormatter.Format(hashtable);
        Assert.IsNotNull(result);
        Assert.StartsWith("[First 3 of 4+]:", result);
    }

    [TestMethod]
    public void Format_withHashtableNullValue_formatsWithNull()
    {
        var hashtable = new Hashtable { ["key1"] = null, ["key2"] = 42 };
        var result = ValueFormatter.Format(hashtable);
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
        var result = ValueFormatter.Format(stream);
        Assert.AreEqual("MemoryStream (Length: 5, Position: 0)", result);
    }

    [TestMethod]
    public void Format_withMemoryStreamAtPosition_formatsWithLengthAndPosition()
    {
        var stream = new MemoryStream([1, 2, 3, 4, 5]) { Position = 3 };
        var result = ValueFormatter.Format(stream);
        Assert.AreEqual("MemoryStream (Length: 5, Position: 3)", result);
    }

    [TestMethod]
    public void Format_withEmptyMemoryStream_formatsWithZeroLength()
    {
        var stream = new MemoryStream();
        var result = ValueFormatter.Format(stream);
        Assert.AreEqual("MemoryStream (Length: 0, Position: 0)", result);
    }

    [TestMethod]
    public void Format_withDisposedStream_returnsNull()
    {
        var stream = new MemoryStream();
        stream.Dispose();
        var result = ValueFormatter.Format(stream);
        Assert.IsNull(result);
    }
    #endregion

    #region Format(Type)
    [TestMethod]
    public void Format_withIntType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(int));
        Assert.AreEqual("int", result);
    }

    [TestMethod]
    public void Format_withStringType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(string));
        Assert.AreEqual("string", result);
    }

    [TestMethod]
    public void Format_withBoolType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(bool));
        Assert.AreEqual("bool", result);
    }

    [TestMethod]
    public void Format_withGenericList_returnsGenericNotation()
    {
        var result = ValueFormatter.Format(typeof(List<string>));
        Assert.AreEqual("List<string>", result);
    }

    [TestMethod]
    public void Format_withGenericDictionary_returnsGenericNotation()
    {
        var result = ValueFormatter.Format(typeof(Dictionary<string, int>));
        Assert.AreEqual("Dictionary<string, int>", result);
    }

    [TestMethod]
    public void Format_withNestedGeneric_returnsNestedNotation()
    {
        var result = ValueFormatter.Format(typeof(Dictionary<string, List<int>>));
        Assert.AreEqual("Dictionary<string, List<int>>", result);
    }

    [TestMethod]
    public void Format_withNullableInt_returnsNullableSyntax()
    {
        var result = ValueFormatter.Format(typeof(int?));
        Assert.AreEqual("int?", result);
    }

    [TestMethod]
    public void Format_withIntArray_returnsArrayNotation()
    {
        var result = ValueFormatter.Format(typeof(int[]));
        Assert.AreEqual("int[]", result);
    }

    [TestMethod]
    public void Format_withMultiDimensionalArray_returnsArrayNotation()
    {
        var result = ValueFormatter.Format(typeof(int[,]));
        Assert.AreEqual("int[,]", result);
    }

    [TestMethod]
    public void Format_withThreeDimensionalArray_returnsArrayNotation()
    {
        var result = ValueFormatter.Format(typeof(int[,,]));
        Assert.AreEqual("int[,,]", result);
    }

    [TestMethod]
    public void Format_withGenericNullable_returnsNullableSyntax()
    {
        var result = ValueFormatter.Format(typeof(List<int?>));
        Assert.AreEqual("List<int?>", result);
    }
    #endregion

    #region Complex Scenarios
    [TestMethod]
    public void Format_withNestedCollections_formatsRecursively()
    {
        var list = new List<List<int>> { new() { 1, 2 }, new() { 3, 4 } };
        var result = ValueFormatter.Format(list);
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
        var result = ValueFormatter.Format(dict);
        Assert.AreEqual("[2]: {{\"a\": [2]: [1, 2]}, {\"b\": [2]: [3, 4]}}", result);
    }

    [TestMethod]
    public void Format_withTupleContainingCollection_formatsRecursively()
    {
        var tuple = ("list", new List<int> { 1, 2, 3 });
        var result = ValueFormatter.Format(tuple);
        Assert.AreEqual("(\"list\", [3]: [1, 2, 3])", result);
    }
    #endregion

    #region Additional Type Coverage
    [TestMethod]
    public void Format_withDecimalType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(decimal));
        Assert.AreEqual("decimal", result);
    }

    [TestMethod]
    public void Format_withFloatType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(float));
        Assert.AreEqual("float", result);
    }

    [TestMethod]
    public void Format_withLongType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(long));
        Assert.AreEqual("long", result);
    }

    [TestMethod]
    public void Format_withByteType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(byte));
        Assert.AreEqual("byte", result);
    }

    [TestMethod]
    public void Format_withShortType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(short));
        Assert.AreEqual("short", result);
    }

    [TestMethod]
    public void Format_withObjectType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(object));
        Assert.AreEqual("object", result);
    }

    [TestMethod]
    public void Format_withVoidType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(void));
        Assert.AreEqual("void", result);
    }

    [TestMethod]
    public void Format_withCharType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(char));
        Assert.AreEqual("char", result);
    }

    [TestMethod]
    public void Format_withDoubleType_returnsAlias()
    {
        var result = ValueFormatter.Format(typeof(double));
        Assert.AreEqual("double", result);
    }

    [TestMethod]
    public void Format_withCustomType_returnsTypeName()
    {
        var result = ValueFormatter.Format(typeof(ValueFormatterTests));
        Assert.AreEqual("ValueFormatterTests", result);
    }
    #endregion

    #region Edge Cases
    [TestMethod]
    public void Format_withVeryLongString_returnsQuoted()
    {
        var longString = new string('a', 1000);
        var result = ValueFormatter.Format(longString);
        Assert.StartsWith("\"", result);
        Assert.EndsWith("\"", result);
        Assert.AreEqual(1002, result!.Length); // 1000 chars + 2 quotes
    }

    [TestMethod]
    public void Format_withObjectWithCustomToString_returnsToStringResult()
    {
        var obj = new CustomObject();
        var result = ValueFormatter.Format(obj);
        Assert.AreEqual("CustomObject", result);
    }

    [TestMethod]
    public void Format_withEnumValue_returnsEnumName()
    {
        var result = ValueFormatter.Format(DayOfWeek.Monday);
        Assert.AreEqual("Monday", result);
    }

    [TestMethod]
    public void Format_withNullableHasValue_returnsValue()
    {
        int? nullable = 42;
        var result = ValueFormatter.Format(nullable);
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Format_withNullableNoValue_returnsNull()
    {
        int? nullable = null;
        var result = ValueFormatter.Format(nullable);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_withJaggedArray_returnsArrayNotation()
    {
        var result = ValueFormatter.Format(typeof(int[][]));
        Assert.AreEqual("int[][]", result);
    }

    [TestMethod]
    public void Format_withComplexGenericType_returnsNestedGenerics()
    {
        var result = ValueFormatter.Format(typeof(Dictionary<string, List<Dictionary<int, string>>>));
        Assert.AreEqual("Dictionary<string, List<Dictionary<int, string>>>", result);
    }

    [TestMethod]
    public void Format_withArrayOfNullable_returnsArrayNotation()
    {
        var result = ValueFormatter.Format(typeof(int?[]));
        Assert.AreEqual("int?[]", result);
    }

    private class CustomObject
    {
        public override string ToString() => "CustomObject";
    }
    #endregion
}
