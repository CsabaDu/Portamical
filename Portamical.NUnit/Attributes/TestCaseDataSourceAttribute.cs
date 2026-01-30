// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using NUnit.Framework.Interfaces;
using Portamical.Core.Identity.Model;
using Portamical.NUnit.TestDataTypes;

namespace Portamical.NUnit.Attributes;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class TestCaseDataSourceAttributeBase(
    string sourceName,
    Type? sourceType = null,
    object?[]? methodParams = null)
: NUnitAttribute, ITestBuilder, IImplyFixture
{
    private readonly TestCaseSourceAttribute _innerAttribute =
    Create(sourceName, sourceType, methodParams);

    private static TestCaseSourceAttribute Create(
    string sourceName,
    Type? sourceType,
    object?[]? methodParams)
    {
        if (sourceType is not null && methodParams is not null)
        {
            return new TestCaseSourceAttribute(sourceType, sourceName, methodParams);
        }

        if (sourceType is not null)
        {
            return new TestCaseSourceAttribute(sourceType, sourceName);
        }

        if (methodParams is not null)
        {
            return new TestCaseSourceAttribute(sourceName, methodParams);
        }

        return new TestCaseSourceAttribute(sourceName);
    }

    public string? Category
    {
        get => _innerAttribute.Category;
        set => _innerAttribute.Category = value;
    }


    /// <summary>
    /// Builds any number of tests from the specified method and context.
    /// </summary>
    /// <param name="method">The IMethod for which tests are to be constructed.</param>
    /// <param name="suite">The suite to which the tests will be added.</param>
    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
    {
        foreach (var testMethod in _innerAttribute.BuildFrom(method, suite))
        {
            var testName = testMethod.Name;
            var hasFullNameProperty =
                testMethod.Properties.Get(TestCaseTestData.HasFullNameProperty);

            if (hasFullNameProperty is not bool hasFullName || !hasFullName)
            {
                testMethod.Name = NamedCase.CreateDisplayName(method.Name, testName)!;
            }

            yield return testMethod;
        }
    }
}

public sealed class TestCaseDataSourceAttribute : TestCaseDataSourceAttributeBase
{
    public TestCaseDataSourceAttribute(string sourceName)
    : base(sourceName)
    {
    }

    public TestCaseDataSourceAttribute(string sourceName, object?[] methodParams)
    : base(sourceName, methodParams: methodParams)
    {
    }

    public TestCaseDataSourceAttribute(Type sourceType, string sourceName)
    : base(sourceName, sourceType: sourceType)
    {
    }

    public TestCaseDataSourceAttribute(Type sourceType, string sourceName, object?[] methodParams)
    : base(sourceName, sourceType: sourceType, methodParams: methodParams)
    {
    }
}
