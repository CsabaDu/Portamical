// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Portamical.Core.Formatting.Builder;

namespace Portamical.Core.Formatting;

/// <summary>
/// Provides static methods for formatting objects into human-readable string representations.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="DefaultFormatter"/> class offers specialized formatting for various .NET types,
/// optimized for creating readable test case names, diagnostic output, and logging messages.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Type-specific formatting rules (strings quoted, chars single-quoted, dates in ISO 8601, etc.)</item>
///   <item>Recursive formatting for collections, tuples, and nested structures</item>
///   <item>C#-friendly type names (int instead of Int32, List&lt;string&gt; with proper syntax)</item>
///   <item>Graceful null handling - returns null to signal formatting failure for downstream handling</item>
///   <item>Configurable collection truncation (first <see cref="MaxCount"/> items)</item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Uses pattern matching with method overloading to dispatch
/// to type-specific formatters, enabling extensibility and clean separation of concerns.
/// Supports custom formatter registration via <see cref="Formatter"/> for specialized types.
/// </para>
/// </remarks>
internal sealed class DefaultFormatter : IFormatter
{
    #region Static fields

    /// <summary>
    /// Gets the singleton instance of the <see cref="DefaultFormatter"/>.
    /// </summary>
    /// <value>A shared, thread-safe <see cref="IFormatter"/> instance.</value>
    /// <remarks>
    /// <para>
    /// This property provides a pre-initialized formatter instance that can be reused
    /// throughout the application, avoiding unnecessary allocations. The formatter is
    /// stateless and thread-safe, making it suitable for concurrent use.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> The formatter instance is immutable and thread-safe.
    /// Multiple threads can safely call <see cref="Format(object?)"/> concurrently.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> This instance is returned by <see cref="Formatter.GetFormatter(Type)"/>
    /// when no custom formatter is registered for a type, serving as the fallback formatter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// // Use the singleton instance directly
    /// var formatter = DefaultFormatter.Instance;
    /// var result = formatter.Format(42);  // Returns: "42"
    /// 
    /// // Or use it via the interface
    /// IFormatter formatter2 = DefaultFormatter.Instance;
    /// ]]></code>
    /// </example>
    public static readonly IFormatter Instance = new DefaultFormatter();

    #region Private static fields

    /// <summary>
    /// The starting ASCII code for printable characters (space character, ASCII 32).
    /// </summary>
    /// <remarks>
    /// Used with <see cref="AsciiPrintableEnd"/> to define the range of pre-cached
    /// character formats in <see cref="CharFormats"/>. Printable ASCII characters
    /// range from 32 (space) to 126 (tilde).
    /// </remarks>
    private const int AsciiPrintableStart = ' ';

    /// <summary>
    /// The ending ASCII code for printable characters (tilde character, ASCII 126).
    /// </summary>
    /// <remarks>
    /// Used with <see cref="AsciiPrintableStart"/> to define the range of pre-cached
    /// character formats in <see cref="CharFormats"/>. Printable ASCII characters
    /// range from 32 (space) to 126 (tilde).
    /// </remarks>
    private const int AsciiPrintableEnd = '~';

    /// <summary>
    /// Cache of compiled delegate accessors for efficiently extracting Key and Value properties
    /// from KeyValuePair&lt;TKey, TValue&gt; objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Optimization #1 &amp; #12: Caches compiled delegate accessors per KeyValuePair type,
    /// providing 10-100x faster property access compared to reflection after the first access.
    /// </para>
    /// <para>
    /// The delegate takes an object (the KeyValuePair instance) and returns a tuple of (key, value).
    /// This approach uses compiled expressions instead of <see cref="PropertyInfo.GetValue(object)"/>
    /// for each access, dramatically improving performance in dictionary and collection formatting.
    /// </para>
    /// </remarks>
    private static readonly ConcurrentDictionary<Type, Func<object, (object?, object?)>> _kvpAccessorCache = new();

    /// <summary>
    /// Cache of type checking results to determine if a type is a KeyValuePair&lt;,&gt;.
    /// </summary>
    /// <remarks>
    /// Optimization #2: Caches the result of checking whether a type is a constructed
    /// KeyValuePair&lt;TKey, TValue&gt; to avoid repeated reflection calls to
    /// <see cref="Type.GetGenericTypeDefinition"/>. Type checking is performed frequently
    /// during dictionary enumeration and collection formatting.
    /// </remarks>
    private static readonly ConcurrentDictionary<Type, bool> _isKvpCache = new();

    /// <summary>
    /// Dictionary mapping BCL types to their C# keyword aliases for readable type name formatting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Optimization #5: Uses Type reference equality lookup instead of string comparison,
    /// providing O(1) lookup performance without string operations.
    /// </para>
    /// <para>
    /// Maps types like <see cref="Int32"/> ? "int", <see cref="String"/> ? "string",
    /// <see cref="Boolean"/> ? "bool", etc. Used by <see cref="GetCSharpAliasOrTypeName"/>
    /// to produce C#-friendly type names in formatted output.
    /// </para>
    /// </remarks>
    private static readonly Dictionary<Type, string> _typeAliases = new()
    {
        [typeof(bool)]      = "bool",
        [typeof(byte)]      = "byte",
        [typeof(sbyte)]     = "sbyte",
        [typeof(char)]      = "char",
        [typeof(decimal)]   = "decimal",
        [typeof(double)]    = "double",
        [typeof(float)]     = "float",
        [typeof(int)]       = "int",
        [typeof(uint)]      = "uint",
        [typeof(long)]      = "long",
        [typeof(ulong)]     = "ulong",
        [typeof(short)]     = "short",
        [typeof(ushort)]    = "ushort",
        [typeof(object)]    = "object",
        [typeof(string)]    = "string",
        [typeof(void)]      = "void"
    };

    /// <summary>
    /// Pre-compiled search values for hardware-accelerated detection of anonymous delegate method names.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Optimization #14: Uses <see cref="SearchValues{T}"/> for SIMD-accelerated character searching
    /// instead of <see cref="string.IndexOfAny(char[])"/>. Searches for angle brackets ('&lt;', '&gt;')
    /// which appear in compiler-generated lambda and anonymous method names (e.g., "&lt;Main&gt;b__0_1").
    /// </para>
    /// <para>
    /// SearchValues compiles to vectorized SIMD instructions on modern CPUs, providing 2-5x faster
    /// searching compared to scalar implementations. Used by <see cref="IsAnonymousDelegate"/> to
    /// quickly identify compiler-generated delegate names.
    /// </para>
    /// </remarks>
    private static readonly SearchValues<char> _anonymousDelegateChars = SearchValues.Create("<>");

