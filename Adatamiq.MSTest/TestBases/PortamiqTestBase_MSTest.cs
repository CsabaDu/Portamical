// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.TestBases;
using Adatamiq.Converters;
using Adatamiq.TestDataTypes;
using Adatamiq.Strategy;

namespace Adatamiq.MSTest.TestBases;

public abstract class PortamiqTestBase_MSTest
: PortamiqTestBase
{
    protected static TException? AssertThrowsDetails<TException>(
        Action attempt,
        TException expected)
    where TException : Exception
    {
        var expectedType = expected.GetType();
        try
        {
            attempt();

            Assert.Fail($"Expected {expectedType.Name} was not thrown.");
        }
        catch (TException actual)
        {
            Assert.IsInstanceOfType(actual, expectedType);

            if (expected.Message is string expectedMessage)
            {
                Assert.AreEqual(expectedMessage, actual.Message);
            }

            if (expected is ArgumentException argExpected &&
                argExpected.ParamName is string expectedParamName &&
                actual is ArgumentException argActual)
            {
                Assert.AreEqual(expectedParamName, argActual.ParamName);
            }

            return actual;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }

        return null;
    }

    protected IEnumerable<object?[]> ConvertToNamedObjectArrays<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(
        ArgsCode,
        PropsCode.All);
}
