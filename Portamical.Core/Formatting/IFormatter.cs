// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Formatting;

/// <summary>
/// Defines a contract for custom value formatters that convert objects to string representations
/// for test case naming.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides the extensibility mechanism for the Portamical formatting system.
/// Custom formatters can be registered in <see cref="ValueFormatter.Registry"/> to override
/// or extend the built-in formatting behavior for specific types.
/// </para>
/// <para>
/// <strong>Registry Integration:</strong> Formatters registered in <c>ValueFormatter.Registry</c>
/// are consulted <em>before</em> the built-in pattern matching logic, enabling domain-specific
/// formatting without modifying the core library.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Formatter implementations should be thread-safe as they may
/// be called concurrently from multiple test threads. Avoid mutable state or use appropriate
/// synchronization.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Prefer implementing <see cref="IFormatter{T}"/> for type-safe
/// formatters. The non-generic <see cref="IFormatter"/> interface is primarily for internal use
/// and registry storage.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implement a custom formatter for domain types
/// public class ProductIdFormatter : IFormatter&lt;ProductId&gt;
/// {
///     public string Format(ProductId value)
///     {
///         return $"PROD-{value.Id:D6}";
///     }
///     
///     // Explicit interface implementation for non-generic version
///     string? IFormatter.Format(object value)
///     {
///         return value is ProductId id ? Format(id) : null;
///     }
/// }
/// 
/// // Register the formatter globally
/// ValueFormatter.Registry[typeof(ProductId)] = new ProductIdFormatter();
/// 
/// // Now all test cases automatically use custom formatting
/// var testData = CreateTestDataReturns(
///     definition: "Get product",
///     expected: new ProductId(42),
///     arg1: userId);
/// // TestCaseName: "Get product =&gt; returns PROD-000042" ✅
/// </code>
/// </example>
/// <seealso cref="IFormatter{T}"/>
/// <seealso cref="ValueFormatter"/>
public interface IFormatter
{
    /// <summary>
    /// Formats the specified value as a string for test case naming.
    /// </summary>
    /// <param name="value">The value to format. May be null.</param>
    /// <returns>
    /// A formatted string representation of the value, or <see langword="null"/> if the formatter
    /// does not support the value's type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Implementation Guidance:</strong>
    /// <list type="bullet">
    ///   <item>Return <see langword="null"/> if the formatter does not support the value's type</item>
    ///   <item>Return <c>"null"</c> (the string literal) for null values if the type is supported</item>
    ///   <item>Avoid throwing exceptions; return <see langword="null"/> for unsupported types instead</item>
    ///   <item>Ensure thread-safety if the formatter maintains state</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Note:</strong> This method is primarily used by the registry lookup mechanism.
    /// Type-safe implementations should prefer <see cref="IFormatter{T}.Format(T)"/>.
    /// </para>
    /// </remarks>
    string? Format(object value);
}

/// <summary>
/// Defines a type-safe contract for custom value formatters that convert values of type <typeparamref name="T"/>
/// to string representations for test case naming.
/// </summary>
/// <typeparam name="T">
/// The type of value this formatter handles. Use <see langword="in"/> variance for compatibility
/// with derived types.
/// </typeparam>
/// <remarks>
/// <para>
/// This generic interface extends <see cref="IFormatter"/> to provide type-safe formatting for
/// specific value types. It is the recommended interface for implementing custom formatters.
/// </para>
/// <para>
/// <strong>Type Variance:</strong> The <see langword="in"/> modifier on <typeparamref name="T"/>
/// enables contravariance, allowing a formatter for a base type to handle derived types:
/// </para>
/// <code>
/// IFormatter&lt;object&gt; baseFormatter = new ObjectFormatter();
/// IFormatter&lt;string&gt; stringFormatter = baseFormatter;  // ✅ Valid due to contravariance
/// </code>
/// <para>
/// <strong>Implementation Pattern:</strong> Implement both <see cref="Format(T)"/> (type-safe)
/// and <see cref="IFormatter.Format(object)"/> (registry support). The non-generic method should
/// delegate to the type-safe version after type checking.
/// </para>
/// <para>
/// <strong>Null Handling:</strong> If <typeparamref name="T"/> is a nullable reference type or
/// <see cref="Nullable{T}"/>, the formatter must handle null values appropriately, typically by
/// returning <c>"null"</c>.
/// </para>
/// <para>
/// <strong>Performance:</strong> Implementations should minimize allocations and avoid blocking
/// operations, as they are called frequently during test case name generation.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Type-safe formatter for custom domain types
/// public sealed class MoneyFormatter : IFormatter&lt;Money&gt;
/// {
///     // Type-safe method - preferred for implementation
///     public string Format(Money value)
///     {
///         return $"{value.Currency} {value.Amount:N2}";
///     }
///     
///     // Explicit interface implementation for registry support
///     string? IFormatter.Format(object value)
///     {
///         return value is Money money ? Format(money) : null;
///     }
/// }
/// 
/// // Register and use
/// ValueFormatter.Registry[typeof(Money)] = new MoneyFormatter();
/// 
/// var price = new Money { Currency = "USD", Amount = 99.99m };
/// var formatted = ValueFormatter.Format(price);
/// // Result: "USD 99.99" ✅
/// 
/// // Automatically applied in test data
/// var testData = CreateTestDataReturns(
///     definition: "Get product price",
///     expected: price,
///     arg1: productId);
/// // TestCaseName: "Get product price =&gt; returns USD 99.99" ✅
/// </code>
/// </example>
/// <seealso cref="IFormatter"/>
/// <seealso cref="ValueFormatter.Registry"/>
/// <seealso cref="Formatting.Model.Formatter"/>
public interface IFormatter<in T> : IFormatter
{
    /// <summary>
    /// Formats the specified value as a string for test case naming.
    /// </summary>
    /// <param name="value">The value to format. May be null if <typeparamref name="T"/> is nullable.</param>
    /// <returns>
    /// A formatted string representation of the value. Should return <c>"null"</c> for null values
    /// if <typeparamref name="T"/> is a nullable type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Implementation Guidelines:</strong>
    /// <list type="bullet">
    ///   <item><strong>Null values:</strong> Return <c>"null"</c> for null inputs</item>
    ///   <item><strong>Conciseness:</strong> Keep output brief but descriptive (typically &lt; 50 chars)</item>
    ///   <item><strong>Clarity:</strong> Use formats that align with C# literal syntax when appropriate</item>
    ///   <item><strong>Consistency:</strong> Use the same format for equivalent values</item>
    ///   <item><strong>Thread-safety:</strong> Ensure the method is safe for concurrent calls</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Performance Considerations:</strong> This method may be called repeatedly during
    /// test execution. Optimize for common cases and avoid expensive operations like reflection,
    /// database access, or network calls.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example implementation for a custom type
    /// public class EmailFormatter : IFormatter&lt;EmailAddress&gt;
    /// {
    ///     public string Format(EmailAddress value)
    ///     {
    ///         if (value is null)
    ///             return "null";
    ///         
    ///         // Format as quoted string for clarity
    ///         return $"\"{value.LocalPart}@{value.Domain}\"";
    ///     }
    ///     
    ///     string? IFormatter.Format(object value)
    ///     {
    ///         return value is EmailAddress email ? Format(email) : null;
    ///     }
    /// }
    /// 
    /// // Usage examples
    /// var formatter = new EmailFormatter();
    /// formatter.Format(new EmailAddress("user", "example.com"));  // "user@example.com"
    /// formatter.Format(null);  // "null"
    /// </code>
    /// </example>
    string? Format(T value);
}
