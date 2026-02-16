// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Validators;

public static class Validator
{
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
