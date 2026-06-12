// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using static Portamical.Core.Formatting.Model.Formatter;
using static Portamical.Core.Safety.Validator;

namespace Portamical.Core.Formatting;

/// <summary>
/// Provides static methods for formatting objects into human-readable string representations.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ValueFormatter"/> class offers specialized formatting for various .NET types,
/// optimized for creating readable test case names, diagnostic output, and logging messages.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Type-specific formatting rules (strings quoted, chars single-quoted, dates in ISO 8601, etc.)</item>
///   <item>Recursive formatting for collections, tuples, and nested structures</item>
///   <item>C#-friendly expectedType names (int instead of Int32, List&lt;string&gt; with proper syntax)</item>
///   <item>Graceful null handling - returns null to signal formatting failure for downstream handling</item>
///   <item>Configurable collection truncation (first <see cref="MaxCount"/> items)</item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Uses pattern matching with method overloading to dispatch
/// to expectedType-specific formatters, enabling extensibility and clean separation of concerns.
/// Supports custom formatter registration via <see cref="Registry"/> for specialized types.
/// </para>
/// </remarks>
public static class ValueFormatter
{
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
        [.. Enumerable.Range(32, 95).Select(i => $"'{(char)i}'")];

    /// <summary>
    /// Thread-safe registry of custom formatters for specific types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses <see cref="ConcurrentDictionary{TKey, TValue}"/> to provide lock-free thread-safe
    /// access for concurrent reads and writes. Multiple threads can safely register formatters
    /// and format objects simultaneously without external synchronization.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> Register formatters via <see cref="RegisterFormatter(Type, IFormatter)"/>
    /// for domain-specific types, complex objects, or types that need specialized string representations
    /// in test case names. Unregister via <see cref="UnregisterFormatter(Type)"/>.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Lock-free reads are O(1). Registered formatters are consulted
    /// before pattern matching, providing an efficient extension point without modifying core logic.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> All operations (register, unregister, lookup) are thread-safe.
    /// </para>
    /// </remarks>
    private static readonly ConcurrentDictionary<Type, IFormatter> _registry = new();