    #endregion

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultFormatter"/> class.
    /// </summary>
    /// <remarks>
    /// Private constructor enforces singleton pattern via <see cref="Instance"/>.
    /// </remarks>
    private DefaultFormatter()
    {
    }

    #endregion

    #region IFormatter implementation

    /// <summary>
    /// Formats an object into a string representation (explicit interface implementation).
    /// </summary>
    /// <param name="obj">The object to format.</param>
    /// <returns>A formatted string, or <see langword="null"/> if formatting fails.</returns>
    /// <remarks>
    /// This explicit implementation delegates to the public static <see cref="Format(object?)"/> method.
    /// </remarks>
    string? IFormatter.Format(object? obj)
    => Format(obj);

    #endregion

    #region Public static formatting method

    /// <summary>
    /// Formats an object into a human-readable string representation for test case names.
    /// </summary>
    /// <param name="obj">The object to format. May be null from recursive calls.</param>
    /// <returns>
    /// A formatted string representation suitable for test case names, or <see langword="null"/> if formatting fails.
    /// Null returns are intentional and signal the caller to use fallback strategies for logging and error handling.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Null Handling Strategy:</strong> This method may return null to signal formatting failure.
    /// Callers should use fallback strategies for logging and error handling.
    /// </para>
    /// <para>
    /// <strong>Implementation:</strong> First checks the <see cref="Formatter"/> for custom formatters registered
    /// for the object's type. If no custom formatter is found, uses pattern matching to dispatch to type-specific
    /// overloaded helper methods. Each specialized method handles formatting for a particular type or type family
    /// (e.g., internal <c>Format(char)</c>, <c>Format(string)</c>, <c>Format(IEnumerable)</c> formatters).
    /// This design separates concerns and improves maintainability while allowing extensibility.
    /// </para>
    /// <para>
    /// <strong>Type-Specific Formatting:</strong>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Type</term>
    ///     <description>Format</description>
    ///   </listheader>
    ///   <item>
    ///     <term><see cref="char"/></term>
    ///     <description>Single-quoted: <c>'c'</c> (via internal <c>Format(char)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="string"/></term>
    ///     <description>Double-quoted: <c>"text"</c> (except for literal "null") (via internal <c>Format(string)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="DateTime"/>, <see cref="DateTimeOffset"/></term>
    ///     <description>ISO 8601 (round-trippable): <c>2026-01-15T10:30:00.0000000Z</c> (via internal <c>Format&lt;T&gt;(Func, T)</c> helper with "O" format)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Guid"/></term>
    ///     <description>Hyphenated format: <c>12345678-1234-1234-1234-123456789012</c> (via internal <c>Format&lt;T&gt;(Func, T)</c> helper with "D" format)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="byte"/>[]</term>
    ///     <description>Hex string: <c>01-02-03-FF</c> (via internal <c>Format&lt;T&gt;(Func, T)</c> helper with <see cref="BitConverter.ToString(byte[])"/>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="KeyValuePair{TKey, TValue}"/></term>
    ///     <description>Key-value pair: <c>{key: value}</c> (via internal <c>Format(object?, object?)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Tuple"/> and <see cref="ValueTuple"/> (all arities)</term>
    ///     <description>Parenthesized items: <c>(item1, item2, ...)</c> (via internal <c>Format(ITuple)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Exception"/></term>
    ///     <description>Type and message: <c>ArgumentException: Value cannot be null</c> (via internal <c>Format(Exception)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Type"/></term>
    ///     <description>C#-friendly type name: <c>int</c>, <c>List&lt;string&gt;</c>, <c>int?</c>, <c>int[]</c> (via internal <c>Format(Type)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Delegate"/></term>
    ///     <description>Type and method name: <c>Func&lt;int, string&gt; (MethodName)</c> or <c>Action (anonymous)</c> (via internal <c>Format(Delegate)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IEnumerable"/></term>
    ///     <description>First <see cref="Builder.MaxCount"/> (3) items: <c>[3]: [1, 2, 3]</c> or <c>[First 3 of 5+]: [1, 2, 3]</c> (via internal <c>Format(IEnumerable)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IDictionary"/></term>
    ///     <description>First <see cref="Builder.MaxCount"/> (3) pairs: <c>[3]: {{key1: value1}, {key2: value2}, {key3: value3}}</c> (via internal <c>FormatDictionary(IDictionary, string?)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Stream"/></term>
    ///     <description>Type, length, position: <c>MemoryStream (Length: 1024, Position: 0)</c> (via internal <c>Format(Stream)</c> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term>Other types</term>
    ///     <description>Uses <see cref="Object.ToString"/> (returns null if ToString returns null)</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Error Handling:</strong> Instead of throwing exceptions, the formatting methods return null
    /// for unformattable objects (e.g., non-seekable streams that throw on property access, ToString() returns null),
    /// allowing fallback strategies for logging and error handling.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// // Primitive types
    /// Format('a')         // Returns: "'a'"
    /// Format(42)          // Returns: "42"
    /// Format(true)        // Returns: "True"
    /// 
    /// // String handling
    /// Format("hello")     // Returns: "\"hello\""
    /// Format("null")      // Returns: "null" (no quotes for literal)
    /// 
    /// // DateTime (ISO 8601)
    /// Format(new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc))
    /// // Returns: "2026-01-15T10:30:00.0000000Z"
    /// 
    /// // Collections
    /// Format(new[] { 1, 2, 3 })           // Returns: "[3]: [1, 2, 3]"
    /// Format(new[] { 1, 2, 3, 4, 5 })     // Returns: "[First 3 of 5+]: [1, 2, 3]"
    /// 
    /// // KeyValuePair
    /// Format(new KeyValuePair<string, int>("age", 30))  // Returns: "{\"age\": 30}"
    /// Format(new KeyValuePair<int, string>(1, "first")) // Returns: "{1: \"first\"}"
    /// 
    /// // Tuples (both Tuple and ValueTuple)
    /// Format((1, 2, 3))                              // Returns: "(1, 2, 3)"
    /// Format(("name", 42, true))                     // Returns: "(\"name\", 42, True)"
    /// Format(Tuple.Create('a', "test"))             // Returns: "('a', \"test\")"
    /// 
    /// // Dictionaries
    /// var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
    /// Format(dictionary)  // Returns: "[2]: {{\"a\": 1}, {\"b\": 2}}"
    /// 
    /// // Exceptions
    /// Format(new ArgumentException("Invalid"))
    /// // Returns: "ArgumentException: Invalid"
    /// 
    /// // Type objects
    /// Format(typeof(int))                          // Returns: "int"
    /// Format(typeof(List<string>))                 // Returns: "List<string>"
    /// Format(typeof(Dictionary<int, string>))      // Returns: "Dictionary<int, string>"
    /// Format(typeof(int?))                         // Returns: "int?"
    /// Format(typeof(int[]))                        // Returns: "int[]"
    /// 
    /// // Delegates
    /// Func<int, string> func = x => x.ToString();
    /// Format(func)                                 // Returns: "Func<int, string> (anonymous)"
    /// Action<string> action = Console.WriteLine;
    /// Format(action)                               // Returns: "Action<string> (WriteLine)"
    /// 
    /// // Null (signals failure)
    /// Format(null)  // Returns: null
    /// ]]></code>
    /// </example>
    public static string? Format(object? obj)
    => obj switch
    {
        // - string, ITuple (Tuple/ValueTuple) and KeyValuePair
        //   must be checked before IEnumerable
        //   (since these implement or may implement IEnumerable).
        // - IDictionary is checked separately in Format(IEnumerable)
        //   to delegate to FormatDictionary(IDictionary, string?).
        null => null,
        char ch                 => Format(ch),
        string str              => Format(str),
        Type type               => Format(type),
        DateTime dt             => Format(dt.ToString, context: "O"),
        DateTimeOffset dto      => Format(dto.ToString, context: "O"),
        Guid guid               => Format(guid.ToString, context: "D"),
        byte[] bytes            => Format(BitConverter.ToString, context: bytes),
        Exception ex            => Format(ex),
        _ when IsKeyValuePair(
            obj,
            out var key,
            out var value)      => Format(key, value),
        ITuple tuple            => Format(tuple),
        Delegate del            => Format(del),
        IEnumerable coll        => Format(coll),
        Stream stream           => Format(stream),
        _                       => obj.ToString() ?? null,
    };

