// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;
using static Portamical.Core.Formatting.Builder;

namespace Tests.Portamical.Core.Formatting;

/// <summary>
/// Unit tests for <see cref="Builder"/> static utility methods and constants.
/// </summary>
[TestClass]
public class BuilderTests
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

    [TestMethod]
    public void FallbackIfNull_withSpecialCharacters_returnsOriginal()
    {
        var input = "\n\t\r";
        var result = FallbackIfNull(input);
        Assert.AreEqual("\n\t\r", result);
        Assert.AreSame(input, result);
    }

    [TestMethod]
    public void FallbackIfNull_withUnicodeCharacters_returnsOriginal()
    {
        var input = "Hello \u4E2D\u6587";
        var result = FallbackIfNull(input);
        Assert.AreEqual("Hello \u4E2D\u6587", result);
        Assert.AreSame(input, result);
    }
    #endregion

    #region FallbackIfNullSeparator
    [TestMethod]
    public void FallbackIfNullSeparator_withNull_returnsCommaSeparator()
    {
        var result = FallbackIfNullSeparator(null);
        Assert.AreEqual(", ", result);
    }

    [TestMethod]
    public void FallbackIfNullSeparator_withEmptyString_returnsEmptyString()
    {
        var result = FallbackIfNullSeparator("");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void FallbackIfNullSeparator_withNonNullString_returnsOriginal()
    {
        var input = " | ";
        var result = FallbackIfNullSeparator(input);
        Assert.AreEqual(" | ", result);
        Assert.AreSame(input, result);
    }

    [TestMethod]
    public void FallbackIfNullSeparator_withWhitespace_returnsOriginal()
    {
        var result = FallbackIfNullSeparator("   ");
        Assert.AreEqual("   ", result);
    }

    [TestMethod]
    public void FallbackIfNullSeparator_withCustomSeparator_returnsOriginal()
    {
        var input = "; ";
        var result = FallbackIfNullSeparator(input);
        Assert.AreEqual("; ", result);
        Assert.AreSame(input, result);
    }

    [TestMethod]
    public void FallbackIfNullSeparator_withNoSpace_returnsOriginal()
    {
        var input = ",";
        var result = FallbackIfNullSeparator(input);
        Assert.AreEqual(",", result);
        Assert.AreSame(input, result);
    }

    [TestMethod]
    public void FallbackIfNullSeparator_withNewline_returnsOriginal()
    {
        var input = "\n";
        var result = FallbackIfNullSeparator(input);
        Assert.AreEqual("\n", result);
        Assert.AreSame(input, result);
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

    [TestMethod]
    public void CopyAsSpan_withSingleCharacter_copiesCorrectly()
    {
        var buffer = new char[5];
        var span = new Span<char>(buffer);

        CopyAsSpan("A", span, 2);

        Assert.AreEqual('\0', buffer[0]);
        Assert.AreEqual('\0', buffer[1]);
        Assert.AreEqual('A', buffer[2]);
    }

    [TestMethod]
    public void CopyAsSpan_withExactFit_copiesCorrectly()
    {
        var buffer = new char[3];
        var span = new Span<char>(buffer);

        CopyAsSpan("abc", span, 0);

        Assert.AreEqual('a', buffer[0]);
        Assert.AreEqual('b', buffer[1]);
        Assert.AreEqual('c', buffer[2]);
    }

    [TestMethod]
    public void CopyAsSpan_withUnicodeCharacters_copiesCorrectly()
    {
        var buffer = new char[10];
        var span = new Span<char>(buffer);

        CopyAsSpan("\u4E2D\u6587", span, 0);

        Assert.AreEqual('\u4E2D', buffer[0]);
        Assert.AreEqual('\u6587', buffer[1]);
    }

    [TestMethod]
    public void CopyAsSpan_withIndexExceedingBaseLength_adjustsIndexToBaseLength()
    {
        // Arrange
        var buffer = new char[5];
        Array.Fill(buffer, 'x');
        var span = new Span<char>(buffer);

        // Act - index 10 exceeds buffer length 5, should be adjusted to 5
        // Using empty string to avoid exception when copying to empty span
        CopyAsSpan("", span, 10);

        // Assert - buffer should remain unchanged as nothing is copied
        Assert.AreEqual('x', buffer[0]);
        Assert.AreEqual('x', buffer[1]);
        Assert.AreEqual('x', buffer[2]);
        Assert.AreEqual('x', buffer[3]);
        Assert.AreEqual('x', buffer[4]);
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
        var result = CreateSeparatedString("", ", ", "test");
        Assert.AreEqual(", test", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptyAppendix_concatenatesCorrectly()
    {
        var result = CreateSeparatedString("base", ": ", "");
        Assert.AreEqual("base: ", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withEmptySeparator_concatenatesCorrectly()
    {
        var result = CreateSeparatedString("test", "", "case");
        Assert.AreEqual("testcase", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withAllEmptyStrings_returnsEmptyString()
    {
        var result = CreateSeparatedString("", "", "");
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withComplexSeparator_concatenatesCorrectly()
    {
        var result = CreateSeparatedString("Method", " - ", "param1");
        Assert.AreEqual("Method - param1", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withNullBase_fallsBackToNullString()
    {
        var result = CreateSeparatedString(null!, ", ", "value");
        Assert.AreEqual("null, value", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withNullAppendix_fallsBackToNullString()
    {
        var result = CreateSeparatedString("base", ": ", null!);
        Assert.AreEqual("base: null", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withNullSeparator_fallsBackToComma()
    {
        var result = CreateSeparatedString("first", null!, "second");
        Assert.AreEqual("first, second", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withAllNull_fallsBackToDefaults()
    {
        var result = CreateSeparatedString(null!, null!, null!);
        Assert.AreEqual("null, null", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withNullBaseAndSeparator_fallsBackCorrectly()
    {
        var result = CreateSeparatedString(null!, null!, "test");
        Assert.AreEqual("null, test", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withLongStrings_concatenatesCorrectly()
    {
        var baseStr = new string('a', 100);
        var appendix = new string('b', 100);
        var result = CreateSeparatedString(baseStr, " - ", appendix);

        Assert.AreEqual(203, result.Length); // 100 + 3 (" - ") + 100
        Assert.StartsWith(new string('a', 100), result);
        Assert.EndsWith(new string('b', 100), result);
        Assert.Contains(" - ", result);
    }

    [TestMethod]
    public void CreateSeparatedString_withMultilineSeparator_concatenatesCorrectly()
    {
        var result = CreateSeparatedString("line1", "\n", "line2");
        Assert.AreEqual("line1\nline2", result);
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

    [TestMethod]
    public void JoinWithComma_withSingleEmptyString_returnsEmptyString()
    {
        var items = new List<string?> { "" };
        var result = JoinWithComma(items);
        Assert.AreEqual("", result);
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

    [TestMethod]
    public void JoinWithComma_withTwoEmptyStrings_returnsCommaSeparator()
    {
        var items = new List<string?> { "", "" };
        var result = JoinWithComma(items);
        Assert.AreEqual(", ", result);
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

    [TestMethod]
    public void JoinWithComma_withReadOnlyCollection_returnsCommaSeparated()
    {
        var items = new System.Collections.ObjectModel.ReadOnlyCollection<string?>(["a", "b", "c"]);
        var result = JoinWithComma(items);
        Assert.AreEqual("a, b, c", result);
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

    [TestMethod]
    public void JoinWithComma_withWhitespaceStrings_preservesWhitespace()
    {
        var items = new List<string?> { "  ", "\t", "\n" };
        var result = JoinWithComma(items);
        Assert.AreEqual("  , \t, \n", result);
    }
    #endregion

    #region JoinWithSeparator - Custom Separators
    [TestMethod]
    public void JoinWithSeparator_withCustomSeparator_joinsCorrectly()
    {
        var items = new List<string?> { "a", "b", "c" };
        var result = JoinWithSeparator(items, " | ");
        Assert.AreEqual("a | b | c", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withNullSeparator_fallsBackToComma()
    {
        var items = new List<string?> { "first", "second", "third" };
        var result = JoinWithSeparator(items, null!);
        Assert.AreEqual("first, second, third", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withEmptySeparator_joinsWithoutSeparator()
    {
        var items = new List<string?> { "a", "b", "c" };
        var result = JoinWithSeparator(items, "");
        Assert.AreEqual("abc", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withSemicolonSeparator_joinsCorrectly()
    {
        var items = new List<string?> { "one", "two", "three" };
        var result = JoinWithSeparator(items, "; ");
        Assert.AreEqual("one; two; three", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withNullItems_returnsNullString()
    {
        var result = JoinWithSeparator(null!, " | ");
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withEmptyItemsAndNullSeparator_returnsEmpty()
    {
        var items = new List<string?>();
        var result = JoinWithSeparator(items, null!);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withNullItemElementsAndCustomSeparator_convertsNulls()
    {
        var items = new List<string?> { "a", null, "c" };
        var result = JoinWithSeparator(items, " - ");
        Assert.AreEqual("a - null - c", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withSingleItemAndNullSeparator_returnsItem()
    {
        var items = new List<string?> { "solo" };
        var result = JoinWithSeparator(items, null!);
        Assert.AreEqual("solo", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withNewlineSeparator_joinsCorrectly()
    {
        var items = new List<string?> { "line1", "line2", "line3" };
        var result = JoinWithSeparator(items, "\n");
        Assert.AreEqual("line1\nline2\nline3", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withTabSeparator_joinsCorrectly()
    {
        var items = new List<string?> { "col1", "col2", "col3" };
        var result = JoinWithSeparator(items, "\t");
        Assert.AreEqual("col1\tcol2\tcol3", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withLongSeparator_joinsCorrectly()
    {
        var items = new List<string?> { "a", "b" };
        var separator = new string('-', 10);
        var result = JoinWithSeparator(items, separator);
        Assert.AreEqual("a" + separator + "b", result);
    }
    #endregion

    #region JoinWithSeparator - Non-List Collections
    [TestMethod]
    public void JoinWithSeparator_withNonListCollection_joinsCorrectly()
    {
        var items = new HashSet<string?> { "x", "y", "z" };
        var result = JoinWithSeparator(items, " & ");
        // Order may vary, check all parts are present
        Assert.Contains("x", result);
        Assert.Contains("y", result);
        Assert.Contains("z", result);
        Assert.Contains(" & ", result);
    }

    [TestMethod]
    public void JoinWithSeparator_withEnumerable_joinsCorrectly()
    {
        var items = Enumerable.Range(1, 4).Select(i => $"item{i}");
        var result = JoinWithSeparator(items, " :: ");
        Assert.AreEqual("item1 :: item2 :: item3 :: item4", result);
    }
    #endregion

    #region Performance and Stress Tests
    [TestMethod]
    public void JoinWithComma_withVeryLargeCollection_performsCorrectly()
    {
        var items = Enumerable.Range(1, 1000).Select(i => i.ToString()).ToList();
        var result = JoinWithComma(items);

        Assert.IsNotNull(result);
        Assert.Contains("1", result);
        Assert.Contains("1000", result);
        Assert.AreEqual(999, result.Count(c => c == ',')); // 999 commas for 1000 items
    }

    [TestMethod]
    public void CreateSeparatedString_calledRepeatedly_producesConsistentResults()
    {
        for (int i = 0; i < 100; i++)
        {
            var result = CreateSeparatedString("base", " - ", "appendix");
            Assert.AreEqual("base - appendix", result);
        }
    }
    #endregion
}
