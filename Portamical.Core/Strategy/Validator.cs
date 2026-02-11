// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Diagnostics;

namespace Portamical.Core.Strategy;

/// <summary>
/// Provides extension methods for validating and handling enumeration values in a type-safe manner.
/// </summary>
/// <remarks>The <see cref="Validator"/> class offers utility methods to ensure that enumeration values are
/// valid and to standardize exception handling for invalid enum arguments. These methods are intended to simplify
/// validation logic and promote consistent error reporting when working with strongly typed enums.</remarks>
public static class Validator
{
    private static long LogCounter;

    /// <summary>
    /// Returns the preferred value if it is not null, empty, or consists only of white-space characters; otherwise,
    /// returns a fallback label with a unique index appended.
    /// </summary>
    /// <remarks>If <paramref name="preferredValue"/> is null, empty, or white space, a warning is logged and
    /// the fallback label is returned with a unique index to aid in identifying fallback occurrences in logs and
    /// reports. This method is intended for use in test data scenarios where a meaningful value is required for
    /// reporting or logging.</remarks>
    /// <param name="fallbackLabel">The fallback label to use if <paramref name="preferredValue"/> is null, empty, or white space. Cannot be null.</param>
    /// <param name="preferredValue">The value to return if it is not null, empty, or white space. May be null.</param>
    /// <param name="methodName">The name of the method requesting the value, used for logging purposes. Cannot be null.</param>
    /// <returns>The <paramref name="preferredValue"/> if it contains non-white-space characters; otherwise, the <paramref
    /// name="fallbackLabel"/> with a unique index appended.</returns>
    public static string FallbackIfNullOrWhiteSpace(
        this string fallbackLabel,
        string? preferredValue,
        string methodName)
    {
        _ = NotNull(fallbackLabel, nameof(fallbackLabel));
        _ = NotNull(methodName, nameof(methodName));

        if (string.IsNullOrWhiteSpace(preferredValue))
        {
            var logIndex = Interlocked.Increment(ref LogCounter);
            var indexedFallback = $"{fallbackLabel} ({logIndex})";

            Trace.TraceWarning(
                $"Portamical log {logIndex}: The '{methodName}' method of the test data object " +
                $"returned a null, empty, or whitespace value. " +
                $"Using indexed fallback label '{indexedFallback}' in the test report.");

            return indexedFallback;
        }

        return preferredValue;
    }

    /// <summary>
    /// Creates a standardized invalid enumeration exception for 'TEnum'-type <see cref="enum"/> values.
    /// Used to maintain consistent error handling across the test data framework.
    /// </summary>
    /// <param name="enumValue">The invalid 'TEnum'-type <see cref="enum"/> value.</param>
    /// <param name="paramName">The name of the newArg that contained the invalid value.</param>
    /// <returns>A new <see cref="InvalidEnumArgumentException"/> instance.</returns>
    public static InvalidEnumArgumentException GetInvalidEnumArgumentException<TEnum>(
        this TEnum enumValue,
        string? paramName)
    where TEnum : struct, Enum
    => new(paramName, (int)(object)enumValue, typeof(TEnum));

    /// <summary>
    /// Validates that the <see cref="enum"/> value is defined in the 'TEnum'-type enumeration.
    /// </summary>
    /// <param name="enumValue">The <see cref="enum"/>  value to validate.</param>
    /// <param name="paramName">The name of the newArg being validated.</param>
    /// <returns>The original <paramref name="propsCode"/> if it is defined.</returns>
    /// <exception cref="InvalidEnumArgumentException">
    /// Thrown when the <paramref name="enumValue"/> is not a defined value in the 'TEnum' enumeration.
    /// </exception>
    public static TEnum Defined<TEnum>(
        this TEnum enumValue,
        string? paramName)
    where TEnum : struct, Enum
    => Enum.IsDefined(enumValue) ?
        enumValue
        : throw enumValue.GetInvalidEnumArgumentException(paramName);

    /// <summary>
    /// Returns an array containing the elements of the specified sequence, ensuring that the sequence is not null or
    /// empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">The sequence to validate and convert to an array. Cannot be null and must contain at least one element.</param>
    /// <param name="paramName">The name of the parameter to include in the exception if the sequence is null or empty.</param>
    /// <returns>An array containing the elements of the specified sequence.</returns>
    /// <exception cref="ArgumentException">Thrown if the sequence is null or contains no elements.</exception>
    public static T[] NotNullOrEmpty<T>(IEnumerable<T>? enumerable, string? paramName)
    {
        // Take a stable snapshot once
        var snapshot = NotNull(enumerable, paramName) as T[]
            ?? [.. enumerable!];

        if (snapshot.Length == 0)
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
    /// <param name="paramName">The name of the parameter to include in the exception if <paramref name="value"/> is null.</param>
    /// <returns>The non-null value of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static T NotNull<T>(T? value, string? paramName)
    => value is null ?
        throw new ArgumentNullException(paramName)
        : value;
}
