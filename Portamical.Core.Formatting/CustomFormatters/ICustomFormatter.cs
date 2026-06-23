// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Formatting.CustomFormatters;

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
/// ICustomFormatter&lt;object&gt; baseFormatter = new ObjectFormatter();
/// ICustomFormatter&lt;string&gt; stringFormatter = baseFormatter;  // ✅ Valid due to contravariance
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
/// public sealed class MoneyFormatter : ICustomFormatter&lt;Money&gt;
/// {
///     // Type-safe method - preferred for implementation
///     public string Format(Money value)
///     {
///         return $"{value.Currency} {value.Amount:N2}";
///     }
///     
///     // Explicit interface implementation for registry support
///     string? ICustomFormatter.Format(object value)
///     {
///         return value is Money money ? Format(money) : null;
///     }
/// }
/// 
/// // Register and use
/// FormatterRegister.RegisterFormatter&lt;Money&gt;(new MoneyFormatter());
/// 
/// var price = new Money { Currency = "USD", Amount = 99.99m };
/// var formatted = FormatterRegister.Format(price);
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
/// <seealso cref="Formatter.Registry"/>
public interface ICustomFormatter<in T> : IFormatter
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
    /// public class EmailFormatter : ICustomFormatter&lt;EmailAddress&gt;
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
    ///     string? ICustomFormatter.Format(object value)
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
    string Format(T value);
}
