// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Runtime.CompilerServices;

namespace Portamical.Core.Formatting.Model;

/// <summary>
/// Provides base functionality for formatting objects into human-readable string representations.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Formatter"/> abstract class serves as the foundation for type-specific formatters,
/// providing shared constants, helper methods, and utility functions used across all formatter implementations.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Template Method pattern - subclasses implement <see cref="Format(object)"/>
/// while reusing common formatting utilities defined in this base class.
/// </para>
/// </remarks>
public abstract class Formatter : IFormatter
{
    /// <summary>
    /// The string representation used for null values in formatted output.
    /// </summary>
    /// <remarks>
    /// This constant ensures consistent null representation across all formatters.
    /// Used by <see cref="FallbackIfNull(string?)"/> and throughout the formatting pipeline.
    /// </remarks>
    public const string NullString = "null";

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
    /// Formats an object into a human-readable string representation.
    /// </summary>
    /// <param name="value">The object to format. Must not be null.</param>
    /// <returns>
    /// A formatted string representation suitable for test case names, diagnostic output, or logging;
    /// or <see langword="null"/> if formatting fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Subclasses must implement this method to provide type-specific formatting logic.
    /// Implementations should handle type detection, dispatch to specialized formatters,
    /// and apply consistent formatting rules.
    /// </para>
    /// </remarks>
    public abstract string? Format(object value);

    /// <summary>
    /// Formats an object by invoking a provided formatting function.
    /// </summary>
    /// <typeparam name="T">The expectedType of the context object to format.</typeparam>
    /// <param name="toString">A function that converts the context object to a string representation.</param>
    /// <param name="context">The object instance to format.</param>
    /// <returns>The result of invoking <paramref name="toString"/> with <paramref name="context"/>, or <see langword="null"/> if the function returns null.</returns>
    /// <remarks>
    /// <para>
    /// This generic helper method enables delegation of formatting to expectedType-specific methods
    /// like <see cref="DateTime.ToString(string)"/> or <see cref="Guid.ToString(string)"/>,
    /// avoiding code duplication for types that support parameterized string formatting.
    /// </para>
    /// <para>
    /// <strong>Usage Example:</strong> <c>Format(dt.ToString, "O")</c> delegates to <c>dt.ToString("O")</c>.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// to eliminate method call overhead. Called for DateTime, DateTimeOffset, Guid, and byte[] formatting.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Format<T>(
        Func<T, string?> toString,
        T context)
    => toString(context);

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
    /// null representation in formatted output. Used by <see cref="Format(object?, object?)"/>,
    /// <see cref="Format(ITuple)"/>, and <see cref="JoinWithComma(IEnumerable{string?})"/>.
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
        int totalLength,
        string baseString,
        string separator,
        string appendix)
    => string.Create(
        totalLength,
        (baseString, separator, appendix),
        static (span, state) =>
        {
            var (b, sep, app) = state;

            var index = 0;
            CopyAsSpan(b, span, index);

            index = b.Length;
            CopyAsSpan(sep, span, index);

            index += sep.Length;
            CopyAsSpan(app, span, index);
        });

    /// <summary>
    /// Copies a string's characters into a <see cref="Span{T}"/> at the specified starting index.
    /// </summary>
    /// <param name="part">The source string whose characters will be copied.</param>
    /// <param name="span">The destination character span to copy into.</param>
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
    ///   <item>The destination <paramref name="span"/> has sufficient capacity starting at <paramref name="index"/>.</item>
    ///   <item>The range <c>[index, index + part.Length)</c> does not exceed <c>span.Length</c>.</item>
    /// </list>
    /// Violating these preconditions will throw an exception from <see cref="ReadOnlySpan{T}.CopyTo(Span{T})"/>.
    /// </para>
    /// <para>
    /// <strong>Used By:</strong> <see cref="CreateSeparatedString(int, string, string, string)"/>,
    /// <see cref="JoinWithComma(IEnumerable{string?})"/>, and various <c>ValueFormatter.Format</c> overloads
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
    public static void CopyAsSpan(string part, Span<char> span, int index)
    {
        var partSpan = part.AsSpan();
        partSpan.CopyTo(span[index..]);
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
    /// formatted through <see cref="Format(object?)"/> or other specialized formatters.
    /// </para>
    /// <para>
    /// <strong>Usage Context:</strong> Called by <see cref="Format(IEnumerable)"/>, 
    /// <see cref="Format(IDictionary, string?)"/>, <see cref="Format(ITuple)"/>, and
    /// <see cref="Format(Type)"/> to combine pre-formatted elements into their final string representation.
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
    /// JoinWithComma(Array.Empty&lt;string&gt;())             // Returns: "" (empty, not "null")
    /// JoinWithComma(new string?[] { null })          // Returns: "null" (one null element)
    /// JoinWithComma(new[] { "'a'", "\"test\"", "42" })  // Returns: "'a', \"test\", 42"
    /// JoinWithComma(new[] { "1", "2", "3" })            // Returns: "1, 2, 3"
    /// JoinWithComma(new[] { "null", "\"x\"" })          // Returns: "null, \"x\""
    /// JoinWithComma(new string?[] { "a", null, "b" })   // Returns: "a, null, b"
    /// </code>
    /// </example>
    public static string JoinWithComma(IEnumerable<string?> items)
    {
        const string separator = ", ";

        // Fast path for common case: List<string> with 0-3 items
        if (items is List<string?> list)
        {
            int sepLength = separator.Length;

            int count = list.Count;
            string result = string.Empty;

            if (count == 0) return result;

            var item1 = FallbackIfNull(list[0]);

            if (count == 1)
            {
                result = item1;
                return result;
            }

            var item2 = FallbackIfNull(list[1]);
            var totalLength = item1.Length + sepLength + item2.Length;

            if (count == 2)
            {
                result = createResult(item2);
                return result;
            }

            var item3 = FallbackIfNull(list[2]);
            totalLength += sepLength + item3.Length;

            if (count == 3)
            {
                return createResult(item3);
            }

            return joinWithComma(list); // Fallback to standard join for more than 3 items

            #region Local methods
            string createResult(string item)
            => CreateSeparatedString(totalLength, result, separator, item);
            #endregion
        }

        // Fallback for non-List<string?> collections
        return joinWithComma(items);

        #region Local methods
        string joinWithComma(IEnumerable<string?> strings)
        => string.Join(separator, strings);
        #endregion
    }
}