    /// <summary>
    /// Formats an object into a human-readable string representation for test case names.
    /// </summary>
    /// <param name="expected">The object to format. May be null from recursive calls.</param>
    /// <returns>
    /// A formatted string representation suitable for test case names, or <see langword="null"/> if formatting fails.
    /// Null returns are intentional and signal the caller to use <see cref="Resolver.FallbackIfNullOrWhiteSpace"/> 
    /// or similar fallback strategies for logging and error handling.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Null Handling Strategy:</strong> This method may return null to signal formatting failure.
    /// Callers should use <see cref="Resolver.FallbackIfNullOrWhiteSpace"/> or similar utilities to log
    /// the failure and provide an indexed fallback label, creating an auditable trail.
    /// </para>
    /// <para>
    /// <strong>Implementation:</strong> First checks the <see cref="Registry"/> for custom formatters registered
    /// for the object's type. If no custom formatter is found, uses pattern matching to dispatch to type-specific
    /// overloaded helper methods. Each specialized method handles formatting for a particular type or type family
    /// (e.g., <c>Format(char)</c>, <c>Format(string)</c>, <c>Format(IEnumerable)</c>).
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
    ///     <description>Single-quoted: <c>'c'</c> (via <c>Format(char)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="string"/></term>
    ///     <description>Double-quoted: <c>"text"</c> (except for literal "null") (via <c>Format(string)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="DateTime"/>, <see cref="DateTimeOffset"/></term>
    ///     <description>ISO 8601 (round-trippable): <c>2026-01-15T10:30:00.0000000Z</c> (via <c>Format&lt;T&gt;(Func, T)</c> with "O" format)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Guid"/></term>
    ///     <description>Hyphenated format: <c>12345678-1234-1234-1234-123456789012</c> (via <c>Format&lt;T&gt;(Func, T)</c> with "D" format)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="byte"/>[]</term>
    ///     <description>Hex string: <c>01-02-03-FF</c> (via <c>Format&lt;T&gt;(Func, T)</c> with <see cref="BitConverter.ToString"/>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="KeyValuePair{TKey, TValue}"/></term>
    ///     <description>Key-value pair: <c>{key: value}</c> (via <c>Format&lt;TKey, TValue&gt;(KeyValuePair)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Tuple"/> and <see cref="ValueTuple"/> (all arities)</term>
    ///     <description>Parenthesized items: <c>(item1, item2, ...)</c> (via <c>Format(ITuple)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Exception"/></term>
    ///     <description>Type and message: <c>ArgumentException: Value cannot be null</c> (via <c>Format(Exception)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Type"/></term>
    ///     <description>C#-friendly expectedType name: <c>int</c>, <c>List&lt;string&gt;</c>, <c>int?</c>, <c>int[]</c> (via <c>Format(Type)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Delegate"/></term>
    ///     <description>Type and method name: <c>Func&lt;int, string&gt; (MethodName)</c> or <c>Action (anonymous)</c> (via <c>Format(Delegate)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IEnumerable"/></term>
    ///     <description>First <see cref="MaxCount"/> (3) items: <c>[3]: [1, 2, 3]</c> or <c>[First 3 of 5+]: [1, 2, 3]</c> (via <c>Format(IEnumerable)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IDictionary"/></term>
    ///     <description>First <see cref="MaxCount"/> (3) pairs: <c>[3]: {{key1: value1}, {key2: value2}, {key3: value3}}</c> (via <c>Format(IDictionary, string?)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Stream"/></term>
    ///     <description>Type, length, position: <c>MemoryStream (Length: 1024, Position: 0)</c> (via <c>Format(Stream)</c>)</description>
    ///   </item>
    ///   <item>
    ///     <term>Other types</term>
    ///     <description>Uses <see cref="object.ToString()"/> (returns null if ToString returns null)</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Error Handling:</strong> Instead of throwing exceptions, the formatting methods return null
    /// for unformattable objects (e.g., non-seekable streams that throw on property access, ToString() returns null),
    /// allowing <see cref="Resolver"/> to log and provide fallback values.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
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
    /// Format(new KeyValuePair&lt;string, int&gt;("age", 30))  // Returns: "{\"age\": 30}"
    /// Format(new KeyValuePair&lt;int, string&gt;(1, "first")) // Returns: "{1: \"first\"}"
    /// 
    /// // Tuples (both Tuple and ValueTuple)
    /// Format((1, 2, 3))                              // Returns: "(1, 2, 3)"
    /// Format(("name", 42, true))                     // Returns: "(\"name\", 42, True)"
    /// Format(Tuple.Create('a', "test"))             // Returns: "('a', \"test\")"
    /// 
    /// // Dictionaries
    /// var dictionary = new Dictionary&lt;string, int&gt; { ["a"] = 1, ["b"] = 2 };
    /// Format(dictionary)  // Returns: "[2]: {{\"a\": 1}, {\"b\": 2}}"
    /// 
    /// // Exceptions
    /// Format(new ArgumentException("Invalid"))
    /// // Returns: "ArgumentException: Invalid"
    /// 
    /// // Type objects
    /// Format(typeof(int))                          // Returns: "int"
    /// Format(typeof(List&lt;string&gt;))                 // Returns: "List&lt;string&gt;"
    /// Format(typeof(Dictionary&lt;int, string&gt;))      // Returns: "Dictionary&lt;int, string&gt;"
    /// Format(typeof(int?))                         // Returns: "int?"
    /// Format(typeof(int[]))                        // Returns: "int[]"
    /// 
    /// // Delegates
    /// Func&lt;int, string&gt; func = x => x.ToString();
    /// Format(func)                                 // Returns: "Func&lt;int, string&gt; (anonymous)"
    /// Action&lt;string&gt; action = Console.WriteLine;
    /// Format(action)                               // Returns: "Action&lt;string&gt; (WriteLine)"
    /// 
    /// // Null (signals failure)
    /// Format(null)  // Returns: null
    /// </code>
    /// </example>
    public static string? Format(object? expected)
    {
        if (expected is null) return null;

        // Check custom formatter registry first (lock-free, thread-safe)
        var expectedType = expected.GetType();

        if (_registry.TryGetValue(expectedType, out var formatter))
        {
            return formatter.Format(expected);
        }

        return expected switch
        {
            // - string, ITuple (Tuple/ValueTuple) and KeyValuePair
            //   must be checked before IEnumerable
            //   (since these implement or may implement IEnumerable).
            // - IDictionary is checked separately in Format(IEnumerable)
            //   to delegate to Format(IDictionary, string?).
            char ch                 => Format(ch),
            string str              => Format(str),
            Type type               => Format(type),
            DateTime dt             => Format(dt.ToString, "O"),
            DateTimeOffset dto      => Format(dto.ToString, "O"),
            Guid guid               => Format(guid.ToString, "D"),
            byte[] bytes            => Format(BitConverter.ToString, bytes),
            Exception ex            => Format(ex),
            _ when IsKeyValuePair(
                expected,
                out var key,
                out var value)      => Format(key, value),
            ITuple tuple            => Format(tuple),
            Delegate del            => Format(del),
            IEnumerable coll        => Format(coll),
            Stream stream           => Format(stream),
            _                       => expected.ToString() ?? null,
        };
    }

