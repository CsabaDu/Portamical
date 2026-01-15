// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.PatternMarkers;
using Adatamiq.Strategy;
using Adatamiq.Validators;

namespace Adatamiq.TestDataTypes.Model.Specialized;

public abstract class TestDataExpected<TResult>(
    string definition,
    TResult expected)
: TestDataBase(definition),
IExpected<TResult>
where TResult : notnull
{
    private const string ExpectedString = "expected";
    private const string ResultsString = "results";

    public TResult Expected { get; init; } = expected;

    public abstract string GetResultPrefix();

    public object GetExpected()
    => Expected;

    protected override object?[] ToArgs(ArgsCode argsCode)
    => Extend(base.ToArgs, argsCode, Expected);

    protected string GetExpectedResult(string? expectedString)
    {
        var resultPrefix = GetResultPrefix();
        var expected = ExpectedString.FallbackIfNullOrEmpty(expectedString);

        return $"{resultPrefix} {expected}";
    }

    protected string GetResultPrefix(string resultPrefix)
    => ResultsString.FallbackIfNullOrEmpty(resultPrefix);

    public override object?[] ToParams(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToParams, argsCode, propsCode,
        propsCode != PropsCode.TestCaseName);
}
