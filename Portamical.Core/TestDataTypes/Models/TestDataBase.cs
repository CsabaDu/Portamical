// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity.Model;
using Portamical.Core.Safety;
using Portamical.Core.Strategy;

namespace Portamical.Core.TestDataTypes.Models;

/// <summary>
/// Abstract base class representing test case data with core functionality for test argument generation.
/// </summary>
/// <param name="definition">The descriptive definition of the test case scenario.</param>
/// <remarks>
/// Provides foundational implementation for:
/// <list type="bullet">
/// <item>Test case naming and identification</item>
/// <item>argument generation strategies</item>
/// <item>Equality comparison based on test case names</item>
/// <item>Conversion to parameter arrays for test execution</item>
/// </list>
/// </remarks>
public abstract class TestDataBase(string definition)
: NamedCase, ITestData
{
    #region Fields
    private const string DefinitionString = "definition";
    private const string Separator = " => ";
    #endregion

    #region Methods
    /// <summary>
    /// Gets the definition string for the current instance.
    /// </summary>
    /// <returns>A string containing the definition. If no definition is set, a fallback value is returned.</returns>
    public string GetDefinition()
    => DefinitionString.FallbackIfNullOrWhiteSpace(
        definition,
        nameof(GetDefinition));

    /// <summary>
    /// Convenience overload of <see cref="ToArgs(ArgsCode, PropsCode)"/> for the most common use case:
    /// use the <see cref="PropsCode.TrimTestCaseName"/> property selection.
    /// </summary>
    /// <param name="argsCode">Determines instance vs properties inclusion.</param>
    /// <returns></returns>
    public object?[] ToArgs(ArgsCode argsCode)
    => ToArgs(argsCode, PropsCode.TrimTestCaseName);

    /// <summary>
    /// Converts the test data to a parameter array with precise control over included properties.
    /// </summary>
    /// <param name="argsCode">Determines instance vs properties inclusion.</param>
    /// <param name="propsCode">Specifies which properties to include when using <see cref="ArgsCode.Properties"/>.</param>
    /// <returns>
    /// A parameter array tailored for test execution based on the specified codes.
    /// </returns>
    /// <exception cref="InvalidEnumArgumentException">
    /// Thrown when invalid enum values are provided.
    /// </exception>
    public virtual object?[] ToArgs(ArgsCode argsCode, PropsCode propsCode)
    {
        if (argsCode == ArgsCode.Properties)
        {
            _ = propsCode.Defined(nameof(propsCode));
        }

        var args = ToObjectArray(argsCode);

        return args.Length == 0 ?
            throw new ArgumentOutOfRangeException(
                nameof(propsCode),
                $"Invalid 'TestDataBase' implementation: 'PropsCode.{propsCode}' produced no arguments. " +
                $"Custom TestData types must override 'ToObjectArray()' to include additional properties beyond 'TestCaseName'. " +
                "Use 'PropsCode.All' to include 'TestCaseName', or ensure your implementation adds at least one property.")
            : args;
    }

    /// <summary>
    /// When implemented in a derived class, returns the result of the operation as a string.
    /// </summary>
    /// <returns>A string that represents the result of the operation. The meaning and format of the result are defined by the
    /// derived class implementation.</returns>
    public abstract string GetResult();
    #endregion

    #region Helper methods
    /// <summary>
    /// Creates a test case name by combining the definition, a separator, and the result.
    /// </summary>
    /// <returns>A string representing the test case name, composed of the definition, separator, and result values.</returns>
    protected string CreateTestCaseName()
    {
        var definition = GetDefinition();
        var result = GetResult();
        var totalLength =
            definition.Length +
            Separator.Length +
            result.Length;

        return string.Create(
            totalLength,
            (definition, Separator, result),
            static (span, state) =>
            {
                var (d, s, r) = state;

                var index = 0;
                copy(d, span, index);

                index = d.Length;
                copy(s, span, index);

                index += s.Length;
                copy(r, span, index);
            });

        #region Local method
        static void copy(string part, Span<char> span, int index)
        => part.AsSpan().CopyTo(span[index..]);
        #endregion
    }

    /// <summary>
    /// Converts the test data to an argument array based on the specified <see cref="ArgsCode"/> parameter.
    /// </summary>
    /// <param name="ArgsCode">Determines whether to include the instance itself or its properties.</param>
    /// <returns>
    /// An array containing:
    /// <list type="bullet">
    /// <item>The test data instance itself when <see cref="ArgsCode.Instance"/></item>
    /// <item>The test case properties when <see cref="ArgsCode.Properties"/></item>
    /// </list>
    /// </returns>
    /// <exception cref="InvalidEnumArgumentException">
    /// Thrown when an undefined <paramref name="ArgsCode"/> value is provided.
    /// </exception>
    protected virtual object?[] ToObjectArray(ArgsCode argsCode)
    => argsCode switch
    {
        ArgsCode.Instance => [this],
        ArgsCode.Properties => [TestCaseName],
        _ => throw argsCode.GetInvalidEnumArgumentException(nameof(argsCode)),
    };

    /// <summary>
    /// Creates a new object array by extending the result of a base conversion with an additional argument, depending
    /// on the specified argument code.
    /// </summary>
    /// <typeparam name="T">The type of the additional argument to append to the array.</typeparam>
    /// <param name="baseToObjectArray">A function that converts the specified <paramref name="argsCode"/> to an array of objects representing the base
    /// arguments.</param>
    /// <param name="argsCode">A value that determines how the arguments are processed and whether the additional argument is appended.</param>
    /// <param name="newArg">The additional argument to append to the array when the argument code indicates properties. This value can be
    /// null.</param>
    /// <returns>An array of objects representing the combined arguments. If <paramref name="argsCode"/> is <see
    /// cref="ArgsCode.Instance"/>, returns the base array; if <see cref="ArgsCode.Properties"/>, returns the base array
    /// with <paramref name="newArg"/> appended.</returns>
    protected static object?[] Extend<T>(
    Func<ArgsCode, object?[]> baseToObjectArray,
    ArgsCode argsCode,
    T? newArg)
    {
        var baseObjectArray = baseToObjectArray(argsCode);

        return argsCode switch
        {
            ArgsCode.Instance => baseObjectArray,
            ArgsCode.Properties => [.. baseObjectArray, newArg],
            _ => throw argsCode.GetInvalidEnumArgumentException(nameof(argsCode)),
        };
    }

    /// <summary>
    /// Returns a trimmed array of arguments based on the specified argument and property codes.
    /// </summary>
    /// <param name="baseToArgs">A function that generates an array of arguments from the provided argument and property codes.</param>
    /// <param name="argsCode">The code representing the argument selection strategy.</param>
    /// <param name="propsCode">The code representing the property selection strategy.</param>
    /// <param name="propsCodeMatches">true if the property code matches the expected criteria; otherwise, false.</param>
    /// <returns>An array of objects representing the arguments. If the argument code is set to use properties and the property
    /// code matches, the returned array excludes the first element; otherwise, the full array is returned.</returns>
    protected static object?[] Trim(
        Func<ArgsCode, PropsCode, object?[]> baseToArgs,
        ArgsCode argsCode,
        PropsCode propsCode,
        bool propsCodeMatches)
    {
        var baseArgs = baseToArgs(argsCode, propsCode);
        var strategyMatches =
            argsCode == ArgsCode.Properties &&
            propsCodeMatches;

        return strategyMatches ?
            baseArgs[1..]
            : baseArgs;
    }
    #endregion
}