    #region Formatter Registration API

    /// <summary>
    /// Registers a custom formatter for a specific type.
    /// </summary>
    /// <param name="type">The type to register the formatter for. Cannot be null.</param>
    /// <param name="formatter">The formatter implementation. Cannot be null.</param>
    /// <returns>
    /// <see langword="true"/> if the formatter was registered successfully;
    /// <see langword="false"/> if a formatter for this type already exists (no overwrite).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently
    /// from multiple threads. Uses <see cref="ConcurrentDictionary{TKey, TValue}.TryAdd"/> which
    /// guarantees atomic insertion without locks.
    /// </para>
    /// <para>
    /// <strong>Overwrite Protection:</strong> If a formatter is already registered for the type,
    /// this method returns <see langword="false"/> without modifying the existing registration.
    /// Use <see cref="UnregisterFormatter(Type)"/> first if you need to replace a formatter.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> Call during application startup or test initialization to register
    /// custom formatters for domain-specific types.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="type"/> or <paramref name="formatter"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Register a custom formatter for MyCustomType
    /// public class MyCustomFormatter : IFormatter
    /// {
    ///     public string? Format(object? obj) => obj switch
    ///     {
    ///         MyCustomType custom => $"Custom[{custom.Id}]",
    ///         _ => null
    ///     };
    /// }
    /// 
    /// // Thread-safe registration
    /// bool registered = ValueFormatter.RegisterFormatter(typeof(MyCustomType), new MyCustomFormatter());
    /// if (registered)
    /// {
    ///     // Formatter registered successfully
    ///     var result = ValueFormatter.Format(new MyCustomType { Id = 42 });
    ///     // Returns: "Custom[42]"
    /// }
    /// </code>
    /// </example>
    public static bool RegisterFormatter(Type type, IFormatter formatter)
    => _registry.TryAdd(
        NotNull(type, nameof(type)),
        NotNull(formatter, nameof(formatter)));

    /// <summary>
    /// Registers a custom formatter for a specific type using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type to register the formatter for.</typeparam>
    /// <param name="formatter">The formatter implementation. Cannot be null.</param>
    /// <returns>
    /// <see langword="true"/> if the formatter was registered successfully;
    /// <see langword="false"/> if a formatter for this type already exists (no overwrite).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload of <see cref="RegisterFormatter(Type, IFormatter)"/>
    /// that uses compile-time type safety via generics.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="formatter"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Generic registration (compile-time type safety)
    /// bool registered = ValueFormatter.RegisterFormatter&lt;MyCustomType&gt;(new MyCustomFormatter());
    /// </code>
    /// </example>
    public static bool RegisterFormatter<T>(IFormatter formatter)
    => RegisterFormatter(typeof(T), formatter);