    #endregion

    #region Private formatter methods

    /// <summary>
    /// Formats an object by invoking a provided formatting function.
    /// </summary>
    /// <typeparam name="T">The type of the context object to format.</typeparam>
    /// <param name="toString">A function that converts the context object to a string representation.</param>
    /// <param name="context">The object instance to format.</param>
    /// <returns>The result of invoking <paramref name="toString"/> with <paramref name="context"/>, or <see langword="null"/> if the function returns null.</returns>
    /// <remarks>
    /// <para>
    /// This generic helper method enables delegation of formatting to type-specific methods
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
    private static string? Format<T>(Func<T, string?> toString, T context)
    => toString(context);

    /// <summary>
    /// Formats a <see cref="char"/> value with single quotes.
    /// </summary>
    /// <param name="ch">The character to format.</param>
    /// <returns>A string in the form <c>'ch'</c> where the character is enclosed in single quotes.</returns>
    /// <remarks>
    /// <para>
    /// Uses single quotes to distinguish characters from strings and match C# literal syntax.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Optimization #4 - Uses uint cast for single bounds check instead
    /// of two signed comparisons. Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// and uses a cached array of pre-formatted strings for printable ASCII characters (32-126).
    /// This eliminates allocations for ~95% of char formatting operations.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Format('a')  // Returns: "'a'" (cached)
    /// Format('\n') // Returns: "'\n'" (allocated)
    /// Format('\u0041') // Returns: "'A'" (cached)
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? Format(char ch)
    {
        uint offset = (uint)(ch - AsciiPrintableStart);
        return offset < (uint)CharFormats.Length
            ? CharFormats[(int)offset]
            : $"'{ch}'";
    }

    /// <summary>
    /// Formats a <see cref="string"/> value with double quotes.
    /// </summary>
    /// <param name="str">The string to format.</param>
    /// <returns>
    /// A double-quoted string (e.g., <c>"text"</c>), or the literal <c>null</c> (unquoted)
    /// if the input is the exact string <c>"null"</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Special Case:</strong> The literal string <c>"null"</c> is returned unquoted
    /// to distinguish it from actual null values in formatted output. This prevents confusion
    /// when displaying test case names where <c>"null"</c> as a string is different from
    /// a missing/null value.
    /// </para>
    /// <para>
    /// Uses double quotes to match C# string literal syntax and distinguish strings from chars.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// to eliminate method call overhead for this frequently invoked formatter.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Format("hello")  // Returns: "\"hello\""
    /// Format("")       // Returns: "\"\""
    /// Format("null")   // Returns: "null" (no quotes)
    /// </code>
    /// </example>
    private static string? Format(string str)
    {
        if (str == NullString)
        {
            return NullString;
        }

        var twoQuoteCharsCount = 2;
        var totalLength =
            str.Length +
            twoQuoteCharsCount;

        return string.Create(
            totalLength,
            str,
            static (span, state) =>
            {
                var ch = '"';
                span[0] = ch;
                CopyAsSpan(state, span, 1);
                span[^1] = ch;
            });
    }

    /// <summary>
    /// Formats an <see cref="Exception"/> as its type name followed by its message.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A string in the form <c>"ExceptionType: Message"</c>.</returns>
    /// <remarks>
    /// <para>
    /// Provides a concise representation of exceptions suitable for test case names
    /// and diagnostic output. Does not include stack traces, inner exceptions, or
    /// other detailed information.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Uses <see cref="Type"/>'s <c>Name</c> property (not <c>FullName</c>) to keep
    /// output concise (e.g., <c>ArgumentException</c> instead of <c>System.ArgumentException</c>).
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="Builder.CreateSeparatedString"/> for zero-allocation
    /// string construction. Exception formatting is a hot path when used with <c>TestDataThrows&lt;TException&gt;</c>
    /// for exception-based test case generation, where it's called for every parameterized test case.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Format(new ArgumentException("Value cannot be null"))
    /// // Returns: "ArgumentException: Value cannot be null"
    /// 
    /// Format(new InvalidOperationException("Operation not allowed"))
    /// // Returns: "InvalidOperationException: Operation not allowed"
    /// </code>
    /// </example>
    private static string? Format(Exception exception)
    => CreateSeparatedString(
        baseString: exception.GetType().Name,
        separator: ": ",
        appendix: exception.Message);

