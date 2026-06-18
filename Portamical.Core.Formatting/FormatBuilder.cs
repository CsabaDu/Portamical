// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Runtime.CompilerServices;

namespace Portamical.Core.Formatting;

public static class FormatBuilder
{
    /// <summary>
    /// The maximum number of items to include when formatting collections, tuples, and dictionaries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Limits collection output to the first 3 items to keep formatted strings concise and readable.
    /// When collections exceed this limit, output is truncated with a prefix like <c>"First 3 of 5+"</c>.
    /// </para>
    /// <para>
    /// This value balances readability with diagnostic usefulness, providing enough context without
    /// overwhelming test case names or log output with large collections.
    /// </para>
    /// </remarks>
    public const int MaxCount = 3;

    /// <summary>
    /// The string representation used for null values in formatted output.
    /// </summary>
    /// <remarks>
    /// This constant ensures consistent null representation across all formatters.
    /// Used by <see cref="FallbackIfNull(string?)"/> and throughout the formatting pipeline.
    /// </remarks>
    public const string NullString = "null";

    private const string Comma_ = ", ";

    /// <summary>
    /// Provides a fallback string when the input is null, ensuring non-null output.
    /// </summary>
    /// <param name="str">The nullable string to check.</param>
    /// <returns>
    /// The original <paramref name="str"/> if not null; otherwise, <see cref="NullString"/> (<c>"null"</c>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This helper centralizes null-to-"null" conversion across the formatter, maintaining consistent
    /// null representation in formatted output. Used by <see cref="DefaultFormatter"/> methods for
    /// formatting tuples, collections, and dictionaries, and by <see cref="JoinWithComma(IEnumerable{string?})"/>.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// to eliminate method call overhead. This is a frequently called helper on hot paths during
    /// collection, tuple, and dictionary formatting.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FallbackIfNull(string? str)
    => str ?? NullString;

    /// <summary>
    /// Creates a zero-allocation string by concatenating three parts: base, separator, and appendix.
    /// </summary>
    /// <param name="totalLength">The exact total length of the final string (must equal baseString.Length + separator.Length + appendix.Length).</param>
    /// <param name="baseString">The first part of the string (prefix).</param>
    /// <param name="separator">The middle part separating the base from the appendix.</param>
    /// <param name="appendix">The final part of the string (suffix).</param>
    /// <returns>A newly created string containing all three parts concatenated in order.</returns>
    /// <remarks>
    /// <para>
    /// This helper method provides zero-allocation string assembly for three-part patterns
    /// using <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/>.
    /// It eliminates intermediate allocations from string interpolation, concatenation operators,
    /// or <see cref="StringBuilder"/> for this common fixed-layout pattern.
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> Primarily used to construct test case names and formatted output
    /// where a base string (e.g., class/method name) is followed by a separator (e.g., <c>" - "</c>)
    /// and an appendix (e.g., formatted parameter values). The caller is responsible for pre-calculating
    /// <paramref name="totalLength"/> to match the combined length of all three parts.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="CopyAsSpan(string, Span{char}, int)"/> to perform
    /// efficient span-based character copying without intermediate allocations. The static lambda ensures
    /// no closure allocations, and the tuple state captures all three string references for the copy operation.
    /// This approach is faster and more memory-efficient than string concatenation or interpolation for
    /// multi-part strings where lengths are known in advance.
    /// </para>
    /// <para>
    /// <strong>Safety:</strong> The caller must ensure <paramref name="totalLength"/> exactly matches
    /// <c>baseString.Length + separator.Length + appendix.Length</c>. Providing an incorrect length
    /// will cause <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/> to throw
    /// an exception if the span is too small, or produce a string with uninitialized characters if too large.
    /// </para>
    /// <para>
    /// <strong>Used By:</strong> <see cref="TestDataBase"/> (for test case name formatting),
    /// <see cref="JoinWithComma(IEnumerable{string?})"/> (for two-item list fast path), and potentially
    /// other formatters requiring three-part string assembly.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical usage: "TestMethod - param1, param2"
    /// var baseStr = "TestMethod";
    /// var sep = " - ";
    /// var app = "param1, param2";
    /// var totalLen = baseStr.Length + sep.Length + app.Length; // 14 + 3 + 13 = 30
    /// 
    /// var testCaseName = CreateSeparatedString(totalLen, baseStr, sep, app);
    /// // Result: "TestMethod - param1, param2"
    /// 
    /// // Another example: "Class.Method: value"
    /// var result = CreateSeparatedString(18, "Class.Method", ": ", "value");
    /// // Result: "Class.Method: value"
    /// </code>
    /// </example>
    public static string CreateSeparatedString(
        string baseString,
        string separator,
        string appendix)
    {
        baseString = FallbackIfNull(baseString);
        separator = NotNullSeparator(separator);
        appendix = FallbackIfNull(appendix);
        var totalLength =
            baseString.Length +
            separator.Length +
            appendix.Length;

        return string.Create(
            totalLength,
            (baseString, separator, appendix),
            static (span, state) =>
            {
                var (bs, sep, app) = state;

                var i = 0;
                CopyAsSpan(bs, span, i);

                i = bs.Length;
                CopyAsSpan(sep, span, i);

                i += sep.Length;
                CopyAsSpan(app, span, i);
            });
    }

