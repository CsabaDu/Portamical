// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

public abstract class TestDataReturns<TStruct>
: TestDataExpected<TStruct>,
IReturns<TStruct>
where TStruct : struct
{
    private protected TestDataReturns(
        string definition,
        TStruct expected)
    : base(definition, expected)
    {
    }

    private const string ReturnsString = "returns";

    /// <inheritdoc/>
    public override sealed string GetResultPrefix()
    => GetResultPrefix(ReturnsString);

    /// <summary>
    /// Gets the TrimTestCaseName return value as an object.
    /// </summary>
    /// <returns>The string representation of the boxed 'TrimTestCaseName' value.</returns>
    public override sealed string GetResult()
    => GetExpectedResult(Expected.ToString());

    /// <inheritdoc/>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimReturnsExpected);
}
