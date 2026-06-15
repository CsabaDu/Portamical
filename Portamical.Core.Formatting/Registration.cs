// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;
using System.Collections.Concurrent;

namespace Portamical.Core.Formatting;

public static class Registration
{
    /// <summary>
    /// Thread-safe registry of custom formatters for specific types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses <see cref="ConcurrentDictionary{TKey, TValue}"/> to provide lock-free thread-safe
    /// access for concurrent reads and writes. Multiple threads can safely register formatters
    /// and formatExpected objects simultaneously without external synchronization.
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
    ///     public string? formatExpected(object? obj) => obj switch
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
    ///     var result = ValueFormatter.formatExpected(new MyCustomType { Id = 42 });
    ///     // Returns: "Custom[42]"
    /// }
    /// </code>
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
    /// formatter to its default st.
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

    public static IFormatter GetFormatter(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (_registry.TryGetValue(type, out var formatter))
        {
            return formatter;
        }

        return DefaultFormatter.Instance;
    }
}
