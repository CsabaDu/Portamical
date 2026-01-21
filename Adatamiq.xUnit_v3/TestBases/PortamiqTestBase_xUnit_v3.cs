// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.TestBases;
using Adatamiq.xUnit_v3.Converters;
using Adatamiq.xUnit_v3.DataProviders.Model;
using Adatamiq.xUnit_v3.TestDataTypes;

namespace Adatamiq.xUnit_v3.TestBases;

public abstract class PortamiqTestBase_xUnit_v3
: PortamiqTestBase
{
    protected static TException? AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var expectedType = expected.GetType();
        var actual = Record.Exception(attempt);

        if (actual is null)
        {
            Assert.Fail($"Expected {expectedType.Name} was not thrown.");
        }

        if (actual is not TException)
        {
            Assert.Fail($"Unexpected exception type: {actual.GetType().Name}");

            return null;
        }

        Assert.IsType(expectedType, actual);

        if (expected.Message is string expectedMessage)
        {
            Assert.Equal(expectedMessage, actual!.Message);
        }

        if (expected is ArgumentException argExpected &&
            argExpected.ParamName is string expectedParamName &&
            actual is ArgumentException argActual)
        {
            Assert.Equal(expectedParamName, argActual.ParamName);
        }

        return (TException)actual;
    }


    public IEnumerable<ITheoryTestDataRow> ConvertToTheoryTestDataRows<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestDataRowCollection(
        ArgsCode,
        testMethodName);

    public TheoryTestData<TTestData> ConvertToTheoryTestData<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTheoryTestData(
        ArgsCode,
        testMethodName);
}