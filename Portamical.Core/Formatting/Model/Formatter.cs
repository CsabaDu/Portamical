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

    #region Public formatting helper methods

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
    /// Creates a zero-allocation string by concatenating three parts: base, Separator, and insert.
    /// </summary>
    /// <param name="totalLength">The exact total length of the final string (must equal baseString.Length + Separator.Length + insert.Length).</param>
    /// <param name="baseString">The first insert of the string (prefix).</param>
    /// <param name="separator">The middle insert separating the base from the insert.</param>
    /// <param name="appendix">The final insert of the string (suffix).</param>
    /// <returns>A newly created string containing all three parts concatenated in order.</returns>
    /// <remarks>
    /// <para>
    /// This helper method provides zero-allocation string assembly for three-insert patterns
    /// using <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/>.
    /// It eliminates intermediate allocations from string interpolation, concatenation operators,
    /// or <see cref="StringBuilder"/> for this common fixed-layout pattern.
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> Primarily used to construct test case names and formatted output
    /// where a base string (e.g., class/method name) is followed by a Separator (e.g., <c>" - "</c>)
    /// and an insert (e.g., formatted parameter values). The caller is responsible for pre-calculating
    /// <paramref name="totalLength"/> to match the combined length of all three parts.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="CopyAsSpan(string, Span{char}, int)"/> to perform
    /// efficient baseSpan-based character copying without intermediate allocations. The static lambda ensures
    /// no closure allocations, and the tuple state captures all three string references for the copy operation.
    /// This approach is faster and more memory-efficient than string concatenation or interpolation for
    /// multi-insert strings where lengths are known in advance.
    /// </para>
    /// <para>
    /// <strong>Safety:</strong> The caller must ensure <paramref name="totalLength"/> exactly matches
    /// <c>baseString.Length + Separator.Length + insert.Length</c>. Providing an incorrect length
    /// will cause <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/> to throw
    /// an exception if the baseSpan is too small, or produce a string with uninitialized characters if too large.
    /// </para>
    /// <para>
    /// <strong>Used By:</strong> <see cref="TestDataBase"/> (for test case name formatting),
    /// <see cref="JoinWithComma(IEnumerable{string?})"/> (for two-item list fast path), and potentially
    /// other formatters requiring three-insert string assembly.
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

            var i = 0;
            CopyAsSpan(b, span, i);

            i = b.Length;
            CopyAsSpan(sep, span, i);

            i += sep.Length;
            CopyAsSpan(app, span, i);
        });

    /// <summary>
    /// Copies a string's characters into a <see cref="Span{T}"/> at the specified starting i.
    /// </summary>
    /// <param name="insert">The source string whose characters will be copied.</param>
    /// <param name="baseSpan">The destination character baseSpan to copy into.</param>
    /// <param name="index">The zero-based starting i in the destination baseSpan where copying begins.</param>
    /// <remarks>
    /// <para>
    /// This helper method is a core building block for zero-allocation string construction
    /// throughout the formatter infrastructure. It enables efficient string assembly by directly
    /// copying character data into pre-allocated baseSpan buffers created by <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/>.
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> Called within <see cref="string.Create{TState}(int, TState, SpanAction{char, TState})"/>
    /// lambda expressions to copy string fragments into their final positions in the output buffer.
    /// Eliminates intermediate string allocations from interpolation, concatenation, or <see cref="StringBuilder"/> usage.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// to eliminate method call overhead. Uses <see cref="MemoryExtensions.AsSpan(string)"/> for
    /// zero-allocation conversion and <see cref="Span{T}.Slice(int)"/> (via range syntax <c>[i..]</c>)
    /// for efficient offset calculation. The JIT compiler can optimize baseSpan operations into efficient
    /// memory copy instructions (e.g., <c>memcpy</c> intrinsics on modern CPUs).
    /// </para>
    /// <para>
    /// <strong>Safety:</strong> The caller is responsible for ensuring that:
    /// <list type="bullet">
    ///   <item>The destination <paramref name="baseSpan"/> has sufficient capacity starting at <paramref name="index"/>.</item>
    ///   <item>The range <c>[i, i + insert.Length)</c> does not exceed <c>baseSpan.Length</c>.</item>
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
    /// var result = string.Create(totalLength, (part1, part2), static (baseSpan, state) =>
    /// {
    ///     var (p1, p2) = state;
    ///     
    ///     CopyAsSpan(p1, baseSpan, 0);           // Copy "Hello" at i 0
    ///     baseSpan[p1.Length] = ' ';             // Write space at i 5
    ///     CopyAsSpan(p2, baseSpan, p1.Length + 1); // Copy "World" at i 6
    /// });
    /// // result: "Hello World"
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyAsSpan(string insert, Span<char> baseSpan, int index)
    {
        var insertSpan = insert.AsSpan();
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
    /// JoinWithSeparator(Array.Empty&lt;string&gt;())             // Returns: "" (empty, not "null")
    /// JoinWithSeparator(new string?[] { null })          // Returns: "null" (one null element)
    /// JoinWithSeparator(new[] { "'a'", "\"test\"", "42" })  // Returns: "'a', \"test\", 42"
    /// JoinWithSeparator(new[] { "1", "2", "3" })            // Returns: "1, 2, 3"
    /// JoinWithSeparator(new[] { "null", "\"x\"" })          // Returns: "null, \"x\""
    /// JoinWithSeparator(new string?[] { "a", null, "b" })   // Returns: "a, null, b"
    /// </code>
    /// </example>
    public static string JoinWithComma(IEnumerable<string?> items)
    {
        if (items is ICollection<string?> collection &&
            collection.Count == 0)
        {
            return string.Empty;
        }

        if (items is IList<string?> list)
        {
            return JoinWithSeparator(list);
        }

        return JoinWithSeparator(items);
    }

    #endregion

    #region Private helpers

    private const string Separator = ", ";

    private static string JoinWithSeparator(IList<string?> list)
    {
        // Fast path for common case: List<string> with 1-3 items
        var count = list.Count;
        var i = 0;
        var result = getIndexedItem();

        if (isCountEqualToIncrementedIndex()) return result;

        var totalLength = result.Length;

        while (i < 3)
        {
            var item = getIndexedItem();
            totalLength += Separator.Length + item.Length;
            result = CreateSeparatedString(
                totalLength,
                result,
                Separator,
                item);

            if (isCountEqualToIncrementedIndex()) return result;
        }

        // Fallback to standard join for more than 3 items
        return JoinWithSeparator((IEnumerable<string?>)list);

        #region Local methods
        string getIndexedItem()
        => FallbackIfNull(list[i]);

        bool isCountEqualToIncrementedIndex()
        => count == ++i;
        #endregion
    }

    private static string JoinWithSeparator(IEnumerable<string?> strings)
    => string.Join(Separator, strings.Select(FallbackIfNull));

    #endregion
}

