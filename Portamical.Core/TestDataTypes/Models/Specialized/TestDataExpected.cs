// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;
using Portamical.Core.Validators;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

public abstract class TestDataExpected<TResult>
: TestDataBase,
IExpected<TResult>
where TResult : notnull
{
    protected TestDataExpected(
        string definition,
        TResult expected)
    : base(definition)
    {
        Expected = expected;
        TestCaseName = CreateTestCaseName();
    }

    private const string ExpectedString = "expected";
    private const string ResultsString = "results";

    public TResult Expected { get; init; }

    #region Properties
    /// <summary>
    /// Gets the unique name of the test case associated with this instance.
    /// </summary>
    public override sealed string TestCaseName { get; init; }
    #endregion

    public abstract string GetResultPrefix();

    public object GetExpected()
    => Expected;

    protected override object?[] ToObjectArray(ArgsCode argsCode)
    => Extend(base.ToObjectArray, argsCode, Expected);

    protected string GetExpectedResult(string? expectedString)
    {
        var resultPrefix = GetResultPrefix();
        var expected = ExpectedString.FallbackIfNullOrWhiteSpace(expectedString, nameof(GetExpected));

        return $"{resultPrefix} {expected}";
    }

    protected string GetResultPrefix(string resultPrefix)
    => ResultsString.FallbackIfNullOrWhiteSpace(resultPrefix, nameof(GetResultPrefix));

    public override object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode != PropsCode.All);
}
