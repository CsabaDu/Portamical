// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Collections.Concurrent;

namespace Portamical.Core.Formatting;

/// <summary>
/// Provides a thread-safe registry for managing custom formatters for specific types.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Formatter"/> class allows registration, retrieval, and management of custom
/// <see cref="IFormatter"/> implementations for specific types. This enables extensibility of the
/// formatting system without modifying core formatter logic. Registered formatters are consulted
/// before pattern matching in <see cref="DefaultFormatter"/>, allowing domain-specific formatting rules.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Thread-safe concurrent formatter registration and unregistration</item>
///   <item>Type-specific formatter lookup and retrieval</item>
///   <item>Generic and non-generic API overloads for type safety</item>
///   <item>Centralized formatter management with inspection capabilities</item>
///   <item>Convenience <see cref="DefaultFormatter.Format(object?)"/> method for direct formatting</item>
/// </list>
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Static registry pattern with lock-free concurrent access
/// using <see cref="ConcurrentDictionary{TKey, TValue}"/>. All operations are thread-safe
/// and can be called concurrently without external synchronization.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All public methods and properties are fully thread-safe.
/// The internal registry uses lock-free atomic operations for optimal concurrent performance.
/// </para>
/// <para>
/// <strong>Integration:</strong> Works seamlessly with <see cref="DefaultFormatter"/> which
/// automatically consults registered formatters before applying default formatting rules.
/// </para>
/// <para>
/// <strong>Performance:</strong> Formatter lookups are O(1) lock-free reads. Registration
/// and unregistration operations use atomic dictionary operations without blocking.
/// </para>
/// </remarks>
/// <example>
/// <code><![CDATA[
/// // Define a custom formatter
/// public class PersonFormatter : IFormatter
/// {
///     public string? Format(object? obj) => obj switch
///     {
///         Person p => $"{p.FirstName} {p.LastName} (Age: {p.Age})",
///         _ => null
///     };
/// }
/// 
/// // Register the formatter
/// Formatter.RegisterFormatter<Person>(new PersonFormatter());
/// 
/// // Use it directly
/// var person = new Person { FirstName = "John", LastName = "Doe", Age = 30 };
/// string result = Formatter.Format(person);
/// // Returns: "John Doe (Age: 30)"
/// 
/// // Or use through DefaultFormatter (automatically uses registered formatter)
/// string result2 = DefaultFormatter.Format(person);
/// // Returns: "John Doe (Age: 30)"
/// 
/// // Cleanup
/// Formatter.UnregisterFormatter<Person>();
/// ]]></code>
/// </example>
public static class Formatter
{
    #region Registry API

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
    /// Gets the internal formatter registry for testing purposes.
    /// </summary>
    /// <value>A read-only view of the formatter registry.</value>
    /// <remarks>
    /// <para>
    /// <strong>Warning:</strong> This property is exposed primarily for unit testing
    /// and should not be used in production code. Use the public registration methods
    /// (<see cref="RegisterFormatter(Type, IFormatter)"/>, <see cref="UnregisterFormatter(Type)"/>, etc.)
    /// for normal formatter management.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> The returned dictionary is thread-safe for reads and writes.
    /// </para>
    /// </remarks>
    public static IReadOnlyDictionary<Type, IFormatter> Registry
    => _registry;

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
    /// <code><![CDATA[
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
    /// bool registered = Formatter.RegisterFormatter(typeof(MyCustomType), new MyCustomFormatter());
    /// if (registered)
    /// {
    ///     // Formatter registered successfully
    ///     var formatter = Formatter.GetFormatter(typeof(MyCustomType));
    ///     var result = formatter.Format(new MyCustomType { Id = 42 });
    ///     // Returns: "Custom[42]"
    /// }
    /// ]]></code>
    /// </example>
    public static bool RegisterFormatter(Type type, IFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(formatter);

        return _registry.TryAdd(type, formatter);
    }

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
    /// <code><![CDATA[
    /// // Generic registration (compile-time type safety)
    /// bool registered = Formatter.RegisterFormatter<MyCustomType>(new MyCustomFormatter());
    /// ]]></code>
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
    /// After unregistration, <see cref="GetFormatter(Type)"/> will fall back to the default
    /// <see cref="DefaultFormatter.Instance"/> for objects of this type.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code><![CDATA[
    /// // Unregister a formatter
    /// bool unregistered = Formatter.UnregisterFormatter(typeof(MyCustomType));
    /// if (unregistered)
    /// {
    ///     // Formatter removed, will use default formatting now
    /// }
    /// ]]></code>
    /// </example>
    public static bool UnregisterFormatter(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return _registry.TryRemove(type, out _);
    }

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
    /// <code><![CDATA[
    /// // Generic unregistration (compile-time type safety)
    /// bool unregistered = Formatter.UnregisterFormatter<MyCustomType>();
    /// ]]></code>
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
    /// <code><![CDATA[
    /// if (Formatter.IsFormatterRegistered(typeof(MyCustomType)))
    /// {
    ///     // Custom formatter is active
    /// }
    /// ]]></code>
    /// </example>
    public static bool IsFormatterRegistered(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return _registry.ContainsKey(type);
    }

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
    /// <code><![CDATA[
    /// if (Formatter.IsFormatterRegistered<MyCustomType>())
    /// {
    ///     // Custom formatter is active
    /// }
    /// ]]></code>
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
    /// formatter registry to its default state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// // Clear all custom formatters (e.g., in test cleanup)
    /// Formatter.ClearFormatters();
    /// ]]></code>
    /// </example>
    public static void ClearFormatters()
    => _registry.Clear();