/// <summary>
/// Provides a generic base class for type-safe formatters that convert values of type <typeparamref name="T"/>
/// into human-readable string representations.
/// </summary>
/// <typeparam name="T">The type of value this formatter handles.</typeparam>
/// <remarks>
/// <para>
/// This generic abstract class extends <see cref="Formatter"/> to provide type-safe formatting
/// for specific value types. It implements the Template Method pattern by providing the infrastructure
/// for type checking and delegation, while subclasses implement the type-specific formatting logic.
/// </para>
/// <para>
/// <strong>Design Benefits:</strong>
/// <list type="bullet">
///   <item><strong>Type Safety:</strong> Compile-time type checking eliminates casting errors</item>
///   <item><strong>Separation of Concerns:</strong> Base class handles type checking; subclasses focus on formatting</item>
///   <item><strong>Interface Compliance:</strong> Automatically implements both <see cref="IFormatter"/> and <see cref="IFormatter{T}"/></item>
///   <item><strong>Reusability:</strong> Inherit utility methods from <see cref="Formatter"/> base class</item>
/// </list>
/// </para>
/// <para>
/// <strong>Implementation Pattern:</strong> Subclasses need only implement <see cref="Format(T)"/>
/// with type-specific formatting logic. The base class automatically handles:
/// <list type="bullet">
///   <item>Type checking in <see cref="Format(object)"/></item>
///   <item>Delegation to the type-safe <see cref="Format(T)"/> method</item>
///   <item>Returning <see langword="null"/> for incompatible types</item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be stateless or use appropriate synchronization
/// if maintaining state, as formatters may be called concurrently from multiple threads during test execution.
/// </para>
/// <para>
/// <strong>Performance:</strong> The sealed <see cref="Format(object)"/> override uses pattern matching
/// (<c>is T</c>) for efficient type checking without reflection overhead. The JIT compiler can optimize
/// this check to a simple type comparison for reference types or unboxing for value types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example: Custom formatter for a domain type
/// public sealed class ProductIdFormatter : Formatter&lt;ProductId&gt;
/// {
///     public override string Format(ProductId value)
///     {
///         if (value is null)
///             return NullString;  // Use base class constant
///         
///         return $"PROD-{value.Id:D6}";
///     }
/// }
/// 
/// // Usage with ValueFormatter registry
/// var formatter = new ProductIdFormatter();
/// ValueFormatter.RegisterFormatter&lt;ProductId&gt;(formatter);
/// 
/// var productId = new ProductId { Id = 42 };
/// var formatted = ValueFormatter.Format(productId);
/// // Result: "PROD-000042" ✅
/// 
/// // Automatic type safety
/// object obj = productId;
/// var formatted2 = formatter.Format(obj);  // Uses type checking, returns "PROD-000042"
/// 
/// var wrongType = "not a ProductId";
/// var formatted3 = formatter.Format(wrongType);  // Returns null (type mismatch)
/// </code>
/// 
/// <code>
/// // Example: Formatter using base class utilities
/// public sealed class RangeFormatter : Formatter&lt;Range&gt;
/// {
///     public override string Format(Range value)
///     {
///         if (value is null)
///             return FallbackIfNull(null);  // Use base class helper
///         
///         // Use JoinWithComma for consistent formatting
///         var parts = new[] { value.Start.ToString(), value.End.ToString() };
///         return $"[{JoinWithComma(parts)}]";
///     }
/// }
/// 
/// var range = new Range(1, 10);
/// var formatter = new RangeFormatter();
/// var result = formatter.Format(range);
/// // Result: "[1, 10]" ✅
/// </code>
/// </example>
/// <seealso cref="Formatter"/>
/// <seealso cref="IFormatter{T}"/>
/// <seealso cref="ValueFormatter"/>
public abstract class Formatter<T>
: Formatter,
IFormatter<T>
{
    /// <summary>
    /// Formats a value of type <typeparamref name="T"/> into a human-readable string representation.
    /// </summary>
    /// <param name="value">The value of type <typeparamref name="T"/> to format. May be null if <typeparamref name="T"/> is a nullable type.</param>
    /// <returns>
    /// A formatted string representation of the value suitable for test case names, diagnostic output, or logging;
    /// or <see langword="null"/> if formatting fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Implementation Requirements:</strong>
    /// <list type="bullet">
    ///   <item><strong>Null Handling:</strong> Return <see cref="Formatter.NullString"/> (<c>"null"</c>) for null values if <typeparamref name="T"/> is nullable</item>
    ///   <item><strong>Consistency:</strong> Produce the same output for equivalent values</item>
    ///   <item><strong>Conciseness:</strong> Keep output brief but descriptive (typically &lt; 50 characters)</item>
    ///   <item><strong>Clarity:</strong> Use formats that align with C# literal syntax when appropriate</item>
    ///   <item><strong>Thread Safety:</strong> Ensure the method is safe for concurrent calls</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Base Class Utilities:</strong> Implementations can leverage inherited helper methods:
    /// <list type="bullet">
    ///   <item><see cref="Formatter.FallbackIfNull(string?)"/> - Convert null to <c>"null"</c></item>
    ///   <item><see cref="Formatter.JoinWithComma(IEnumerable{string?})"/> - Join formatted parts</item>
    ///   <item><see cref="Formatter.CreateSeparatedString(int, string, string, string)"/> - Zero-allocation string assembly</item>
    ///   <item><see cref="Formatter.CopyAsSpan(string, Span{char}, int)"/> - Efficient string copying</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> This method may be called frequently during test case name generation.
    /// Optimize for common cases and avoid expensive operations like reflection, I/O, or complex computations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Simple value formatter
    /// public class PercentageFormatter : Formatter&lt;decimal&gt;
    /// {
    ///     public override string Format(decimal value)
    ///     {
    ///         return $"{value:P0}";  // Format as percentage
    ///     }
    /// }
    /// 
    /// var formatter = new PercentageFormatter();
    /// formatter.Format(0.75m);  // "75%"
    /// formatter.Format(1.0m);   // "100%"
    /// </code>
    /// 
    /// <code>
    /// // Nullable value formatter with base class utilities
    /// public class OptionalStringFormatter : Formatter&lt;string?&gt;
    /// {
    ///     public override string Format(string? value)
    ///     {
    ///         // Use base class helper for null handling
    ///         if (value is null)
    ///             return FallbackIfNull(null);
    ///         
    ///         return $"\"{value}\"";
    ///     }
    /// }
    /// 
    /// var formatter = new OptionalStringFormatter();
    /// formatter.Format("test");  // "\"test\""
    /// formatter.Format(null);    // "null"
    /// </code>
    /// </example>
    public abstract string? Format(T value);

    /// <summary>
    /// Formats an object value by checking its type and delegating to the type-safe <see cref="Format(T)"/> method.
    /// </summary>
    /// <param name="value">The object to format. May be null.</param>
    /// <returns>
    /// A formatted string representation if <paramref name="value"/> is of type <typeparamref name="T"/>;
    /// otherwise, <see langword="null"/> to indicate type incompatibility.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides the bridge between the non-generic <see cref="IFormatter"/> interface
    /// (used by the <see cref="ValueFormatter"/> registry) and the type-safe <see cref="Format(T)"/> method.
    /// </para>
    /// <para>
    /// <strong>Implementation:</strong> Uses pattern matching (<c>value is T typedValue</c>) to perform
    /// efficient type checking. If the type matches, delegates to the abstract <see cref="Format(T)"/>
    /// method; otherwise, returns <see langword="null"/> to signal incompatibility.
    /// </para>
    /// <para>
    /// <strong>Why Sealed:</strong> This method is marked <see langword="sealed"/> to prevent subclasses
    /// from overriding it. The type-checking logic is standardized and should not vary across implementations.
    /// Subclasses customize behavior by implementing <see cref="Format(T)"/> instead.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Pattern matching with <c>is T</c> is optimized by the JIT compiler:
    /// <list type="bullet">
    ///   <item><strong>Reference types:</strong> Simple type comparison (virtual method table check)</item>
    ///   <item><strong>Value types:</strong> Unboxing operation with type verification</item>
    ///   <item><strong>Nullable types:</strong> Null check followed by underlying type verification</item>
    /// </list>
    /// No reflection is used, making this approach efficient even in hot paths.
    /// </para>
    /// <para>
    /// <strong>Null Handling:</strong> Null values are passed through to <see cref="Format(T)"/> if
    /// <typeparamref name="T"/> is a nullable type (reference type or <see cref="Nullable{T}"/>).
    /// For non-nullable value types, null will fail the type check and return <see langword="null"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Int32Formatter : Formatter&lt;int&gt;
    /// {
    ///     public override string Format(int value) => value.ToString();
    /// }
    /// 
    /// var formatter = new Int32Formatter();
    /// 
    /// // Type-safe calls
    /// formatter.Format(42);        // "42" (via Format(int))
    /// 
    /// // Non-generic calls (via Format(object))
    /// object obj1 = 42;
    /// formatter.Format(obj1);      // "42" ✅ (unboxing succeeds)
    /// 
    /// object obj2 = "42";
    /// formatter.Format(obj2);      // null ✅ (type mismatch, string != int)
    /// 
    /// object? obj3 = null;
    /// formatter.Format(obj3);      // null ✅ (null is not an int)
    /// </code>
    /// 
    /// <code>
    /// // Nullable reference type example
    /// public class StringFormatter : Formatter&lt;string?&gt;
    /// {
    ///     public override string Format(string? value)
    ///         => value is null ? "null" : $"\"{value}\"";
    /// }
    /// 
    /// var formatter = new StringFormatter();
    /// 
    /// // Type-safe calls
    /// formatter.Format("test");    // "\"test\""
    /// formatter.Format(null);      // "null"
    /// 
    /// // Non-generic calls
    /// object obj1 = "test";
    /// formatter.Format(obj1);      // "\"test\"" ✅ (type matches)
    /// 
    /// object? obj2 = null;
    /// formatter.Format(obj2);      // "null" ✅ (null is valid for string?)
    /// 
    /// object obj3 = 123;
    /// formatter.Format(obj3);      // null ✅ (int != string)
    /// </code>
    /// </example>
    public override sealed string? Format(object value)
    => value is T typedValue ? Format(typedValue) : null;
}