    /// <summary>
    /// Unregisters a custom formatter for a specific type.
    /// </summary>
    /// <param name="type">The type to unregister the formatter for. Cannot be null.</param>
    /// <returns>
    /// <see langword="true"/> if the formatter was unregistered successfully;
    /// <see langword="false"/> if no formatter was registered for this type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently
    /// from multiple threads. Uses <see cref="ConcurrentDictionary{TKey, TValue}.TryRemove(TKey, out TValue)"/>
    /// which guarantees atomic removal without locks.
    /// </para>
    /// <para>
    /// After unregistration, <see cref="Format(object?)"/> will fall back to the default
    /// pattern matching logic for objects of this type.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Unregister a formatter
    /// bool unregistered = ValueFormatter.UnregisterFormatter(typeof(MyCustomType));
    /// if (unregistered)
    /// {
    ///     // Formatter removed, will use default formatting now
    /// }
    /// </code>
    /// </example>
    public static bool UnregisterFormatter(Type type)
    => _registry.TryRemove(
        NotNull(type, nameof(type)),
        out _);

    /// <summary>
    /// Unregisters a custom formatter for a specific type using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type to unregister the formatter for.</typeparam>
    /// <returns>
    /// <see langword="true"/> if the formatter was unregistered successfully;
    /// <see langword="false"/> if no formatter was registered for this type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload of <see cref="UnregisterFormatter(Type)"/>
    /// that uses compile-time type safety via generics.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Generic unregistration (compile-time type safety)
    /// bool unregistered = ValueFormatter.UnregisterFormatter&lt;MyCustomType&gt;();
    /// </code>
    /// </example>
    public static bool UnregisterFormatter<T>()
    => UnregisterFormatter(typeof(T));

    /// <summary>
    /// Checks if a custom formatter is registered for a specific type.
    /// </summary>
    /// <param name="type">The type to check. Cannot be null.</param>
    /// <returns>
    /// <see langword="true"/> if a formatter is registered for this type;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// if (ValueFormatter.IsFormatterRegistered(typeof(MyCustomType)))
    /// {
    ///     // Custom formatter is active
    /// }
    /// </code>
    /// </example>
    public static bool IsFormatterRegistered(Type type)
    => _registry.ContainsKey(NotNull(type, nameof(type)));

    /// <summary>
    /// Checks if a custom formatter is registered for a specific type using a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>
    /// <see langword="true"/> if a formatter is registered for this type;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience overload of <see cref="IsFormatterRegistered(Type)"/>
    /// that uses compile-time type safety via generics.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (ValueFormatter.IsFormatterRegistered&lt;MyCustomType&gt;())
    /// {
    ///     // Custom formatter is active
    /// }
    /// </code>
    /// </example>
    public static bool IsFormatterRegistered<T>()
    => IsFormatterRegistered(typeof(T));

    /// <summary>
    /// Clears all registered custom formatters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// Uses <see cref="ConcurrentDictionary{TKey, TValue}.Clear"/> which is atomic.
    /// </para>
    /// <para>
    /// <strong>Usage:</strong> Typically called during test teardown or when resetting the
    /// formatter to its default state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear all custom formatters (e.g., in test cleanup)
    /// ValueFormatter.ClearFormatters();
    /// </code>
    /// </example>
    public static void ClearFormatters()
    => _registry.Clear();

    /// <summary>
    /// Gets the number of currently registered custom formatters.
    /// </summary>
    /// <value>The count of registered formatters.</value>
    /// <remarks>
    /// <para>
    /// <strong>Thread Safety:</strong> This property is thread-safe. However, the count may change
    /// immediately after reading due to concurrent registrations/unregistrations from other threads.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// int count = ValueFormatter.RegisteredFormatterCount;
    /// Console.WriteLine($"Custom formatters: {count}");
    /// </code>
    /// </example>
    public static int RegisteredFormatterCount
    => _registry.Count;

    #endregion

    #region Private formatter methods
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
    private static string? Format<T>(Func<T, string?> toString, T context)
    => toString(context);

    /// <summary>
    /// Formats a <see cref="char"/> value with single quotes.
    /// </summary>
    /// <param name="ch">The character to format.</param>
    /// <returns>A string in the form <c>'c'</c> where <c>c</c> is the character value.</returns>
    /// <remarks>
    /// <para>
    /// Uses single quotes to distinguish characters from strings and match C# literal syntax.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked with <see cref="MethodImplOptions.AggressiveInlining"/>
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
        var IsAsciiPrintable = ch >= 32 && ch < 127;

