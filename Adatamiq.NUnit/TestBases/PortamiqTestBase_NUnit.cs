// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.NUnit.Converters;
using Adatamiq.NUnit.TestDataTypes;
using Adatamiq.TestBases;

namespace Adatamiq.NUnit.TestBases;

public abstract class PortamiqTestBase_NUnit
: PortamiqTestBase
{
    protected static void AssertMultiple(Action assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            assertions();
        }
    }

    protected static async Task AssertMultipleAsync(Func<Task> assertions)
    {
        using (Assert.EnterMultipleScope())
        {
            await assertions().ConfigureAwait(false);
        }
    }

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
            AssertMultiple(() =>
            {
                Assert.That(actual, Is.TypeOf(expectedType));

                if (expected.Message is string expectedMessage)
                {
                    Assert.That(actual.Message, Is.EqualTo(expectedMessage));
                }

                if (expected is ArgumentException argExpected &&
                    argExpected.ParamName is string expectedParamName &&
                    actual is ArgumentException argActual)
                {
                    Assert.That(argActual.ParamName, Is.EqualTo(expectedParamName));
                }
            });

            return actual;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }

        return null;
    }

    protected IEnumerable<TestCaseData> ConvertToTestCaseDatas<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseDataCollection(
        ArgsCode,
        testMethodName);

    protected IEnumerable<TestCaseTestData<TTestData>> ConvertToTestCaseTestDatas<TTestData>(
        IEnumerable<TTestData> testDataCollection,
        string? testMethodName = null)
    where TTestData : notnull, ITestData
    => testDataCollection.ToTestCaseTestDataCollection(
        ArgsCode,
        testMethodName);
}