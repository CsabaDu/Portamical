// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.Safety;

/// <summary>
/// Provides validation methods for enumeration values with standardized error handling.
/// </summary>
/// <remarks>
/// <para>
/// This class offers extension methods for validating <see cref="Enum"/> values, ensuring they are
/// defined within their enumeration type. It provides consistent exception messages across the
/// Portamical framework.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Static utility class with extension methods for fluent validation.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All methods are thread-safe (stateless).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public enum ArgsCode { Instance, Properties }
/// 
/// // Validate enum value
/// var validated = argsCode.Defined(nameof(argsCode));
/// 
/// // Throws InvalidEnumArgumentException if invalid
/// var invalid = (ArgsCode)99;
/// invalid.Defined(nameof(invalid)); // Throws
/// </code>
/// </example>
public static class EnumValidator
{
    /// <summary>
    /// Creates a standardized <see cref="InvalidEnumArgumentException"/> for invalid enumeration values.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <param name="enumValue">The invalid enumeration value.</param>
    /// <param name="paramName">The name of the parameter that contained the invalid value.</param>
    /// <returns>
    /// A new <see cref="InvalidEnumArgumentException"/> with the parameter name, integer value,
    /// and enumeration type.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is used internally by <see cref="Defined{TEnum}(TEnum, string)"/> to create
    /// consistent exception messages. It can also be used directly when custom validation is needed.
    /// </para>
    /// <para>
    /// <strong>Exception Message Format:</strong> The resulting exception contains the parameter name,
    /// the invalid integer value, and the enumeration type name for clear error diagnostics.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> For enumerations with <see langword="long"/> or <see langword="ulong"/> 
    /// underlying types, the value is cast to <see langword="int"/>, which may truncate large values. 
    /// This is a limitation of the <see cref="InvalidEnumArgumentException"/> constructor.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var invalid = (ArgsCode)99;
    /// var exception = invalid.GetInvalidEnumArgumentException(nameof(invalid));
    /// throw exception;
    /// // Exception message: The value of argument 'invalid' (99) is invalid for Enum type 'ArgsCode'. (Parameter 'invalid')
    /// </code>
    /// </example>
    public static InvalidEnumArgumentException GetInvalidEnumArgumentException<TEnum>(
        this TEnum enumValue,
        string paramName)
    where TEnum : struct, Enum
    => new(paramName, (int)(object)enumValue, typeof(TEnum));

    /// <summary>
    /// Validates that the enumeration value is defined in the <typeparamref name="TEnum"/> enumeration.
    /// </summary>
    /// <typeparam name="TEnum">The enumeration type to validate against.</typeparam>
    /// <param name="enumValue">The enumeration value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>
    /// The original <paramref name="enumValue"/> if it is defined in <typeparamref name="TEnum"/>.
    /// </returns>
    /// <exception cref="InvalidEnumArgumentException">
    /// Thrown when <paramref name="enumValue"/> is not a defined value in the <typeparamref name="TEnum"/> enumeration.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This extension method uses <see cref="Enum.IsDefined(Enum)"/> to validate the enum value.
    /// If validation fails, it throws an exception created by 
    /// <see cref="GetInvalidEnumArgumentException{TEnum}(TEnum, string)"/>.
    /// </para>
    /// <para>
    /// <strong>Fluent Validation:</strong> This method returns the validated value, enabling fluent chaining:
    /// <code>
    /// var validated = argsCode.Defined(nameof(argsCode)).ToString();
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> For small enumerations (&lt;= 64 values), validation is O(1). 
    /// For larger enumerations, validation is O(log n) using binary search.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid enumeration
    /// ArgsCode argsCode = ArgsCode.Instance;
    /// var validated = argsCode.Defined(nameof(argsCode)); // Returns ArgsCode.Instance
    /// 
    /// // Invalid enumeration (throws)
    /// ArgsCode invalid = (ArgsCode)999;
    /// var result = invalid.Defined(nameof(invalid)); 
    /// // Throws: InvalidEnumArgumentException: The value of argument 'invalid' (999) is invalid for Enum type 'ArgsCode'.
    /// </code>
    /// </example>
    public static TEnum Defined<TEnum>(
        this TEnum enumValue,
        string paramName)
    where TEnum : struct, Enum
    => Enum.IsDefined(enumValue) ?
        enumValue
        : throw enumValue.GetInvalidEnumArgumentException(paramName);
}