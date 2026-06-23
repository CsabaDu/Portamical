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
/// <see cref="ICustomFormatter"/> implementations for specific types. This enables extensibility of the
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
/// <code>
/// // Define a custom formatter
/// public class PersonFormatter : ICustomFormatter
/// {
///     public string? Format(object? obj) => obj switch
///     {
///         Person p => $"{p.FirstName} {p.LastName} (Age: {p.Age})",
///         _ => null
///     };
/// }
/// 
/// // Register the formatter
/// FormatterRegister.RegisterFormatter&lt;Person&gt;(new PersonFormatter());
/// 
/// // Use it directly
/// var person = new Person { FirstName = "John", LastName = "Doe", Age = 30 };
/// string result = FormatterRegister.Format(person);
/// // Returns: "John Doe (Age: 30)"
/// 
/// // Or use through DefaultFormatter (automatically uses registered formatter)
/// string result2 = DefaultFormatter.Format(person);
/// // Returns: "John Doe (Age: 30)"
/// 
/// // Cleanup
/// FormatterRegister.UnregisterFormatter&lt;Person&gt;();
/// </code>
/// </example>
public static class Formatter
{
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
    /// <strong>Usage:</strong> Register formatters via <see cref="RegisterFormatter(Type, ICustomFormatter)"/>
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
    private static readonly ConcurrentDictionary<Type, ICustomFormatter> _registry = new();

    /// <summary>
    /// Gets the internal formatter registry for testing purposes.
    /// </summary>
    /// <value>A read-only view of the formatter registry.</value>
    /// <remarks>
    /// <para>
    /// <strong>Warning:</strong> This property is exposed primarily for unit testing
    /// and should not be used in production code. Use the public registration methods
    /// (<see cref="RegisterFormatter(Type, ICustomFormatter)"/>, <see cref="UnregisterFormatter(Type)"/>, etc.)
    /// for normal formatter management.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> The returned dictionary is thread-safe for reads and writes.
    /// </para>
    /// </remarks>
    public static IReadOnlyDictionary<Type, ICustomFormatter> Registry
    => _registry;

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
    /// int count = FormatterRegister.RegisteredFormatterCount;
    /// Console.WriteLine($"Custom formatters: {count}");
    /// </code>
    /// </example>
    public static int RegisteredFormatterCount
    => _registry.Count;

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
    /// public class MyCustomFormatter : ICustomFormatter
    /// {
    ///     public string? Format(object? obj) => obj switch
    ///     {
    ///         MyCustomType custom => $"Custom[{custom.Id}]",
    ///         _ => null
    ///     };
    /// }
    /// 
    /// // Thread-safe registration
    /// bool registered = FormatterRegister.RegisterFormatter(typeof(MyCustomType), new MyCustomFormatter());
    /// if (registered)
    /// {
    ///     // Formatter registered successfully
    ///     var formatter = FormatterRegister.GetFormatter(typeof(MyCustomType));
    ///     var result = formatter.Format(new MyCustomType { Id = 42 });
    ///     // Returns: "Custom[42]"
    /// }
    /// </code>
    /// </example>
    public static bool RegisterFormatter(Type type, ICustomFormatter formatter)
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
    /// This is a convenience overload of <see cref="RegisterFormatter(Type, ICustomFormatter)"/>
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
    /// bool registered = FormatterRegister.RegisterFormatter&lt;MyCustomType&gt;(new MyCustomFormatter());
    /// </code>
    /// </example>
    public static bool RegisterFormatter<T>(ICustomFormatter formatter)
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
    /// <code>
    /// // Unregister a formatter
    /// bool unregistered = FormatterRegister.UnregisterFormatter(typeof(MyCustomType));
    /// if (unregistered)
    /// {
    ///     // Formatter removed, will use default formatting now
    /// }
    /// </code>
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
    /// <code>
    /// // Generic unregistration (compile-time type safety)
    /// bool unregistered = FormatterRegister.UnregisterFormatter&lt;MyCustomType&gt;();
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
    /// if (FormatterRegister.IsFormatterRegistered(typeof(MyCustomType)))
    /// {
    ///     // Custom formatter is active
    /// }
    /// </code>
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
    /// <code>
    /// if (FormatterRegister.IsFormatterRegistered&lt;MyCustomType&gt;())
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
    /// formatter registry to its default state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Clear all custom formatters (e.g., in test cleanup)
    /// FormatterRegister.ClearFormatters();
    /// </code>
    /// </example>
    public static void ClearFormatters()
    => _registry.Clear();

    /// <summary>
    /// Gets the formatter registered for the specified type, or returns the default formatter if none is registered.
    /// </summary>
    /// <param name="type">The type to get a formatter for. Cannot be null.</param>
    /// <returns>
    /// The registered <see cref="ICustomFormatter"/> for the specified type, or <see cref="DefaultFormatter.Instance"/>
    /// if no custom formatter is registered.
    /// </returns>
    /// <remarks>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Get formatter for a type (custom or default)
    /// var formatter = FormatterRegister.GetFormatter(typeof(MyCustomType));
    /// var result = formatter.Format(myObject);
    /// 
    /// // Example with registered custom formatter
    /// FormatterRegister.RegisterFormatter&lt;MyType&gt;(new MyCustomFormatter());
    /// var customFormatter = FormatterRegister.GetFormatter(typeof(MyType)); // Returns MyCustomFormatter
    /// 
    /// // Example without registered formatter
    /// var defaultFormatter = FormatterRegister.GetFormatter(typeof(int)); // Returns DefaultFormatter.Instance
    /// </code>
    /// </example>
    public static ICustomFormatter GetFormatter(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (_registry.TryGetValue(type, out var formatter))
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
    /// The registered <see cref="ICustomFormatter"/> for the specified type, or <see cref="DefaultFormatter.Instance"/>
    /// if no custom formatter is registered.
    /// </returns>
    /// <remarks>
    /// This is a convenience overload of <see cref="GetFormatter(Type)"/>
    /// that uses compile-time type safety via generics.
    /// </remarks>
    public static ICustomFormatter GetFormatter<T>()
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
    /// <see cref="ICustomFormatter.Format(object?)"/> into a single call.
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
