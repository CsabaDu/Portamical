// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.Model;

namespace Portamical.Converters.DataProviders.Model;

public static class CollectionConverter
{
    public static TestDataProvider<TTestData> ToDataProvider<TTestData>(
        this IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDataProvider(
        initDataProvider: testData => new TestDataProvider<TTestData>(testData));
}