        if (IsAsciiPrintable)
        {
            return CharFormats[ch - 32];
        }

        return $"'{ch}'";
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? Format(string str)
    {
        if (str == NullString)
        {
            return NullString;
        }

        // Zero-allocation string building: "str"
        var totalLength = 2 + str.Length; // Two quote characters + string content
        return string.Create(
            totalLength,
            str,
            static (span, state) =>
        {
            span[0] = '"';
            CopyAsSpan(state, span, 1);
            span[^1] = '"';
        });
    }

    /// <summary>
    /// Formats an <see cref="Exception"/> as its expectedType name followed by its message.
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
    /// <strong>Note:</strong> Uses <see cref="Type.Name"/> (not <c>FullName</c>) to keep
    /// output concise (e.g., <c>ArgumentException</c> instead of <c>System.ArgumentException</c>).
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because exception formatting is not a hot path and involves string concatenation.
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
    => $"{exception.GetType().Name}: {exception.Message}";

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
    /// Uses <see cref="FallbackIfNull(string?)"/> to convert null formatting results to <c>"null"</c>.
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

        // Zero-allocation string building: {key: value}
        var totalLength = 4 + formattedKey.Length + formattedValue.Length; // "{", ": ", "}"
        return string.Create(
            totalLength,
            (formattedKey, formattedValue),
            static (span, state) =>
        {
            var (k, v) = state;
            var index = 0;

            span[index++] = '{';
            CopyAsSpan(k, span, index);

            index += k.Length;
            span[index++] = ':';
            span[index++] = ' ';

            CopyAsSpan(v, span, index);
            index += v.Length;
            span[index] = '}';
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
    /// supporting both <see cref="Tuple"/> (reference expectedType) and <see cref="ValueTuple"/> (value expectedType)
    /// of any arity (1-8+ elements, including nested tuples).
    /// </para>
    /// <para>
    /// Recursively calls <see cref="Format(object?)"/> for each element to ensure
    /// consistent formatting across all types (strings quoted, dates in ISO 8601, etc.).
    /// Uses <see cref="FallbackIfNull(string?)"/> to convert null formatting results to <c>"null"</c>.
    /// </para>
    /// <para>
    /// <strong>Why use ITuple instead of Tuple.ToString()?</strong>
    /// While <see cref="Tuple.ToString"/> produces <c>(item1, item2)</c> output,
    /// this method applies our custom formatting rules recursively. For example,
    /// strings are double-quoted, chars are single-quoted, and dates use ISO 8601 format.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// due to the loop and recursive formatting. Tuple formatting is not a hot path in typical usage.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Format((1, 2, 3))                    // Returns: "(1, 2, 3)"
    /// Format(("hello", 'a', true))         // Returns: "(\"hello\", 'a', True)"
    /// Format(Tuple.Create(1, "test"))     // Returns: "(1, \"test\")"
    /// Format(("date", DateTime.UtcNow))    // Returns: "(\"date\", 2026-01-15T10:30:00.0000000Z)"
    /// </code>
    /// </example>
    private static string? Format(ITuple tuple)
    {
        var length = tuple.Length;
        var items = new List<string>(length);

        for (int i = 0; i < length; i++)
        {
            var item = tuple[i];
            items.Add(FallbackIfNull(Format(item)));
        }

        return $"({JoinWithComma(items)})";
    }

    /// <summary>
    /// Formats a <see cref="Delegate"/> showing its expectedType and method name.
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
    /// The output includes the delegate's expectedType with generic parameters formatted using
    /// <see cref="Format(Type)"/> for C#-friendly expectedType names.
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
    /// because delegate formatting involves expectedType formatting and string operations. Delegate
    /// formatting is infrequent compared to primitive types.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Anonymous lambda
    /// Func&lt;int, string&gt; func = x => x.ToString();
    /// Format(func)  // Returns: "Func&lt;int, string&gt; (anonymous)"
    /// 
    /// // Named method reference
    /// Action&lt;string&gt; action = Console.WriteLine;
    /// Format(action)  // Returns: "Action&lt;string&gt; (WriteLine)"
    /// 
    /// // Simple Action
    /// Action simple = () => Console.WriteLine("test");
    /// Format(simple)  // Returns: "Action (anonymous)"
    /// 
    /// // Custom delegate
    /// Predicate&lt;int&gt; pred = IsPositive;
    /// Format(pred)  // Returns: "Predicate&lt;int&gt; (IsPositive)"
    /// </code>
    /// </example>
    private static string? Format(Delegate del)
    {
        const string anonymousMethodName = "anonymous";
        const string lambdaPrefix = "lambda_";
        var delegateType = Format(del.GetType());
        var methodName = del.Method.Name;

        // Detect compiler-generated names for anonymous methods/lambdas
        var isAnonymous = methodName.Contains('<') || methodName.Contains('>') || methodName.StartsWith(lambdaPrefix);
        var displayName = isAnonymous ? anonymousMethodName : methodName;

        // Zero-allocation string building: DelegateType (displayName)
        var totalLength = delegateType!.Length + 3 + displayName.Length; // " (", ")"
        return string.Create(
            totalLength,
            (delegateType, displayName),
            static (span, state) =>
        {
            var (type, name) = state;
            var index = 0;

            CopyAsSpan(type, span, index);
            index += type.Length;

            span[index++] = ' ';
            span[index++] = '(';

            CopyAsSpan(name, span, index);
            index += name.Length;

            span[index] = ')';
        });
    }

    /// <summary>
    /// Formats a <see cref="Type"/> into a C#-friendly expectedType name.
    /// </summary>
    /// <param name="type">The expectedType to format.</param>
    /// <returns>
    /// A C#-friendly expectedType name using aliases (e.g., "int" instead of "Int32"), 
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
    /// <code>
    /// Format(typeof(int))                          // "int"
    /// Format(typeof(string))                       // "string"
    /// Format(typeof(List&lt;int&gt;))                    // "List&lt;int&gt;"
    /// Format(typeof(Dictionary&lt;string, int&gt;))      // "Dictionary&lt;string, int&gt;"
    /// Format(typeof(int?))                         // "int?"
    /// Format(typeof(int[]))                        // "int[]"
    /// Format(typeof(int[,]))                       // "int[,]"
    /// Format(typeof(List&lt;int?&gt;))                   // "List&lt;int?&gt;"
    /// </code>
    /// </example>
    private static string? Format(Type type)
    {
        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType is null)
            {
                return type.Name;
            }

            var formattedElement = Format(elementType)!;
            var rank = type.GetArrayRank();

            if (rank == 1)
            {
                // Zero-allocation string building: elementType[]
                var totalLength = formattedElement.Length + 2; // "[]"
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
                var totalLength = formattedElement.Length + 2 + commaCount; // "[" + commas + "]"
                return string.Create(
                    totalLength,
                    (formattedElement, commaCount),
                    static (span, state) =>
                {
                    var (element, count) = state;
                    CopyAsSpan(element, span, 0);

                    var index = element.Length;
                    span[index++] = '[';

                    for (int i = 0; i < count; i++)
                    {
                        span[index++] = ',';
                    }

                    span[index] = ']';
                });
            }
        }

        // Handle Nullable<T>
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType is not null)
        {
            var formattedUnderlying = Format(underlyingType)!;

            // Zero-allocation string building: underlyingType?
            var totalLength = formattedUnderlying.Length + 1; // "?"
            return string.Create(
                totalLength,
                formattedUnderlying,
                static (span, state) =>
            {
                CopyAsSpan(state, span, 0);
                span[^1] = '?';
            });
        }