    /// <summary>
    /// Copies a string's characters into a <see cref="Span{T}"/> at the specified starting index.
    /// </summary>
    /// <param name="insertStr">The source string whose characters will be copied.</param>
    /// <param name="baseSpan">The destination character span to copy into.</param>
    /// <param name="index">The zero-based starting index in the destination span where copying begins.</param>
    /// <remarks>
    /// <para>
    /// This helper method is a core building block for zero-allocation string construction
    /// throughout the formatter infrastructure. It enables efficient string assembly by directly
    /// copying character data into pre-allocated span buffers created by <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/>.
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> Called within <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/>
    /// lambda expressions to copy string fragments into their final positions in the output buffer.
    /// Eliminates intermediate string allocations from interpolation, concatenation, or <see cref="StringBuilder"/> usage.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// to eliminate method call overhead. Uses <see cref="MemoryExtensions.AsSpan(string)"/> for
    /// zero-allocation conversion and <see cref="Span{T}.Slice(int)"/> (via range syntax <c>[index..]</c>)
    /// for efficient offset calculation. The JIT compiler can optimize span operations into efficient
    /// memory copy instructions (e.g., <c>memcpy</c> intrinsics on modern CPUs).
    /// </para>
    /// <para>
    /// <strong>Safety:</strong> The caller is responsible for ensuring that:
    /// <list type="bullet">
    ///   <item>The destination <paramref name="baseSpan"/> has sufficient capacity starting at <paramref name="index"/>.</item>
    ///   <item>The range <c>[index, index + insertStr.Length)</c> does not exceed <c>baseSpan.Length</c>.</item>
    /// </list>
    /// Violating these preconditions will throw an exception from <see cref="ReadOnlySpan{T}.CopyTo(Span{T})"/>.
    /// </para>
    /// <para>
    /// <strong>Used By:</strong> <see cref="CreateSeparatedString(int, string, string, string)"/>,
    /// <see cref="JoinWithComma(IEnumerable{string?})"/>, and various <c>DefaultFormatter.Format</c> overloads
    /// for strings, key-value pairs, delegates, types (arrays, nullable, generics), and other composite types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Typical usage inside string.Create:
    /// var part1 = "Hello";
    /// var part2 = "World";
    /// var totalLength = part1.Length + 1 + part2.Length; // "Hello World"
    /// 
    /// var result = string.Create(totalLength, (part1, part2), static (span, state) =>
    /// {
    ///     var (p1, p2) = state;
    ///     
    ///     CopyAsSpan(p1, span, 0);           // Copy "Hello" at index 0
    ///     span[p1.Length] = ' ';             // Write space at index 5
    ///     CopyAsSpan(p2, span, p1.Length + 1); // Copy "World" at index 6
    /// });
    /// // result: "Hello World"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyAsSpan(string insertStr, Span<char> baseSpan, int index)
    {
        var baseLength = baseSpan.Length;
        if (index > baseLength)
        {
            index = baseLength;
        }
        var insertSpan = insertStr.AsSpan();
        insertSpan.CopyTo(baseSpan[index..]);
    }

