// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

public abstract class TestDataThrows<TException>
: TestDataExpected<TException>,
IThrows<TException>
where TException : Exception
{
    private protected TestDataThrows(
        string definition,
        TException expected)
    : base(definition, expected)
    {
    }
    
    private const string ThrowsString = "throws";

    /// <inheritdoc/>
    public override sealed string GetResultPrefix()
    => GetResultPrefix(ThrowsString);

    /// <summary>
    /// Gets the TrimTestCaseName exception instance.
    /// </summary>
    /// <returns>The exception object that should be thrown.</returns>
    public override sealed string GetResult()
    => GetExpectedResult(Expected.GetType().Name);

    /// <inheritdoc/>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimThrowsExpected);
}
