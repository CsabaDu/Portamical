// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Collections;
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
public sealed class DefaultFormatter : IFormatter
{
    private DefaultFormatter()
    {
    }

    string? IFormatter.Format(object? obj)
    => Format(obj);

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
    /// <code>
    /// // Use the singleton instance directly
    /// var formatter = DefaultFormatter.Instance;
    /// var result = formatter.Format(42);  // Returns: "42"
    /// 
    /// // Or use it via the interface
    /// ICustomFormatter formatter = DefaultFormatter.Instance;
    /// </code>
    /// </example>
    public static readonly IFormatter Instance = new DefaultFormatter();

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
    /// (e.g., internal <ch>Format(char)</ch>, <ch>Format(string)</ch>, <ch>Format(IEnumerable)</ch> formatters).
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
    ///     <description>Single-quoted: <ch>'ch'</ch> (via internal <ch>Format(char)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="string"/></term>
    ///     <description>Double-quoted: <ch>"text"</ch> (except for literal "null") (via internal <ch>Format(string)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="DateTime"/>, <see cref="DateTimeOffset"/></term>
    ///     <description>ISO 8601 (round-trippable): <ch>2026-01-15T10:30:00.0000000Z</ch> (via internal <ch>Format&lt;T&gt;(Func, T)</ch> helper with "O" format)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Guid"/></term>
    ///     <description>Hyphenated format: <ch>12345678-1234-1234-1234-123456789012</ch> (via internal <ch>Format&lt;T&gt;(Func, T)</ch> helper with "D" format)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="byte"/>[]</term>
    ///     <description>Hex string: <ch>01-02-03-FF</ch> (via internal <ch>Format&lt;T&gt;(Func, T)</ch> helper with <see cref="BitConverter.ToString(byte[])"/>)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="KeyValuePair{TKey, TValue}"/></term>
    ///     <description>Key-value pair: <ch>{key: value}</ch> (via internal <ch>Format(object?, object?)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Tuple"/> and <see cref="ValueTuple"/> (all arities)</term>
    ///     <description>Parenthesized items: <ch>(item1, item2, ...)</ch> (via internal <ch>Format(ITuple)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Exception"/></term>
    ///     <description>Type and message: <ch>ArgumentException: Value cannot be null</ch> (via internal <ch>Format(Exception)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Type"/></term>
    ///     <description>C#-friendly type name: <ch>int</ch>, <ch>List&lt;string&gt;</ch>, <ch>int?</ch>, <ch>int[]</ch> (via internal <ch>Format(Type)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Delegate"/></term>
    ///     <description>Type and method name: <ch>Func&lt;int, string&gt; (MethodName)</ch> or <ch>Action (anonymous)</ch> (via internal <ch>Format(Delegate)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IEnumerable"/></term>
    ///     <description>First <see cref="MaxCount"/> (3) items: <ch>[3]: [1, 2, 3]</ch> or <ch>[First 3 of 5+]: [1, 2, 3]</ch> (via internal <ch>Format(IEnumerable)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IDictionary"/></term>
    ///     <description>First <see cref="MaxCount"/> (3) pairs: <ch>[3]: {{key1: value1}, {key2: value2}, {key3: value3}}</ch> (via internal <ch>Format(IDictionary, string?)</ch> formatter)</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Stream"/></term>
    ///     <description>Type, length, position: <ch>MemoryStream (Length: 1024, Position: 0)</ch> (via internal <ch>Format(Stream)</ch> formatter)</description>
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
    /// allowing fallback strategies for logging and error handling.
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
    public static string? Format(object? obj)
    {
        // - string, ITuple (Tuple/ValueTuple) and KeyValuePair
        //   must be checked before IEnumerable
        //   (since these implement or may implement IEnumerable).
        // - IDictionary is checked separately in Format(IEnumerable)
        //   to delegate to FormatDictionary(IDictionary, string?).
        return obj switch
        {
            null                    => null,
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
    }

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
    /// <strong>Usage Example:</strong> <ch>Format(dt.ToString, "O")</ch> delegates to <ch>dt.ToString("O")</ch>.
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
    private static string? Format(char ch)
    => ch >= AsciiPrintableStart && ch <= AsciiPrintableEnd ?
        CharFormats[ch - AsciiPrintableStart]
        : $"'{ch}'";

    /// <summary>
    /// Formats a <see cref="string"/> value with double quotes.
    /// </summary>
    /// <param name="str">The string to format.</param>
    /// <returns>
    /// A double-quoted string (e.g., <ch>"text"</ch>), or the literal <ch>null</ch> (unquoted)
    /// if the input is the exact string <ch>"null"</ch>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Special Case:</strong> The literal string <ch>"null"</ch> is returned unquoted
    /// to distinguish it from actual null values in formatted output. This prevents confusion
    /// when displaying test case names where <ch>"null"</ch> as a string is different from
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
    /// <returns>A string in the form <ch>"ExceptionType: Message"</ch>.</returns>
    /// <remarks>
    /// <para>
    /// Provides a concise representation of exceptions suitable for test case names
    /// and diagnostic output. Does not include stack traces, inner exceptions, or
    /// other detailed information.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> Uses <see cref="Type"/>'s <c>Name</c> property (not <c>FullName</c>) to keep
    /// output concise (e.g., <ch>ArgumentException</ch> instead of <ch>System.ArgumentException</ch>).
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Uses <see cref="CreateSeparatedString"/> for zero-allocation
    /// string construction. Exception formatting is a hot path when used with <c>TestDataReturns&lt;TException&gt;</c>
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
    /// <returns>A formatted string in the form <ch>"{key: value}"</ch>.</returns>
    /// <remarks>
    /// <para>
    /// Recursively calls <see cref="Format(object?)"/> for both key and value to ensure
    /// consistent formatting (e.g., strings are quoted, chars are single-quoted, etc.).
    /// Uses <see cref="FallbackIfNull(string?)"/> to convert null formatting results to <ch>"null"</ch>.
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
    /// <returns>A formatted string in the form <ch>"(item1, item2, ...)"</ch>.</returns>
    /// <remarks>
    /// <para>
    /// Uses the <see cref="ITuple"/> interface to access tuple elements generically,
    /// supporting both <see cref="Tuple"/> (reference type) and <see cref="ValueTuple"/> (value type)
    /// of any arity (1-8+ elements, including nested tuples).
    /// </para>
    /// <para>
    /// Recursively calls <see cref="Format(object?)"/> for each element to ensure
    /// consistent formatting across all types (strings quoted, dates in ISO 8601, etc.).
    /// Uses <see cref="FallbackIfNull(string?)"/> to convert null formatting results to <ch>"null"</ch>.
    /// </para>
    /// <para>
    /// <strong>Why use ITuple instead of Tuple.ToString()?</strong>
    /// While <see cref="Tuple"/>'s <c>ToString()</c> method produces <ch>(item1, item2)</ch> output,
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
    /// Formats a <see cref="Delegate"/> showing its type and method name.
    /// </summary>
    /// <param name="del">The delegate to format.</param>
    /// <returns>
    /// A string in the form <ch>"DelegateType (MethodName)"</ch> for named methods,
    /// or <ch>"DelegateType (anonymous)"</ch> for anonymous methods and lambdas.
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
    ///   <item><strong>Named methods:</strong> Shows the actual method name (e.g., <ch>WriteLine</ch>, <ch>ToString</ch>)</item>
    ///   <item><strong>Anonymous methods/lambdas:</strong> Shows <ch>"anonymous"</ch> for compiler-generated names</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because delegate formatting involves type formatting and string operations. Delegate
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
        var isAnonymous = methodName.Contains('<') ||
            methodName.Contains('>') ||
            methodName.StartsWith(lambdaPrefix);
        var displayName = isAnonymous ?
            anonymousMethodName
            : methodName;
        var spaceAndParensCount = 3; // " (", ")"

        // Zero-allocation string building: DelegateType (displayName)
        var totalLength = delegateType!.Length +
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

        // Use C# type aliases for primitive types
        // or fallback to type name for non-primitive types
        return GetCSharpAliasOrTypeName(type);
    }

