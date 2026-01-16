// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)using System;

using Adatamiq.Identity;
using Adatamiq.Identity.Model;
using Adatamiq.Validators;
using System.ComponentModel;
using System.Reflection;

namespace Adatamiq.MSTest.Attributes;

/// <summary>
/// Custom DynamicData attribute that wraps MSTest's sealed DynamicDataAttribute
/// to provide custom display names via TestDataFactory.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DynamicTestDataAttributeBase : Attribute, ITestDataSource
{
    private readonly DynamicDataAttribute _dynamicDataAttribute;

    #region Constructors
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DynamicTestDataAttributeBase(
        string dynamicDataSourceName,
        DynamicDataSourceType dynamicDataSourceType)
    {
        _dynamicDataAttribute = new DynamicDataAttribute(
            dynamicDataSourceName,
            dynamicDataSourceType);
    }

    public DynamicTestDataAttributeBase(string dynamicDataSourceName)
    {
        _dynamicDataAttribute =
            new DynamicDataAttribute(dynamicDataSourceName);
    }

    public DynamicTestDataAttributeBase(
        string dynamicDataSourceName,
        params object?[] dynamicDataSourceArguments)
    {
        _dynamicDataAttribute = new DynamicDataAttribute(
            dynamicDataSourceName,
            dynamicDataSourceArguments);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public DynamicTestDataAttributeBase(
        string dynamicDataSourceName,
        Type dynamicDataDeclaringType,
        DynamicDataSourceType dynamicDataSourceType)
    {
        _dynamicDataAttribute = new DynamicDataAttribute(
            dynamicDataSourceName,
            dynamicDataDeclaringType,
            dynamicDataSourceType);
    }

    public DynamicTestDataAttributeBase(
        string dynamicDataSourceName,
        Type dynamicDataDeclaringType)
    {
        _dynamicDataAttribute = new DynamicDataAttribute(
            dynamicDataSourceName,
            dynamicDataDeclaringType);
    }

    public DynamicTestDataAttributeBase(
        string dynamicDataSourceName,
        Type dynamicDataDeclaringType,
        params object?[] dynamicDataSourceArguments)
    {
        _dynamicDataAttribute = new DynamicDataAttribute
            (dynamicDataSourceName,
            dynamicDataDeclaringType,
            dynamicDataSourceArguments);
    }
    #endregion

    #region ITestDataSource members
    /// <inheritdoc />
    public IEnumerable<object?[]> GetData(MethodInfo testMethod)
    => _dynamicDataAttribute.GetData(testMethod);

    /// <inheritdoc />
    public string? GetDisplayName(
        MethodInfo testMethod,
        params object?[]? data)
    {
        var displayName = data?[0] is string or INamedCase ?
            NamedCase.CreateDisplayName(
                testMethod.Name,
                data)
            : null;

        var defaultName =
            _dynamicDataAttribute.GetDisplayName(
                testMethod,
                data)
            ?? string.Empty;

        return defaultName.FallbackIfNullOrEmpty(displayName);
    }
    #endregion
}

public class DynamicTestDataAttribute
: DynamicTestDataAttributeBase
{
    #region Constructors
    public DynamicTestDataAttribute(string dynamicDataSourceName)
    : base(dynamicDataSourceName)
    {
    }

    public DynamicTestDataAttribute(
        string dynamicDataSourceName,
        DynamicDataSourceType dynamicDataSourceType)
    : base(
        dynamicDataSourceName,
        dynamicDataSourceType)
    {
    }

    public DynamicTestDataAttribute(
        string dynamicDataSourceName,
        params object?[] dynamicDataSourceArguments)
    : base(
        dynamicDataSourceName,
        dynamicDataSourceArguments)
    {
    }

    public DynamicTestDataAttribute(
        string dynamicDataSourceName,
        Type dynamicDataDeclaringType)
    : base(
        dynamicDataSourceName,
        dynamicDataDeclaringType)
    {
    }

    public DynamicTestDataAttribute(
        string dynamicDataSourceName,
        Type dynamicDataDeclaringType,
        DynamicDataSourceType dynamicDataSourceType)
    : base(
        dynamicDataSourceName,
        dynamicDataDeclaringType,
        dynamicDataSourceType)
    {
    }

    public DynamicTestDataAttribute(
        string dynamicDataSourceName,
        Type dynamicDataDeclaringType,
        params object?[] dynamicDataSourceArguments)
    : base(dynamicDataSourceName,
        dynamicDataDeclaringType,
        dynamicDataSourceArguments)
    {
    }
    #endregion
}