    /// <summary>
    /// Joins a collection of pre-formatted string items into a comma-separated string.
    /// </summary>
    /// <param name="items">The collection of formatted string items to join. May contain null elements.</param>
    /// <returns>
    /// A comma-separated string representation of the items. Returns an empty string for empty collections,
    /// distinguishing them from collections containing a single null element (which returns <c>"null"</c>).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This helper method is used internally to join pre-formatted items into a single string
    /// for collections, tuples, and dictionaries. It assumes that the items have already been
    /// formatted through <see cref="DefaultFormatter.Format(object?)"/> or other specialized formatters.
    /// </para>
    /// <para>
    /// <strong>Usage Context:</strong> Called by <see cref="DefaultFormatter"/> methods such as
    /// <c>Format(IEnumerable)</c>, <c>Format(IDictionary, string?)</c>, <c>Format(ITuple)</c>, and
    /// <c>Format(Type)</c> to combine pre-formatted elements into their final string representation.
    /// </para>
    /// <para>
    /// <strong>Null Handling:</strong> Accepts nullable strings (<c>string?</c>) in the input collection. 
    /// Null items are converted to <c>"null"</c> via <see cref="FallbackIfNull(string?)"/>, ensuring
    /// clear distinction between empty collections and collections with null elements.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Optimized for the common case of <see cref="List{T}"/> with 0-3 items.
    /// Uses <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/> for 2-3 item lists
    /// to avoid intermediate string allocations via interpolation. This zero-allocation approach directly writes
    /// to the final string buffer using <see cref="Span{T}"/>, eliminating GC pressure for the most common cases
    /// (tuples, small collections, generic type arguments). The method is intentionally not inlined due to its
    /// size and complexity, but the fast paths are optimized for minimal overhead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Internal usage examples (items are already formatted)
    /// JoinWithSeparator(Array.Empty&lt;string&gt;())             // Returns: "" (empty, not "null")
    /// JoinWithSeparator(new string?[] { null })          // Returns: "null" (one null element)
    /// JoinWithSeparator(new[] { "'a'", "\"test\"", "42" })  // Returns: "'a', \"test\", 42"
    /// JoinWithSeparator(new[] { "1", "2", "3" })            // Returns: "1, 2, 3"
    /// JoinWithSeparator(new[] { "null", "\"x\"" })          // Returns: "null, \"x\""
    /// JoinWithSeparator(new string?[] { "a", null, "b" })   // Returns: "a, null, b"
    /// </code>
    /// </example>
    public static string JoinWithComma(IEnumerable<string?> items)
    => JoinWithSeparator(items, Comma_);

    /// <summary>
    /// Joins a collection of pre-formatted string items with a custom separator.
    /// </summary>
    /// <param name="items">The collection of formatted string items to join. May contain null elements or be null itself.</param>
    /// <param name="separator">The separator to use between items. If null, defaults to <c>", "</c>.</param>
    /// <returns>
    /// A separator-delimited string representation of the items. Returns <c>"null"</c> if <paramref name="items"/> 
    /// is null, or an empty string for empty collections.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the core joining method that <see cref="JoinWithComma(IEnumerable{string?})"/> delegates to.
    /// It provides flexibility for using custom separators beyond the default comma-space separator.
    /// </para>
    /// <para>
    /// <strong>Null Handling:</strong> 
    /// <list type="bullet">
    ///   <item>If <paramref name="items"/> is null, returns <c>"null"</c></item>
    ///   <item>If <paramref name="separator"/> is null, uses <c>", "</c> as default</item>
    ///   <item>Null elements within the collection are converted to <c>"null"</c> via <see cref="FallbackIfNull(string?)"/></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses optimized code paths for <see cref="IList{T}"/> collections
    /// with up to <see cref="MaxCount"/> items, employing zero-allocation string building techniques.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Custom separator
    /// JoinWithSeparator(new[] { "a", "b", "c" }, " | ")  // Returns: "a | b | c"
    /// JoinWithSeparator(new[] { "1", "2" }, "; ")         // Returns: "1; 2"
    /// 
    /// // Null handling
    /// JoinWithSeparator(null, ", ")                       // Returns: "null"
    /// JoinWithSeparator(new string[] { }, ", ")           // Returns: ""
    /// JoinWithSeparator(new[] { "a", null }, " - ")       // Returns: "a - null"
    /// </code>
    /// </example>
    public static string JoinWithSeparator(IEnumerable<string?> items, string separator)
    {
        if (items is null)
        {
            return NullString;
        }

        separator = NotNullSeparator(separator);

        if (items is not ICollection<string?> collection)
        {
            return joinWithSeparatorBase();
        }

        if (collection.Count == 0)
        {
            return string.Empty;
        }

        if (items is IList<string?> list)
        {
            return JoinWithSeparator(list, separator);
        }

        return joinWithSeparatorBase();

        #region Local methods
        string joinWithSeparatorBase()
        => JoinWithSeparatorBase(items, separator);
        #endregion
    }

    #region Private methods

    private static string NotNullSeparator(string? separator)
    => separator ?? Comma_;

    private static string JoinWithSeparator(IList<string?> list, string separator)
    {
        // Fast path for common case: List<string> with not more than MaxCount items

        var i = 0;
        var result = getIndexedItem();

        if (isCountEqualToIncrementedIndex()) return result;

        //var totalLength = result.Length;

        while (i < MaxCount)
        {
            var item = getIndexedItem();
            //totalLength += separator.Length + item.Length;
            result = CreateSeparatedString(
                //totalLength,
                result,
                separator,
                item);

            if (isCountEqualToIncrementedIndex()) return result;
        }

        // Fallback to standard join for more than MaxCount items
        return JoinWithSeparatorBase(list, separator);

        #region Local methods
        string getIndexedItem()
        => FallbackIfNull(list[i]);

        bool isCountEqualToIncrementedIndex()
        => list.Count == ++i;
        #endregion
    }

    private static string JoinWithSeparatorBase(IEnumerable<string?> items, string separator)
    => string.Join(separator, items.Select(FallbackIfNull));

    #endregion
}
