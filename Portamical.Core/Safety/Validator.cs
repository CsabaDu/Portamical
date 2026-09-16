// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Safety;

/// <summary>
/// Provides validation methods for common argument checking patterns with standardized error handling.
/// </summary>
/// <remarks>
/// <para>
/// This class offers utility methods for validating method arguments, ensuring they meet
/// required conditions. It provides consistent exception messages and types across the
/// Portamical framework.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Static utility class with guard clause methods.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are thread-safe (stateless).
/// </para>
/// <para>
/// <strong>Performance:</strong> Critical validation methods are marked with
/// <see cref="MethodImplAttribute"/> using <see cref="MethodImplOptions.AggressiveInlining"/>
/// to eliminate method call overhead on hot paths (constructor validation, property initialization).
/// </para>
/// <para>
/// <strong>Key Methods:</strong>
/// <list type="bullet">
///   <item><see cref="NotNull{T}(T, string)"/> - Validates non-null values</item>
///   <item><see cref="NotNullOrEmpty{T}(IEnumerable{T}, string)"/> - Validates non-empty sequences</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class TestData
/// {
///     public TestData(string name, IEnumerable&lt;int&gt; values)
///     {
///         Name = NotNull(name, nameof(name));
///         Values = NotNullOrEmpty(values, nameof(values));
///     }
///     
///     public string Name { get; }
///     public int[] Values { get; }
/// }
/// </code>
/// </example>
public static class Validator
{
    /// <summary>
    /// Returns an array containing the elements of the specified sequence, ensuring that the sequence is not
    /// <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">
    /// The sequence to validate and convert to an array. Cannot be <see langword="null"/> and must contain at least one element.
    /// </param>
    /// <param name="paramName">
    /// The name of the parameter to include in the exception if the sequence is <see langword="null"/> or empty.
    /// </param>
    /// <returns>
    /// An array containing the elements of the specified sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="enumerable"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="enumerable"/> contains no elements.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This overload delegates to <see cref="NotNullOrEmpty{T}(IEnumerable{T}, string, out int)"/>,
    /// discarding the resulting element count. Use that overload directly when the count is also needed,
    /// to avoid recomputing <c>snapshot.Length</c> separately.
    /// </para>
    /// <para>
    /// <strong>Snapshot Behavior:</strong> Internally, the sequence is materialized into an array via
    /// <see cref="Resolver.SnapshotOrNull{T}(IEnumerable{T}?)"/> to avoid multiple enumeration. If the input
    /// is already an array, it is returned directly without allocation. Otherwise, a new array is created.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// <list type="bullet">
    ///   <item>Already array: O(1) - direct return</item>
    ///   <item>Other sequence: O(n) - creates a new array</item>
    ///   <item>This method is marked for aggressive inlining to minimize overhead in hot paths</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> The returned array is thread-safe for reading.
    /// The original sequence is not accessed after validation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid sequence
    /// var numbers = new[] { 1, 2, 3 };
    /// var validated = NotNullOrEmpty(numbers, nameof(numbers)); // Returns array
    /// 
    /// // Invalid: null
    /// IEnumerable&lt;int&gt;? nullSeq = null;
    /// var result1 = NotNullOrEmpty(nullSeq, nameof(nullSeq)); 
    /// // Throws: ArgumentNullException
    /// 
    /// // Invalid: empty
    /// var empty = Enumerable.Empty&lt;int&gt;();
    /// var result2 = NotNullOrEmpty(empty, nameof(empty)); 
    /// // Throws: ArgumentException: The sequence must contain at least one element.
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] NotNullOrEmpty<T>(IEnumerable<T>? enumerable, string? paramName)
    => NotNullOrEmpty(enumerable, paramName, out _);

    /// <summary>
    /// Returns an array containing the elements of the specified sequence, ensuring that the sequence is not
    /// <see langword="null"/> or empty, and outputs the element count.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">
    /// The sequence to validate and convert to an array. Cannot be <see langword="null"/> and must contain at least one element.
    /// </param>
    /// <param name="paramName">
    /// The name of the parameter to include in the exception if the sequence is <see langword="null"/> or empty.
    /// </param>
    /// <param name="count">
    /// When this method returns, contains the number of elements in the returned array. Set even if the method
    /// throws due to an empty sequence, but left at its default value (<c>0</c>) if <paramref name="enumerable"/>
    /// is <see langword="null"/>, since the exception is thrown before assignment.
    /// </param>
    /// <returns>
    /// An array containing the elements of the specified sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="enumerable"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="enumerable"/> contains no elements.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the primary implementation for sequence validation. It snapshots <paramref name="enumerable"/>
    /// into an array via <see cref="Resolver.SnapshotOrNull{T}(IEnumerable{T}?)"/> (avoiding multiple enumeration
    /// and reusing the source array when the input is already an array), validates the snapshot is not
    /// <see langword="null"/> via <see cref="NotNull{T}(T, string)"/>, and then validates it is not empty.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> Marked for aggressive inlining, and avoids a second traversal or
    /// re-evaluation of <c>Length</c> by exposing the count as an <see langword="out"/> parameter. Prefer this
    /// overload over <see cref="NotNullOrEmpty{T}(IEnumerable{T}, string)"/> when the caller also needs the
    /// element count, since the count is otherwise already known.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> The returned array is thread-safe for reading. The original sequence
    /// is not accessed after validation.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var numbers = new[] { 1, 2, 3 };
    /// var validated = NotNullOrEmpty(numbers, nameof(numbers), out int count); // count == 3
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] NotNullOrEmpty<T>(IEnumerable<T>? enumerable, string? paramName, out int count)
    {
        var snapshot = NotNull(
            Resolver.SnapshotOrNull(enumerable), paramName);
        count = snapshot.Length;

        if (count == 0)
        {
            throw new ArgumentException(
                "The sequence must contain at least one element.",
                paramName);
        }

        return snapshot;
    }

    /// <summary>
    /// Ensures that the specified value is not null, throwing an exception if it is.
    /// </summary>
    /// <typeparam name="T">The type of the value to check for null.</typeparam>
    /// <param name="value">The value to validate as non-null.</param>
    /// <param name="paramName">
    /// The name of the parameter to include in the exception if <paramref name="value"/> is null.
    /// </param>
    /// <returns>
    /// The non-null value of <paramref name="value"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <strong>Fluent Pattern:</strong> This method returns the validated value, enabling
    /// fluent initialization:
    /// <code>
    /// Name = NotNull(name, nameof(name));
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Nullability:</strong> The return type is <typeparamref name="T"/> (non-nullable)
    /// when <paramref name="value"/> is successfully validated. This enables null-safe code flow
    /// in nullable reference type contexts (C# 8+).
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> This method is marked for aggressive inlining to eliminate
    /// method call overhead on the critical success path. The fast path (non-null) compiles to
    /// a single null check instruction.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class TestData
    /// {
    ///     public TestData(string name)
    ///     {
    ///         // Fluent validation and assignment
    ///         Name = NotNull(name, nameof(name));
    ///     }
    ///     
    ///     public string Name { get; }
    /// }
    /// 
    /// // Usage
    /// var test = new TestData("valid"); // OK
    /// var invalid = new TestData(null); // Throws ArgumentNullException
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(T? value, string? paramName)
    => value is null ?
        throw new ArgumentNullException(paramName)
        : value;
}