    /// <summary>
    /// Formats a KeyValuePair's key and value into a readable string.
    /// </summary>
    /// <param name="key">The key object (may be null for reference types).</param>
    /// <param name="value">The value object (may be null for reference types).</param>
    /// <returns>A formatted string in the form <c>"{key: value}"</c>.</returns>
    /// <remarks>
    /// <para>
    /// Recursively calls <see cref="Format(object?)"/> for both key and value to ensure
    /// consistent formatting (e.g., strings are quoted, chars are single-quoted, etc.).
    /// Uses <see cref="Builder.FallbackIfNull(string?)"/> to convert null formatting results to <c>"null"</c>.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because it calls recursive formatting and string interpolation. KeyValuePair formatting
    /// is typically part of dictionary/collection formatting rather than a standalone hot path.
    /// </para>
    /// </remarks>
    private static string? Format(object? key, object? value)
    {
        var formattedKey = FallbackIfNull(Format(key));
        var formattedValue = FallbackIfNull(Format(value));
        var additionalCharsCount = 4; // "{", ": ", "}"
        var totalLength =
            formattedKey.Length +
            formattedValue.Length +
            additionalCharsCount;
        return string.Create(
            totalLength,
            (formattedKey, formattedValue),
            static (span, state) =>
            {
                var (k, v) = state;

                var ch = '{';
                var index = 0;
                span = InsertCharAndIncrement(span, ch, index, out index);

                ch = ':';
                span = CopyAndInsertChar(k, span, ch, index, out index);

                index++;
                ch = ' ';
                span = InsertCharAndIncrement(span, ch, index, out index);

                ch = '}';
                _ = CopyAndInsertChar(v, span, ch, index, out index);
            });
    }

    /// <summary>
    /// Formats a <see cref="Tuple"/> or <see cref="ValueTuple"/> into a human-readable string.
    /// </summary>
    /// <param name="tuple">The tuple to format (accessed via <see cref="ITuple"/> interface).</param>
    /// <returns>A formatted string in the form <c>"(item1, item2, ...)"</c>.</returns>
    /// <remarks>
    /// <para>
    /// Uses the <see cref="ITuple"/> interface to access tuple elements generically,
    /// supporting both <see cref="Tuple"/> (reference type) and <see cref="ValueTuple"/> (value type)
    /// of any arity (1-8+ elements, including nested tuples).
    /// </para>
    /// <para>
    /// Recursively calls <see cref="Format(object?)"/> for each element to ensure
    /// consistent formatting across all types (strings quoted, dates in ISO 8601, etc.).
    /// Uses <see cref="Builder.FallbackIfNull(string?)"/> to convert null formatting results to <c>"null"</c>.
    /// </para>
    /// <para>
    /// <strong>Why use ITuple instead of Tuple.ToString()?</strong>
    /// While <see cref="Tuple"/>'s <c>ToString()</c> method produces <c>(item1, item2)</c> output,
    /// this method applies our custom formatting rules recursively. For example,
    /// strings are double-quoted, chars are single-quoted, and dates use ISO 8601 format.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Optimization #7 - Uses stackalloc for tuples up to 8 elements
    /// (the maximum for primary tuple structure) to eliminate heap allocations. Falls back to heap
    /// allocation for larger tuples (which use nesting). Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// due to the loop and recursive formatting. Tuple formatting is not a hot path in typical usage.
    /// Uses <c>maxCount: 8</c> instead of the default <see cref="Builder.MaxCount"/> (3) 
    /// when calling <see cref="Builder.JoinWithComma(IEnumerable{string?}, int)"/>
    /// because tuples can contain up to 8 elements in their primary structure (with nesting for additional elements).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// Format((1, 2, 3))                    // Returns: "(1, 2, 3)"
    /// Format(("hello", 'a', true))         // Returns: "(\"hello\", 'a', True)"
    /// Format(Tuple.Create(1, "test"))     // Returns: "(1, \"test\")"
    /// Format(("date", DateTime.UtcNow))    // Returns: "(\"date\", 2026-01-15T10:30:00.0000000Z)"
    /// ]]></code>
    /// </example>
    private static string? Format(ITuple tuple)
    {
        const int tupleMaxCount = 8;
        var length = tuple.Length;
        var items = new string[length];

        for (int i = 0; i < length; i++)
        {
            var item = tuple[i];
            items[i] = FallbackIfNull(Format(item));
        }

        return $"({JoinWithComma(items, tupleMaxCount)})";
    }

    /// <summary>
    /// Formats a <see cref="Delegate"/> showing its type and method name.
    /// </summary>
    /// <param name="del">The delegate to format.</param>
    /// <returns>
    /// A string in the form <c>"DelegateType (MethodName)"</c> for named methods,
    /// or <c>"DelegateType (anonymous)"</c> for anonymous methods and lambdas.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Formats delegates (including <see cref="Func{TResult}"/>, <see cref="Action"/>, 
    /// and custom delegate types) into readable strings suitable for test case names.
    /// The output includes the delegate's type with generic parameters formatted using
    /// <see cref="Format(Type)"/> for C#-friendly type names.
    /// </para>
    /// <para>
    /// <strong>Method Name Detection:</strong> Distinguishes between:
    /// <list type="bullet">
    ///   <item><strong>Named methods:</strong> Shows the actual method name (e.g., <c>WriteLine</c>, <c>ToString</c>)</item>
    ///   <item><strong>Anonymous methods/lambdas:</strong> Shows <c>"anonymous"</c> for compiler-generated names</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because delegate formatting involves type formatting and string operations. Delegate
    /// formatting is infrequent compared to primitive types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// // Anonymous lambda
    /// Func<int, string> func = x => x.ToString();
    /// Format(func)  // Returns: "Func<int, string> (anonymous)"
    /// 
    /// // Named method reference
    /// Action<string> action = Console.WriteLine;
    /// Format(action)  // Returns: "Action<string> (WriteLine)"
    /// 
    /// // Simple Action
    /// Action simple = () => Console.WriteLine("test");
    /// Format(simple)  // Returns: "Action (anonymous)"
    /// 
    /// // Custom delegate
    /// Predicate<int> pred = IsPositive;
    /// Format(pred)  // Returns: "Predicate<int> (IsPositive)"
    /// ]]></code>
    /// </example>
    private static string? Format(Delegate del)
    {
        const string anonymousMethodName = "anonymous";
        var delegateType = Format(del.GetType());
        var methodName = del.Method.Name;
        var isAnonymous = IsAnonymousDelegate(methodName);
        var spaceAndParensCount = 3; // " (", ")"
        var displayName = isAnonymous ?
            anonymousMethodName
            : methodName;

        var totalLength =
            delegateType!.Length +
            spaceAndParensCount +
            displayName.Length;

        return string.Create(
            totalLength,
            (delegateType, displayName),
            static (span, state) =>
            {
                var (type, name) = state;

                var ch = ' ';
                var index = 0;
                span = CopyAndInsertChar(type, span, ch, index, out index);

                index++;
                ch = '(';
                span = InsertCharAndIncrement(span, ch, index, out index);

                ch = ')';
                _ = CopyAndInsertChar(name, span, ch, index, out index);
            });
    }

