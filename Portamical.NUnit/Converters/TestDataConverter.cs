// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.NUnit.TestDataTypes;
using static Portamical.NUnit.TestDataTypes.TestCaseTestData;

namespace Portamical.NUnit.Converters;

public static class TestDataConverter
{
    internal static TestCaseTestData<TTestData> ToTestCaseTestData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => From(testData, argsCode, testMethodName);

    internal static TestCaseData ToTestCaseData<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName)
    where TTestData : notnull, ITestData
    {
        var row = TestCaseDataArgsFrom(testData, argsCode);
        var testCaseData = new TestCaseData(row)
        {
            TypeArgs = GetTypeArgs(testData, argsCode),
        }
        .SetDescription(testData.GetDefinition());

        SetHasFullNameProperty(
            testCaseData,
            testData,
            testMethodName,
            out string testName);

        testCaseData = testCaseData.SetName(testName);

        return testData is IReturns returns ?
            testCaseData.Returns(returns.GetExpected())
            : testCaseData;
    }
}