    #endregion

    /// <summary>
    /// Gets the formatter registered for the specified type, or returns the default formatter if none is registered.
    /// </summary>
    /// <param name="type">The type to get a formatter for. May be null.</param>
    /// <returns>
    /// The registered <see cref="IFormatter"/> for the specified type, or <see cref="DefaultFormatter.Instance"/>
    /// if no custom formatter is registered or if <paramref name="type"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Null Handling:</strong> Passing <see langword="null"/> for <paramref name="type"/> intentionally
    /// returns <see cref="DefaultFormatter.Instance"/> as a safe fallback, avoiding null reference exceptions
    /// and providing consistent formatting behavior.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// Uses <see cref="ConcurrentDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> for lock-free reads.
    /// </para>
    /// <para>
    /// <strong>Usage Pattern:</strong> This method implements the fallback pattern where custom formatters
    /// take precedence over the default formatter. It's called internally by formatting infrastructure
    /// but can also be used directly when you need explicit formatter lookup.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// // Get formatter for a type (custom or default)
    /// var formatter = Formatter.GetFormatter(typeof(MyCustomType));
    /// var result = formatter.Format(myObject);
    /// 
    /// // Example with registered custom formatter
    /// Formatter.RegisterFormatter<MyType>(new MyCustomFormatter());
    /// var customFormatter = Formatter.GetFormatter(typeof(MyType)); // Returns MyCustomFormatter
    /// 
    /// // Example without registered formatter
    /// var defaultFormatter = Formatter.GetFormatter(typeof(int)); // Returns DefaultFormatter.Instance
    /// 
    /// // Null type returns default formatter (safe fallback)
    /// var fallbackFormatter = Formatter.GetFormatter(null); // Returns DefaultFormatter.Instance
    /// ]]></code>
    /// </example>
    public static IFormatter GetFormatter(Type? type)
    {
        if (type is not null && _registry.TryGetValue(type, out var formatter))
        {
            return formatter;
        }

        return DefaultFormatter.Instance;
    }

    /// <summary>
    /// Gets the formatter registered for the specified type, or returns the default formatter if none is registered.
    /// </summary>
    /// <typeparam name="T">The type to get a formatter for.</typeparam>
    /// <returns>
    /// The registered <see cref="IFormatter"/> for the specified type, or <see cref="DefaultFormatter.Instance"/>
    /// if no custom formatter is registered.
    /// </returns>
    /// <remarks>
    /// This is a convenience overload of <see cref="GetFormatter(Type)"/>
    /// that uses compile-time type safety via generics.
    /// </remarks>
    public static IFormatter GetFormatter<T>()
    => GetFormatter(typeof(T));

