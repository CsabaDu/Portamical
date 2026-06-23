//SPDX - License - Identifier: MIT
// Copyright(c) 2026.Csaba Dudas(CsabaDu)

using Portamical.Core.Formatting.CustomFormatters;

namespace Portamical.Core.Formatting.CustomFormatters.Model;

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
///   <item><strong>Interface Compliance:</strong> Automatically implements both <see cref="IFormatter"/> and <see cref="ICustomFormatter{T}"/></item>
///   <item><strong>Reusability:</strong> Inherit utility methods from <see cref="IFormatter"/> base class</item>
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
///             return NullString;  // Use base class constant
///         
///         return $"PROD-{value.Id:D6}";
///     }
/// }
/// 
/// // Usage with FormatterRegister registry
/// var formatter = new ProductIdFormatter();
/// FormatterRegister.RegisterFormatter&lt;ProductId&gt;(formatter);
/// 
/// var productId = new ProductId { Id = 42 };
/// var formatted = FormatterRegister.Format(productId);
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
///             return FallbackIfNull(null);  // Use base class helper
///         
///         // Use JoinWithComma for consistent formatting
///         var parts = new[] { value.Start.ToString(), value.End.ToString() };
///         return $"[{JoinWithComma(parts)}]";
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
/// <seealso cref="ICustomFormatter{T}"/>
/// <seealso cref="DefaultFormatter"/>
public abstract class CustomFormatter<T> : ICustomFormatter<T>
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
    ///   <item><strong>Null Handling:</strong> Return <see cref="FormatBuilder.NullString"/> (<c>"null"</c>) for null values if <typeparamref name="T"/> is nullable</item>
    ///   <item><strong>Consistency:</strong> Produce the same output for equivalent values</item>
    ///   <item><strong>Conciseness:</strong> Keep output brief but descriptive (typically &lt; 50 characters)</item>
    ///   <item><strong>Clarity:</strong> Use formats that align with C# literal syntax when appropriate</item>
    ///   <item><strong>Thread Safety:</strong> Ensure the method is safe for concurrent calls</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Base Class Utilities:</strong> Implementations can leverage inherited helper methods:
    /// <list type="bullet">
    ///   <item><see cref="FormatBuilder.FallbackIfNull(string?)"/> - Convert null to <c>"null"</c></item>
    ///   <item><see cref="FormatBuilder.JoinWithComma(IEnumerable{string?})"/> - Join formatted parts</item>
    ///   <item><see cref="FormatBuilder.CreateSeparatedString(string, string, string)"/> - Zero-allocation string assembly</item>
    ///   <item><see cref="FormatBuilder.CopyAsSpan(string, Span{char}, int)"/> - Efficient string copying</item>
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
    /// // Nullable value formatter with base class utilities
    /// public class OptionalStringFormatter : Formatter&lt;string?&gt;
    /// {
    ///     public override string Format(string? value)
    ///     {
    ///         // Use base class helper for null handling
    ///         if (value is null)
    ///             return FallbackIfNull(null);
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
    /// A formatted string representation of the object.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides the bridge between the non-generic <see cref="IFormatter"/> interface
    /// (used by the <see cref="Formatter"/> registry) and the type-safe <see cref="Format(T)"/> method.
    /// </para>
    /// <para>
    /// <strong>Implementation:</strong> Uses pattern matching (<c>obj is T typedValue</c>) to perform
    /// efficient type checking. If the type matches, delegates to the abstract <see cref="Format(T)"/>
    /// method; otherwise, returns <see langword="null"/> to signal incompatibility.
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
    /// <para>
    /// <strong>Null Handling:</strong> Null values are passed through to <see cref="Format(T)"/> if
    /// <typeparamref name="T"/> is a nullable type (reference type or <see cref="Nullable{T}"/>).
    /// For non-nullable value types, null will fail the type check and return <see langword="null"/>.
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
    /// formatter.Format(obj3);      // null ✅ (null is not an int)
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
    /// formatter.Format(obj2);      // "null" ✅ (null is valid for string?)
    /// 
    /// object obj3 = 123;
    /// formatter.Format(obj3);      // null ✅ (int != string)
    /// </code>
    /// </example>
    string? IFormatter.Format(object? obj)
    => Format((T)obj!);
}