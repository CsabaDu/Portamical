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
/// Custom formatters can be registered in <see cref="Formatter.Registry"/> to override
/// or extend the built-in formatting behavior for specific types.
/// </para>
/// <para>
/// <strong>Registry Integration:</strong> Formatters registered in <see cref="Formatter.Registry"/>
/// are consulted <em>before</em> the built-in pattern matching logic, enabling domain-specific
/// formatting without modifying the core library.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> Formatter implementations should be thread-safe as they may
/// be called concurrently from multiple test threads. Avoid mutable state or use appropriate
/// synchronization.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Prefer implementing <see cref="Formatter{T}"/> for type-safe
/// formatters. The non-generic <see cref="IFormatter"/> interface is primarily for internal use
/// and registry storage.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Implement a custom formatter for domain types
/// public class ProductIdFormatter : Formatter&lt;ProductId&gt;
/// {
///     public override string Format(ProductId value)
///     {
///         return $"PROD-{value.Id:D6}";
///     }
/// }
/// 
/// // Register the formatter globally
/// Formatter.RegisterFormatter&lt;ProductId&gt;(new ProductIdFormatter());
/// 
/// // Now all test cases automatically use custom formatting
/// var testData = CreateTestDataReturns(
///     definition: "Get product",
///     expected: new ProductId(42),
///     arg1: userId);
/// // TestCaseName: "Get product => returns PROD-000042" ✅
/// </code>
/// </example>
/// <seealso cref="Formatter{T}"/>
/// <seealso cref="DefaultFormatter"/>
public interface IFormatter
{
    /// <summary>
    /// Formats the specified value as a string for test case naming.
    /// </summary>
    /// <param name="obj">The value to format. May be null.</param>
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
    /// Type-safe implementations should prefer <see cref="Formatter{T}.Format(T)"/>.
    /// </para>
    /// </remarks>
    string? Format(object? obj);
}
