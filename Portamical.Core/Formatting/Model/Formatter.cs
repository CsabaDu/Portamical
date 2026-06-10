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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyAsSpan(string part, Span<char> span, int index)
    => part.AsSpan().CopyTo(span[index..]);

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

        return joinWithComma(items); // Fallback for non-List<string?> collections

        #region Local methods
        string joinWithComma(IEnumerable<string?> strings)
        => string.Join(separator, strings);
        #endregion
    }
}