    /// <summary>
    /// Formats an <see cref="IEnumerable"/> collection showing the first <see cref="MaxCount"/> items.
    /// </summary>
    /// <param name="coll">The collection to format.</param>
    /// <returns>
    /// A string in the form <ch>"[count]: [item1, item2, item3]"</ch> or
    /// <ch>"[First 3 of 5+]: [item1, item2, item3]"</ch> if there are more than <see cref="MaxCount"/> items.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Collection Truncation:</strong> Only the first <see cref="MaxCount"/> (3) items are included
    /// to keep output concise. If the collection contains more items, the prefix shows
    /// <ch>"First 3 of N+"</ch> to indicate truncation.
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
    /// <strong>Performance:</strong> Uses <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, int)"/>
    /// to materialize only <see cref="MaxCount"/> + 1 items, avoiding full enumeration of large collections.
    /// Not marked with <see cref="MethodImplOptions.AggressiveInlining"/> due to complexity.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Format(new[] { 1, 2, 3 })           // Returns: "[3]: [1, 2, 3]"
    /// Format(new[] { 1, 2, 3, 4, 5 })     // Returns: "[First 3 of 4+]: [1, 2, 3]"
    /// Format(new[] { "a", null, "ch" })    // Returns: "[3]: [\"a\", null, \"ch\"]"
    /// Format(new List&lt;char&gt; { 'x', 'y' })  // Returns: "[2]: ['x', 'y']"
    /// </code>
    /// </example>
    private static string? Format(IEnumerable coll)
    {
        var materializedObjects = coll
            .Cast<object?>()
            .Take(MaxCount + 1) // Take one extra to check if there are more than MaxCount
            .ToList();
        var count = materializedObjects.Count;
        var hasMore = count > MaxCount;
        var prefix = hasMore ?
            $"First {MaxCount} of {count}+"
            : $"{count}";

        if (coll is IDictionary dictionary)
        {
            return FormatDictionary(dictionary, prefix);
        }

        var items = materializedObjects
            .Take(MaxCount)
            .Select(item => FallbackIfNull(Format(item)));

        return $"[{prefix}]: [{JoinWithComma(items)}]";
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
            return stream.CanSeek ?
                $"{typeName} (Length: {stream.Length}, Position: {stream.Position})"
                : $"{typeName} (Position: {stream.Position})";
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Helpers

    #region char helpers

    private const int AsciiPrintableStart = ' ';
    private const int AsciiPrintableEnd = '~';

    /// <summary>
    /// Pre-formatted strings for printable ASCII characters (32-126), cached for performance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This cache eliminates string allocations for ~95% of character formatting operations.
    /// Characters are formatted with single quotes: <ch>'a'</ch>, <ch>'Z'</ch>, <ch>'0'</ch>, etc.
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

    #region Span<char> helpers

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

    #endregion

    #region Formatting helpers

    #region KeyValuePair formatting helpers

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
    /// because it contains early returns, type checks, and reflection. KeyValuePair detection is
    /// infrequent compared to primitive formatting.
    /// </para>
    /// </remarks>
    private static bool IsKeyValuePair(object obj, out object? key, out object? value)
    {
        key = null;
        value = null;
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
    /// <code>
    /// var dictionary = new Dictionary&lt;string, int&gt; { ["a"] = 1, ["b"] = 2, ["ch"] = 3 };
    /// Format(dictionary, "3")  // Returns: "[3]: {{\"a\": 1}, {\"b\": 2}, {\"ch\": 3}}"
    /// 
    /// var largeDict = new Dictionary&lt;int, string&gt; { [1] = "one", [2] = "two", [3] = "three", [4] = "four" };
    /// Format(largeDict, "First 3 of 4+")  // Returns: "[First 3 of 4+]: {{1: \"one\"}, {2: \"two\"}, {3: \"three\"}}"
    /// </code>
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
    /// <strong>Design Note:</strong> Not marked with <see cref="MethodImplOptions.AggressiveInlining"/>
    /// because the switch expression is large and this is called only at the end of type-formatting logic.
    /// </para>
    /// </remarks>
    private static string GetCSharpAliasOrTypeName(Type type)
    => type.FullName switch
    {
        "System.Boolean" => "bool",
        "System.Byte"    => "byte",
        "System.SByte"   => "sbyte",
        "System.Char"    => "char",
        "System.Decimal" => "decimal",
        "System.Double"  => "double",
        "System.Single"  => "float",
        "System.Int32"   => "int",
        "System.UInt32"  => "uint",
        "System.Int64"   => "long",
        "System.UInt64"  => "ulong",
        "System.Int16"   => "short",
        "System.UInt16"  => "ushort",
        "System.Object"  => "object",
        "System.String"  => "string",
        "System.Void"    => "void",
        _                => type.Name
    };

    #endregion

    #endregion

    #endregion
}