        // Handle generic types
        if (type.IsGenericType)
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
            var formattedArgs = JoinWithComma(genericArgs.Select(t => Format(t) ?? t.Name));

            // Zero-allocation string building: TypeName<args>
            var totalLength = typeName.Length + 2 + formattedArgs.Length; // "<", ">"
            return string.Create(
                totalLength,
                (typeName, formattedArgs),
                static (span, state) =>
            {
                var (name, args) = state;
                var index = 0;

                CopyAsSpan(name, span, index);
                index += name.Length;

                span[index++] = '<';

                CopyAsSpan(args, span, index);
                index += args.Length;

                span[index] = '>';
            });
        }

        // Use C# expectedType aliases for primitive types
        return GetCSharpAlias(type);
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
    /// delegates to <see cref="Format(IDictionary, string?)"/> for specialized key-value pair formatting.
    /// </para>
    /// <para>
    /// <strong>Recursive Formatting:</strong> Each item is formatted via <see cref="Format(object?)"/>
    /// to apply expectedType-specific formatting rules (strings quoted, chars single-quoted, etc.).
    /// Null items are replaced with the <see cref="NullString"/> constant.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)"/>
    /// to materialize only <see cref="MaxCount"/> + 1 items, avoiding full enumeration of large collections.
    /// Not marked with <see cref="MethodImplOptions.AggressiveInlining"/> due to complexity.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Format(new[] { 1, 2, 3 })           // Returns: "[3]: [1, 2, 3]"
    /// Format(new[] { 1, 2, 3, 4, 5 })     // Returns: "[First 3 of 4+]: [1, 2, 3]"
    /// Format(new[] { "a", null, "c" })    // Returns: "[3]: [\"a\", null, \"c\"]"
    /// Format(new List&lt;char&gt; { 'x', 'y' })  // Returns: "[2]: ['x', 'y']"
    /// </code>
    /// </example>
    private static string? Format(IEnumerable coll)
    {
        var materializedObjects = coll
            .Cast<object?>()
            .Take(MaxCount + 1)
            .ToList();
        var count = materializedObjects.Count;
        var hasMore = count > MaxCount;
        var prefix = hasMore ?
            $"First {MaxCount} of {count}+"
            : $"{count}";

        if (coll is IDictionary dictionary)
        {
            return Format(dictionary, prefix);
        }

        var items = materializedObjects
            .Take(MaxCount)
            .Select(item => Format(item) ?? NullString);

        return $"[{prefix}]: [{JoinWithComma(items)}]";
    }

    /// <summary>
    /// Formats an <see cref="IDictionary"/> showing the first <see cref="MaxCount"/> key-value pairs.
    /// </summary>
    /// <param name="dictionary">The dictionary to format.</param>
    /// <param name="prefix">A prefix string describing the count (e.g., <c>"3"</c> or <c>"First 3 of 5+"</c>).</param>
    /// <returns>
    /// A string in the form <c>"[prefix]: {{key1: value1}, {key2: value2}, {key3: value3}}"</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Dictionary Entry Handling:</strong> Supports both <see cref="DictionaryEntry"/> (non-generic)
    /// and <see cref="KeyValuePair{TKey,TValue}"/> (generic) via reflection. This enables formatting
    /// of both <c>IDictionary</c> and <c>IDictionary&lt;TKey, TValue&gt;</c> implementations.
    /// </para>
    /// <para>
    /// <strong>Recursive Formatting:</strong> Keys and values are formatted via <see cref="Format(object?, object?)"/>
    /// which recursively applies expectedType-specific formatting rules.
    /// </para>
    /// <para>
    /// <strong>Reflection Usage:</strong> For generic <c>Dictionary&lt;TKey, TValue&gt;</c>, uses reflection
    /// to access Key and Value properties from the generic <see cref="KeyValuePair{TKey,TValue}"/> expectedType,
    /// avoiding the need for multiple overloads for every possible key/value expectedType combination.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// due to reflection usage and complexity. Dictionary formatting is not a hot path.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var dictionary = new Dictionary&lt;string, int&gt; { ["a"] = 1, ["b"] = 2, ["c"] = 3 };
    /// Format(dictionary, "3")  // Returns: "[3]: {{\"a\": 1}, {\"b\": 2}, {\"c\": 3}}"
    /// 
    /// var largeDict = new Dictionary&lt;int, string&gt; { [1] = "one", [2] = "two", [3] = "three", [4] = "four" };
    /// Format(largeDict, "First 3 of 4+")  // Returns: "[First 3 of 4+]: {{1: \"one\"}, {2: \"two\"}, {3: \"three\"}}"
    /// </code>
    /// </example>
    private static string? Format(IDictionary dictionary, string? prefix)
    {
        var items = dictionary
            .Cast<object>()
            .Take(MaxCount)
            .Select(item =>
        {
            // Handle both DictionaryEntry (from non-generic IDictionary) and KeyValuePair<,> (from Dictionary<,>)
            if (item is DictionaryEntry de)
            {
                return Format(de.Key, de.Value);
            }
            else
            {
                // Use reflection to access Key and Value properties from KeyValuePair<,>
                var type = item.GetType();
                var (key, value) = GetKvpPropValues(type, item);   

                return Format(key, value);
            }
        });

        return $"[{prefix}]: {{{JoinWithComma(items)}}}";
    }

    /// <summary>
    /// Formats a <see cref="Stream"/> showing its expectedType, length, and current position.
    /// </summary>
    /// <param name="stream">The stream to format.</param>
    /// <returns>
    /// A string showing the stream's expectedType name, length (if seekable), and current position;
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
    /// This allows callers to use fallback formatting via <see cref="Resolver.FallbackIfNullOrWhiteSpace"/>.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because it contains exception handling and conditional logic. Stream formatting is not a hot path.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var ms = new MemoryStream(new byte[1024]);
    /// Format(ms)  // Returns: "MemoryStream (Length: 1024, Position: 0)"
    /// 
    /// var ns = new NetworkStream(socket);
    /// Format(ns)  // Returns: "NetworkStream (Position: 0)"
    /// 
    /// disposedStream.Dispose();
    /// Format(disposedStream)  // Returns: null (exception caught)
    /// </code>
    /// </example>
    private static string? Format(Stream stream)
    {
        var typeName = stream.GetType().Name;

        try
        {
            if (stream.CanSeek)
            {
                return $"{typeName} (Length: {stream.Length}, Position: {stream.Position})";
            }

            return $"{typeName} (Position: {stream.Position})";
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Format helper methods

    private static (object? key, object? value) GetKvpPropValues(Type type, object kvp)
    {
        var key = getPropertyValue("Key");
        var value = getPropertyValue("Value");

        return (key, value);

        #region Local methods
        object? getPropertyValue(string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);
            return propertyInfo?.GetValue(kvp);
        }
        #endregion

    }

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
    /// because it contains early returns, expectedType checks, and reflection. KeyValuePair detection is
    /// infrequent compared to primitive formatting.
    /// </para>
    /// </remarks>
    private static bool IsKeyValuePair(object? obj, out object? key, out object? value)
    {
        key = null;
        value = null;

        if (obj is null)
        {
            return false;
        }

        var type = obj.GetType();

        if (!type.IsGenericType ||
            type.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
        {
            return false;
        }

        (key, value) = GetKvpPropValues(type, obj);

        return true;
    }

    /// <summary>
    /// Gets the C# expectedType alias for common BCL types.
    /// </summary>
    /// <param name="type">The expectedType to get an alias for.</param>
    /// <returns>The C# alias (e.g., "int") if available; otherwise, <see cref="Type.Name"/>.</returns>
    /// <remarks>
    /// <para>
    /// Maps BCL expectedType names to their C# keywords for improved readability.
    /// Called by <see cref="Format(Type)"/> after handling arrays, nullables, and generics.
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because the switch expression is large and this is called only at the end of expectedType-formatting logic.
    /// </para>
    /// </remarks>
    private static string GetCSharpAlias(Type type)
    => type.FullName switch
    {
        "System.Boolean"    => "bool",
        "System.Byte"       => "byte",
        "System.SByte"      => "sbyte",
        "System.Char"       => "char",
        "System.Decimal"    => "decimal",
        "System.Double"     => "double",
        "System.Single"     => "float",
        "System.Int32"      => "int",
        "System.UInt32"     => "uint",
        "System.Int64"      => "long",
        "System.UInt64"     => "ulong",
        "System.Int16"      => "short",
        "System.UInt16"     => "ushort",
        "System.Object"     => "object",
        "System.String"     => "string",
        "System.Void"       => "void",
        _                   => type.Name
    };

    #endregion
}
