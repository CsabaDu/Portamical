// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.Converters;

public static class TestDataConverter
{
    public static object[] ToSingleTestDataRowArray<TTestData>(
        this TTestData testData,
        ArgsCode argsCode,
        string? testMethodName,
        params string[] testCategories)
    where TTestData : notnull, ITestData
    {
        return argsCode == ArgsCode.Instance ?
            toTestDataRowArray(testData)
            : toTestDataRowArray(testData.ToArgs(ArgsCode.Properties));

        #region Local functions

        object[] toTestDataRowArray<T>(T row)
        => [new TestDataRow<T>(row)
        {
            DisplayName = testData.GetDisplayName(testMethodName),
            TestCategories = testCategories,
        }];

        #endregion
    }
}
