// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using NUnit.Framework.Interfaces;
using Portamical.Core.Identity.Model;
using System.Reflection;
using static Portamical.Core.Strategy.Validator;
using static Portamical.NUnit.TestDataTypes.TestCaseTestData;

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
        ArgumentException.ThrowIfNullOrEmpty(sourceName, nameof(sourceName));

        if (sourceType is not null)
        {
            validateSourceType(sourceType, sourceName);
        }

        return (sourceType, methodParams) switch
        {
            (null, null) => new(sourceName),
            (null, not null) => new(sourceName, methodParams),
            (not null, null) => new(sourceType, sourceName),
            _ => new(sourceType, sourceName, methodParams),
        };

        #region Local Methods
        static void validateSourceType(Type sourceType, string sourceName)
        {
            var sourceTypeFullName = sourceType.FullName;

            if (sourceType.IsClass || sourceType.IsInterface)
            {
                var member = sourceType.GetMember(sourceName,
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.FlattenHierarchy);

                if (member.Length == 0)
                {
                    throw new ArgumentException(
                        $"Source member '{sourceName}' " +
                        $"not found or not accessible on type '{sourceTypeFullName}'. " +
                        "Ensure it exists and is public static.",
                        nameof(sourceName));
                }

                var memberInfo = member[0];
                var memberType = getMemberReturnType(memberInfo);

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(memberType))
                {
                    return;
                }

                throw new ArgumentException(
                    $"Source member '{sourceName}' must return an IEnumerable, " +
                    $"but returns '{memberType.FullName}'.",
                    nameof(sourceName));
            }

            var message = sourceType.IsValueType ?
                $"Source type cannot be a struct: {sourceTypeFullName}. " +
                $"Use a class or interface instead."
                : $"Source type must be a class or interface: {sourceTypeFullName}. " +
                    $"Actual type: {sourceType.GetType().Name}";

            throw new ArgumentException(message, nameof(sourceType));
        }

        static Type getMemberReturnType(MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                FieldInfo field => field.FieldType,
                PropertyInfo property => property.PropertyType,
                MethodInfo method => method.ReturnType,
                _ => throw new NotSupportedException(
                    $"Member type {memberInfo.MemberType} is not supported.")
            };
        }
        #endregion
    }

    public string? Category
    {
        get => _innerAttribute.Category;
        set => _innerAttribute.Category = value;
    }

    public string? SourceName
    => _innerAttribute.SourceName;

    public Type? SourceType
    => _innerAttribute.SourceType;

    public object?[]? MethodParams
    => _innerAttribute.MethodParams;

    /// <summary>
    /// Builds any number of tests from the specified method and context.
    /// </summary>
    /// <param name="method">The IMethod for which tests are to be constructed.</param>
    /// <param name="suite">The suite to which the tests will be added.</param>
    public IEnumerable<TestMethod> BuildFrom(IMethodInfo method, Test? suite)
    {
        foreach (var testMethod in _innerAttribute.BuildFrom(method, suite))
        {
            if (shouldRename(testMethod))
            {
                testMethod.Name = NamedCase.CreateDisplayName(
                    method.Name,
                    testMethod.Name)!;
            }

            yield return testMethod;
        }

        #region Local Functions
        static bool shouldRename(TestMethod testMethod)
        {
            var properties = testMethod.Properties;
            var hasFullNameProperty =
                properties.Get(HasFullNameProperty);

            return hasFullNameProperty is bool hasFullName && !hasFullName;
        }
        #endregion
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