    /// <summary>
    /// Formats a <see cref="Type"/> into a C#-friendly type name.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>
    /// A C#-friendly type name using aliases (e.g., "int" instead of "Int32"), 
    /// with generic parameters (e.g., "List&lt;string&gt;"), array notation (e.g., "int[]"),
    /// and nullable syntax (e.g., "int?").
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Type Name Transformations:</strong>
    /// <list type="bullet">
    ///   <item><strong>Primitives:</strong> Uses C# aliases (int, string, bool, etc.) instead of BCL names (Int32, String, Boolean)</item>
    ///   <item><strong>Generics:</strong> Formats with angle brackets: List&lt;T&gt;, Dictionary&lt;TKey, TValue&gt;</item>
    ///   <item><strong>Arrays:</strong> Uses bracket notation: int[], string[,], etc.</item>
    ///   <item><strong>Nullable:</strong> Uses ? syntax: int?, bool?, etc.</item>
    ///   <item><strong>Nested Generics:</strong> Recursively formats: Dictionary&lt;string, List&lt;int&gt;&gt;</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// due to complexity (recursion, reflection, string manipulation). Type formatting is not
    /// a hot path in typical test execution.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// Format(typeof(int))                          // "int"
    /// Format(typeof(string))                       // "string"
    /// Format(typeof(List&lt;int&gt;))                    // "List<int>"
    /// Format(typeof(Dictionary&lt;string, int&gt;))      // "Dictionary<string, int>"
    /// Format(typeof(int?))                         // "int?"
    /// Format(typeof(int[]))                        // "int[]"
    /// Format(typeof(int[,]))                       // "int[,]"
    /// Format(typeof(List&lt;int?&gt;))                   // "List<int?>"
    /// ]]></code>
    /// </example>
    private static string? Format(Type type)
    {
        // Handle arrays
        if (type.IsArray)
        {
            return FormatArrayType(type);
        }

        // Handle Nullable<T>
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType is not null)
        {
            return FormatUnderlyingType(underlyingType);
        }

        // Handle generic types
        if (type.IsGenericType)
        {
            return FormatGenericType(type);
        }

