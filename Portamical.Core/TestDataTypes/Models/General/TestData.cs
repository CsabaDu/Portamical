// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;

namespace Portamical.Core.TestDataTypes.Models.General;

/// <summary>
/// Provides an abstract base class for representing test data with a definition and an associated result.
/// </summary>
/// <remarks>This class is intended to be inherited by types that encapsulate test data scenarios, supplying both
/// the test definition and its expected result. It extends TestDataBase and enforces a consistent contract for
/// retrieving result values and argument representations.</remarks>
public abstract class TestData
: TestDataBase
{
    private protected TestData(
        string definition,
        string result)
    : base(definition)
    {
        _result = result;
    }

    private readonly string _result;
    private const string ResultString = "result";

    /// <summary>
    /// Returns the result string, or a fallback value if the result is null or empty.
    /// </summary>
    /// <returns>A string containing the result. If the result is null or empty, a fallback value is returned instead.</returns>
    public override sealed string GetResult()
    => ResultString.FallbackIfNullOrEmpty(_result);

    /// <summary>
    /// Returns an array of argument values based on the specified argument and property codes.
    /// </summary>
    /// <param name="argsCode">A value that specifies which arguments to include in the returned array.</param>
    /// <param name="propsCode">A value that specifies which properties to include in the returned array.</param>
    /// <returns>An array of objects containing the argument values corresponding to the specified codes. The array may be empty
    /// if no arguments match the criteria.</returns>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode != PropsCode.All);
}