    /// <summary>
    /// Formats a value using the registered formatter for its type, or the default formatter if none is registered.
    /// </summary>
    /// <typeparam name="T">The type of the value to format.</typeparam>
    /// <param name="value">The value to format. May be null for reference types.</param>
    /// <returns>
    /// A formatted string representation of the value, or <see langword="null"/> if formatting fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method that combines <see cref="GetFormatter{T}()"/> and 
    /// <see cref="IFormatter.Format(object?)"/> into a single call.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe and can be called concurrently.
    /// </para>
    /// </remarks>
    public static string? Format<T>(T value)
    {
        var formatter = GetFormatter<T>();

        return formatter.Format(value);
    }
}

/// <summary>
/// Provides a generic base class for type-safe formatters that convert values of type <typeparamref name="T"/>
/// into human-readable string representations.
/// </summary>
/// <typeparam name="T">The type of value this formatter handles.</typeparam>
/// <remarks>
/// <para>
/// This generic abstract class extends <see cref="IFormatter"/> to provide type-safe formatting
/// for specific value types. It implements the Template Method pattern by providing the infrastructure
/// for type checking and delegation, while subclasses implement the type-specific formatting logic.
/// </para>
/// <para>
/// <strong>Design Benefits:</strong>
/// <list type="bullet">
///   <item><strong>Type Safety:</strong> Compile-time type checking eliminates casting errors</item>
///   <item><strong>Separation of Concerns:</strong> Base class handles type checking; subclasses focus on formatting</item>
///   <item><strong>Interface Compliance:</strong> Automatically implements both <see cref="IFormatter"/> and <see cref="Formatter{T}"/></item>
///   <item><strong>Reusability:</strong> Inherit utility methods from <see cref="Formatter{T}"/> base class</item>
/// </list>
/// </para>
/// <para>
/// <strong>Implementation Pattern:</strong> Subclasses need only implement <see cref="Format(T)"/>
/// with type-specific formatting logic. The base class automatically handles:
/// <list type="bullet">
///   <item>Type checking in <see cref="IFormatter.Format(object?)"/></item>
///   <item>Delegation to the type-safe <see cref="Format(T)"/> method</item>
///   <item>Returning <see langword="null"/> for incompatible types</item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Implementations should be stateless or use appropriate synchronization
/// if maintaining state, as formatters may be called concurrently from multiple threads during test execution.
/// </para>
/// <para>
/// <strong>Performance:</strong> The sealed <see cref="IFormatter.Format(object?)"/> implementation uses pattern matching
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
///             return Builder.NullString;  // Use Builder constant
///         
///         return $"PROD-{value.Id:D6}";
///     }
/// }
/// 
/// // Usage with Formatter registry
/// var formatter = new ProductIdFormatter();
/// Formatter.RegisterFormatter&lt;ProductId&gt;(formatter);
/// 
/// var productId = new ProductId { Id = 42 };
/// var formatted = Formatter.Format(productId);
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
///             return Builder.FallbackIfNull(null);  // Use Builder helper
///         
///         // Use JoinWithComma for consistent formatting
///         var parts = new[] { value.Start.ToString(), value.End.ToString() };
///         return $"[{Builder.JoinWithComma(parts)}]";
///     }
/// }
/// 
/// var range = new Range(1, 10);
/// var formatter = new RangeFormatter();
/// var result = formatter.Format(range);
/// // Result: "[1, 10]" ✅
/// </code>
/// </example>
/// <seealso cref="IFormatter"/>
/// <seealso cref="Formatter{T}"/>
/// <seealso cref="DefaultFormatter"/>
public abstract class Formatter<T> : IFormatter
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
    ///   <item><strong>Null Handling:</strong> Return <see cref="Builder.NullString"/> (<c>"null"</c>) for null values if <typeparamref name="T"/> is nullable</item>
    ///   <item><strong>Consistency:</strong> Produce the same output for equivalent values</item>
    ///   <item><strong>Conciseness:</strong> Keep output brief but descriptive (typically &lt; 50 characters)</item>
    ///   <item><strong>Clarity:</strong> Use formats that align with C# literal syntax when appropriate</item>
    ///   <item><strong>Thread Safety:</strong> Ensure the method is safe for concurrent calls</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Base Class Utilities:</strong> Implementations can leverage helper methods from <see cref="Builder"/>:
    /// <list type="bullet">
    ///   <item><see cref="Builder.FallbackIfNull(string?)"/> - Convert null to <c>"null"</c></item>
    ///   <item><see cref="Builder.JoinWithComma(IEnumerable{string?}, int)"/> - Join formatted parts</item>
    ///   <item><see cref="Builder.CreateSeparatedString(string, string, string)"/> - Zero-allocation string assembly</item>
    ///   <item><see cref="Builder.CopyAsSpan(string, Span{char}, int)"/> - Efficient string copying</item>
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
    /// // Nullable value formatter with Builder utilities
    /// public class OptionalStringFormatter : Formatter&lt;string?&gt;
    /// {
    ///     public override string Format(string? value)
    ///     {
    ///         // Use Builder helper for null handling
    ///         if (value is null)
    ///             return Builder.FallbackIfNull(null);
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
    public abstract string Format(T value);

    /// <summary>
    /// Formats an object value by checking its type and delegating to the type-safe <see cref="Format(T)"/> method.
    /// </summary>
    /// <param name="obj">The object to format. May be null.</param>
    /// <returns>
    /// A formatted string representation of the object if the type matches <typeparamref name="T"/>;
    /// otherwise, <see langword="null"/> to indicate the formatter cannot handle this type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides the bridge between the non-generic <see cref="IFormatter"/> interface
    /// (used by the <see cref="Formatter"/> registry) and the type-safe <see cref="Format(T)"/> method.
    /// </para>
    /// <para>
    /// <strong>Implementation:</strong> First checks if <paramref name="obj"/> is null and 
    /// <typeparamref name="T"/> is a nullable type (by testing <c>default(T) is null</c>). If true,
    /// delegates to <see cref="Format(T)"/> with <c>default(T)</c> (null). Otherwise, uses pattern 
    /// matching (<c>obj is T value</c>) to perform efficient type checking. If the type matches, 
    /// delegates to the abstract <see cref="Format(T)"/> method; otherwise, returns <see langword="null"/> 
    /// to signal incompatibility.
    /// </para>
    /// <para>
    /// <strong>Null Check Behavior:</strong> The implementation explicitly handles null values for nullable types:
    /// <list type="bullet">
    ///   <item><strong>Nullable reference types (e.g., <c>string?</c>):</strong> When <paramref name="obj"/> 
    ///   is null and <typeparamref name="T"/> is a reference type, null is passed to <see cref="Format(T)"/>.</item>
    ///   <item><strong>Non-nullable value types (e.g., <c>int</c>):</strong> When <paramref name="obj"/> 
    ///   is null, returns <see langword="null"/> immediately (null cannot be a value type).</item>
    ///   <item><strong>Nullable value types (e.g., <c>int?</c>):</strong> When <paramref name="obj"/> 
    ///   is null and <typeparamref name="T"/> is <see cref="Nullable{T}"/>, null is passed to <see cref="Format(T)"/>.</item>
    /// </list>
    /// This ensures type safety: the formatter only processes values it explicitly supports.
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
    /// formatter.Format(obj3);      // null ✅ (null check fails for non-nullable value type)
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
    /// formatter.Format(obj2);      // "null" ✅ (null check passes for nullable reference type)
    /// 
    /// object obj3 = 123;
    /// formatter.Format(obj3);      // null ✅ (int != string)
    /// </code>
    /// </example>
    string? IFormatter.Format(object? obj)
    {
        if (obj is T value)
        {
            return Format(value);
        }

        if (obj is null && default(T) is null)
        {
            return Format(default!);
        }

        return null;
    }
}