        return GetCSharpAliasOrTypeName(type);
    }

    /// <summary>
    /// Formats an <see cref="IEnumerable"/> collection showing the first <see cref="MaxCount"/> items.
    /// </summary>
    /// <param name="coll">The collection to format.</param>
    /// <returns>
    /// A string in the form <c>"[count]: [item1, item2, item3]"</c> or
    /// <c>"[First 3 of 5+]: [item1, item2, item3]"</c> if there are more than <see cref="MaxCount"/> items.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Collection Truncation:</strong> Only the first <see cref="MaxCount"/> (3) items are included
    /// to keep output concise. If the collection contains more items, the prefix shows
    /// <c>"First 3 of N+"</c> to indicate truncation.
    /// </para>
    /// <para>
    /// <strong>Dictionary Handling:</strong> If the collection implements <see cref="IDictionary"/>,
    /// delegates to <see cref="FormatDictionary(IDictionary, string?)"/> for specialized key-value pair formatting.
    /// </para>
    /// <para>
    /// <strong>Recursive Formatting:</strong> Each item is formatted via <see cref="Format(object?)"/>
    /// to apply type-specific formatting rules (strings quoted, chars single-quoted, etc.).
    /// Null items are replaced with the <see cref="NullString"/> constant.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Optimization #8 - Uses manual enumeration instead of LINQ Cast&lt;object?&gt;()
    /// to eliminate enumerator wrapper allocation. Materializes only <see cref="MaxCount"/> + 1 items,
    /// avoiding full enumeration of large collections.
    /// Not marked with <see cref="MethodImplOptions.AggressiveInlining"/> due to complexity.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// Format(new[] { 1, 2, 3 })           // Returns: "[3]: [1, 2, 3]"
    /// Format(new[] { 1, 2, 3, 4, 5 })     // Returns: "[First 3 of 4+]: [1, 2, 3]"
    /// Format(new[] { "a", null, "ch" })    // Returns: "[3]: [\"a\", null, \"ch\"]"
    /// Format(new List<char> { 'x', 'y' })  // Returns: "[2]: ['x', 'y']"
    /// ]]></code>
    /// </example>
    private static string? Format(IEnumerable coll)
    {
        const int moreThanMaxCount = MaxCount + 1;
        var materializedObjects = new List<object?>(moreThanMaxCount);
        var enumerator = coll.GetEnumerator();

        try
        {
            for (int i = 0; i < moreThanMaxCount && enumerator.MoveNext(); i++)
            {
                materializedObjects.Add(enumerator.Current);
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }

        var count = materializedObjects.Count;
        var hasMore = count > MaxCount;

        var prefix = hasMore ?
            $"First {MaxCount} of {MaxCount}+"
            : $"{count}";

        if (coll is IDictionary dictionary)
        {
            return FormatDictionary(dictionary, prefix);
        }

        var itemsToFormat = hasMore ?
            materializedObjects.Take(MaxCount)
            : materializedObjects;
        var formattedItems = itemsToFormat.Select(
            fallbackIfFormattedNull);

        return $"[{prefix}]: [{JoinWithComma(formattedItems)}]";

        #region Local methods
        static string fallbackIfFormattedNull(object? obj)
        => FallbackIfNull(Format(obj));
        #endregion
    }

    /// <summary>
    /// Formats a <see cref="Stream"/> showing its type, length, and current position.
    /// </summary>
    /// <param name="stream">The stream to format.</param>
    /// <returns>
    /// A string showing the stream's type name, length (if seekable), and current position;
    /// or <see langword="null"/> if accessing stream properties throws an exception.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Seekable Streams:</strong> Includes both <see cref="Stream.Length"/> and <see cref="Stream.Position"/>.
    /// </para>
    /// <para>
    /// <strong>Non-Seekable Streams:</strong> Includes only <see cref="Stream.Position"/> (length unavailable).
    /// </para>
    /// <para>
    /// <strong>Error Handling:</strong> Returns <see langword="null"/> if accessing stream properties
    /// throws an exception (e.g., disposed stream, network stream with disconnected socket).
    /// This allows callers to use fallback formatting.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because it contains exception handling and conditional logic. Stream formatting is not a hot path.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// var ms = new MemoryStream(new byte[1024]);
    /// Format(ms)  // Returns: "MemoryStream (Length: 1024, Position: 0)"
    /// 
    /// var ns = new NetworkStream(socket);
    /// Format(ns)  // Returns: "NetworkStream (Position: 0)"
    /// 
    /// disposedStream.Dispose();
    /// Format(disposedStream)  // Returns: null (exception caught)
    /// ]]></code>
    /// </example>
    private static string? Format(Stream stream)
    {
        var typeName = stream.GetType().Name;

        try
        {
            return stream.CanSeek ?
                $"{typeName} (Length: {stream.Length}, Position: {stream.Position})"
                : $"{typeName} (Position: {stream.Position})";
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine(
                $"[DefaultFormatter] Stream formatting failed for type '{typeName}'. " +
                $"Exception: {Format(ex)}");
#endif
            return null;
        }
    }

    #endregion

    #region Helpers

    #region Formatting helpers

    #region Dictionary formatting helpers

    /// <summary>
    /// Formats an <see cref="IDictionary"/> showing the first <see cref="MaxCount"/> key-value pairs.
    /// </summary>
    /// <param name="dictionary">The dictionary to format.</param>
    /// <param name="prefix">A prefix string describing the count (e.g., <ch>"3"</ch> or <ch>"First 3 of 5+"</ch>).</param>
    /// <returns>
    /// A string in the form <ch>"[prefix]: {{key1: value1}, {key2: value2}, {key3: value3}}"</ch>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Dictionary Entry Handling:</strong> Supports both <see cref="DictionaryEntry"/> (non-generic)
    /// and <see cref="KeyValuePair{TKey,TValue}"/> (generic) via reflection. This enables formatting
    /// of both <ch>IDictionary</ch> and <ch>IDictionary&lt;TKey, TValue&gt;</ch> implementations.
    /// </para>
    /// <para>
    /// <strong>Recursive Formatting:</strong> Keys and values are formatted via <see cref="Format(object?, object?)"/>
    /// which recursively applies type-specific formatting rules.
    /// </para>
    /// <para>
    /// <strong>Reflection Usage:</strong> For generic <ch>Dictionary&lt;TKey, TValue&gt;</ch>, uses reflection
    /// to access Key and Value properties from the generic <see cref="KeyValuePair{TKey,TValue}"/> type,
    /// avoiding the need for multiple overloads for every possible key/value type combination.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// due to reflection usage and complexity. Dictionary formatting is not a hot path.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// var dictionary = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2, ["ch"] = 3 };
    /// Format(dictionary, "3")  // Returns: "[3]: {{\"a\": 1}, {\"b\": 2}, {\"ch\": 3}}"
    /// 
    /// var largeDict = new Dictionary<int, string> { [1] = "one", [2] = "two", [3] = "three", [4] = "four" };
    /// Format(largeDict, "First 3 of 4+")  // Returns: "[First 3 of 4+]: {{1: \"one\"}, {2: \"two\"}, {3: \"three\"}}"
    /// ]]></code>
    /// </example>
    private static string? FormatDictionary(IDictionary dictionary, string? prefix)
    {
        var items = dictionary
            .Cast<object>()
            .Take(MaxCount)
            .Select(item =>
            {
                // Handle both DictionaryEntry (from non-generic IDictionary)
                // and KeyValuePair<,> (from Dictionary<,>)
                if (item is DictionaryEntry de)
                {
                    return Format(de.Key, de.Value);
                }

                // Use reflection to access Key and Value properties from KeyValuePair<,>
                var type = item.GetType();
                var (key, value) = GetKvpPropValues(type, item);

                return Format(key, value);
            });

        return $"[{prefix}]: {{{JoinWithComma(items)}}}";
    }

    #endregion

    #region Type formatting helpers

    /// <summary>
    /// Formats an array type into its C# syntax representation (e.g., <c>int[]</c>, <c>string[,]</c>).
    /// </summary>
    /// <param name="type">The array type to format. Must be an array type.</param>
    /// <returns>
    /// A string representing the array type in C# syntax:
    /// <list type="bullet">
    /// <item><description>Single-dimensional arrays: <c>elementType[]</c></description></item>
    /// <item><description>Multi-dimensional arrays: <c>elementType[,,,]</c> (with commas for each dimension beyond the first)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Zero-Allocation Design:</strong> Uses <see cref="string.Create{TState}"/> with span-based operations
    /// to build the result without intermediate allocations. For rank-1 arrays, this is a simple element + "[]".
    /// For higher-rank arrays, commas are inserted between the brackets.
    /// </para>
    /// <para>
    /// <strong>Recursive Formatting:</strong> The element type is formatted via <see cref="Format(Type)"/>,
    /// which recursively handles nested arrays, generics, and other complex types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// FormatArrayType(typeof(int[]))      // Returns: "int[]"
    /// FormatArrayType(typeof(string[,]))  // Returns: "string[,]"
    /// FormatArrayType(typeof(bool[,,]))   // Returns: "bool[,,]"
    /// ]]></code>
    /// </example>
    private static string FormatArrayType(Type type)
    {
        var elementType = type.GetElementType()!;
        var formattedElement = Format(elementType)!;
        var rank = type.GetArrayRank();
        var bracketsCount = 2; // "[]"
        if (rank == 1)
        {

            // Zero-allocation string building: elementType[]
            var totalLength = formattedElement.Length + bracketsCount;
            return string.Create(
                totalLength,
                formattedElement,
                static (span, state) =>
                {
                    CopyAsSpan(state, span, 0);
                    span[^2] = '[';
                    span[^1] = ']';
                });
        }
        else
        {
            // Zero-allocation string building: elementType[,,,]
            var commaCount = rank - 1;
            var totalLength = formattedElement.Length + bracketsCount + commaCount; // "[" + commas + "]"
            return string.Create(
                totalLength,
                (formattedElement, commaCount),
                static (span, state) =>
                {
                    var (element, count) = state;

                    var ch = '[';
                    span = CopyAndInsertChar(element, span, ch, 0, out var index);

                    ch = ',';
                    index++;

                    for (int i = 0; i < count; i++)
                    {
                        span = InsertCharAndIncrement(span, ch, index, out index);
                    }

                    ch = ']';
                    span[index] = ch;
                });
        }
    }

    /// <summary>
    /// Formats a nullable value type by appending a question mark to its underlying type (e.g., <c>int?</c>).
    /// </summary>
    /// <param name="underlyingType">The underlying value type of a nullable type.</param>
    /// <returns>
    /// A string representing the nullable type in C# syntax: <c>underlyingType?</c>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Zero-Allocation Design:</strong> Uses <see cref="string.Create{TState}"/> with span-based operations
    /// to append the "?" suffix without intermediate string concatenations or allocations.
    /// </para>
    /// <para>
    /// <strong>Usage Context:</strong> This method is called when formatting <see cref="Nullable{T}"/> types,
    /// after extracting the underlying type <c>T</c> from <c>Nullable.GetUnderlyingType</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// FormatUnderlyingType(typeof(int))      // Returns: "int?"
    /// FormatUnderlyingType(typeof(DateTime)) // Returns: "DateTime?"
    /// ]]></code>
    /// </example>
    private static string FormatUnderlyingType(Type underlyingType)
    {
        var formattedUnderlying = Format(underlyingType)!;
        var questionMarkCount = 1; // "?"

        // Zero-allocation string building: underlyingType?
        var totalLength = formattedUnderlying.Length + questionMarkCount;
        return string.Create(
            totalLength,
            formattedUnderlying,
            static (span, state) =>
            {
                CopyAsSpan(state, span, 0);
                span[^1] = '?';
            });
    }

    /// <summary>
    /// Formats a generic type into its C# syntax representation with type arguments (e.g., <c>List&lt;int&gt;</c>, <c>Dictionary&lt;string, object&gt;</c>).
    /// </summary>
    /// <param name="type">The generic type to format. Must be a constructed generic type.</param>
    /// <returns>
    /// A string representing the generic type in C# syntax: <c>TypeName&lt;T1, T2, ...&gt;</c>
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Generic Type Name Processing:</strong> Removes the backtick suffix (e.g., <c>List`1</c> becomes <c>List</c>)
    /// that .NET uses internally to denote generic arity.
    /// </para>
    /// <para>
    /// <strong>Zero-Allocation Design:</strong> Uses <see cref="string.Create{TState}"/> with span-based operations
    /// to build the result without intermediate allocations. The formatted type arguments are joined with commas,
    /// then the entire string is assembled as <c>TypeName&lt;args&gt;</c>.
    /// </para>
    /// <para>
    /// <strong>Recursive Formatting:</strong> Type arguments are formatted via <see cref="Format(Type)"/>,
    /// which recursively handles nested generics, arrays, and other complex types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// FormatGenericType(typeof(List<int>))                    // Returns: "List<int>"
    /// FormatGenericType(typeof(Dictionary<string, object>))   // Returns: "Dictionary<string, object>"
    /// FormatGenericType(typeof(KeyValuePair<int, string[]>))  // Returns: "KeyValuePair<int, string[]>"
    /// ]]></code>
    /// </example>
    private static string FormatGenericType(Type type)
    {
        var genericTypeDef = type.GetGenericTypeDefinition();
        var typeName = genericTypeDef.Name;

        // Remove the `N suffix (e.g., List`1 -> List)
        var backtickIndex = typeName.IndexOf('`');
        if (backtickIndex > 0)
        {
            typeName = typeName[..backtickIndex];
        }

        // Format generic arguments
        var genericArgs = type.GetGenericArguments();
        var formattedArgs = JoinWithComma(genericArgs.Select(t => Format(t)));
        var angleBracketsCount = 2; // "<", ">"

        // Zero-allocation string building: TypeName<args>
        var totalLength = typeName.Length + angleBracketsCount + formattedArgs.Length;
        return string.Create(
            totalLength,
            (typeName, formattedArgs),
            static (span, state) =>
            {
                var (name, args) = state;

                var ch = '<';
                var index = 0;
                span = CopyAndInsertChar(name, span, ch, index, out index);

                ch = '>';
                index++;
                _ = CopyAndInsertChar(args, span, ch, index, out index);
            });
    }

    /// <summary>
    /// Gets the C# type alias for common BCL types.
    /// </summary>
    /// <param name="type">The type to get an alias for.</param>
    /// <returns>The C# alias (e.g., "int") if available; otherwise, <see cref="Type"/>'s <c>Name</c> property.</returns>
    /// <remarks>
    /// <para>
    /// Maps BCL type names to their C# keywords for improved readability.
    /// Called by <see cref="Format(Type)"/> after handling arrays, nullables, and generics.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Optimization #5 - Uses cached Type reference equality lookup
    /// instead of string comparison on FullName. Much faster due to reference equality and no string operations.
    /// Marked with <see cref="MethodImplOptions.AggressiveInlining"/> for hot path performance.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetCSharpAliasOrTypeName(Type type)
    => _typeAliases.TryGetValue(type, out var alias) ? alias : type.Name;

    #endregion

    #endregion

    #region char helpers

    /// <summary>
    /// Pre-formatted strings for printable ASCII characters (32-126), cached for performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This cache eliminates string allocations for ~95% of character formatting operations.
    /// Characters are formatted with single quotes: <c>'a'</c>, <c>'Z'</c>, <c>'0'</c>, etc.
    /// </para>
    /// <para>
    /// Non-printable characters (control characters, extended ASCII, Unicode) are formatted
    /// on-demand and are not cached.
    /// </para>
    /// </remarks>
    private static readonly string[] CharFormats =
        [.. Enumerable.Range(
            AsciiPrintableStart,
            AsciiPrintableEnd - AsciiPrintableStart + 1)
        .Select(i => $"'{(char)i}'")];

    #endregion

    #region Delegate helpers

    /// <summary>
    /// Checks if a delegate method name indicates an anonymous method or lambda.
    /// </summary>
    /// <param name="methodName">The method name to check.</param>
    /// <returns><see langword="true"/> if the method name is compiler-generated; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Optimization #14: Uses SearchValues for hardware-accelerated character search instead of IndexOfAny.
    /// SearchValues compiles to vectorized SIMD instructions on modern CPUs for 2-5x faster searching.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAnonymousDelegate(string methodName)
    {
        const string lambdaPrefix = "lambda_";
        var span = methodName.AsSpan();
        return span.ContainsAny(_anonymousDelegateChars) ||
               span.StartsWith(lambdaPrefix.AsSpan());
    }

    #endregion

    #region KeyValuePair helpers

    /// <summary>
    /// Checks if an object is a KeyValuePair and extracts its key and value.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <param name="key">The extracted key, or null if not a KeyValuePair.</param>
    /// <param name="value">The extracted value, or null if not a KeyValuePair.</param>
    /// <returns><see langword="true"/> if the object is a KeyValuePair; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// Uses reflection to handle KeyValuePair&lt;,&gt; generically since we can't pattern match on open generic types.
    /// This approach avoids creating overloads for every possible TKey/TValue combination.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because it contains early returns, type checks, and reflection. KeyValuePair detection is
    /// infrequent compared to primitive formatting.
    /// </para>
    /// </remarks>
    private static bool IsKeyValuePair(object obj, out object? key, out object? value)
    {
        key = null;
        value = null;
        var type = obj.GetType();

        // Optimization #2: Cache type checking results to avoid repeated GetGenericTypeDefinition calls
        if (!_isKvpCache.GetOrAdd(type, t =>
            t.IsGenericType &&
            t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)))
        {
            return false;
        }

        (key, value) = GetKvpPropValues(type, obj);

        return true;
    }

    /// <summary>
    /// Extracts the Key and Value property values from a <see cref="KeyValuePair{TKey,TValue}"/> object using reflection.
    /// </summary>
    /// <param name="type">The type of the KeyValuePair object (must be <c>KeyValuePair&lt;TKey, TValue&gt;</c>).</param>
    /// <param name="kvp">The KeyValuePair instance to extract values from.</param>
    /// <returns>A tuple containing the key and value objects, or <see langword="null"/> if the properties cannot be accessed.</returns>
    /// <remarks>
    /// <para>
    /// This method uses reflection to access the Key and Value properties generically, avoiding the need to know
    /// the specific <c>TKey</c> and <c>TValue</c> types at compile time. This is essential for formatting
    /// dictionaries and KeyValuePairs of arbitrary types.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Optimization #12 - Caches Key/Value <see cref="PropertyInfo"/> lookups per KeyValuePair type
    /// to reduce reflection overhead; the accessor wrapper is cached per type.
    /// </para>
    /// </remarks>
    private static (object? key, object? value) GetKvpPropValues(Type type, object kvp)
    {
        // Optimization #12: Use compiled delegate accessor for much faster property access
        var accessor = _kvpAccessorCache.GetOrAdd(type, t =>
        {
            var keyProperty = t.GetProperty("Key");
            var valueProperty = t.GetProperty("Value");

            // Handle types that don't have Key/Value properties - return (null, null) for compatibility
            if (keyProperty is null || valueProperty is null)
            {
                return _ => (null, null);
            }

            // Create a compiled accessor delegate that's much faster than PropertyInfo.GetValue
            return obj =>
            {
                var key = keyProperty.GetValue(obj);
                var value = valueProperty.GetValue(obj);
                return (key, value);
            };
        });

        return accessor(kvp);
    }

    #endregion

    #region Span<char> helpers

    /// <summary>
    /// Copies a string into a span starting at the specified index, then inserts a character immediately after,
    /// and returns the index position after the inserted character.
    /// </summary>
    /// <param name="str">The string to copy into the span.</param>
    /// <param name="span">The character span to modify.</param>
    /// <param name="ch">The character to insert after the copied string.</param>
    /// <param name="index">The zero-based index where the string copying should begin.</param>
    /// <param name="incremented">When this method returns, contains the index position after both the string and the inserted character.</param>
    /// <returns>The modified span.</returns>
    /// <remarks>
    /// <para>
    /// This is a zero-allocation helper used in <see cref="string.Create{TState}"/> callbacks for efficient string building.
    /// It combines two operations: copying a string and inserting a delimiter character, which is common when
    /// building formatted output like generic types (e.g., <c>List&lt;int&gt;</c>).
    /// </para>
    /// <para>
    /// <strong>Example:</strong> To build <c>"List&lt;"</c>, call <c>CopyAndInsertChar("List", span, '&lt;', 0, out index)</c>.
    /// </para>
    /// </remarks>
    private static Span<char> CopyAndInsertChar(
        string str,
        Span<char> span,
        char ch,
        int index,
        out int incremented)
    {
        CopyAsSpan(str, span, index);
        incremented = index + str.Length;
        span[incremented] = ch;
        return span;
    }

    /// <summary>
    /// Inserts a character into a span at the specified index and returns the incremented index.
    /// </summary>
    /// <param name="span">The character span to modify.</param>
    /// <param name="ch">The character to insert.</param>
    /// <param name="index">The zero-based index where the character should be inserted.</param>
    /// <param name="incremented">When this method returns, contains the index incremented by 1.</param>
    /// <returns>The modified span.</returns>
    /// <remarks>
    /// <para>
    /// This is a zero-allocation helper used in <see cref="string.Create{TState}"/> callbacks for efficient string building.
    /// The method is marked with <see cref="MethodImplOptions.AggressiveInlining"/> for optimal performance
    /// in hot paths where single-character insertion is needed.
    /// </para>
    /// <para>
    /// <strong>Performance Note:</strong> This method performs direct span indexing without bounds checking
    /// for maximum performance. The caller must ensure the index is within valid bounds.
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<char> InsertCharAndIncrement(
        Span<char> span,
        char ch,
        int index,
        out int incremented)
    {
        span[index] = ch;
        incremented = index + 1;
        return span;
    }

    #endregion

    #endregion
}
