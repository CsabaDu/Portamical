// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.NUnit.TestDataTypes;
using static Adatamiq.NUnit.TestDataTypes.TestCaseTestData;

namespace Adatamiq.NUnit.Converters;

public static class TestDataConverter
{
    public static TestCaseTestData<TTestData> ToTestCaseTestData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => new(testData, argsCode, testMethodName);

    public static TestCaseData ToTestCaseData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        var row = ConvertToReturnsArgs(testData, argsCode);
        var testCaseData = new TestCaseData(row)
        {
            TypeArgs = GetTypeArgs(testData, argsCode),
        }
        .SetDescription(testData.TestCaseName);

        if (!string.IsNullOrEmpty(testMethodName))
        {
            var displayName = testData.GetDisplayName(testMethodName);
            testCaseData = testCaseData.SetName(displayName);
        }

        return testData is IReturns returns ?
            testCaseData.Returns(returns.GetExpected())
            : testCaseData;
    }
}
