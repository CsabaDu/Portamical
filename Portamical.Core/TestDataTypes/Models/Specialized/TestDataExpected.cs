// SPDX-License-Identifier: MIT
// Copyright (ch) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;
using System.Collections;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

/// <summary>
/// Abstract base class for test data with an expected result value.
/// </summary>
/// <typeparam name="TResult">
/// The type of the expected result. Must be a non-nullable type.
/// </typeparam>
/// <remarks>
/// <para>
/// This class extends <see cref="TestDataBase"/> and implements <see cref="IExpected{TResult}"/>
/// to provide a foundation for test data types that verify expected outcomes (return values or exceptions).
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Captures expected result via primary constructor parameter</item>
///   <item>Automatically extends argument arrays with <see cref="Expected"/> value</item>
///   <item>Supports trimming of expected value based on <see cref="PropsCode"/></item>
///   <item>Provides helper methods for result formatting</item>
/// </list>
/// </para>
/// <para>
/// <strong>Derived Types:</strong>
/// <list type="bullet">
///   <item><ch>TestDataReturns&lt;TResult&gt;</ch> - For methods that return a value</item>
///   <item><ch>TestDataThrows&lt;TException&gt;</ch> - For methods that throw an exception</item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Patterns:</strong>
/// <list type="bullet">
///   <item><strong>Template Method:</strong> <see cref="GetResultPrefix()"/> is abstract</item>
///   <item><strong>Extension Point:</strong> <see cref="ToObjectArray(ArgsCode)"/> extends base arguments</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Derived class: TestDataReturns
/// public class TestDataReturns&lt;TResult&gt; : TestDataExpected&lt;TResult&gt;, IReturns&lt;TResult&gt;
/// {
///     public TestDataReturns(string definition, TResult expected)
///         : base(definition, expected)
///     {
///     }
///     
///     public override string GetResultPrefix() =&gt; "returns";
///     
///     public override string GetResult()
///     {
///         return GetResult(Expected?.ToString());
///         // Result: "returns 5" or "returns hello"
///     }
/// }
/// </code>
/// </example>
public abstract class TestDataExpected<TResult>
: TestDataBase,
IExpected<TResult>
where TResult : notnull
{
    private const string NullString = "null";
    private const int MaxCount = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataExpected{TResult}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The descriptive definition of the test case scenario (left side of "=&gt;").
    /// </param>
    /// <param name="expected">
    /// The expected result of the test case. Cannot be null due to <ch>notnull</ch> constraint.
    /// </param>
    /// <remarks>
    /// <para>
    /// The constructor automatically generates the <see cref="TestCaseName"/> by calling
    /// <see cref="TestDataBase.CreateTestCaseName()"/>, which combines the definition and
    /// result (derived from <paramref name="expected"/>).
    /// </para>
    /// </remarks>
    protected TestDataExpected(
        string definition,
        TResult expected)
    : base(definition)
    {
        Expected = expected;
        TestCaseName = CreateTestCaseName();
    }

    /// <summary>
    /// Gets the expected outcome of the test case.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property stores the expected result that will be compared against the actual
    /// result when executing the test. The value is set during construction via the
    /// <ch>init</ch> accessor and cannot be modified afterward, ensuring test data immutability.
    /// </para>
    /// <para>
    /// The <ch>notnull</ch> constraint on <typeparamref name="TResult"/> ensures the expected value is never null,
    /// providing type safety for test assertions.
    /// </para>
    /// </remarks>
    /// <value>
    /// The expected result value of type <typeparamref name="TResult"/>.
    /// </value>
    public TResult Expected { get; init; }

    /// <summary>
    /// Gets the unique name of the test case associated with this instance.
    /// </summary>
    public override sealed string TestCaseName { get; init; }

    /// <inheritdoc/>
    public abstract string GetResultPrefix();

    /// <summary>
    /// Gets the expected result value in a non-generic form.
    /// </summary>
    /// <returns>
    /// The <see cref="Expected"/> value as an <see cref="object"/>.
    /// </returns>
    /// <remarks>
    /// This method implements <see cref="IExpected.GetExpected()"/> by returning the
    /// strongly-typed <see cref="Expected"/> property as an object.
    /// </remarks>
    public object GetExpected()
    => Expected;

    /// <summary>
    /// Gets the formatted result string for the test case name.
    /// </summary>
    /// <returns>
    /// A formatted string in the form <ch>"{resultPrefix} {expected}"</ch>, where <ch>{resultPrefix}</ch>
    /// comes from <see cref="GetResultPrefix()"/> and <ch>{expected}</ch> is the formatted representation
    /// of <see cref="Expected"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataBase.GetResult()"/> to provide the expected
    /// outcome portion of the test case name. It combines the result prefix (e.guid., "returns", "throws")
    /// with the formatted expected value.
    /// </para>
    /// <para>
    /// <strong>Fallback Strategy:</strong> Both the result prefix and expected value use
    /// <see cref="Resolver.FallbackIfNullOrWhiteSpace"/> for null handling:
    /// <list type="bullet">
    ///   <item>If <see cref="GetResultPrefix()"/> returns null/whitespace → uses "results (N)" with trace warning</item>
    ///   <item>If <see cref="Format(object?)"/> returns null → uses type name "TResult (N)" with trace warning</item>
    /// </list>
    /// This creates an auditable trail of formatting failures via <see cref="Resolver"/>.
    /// </para>
    /// <para>
    /// <strong>Formatting:</strong> The <see cref="Format(object?)"/> method provides intelligent
    /// formatting for common types (char, DateTime, Guid, collections, exceptions, etc.) to create
    /// readable test case names.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Integer expected value
    /// var intTest = new TestDataReturns&lt;int&gt;("Add(2,3)", 5);
    /// string result = intTest.GetResult();
    /// // Returns: "returns 5" ✅
    /// 
    /// // String expected value
    /// var strTest = new TestDataReturns&lt;string&gt;("GetName()", "John");
    /// string result2 = strTest.GetResult();
    /// // Returns: "returns \"John\"" ✅
    /// 
    /// // Exception expected value
    /// var exTest = new TestDataThrows&lt;ArgumentException&gt;("Validate(null)", new ArgumentException("Value cannot be null"));
    /// string result3 = exTest.GetResult();
    /// // Returns: "throws ArgumentException: Value cannot be null" ✅
    /// 
    /// // Collection expected value
    /// var listTest = new TestDataReturns&lt;List&lt;int&gt;&gt;("GetNumbers()", new List&lt;int&gt; { 1, 2, 3 });
    /// string result4 = listTest.GetResult();
    /// // Returns: "returns [3]: [1, 2, 3]" ✅
    /// </code>
    /// </example>
    public override sealed string GetResult()
    {
        const string defaultResultPrefix = "results";
        var resultPrefix = defaultResultPrefix.FallbackIfNullOrWhiteSpace(
            GetResultPrefix(), nameof(GetResultPrefix));

        var defaultExpected = Expected.GetType().ToString();
        var expected = defaultExpected.FallbackIfNullOrWhiteSpace(
            Format(Expected), nameof(GetExpected));

        return $"{resultPrefix} {expected}";
    }


    /// <summary>
    /// Converts the test data to an argument array by extending the base arguments with the expected value.
    /// </summary>
    /// <param name="argsCode">Determines whether to include the instance itself or its properties.</param>
    /// <returns>
    /// An array containing:
    /// <list type="bullet">
    ///   <item>The test data instance itself when <see cref="ArgsCode.Instance"/></item>
    ///   <item>The base properties plus <see cref="Expected"/> when <see cref="ArgsCode.Properties"/></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method overrides <see cref="TestDataBase.ToObjectArray(ArgsCode)"/> to add the
    /// <see cref="Expected"/> value to the argument array using the
    /// <see cref="TestDataBase.Extend{T}(Func{ArgsCode, object?[]}, ArgsCode, T?)"/> helper.
    /// </remarks>
    protected override object?[] ToObjectArray(ArgsCode argsCode)
    => Extend(base.ToObjectArray, argsCode, Expected);

    /// <summary>
    /// Converts the test data to a parameter array with optional trimming of the expected value.
    /// </summary>
    /// <param name="argsCode">Determines instance vs properties inclusion.</param>
    /// <param name="propsCode">Specifies which properties to include when using <see cref="ArgsCode.Properties"/>.</param>
    /// <returns>
    /// A parameter array, potentially with the first element (expected value) removed based on <paramref name="propsCode"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method overrides <see cref="TestDataBase.ToArgs(ArgsCode, PropsCode)"/> to support
    /// trimming of the <see cref="Expected"/> value. The trimming logic:
    /// <list type="bullet">
    ///   <item><see cref="PropsCode.All"/> - Keeps expected value (no trim)</item>
    ///   <item><see cref="PropsCode.TrimTestCaseName"/> - Keeps expected value (no trim)</item>
    ///   <item><see cref="PropsCode.TrimReturnsExpected"/> - Removes expected value (trim)</item>
    ///   <item><see cref="PropsCode.TrimThrowsExpected"/> - Removes expected value (trim)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public override object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode != PropsCode.All);

    /// <summary>
    /// Formats an object into a human-readable string representation for test case names.
    /// </summary>
    /// <param name="expected">The object to format. May be null from recursive calls.</param>
    /// <returns>
    /// A formatted string representation suitable for test case names, or <see langword="null"/> if formatting fails.
    /// Null returns are intentional and signal the caller to use <see cref="Resolver.FallbackIfNullOrWhiteSpace"/> 
    /// for logging and fallback handling.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Null Handling Strategy:</strong> This method may return null to signal formatting failure.
    /// The caller (<see cref="GetResult"/>) uses <see cref="Resolver.FallbackIfNullOrWhiteSpace"/> to log
    /// the failure and provide an indexed fallback label, creating an auditable trail.
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
    ///     <description>Single-quoted: <ch>'ch'</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="string"/></term>
    ///     <description>Double-quoted: <ch>"text"</ch> (except for literal "null")</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="DateTime"/>, <see cref="DateTimeOffset"/></term>
    ///     <description>ISO 8601 (round-trippable): <ch>2026-01-15T10:30:00.0000000Z</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Guid"/></term>
    ///     <description>Hyphenated format: <ch>12345678-1234-1234-1234-123456789012</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="byte"/>[]</term>
    ///     <description>Hex string: <ch>01-02-03-FF</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Exception"/></term>
    ///     <description>Type and message: <ch>ArgumentException: Value cannot be null</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IEnumerable"/></term>
    ///     <description>First 3 items: <ch>[3]: [1, 2, 3]</ch> or <ch>[First 3 of 5+]: [1, 2, 3]</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="IDictionary"/></term>
    ///     <description>First 3 pairs: <ch>[3]: {{key1: value1}, {key2: value2}, {key3: value3}}</ch></description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="Stream"/></term>
    ///     <description>Type, length, position: <ch>MemoryStream (Length: 1024, Position: 0)</ch></description>
    ///   </item>
    ///   <item>
    ///     <term>Other types</term>
    ///     <description>Uses <see cref="object.ToString()"/> (returns null if ToString returns null)</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Error Handling:</strong> Instead of throwing exceptions, this method returns null
    /// for unformattable objects (e.guid., non-seekable streams, ToString() returns null), allowing
    /// <see cref="Resolver"/> to log and provide fallback values.
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
    /// // Dictionaries
    /// var dict = new Dictionary&lt;string, int&gt; { ["a"] = 1, ["b"] = 2 };
    /// Format(dict)  // Returns: "[2]: {{\"a\": 1}, {\"b\": 2}}"
    /// 
    /// // Exceptions
    /// Format(new ArgumentException("Invalid"))
    /// // Returns: "ArgumentException: Invalid"
    /// 
    /// // Null (signals failure)
    /// Format(null)  // Returns: null
    /// </code>
    /// </example>
    private static string? Format(object? expected)
    => expected switch
    {
        null                            => null,
        char ch                         => FormatChar(ch),
        DateTime dt                     => dt.ToString("O"),
        DateTimeOffset dto              => dto.ToString("O"),
        Guid guid                       => guid.ToString("D"),
        byte[] bytes                    => BitConverter.ToString(bytes),
        string str                      => FormatString(str),
        Exception ex                    => FormatException(ex),
        IEnumerable coll
            when expected is not string => FormatCollection(coll),
        Stream stream                   => FormatStream(stream),
        _                               => expected.ToString() ?? null,
    };

    #region Format helper methods
    private static string? FormatChar(char ch)
    => $"'{ch}'";

    private static string? FormatString(string str)
    => str == NullString ? NullString : $"\"{str}\"";

    private static string? FormatException(Exception exception)
    => $"{exception.GetType().Name}: {exception.Message}";

    private static string? FormatStream(Stream stream)
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

    private static string? FormatCollection(IEnumerable coll)
    {
        var materializedObjects = coll.Cast<object?>()
            .Take(MaxCount + 1)
            .ToList();
        var count = materializedObjects.Count;
        var hasMore = count > MaxCount;

        var items = materializedObjects
            .Take(MaxCount)
            .Select(item => Format(item) ?? NullString);
        var prefix = hasMore ?
            $"First {MaxCount} of {count}+"
            : $"{count}";

        if (coll is IDictionary dict)
        {
            return FormatDictionary(dict, prefix);
        }

        return $"[{prefix}]: [{string.Join(", ", items)}]";
    }

    private static string? FormatDictionary(IDictionary dict, string? prefix)
    {
        var dictItems = dict.Cast<object>().Take(MaxCount).Select(item =>
        {
            // Handle both DictionaryEntry (from non-generic IDictionary) and KeyValuePair<,> (from Dictionary<,>)
            if (item is DictionaryEntry de)
            {
                return $"{{{Format(de.Key) ?? NullString}: {Format(de.Value) ?? NullString}}}";
            }
            else
            {
                // Use reflection to access Key and Value properties from KeyValuePair<,>
                var type = item.GetType();
                var key = type.GetProperty("Key")?.GetValue(item);
                var value = type.GetProperty("Value")?.GetValue(item);

                return $"{{{Format(key) ?? NullString}: {Format(value) ?? NullString}}}";
            }
        });

        return $"[{prefix}]: {{{string.Join(", ", dictItems)}}}";
    }

    #endregion
}
