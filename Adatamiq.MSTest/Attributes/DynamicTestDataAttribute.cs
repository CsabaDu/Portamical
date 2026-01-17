// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)using System;

using Adatamiq.Identity;
using Adatamiq.Identity.Model;
using Adatamiq.Strategy;
using System.Reflection;

namespace Adatamiq.MSTest.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class DynamicTestDataAttributeBase : Attribute, ITestDataSource
{
    private readonly DynamicDataAttribute _innerAttribute;

    protected DynamicTestDataAttributeBase(
        string sourceName,
        Type? declaringType = null,
        DynamicDataSourceType? sourceType = null,
        object?[]? sourceArgs = null)
    {
        _innerAttribute = Create(sourceName, declaringType, sourceType, sourceArgs);
    }

    private static DynamicDataAttribute Create(
        string sourceName,
        Type? declaringType,
        DynamicDataSourceType? sourceType,
        object?[]? sourceArgs)
    {
        // 1. Declaring type + source type
        if (declaringType is not null && sourceType is not null)
        {
            return new DynamicDataAttribute(sourceName, declaringType, sourceType.Value);
        }

        // 2. Declaring type + source args
        if (declaringType is not null && sourceArgs is not null)
        {
            return new DynamicDataAttribute(sourceName, declaringType, sourceArgs);
        }

        // 3. Declaring type only
        if (declaringType is not null)
        {
            return new DynamicDataAttribute(sourceName, declaringType);
        }

        // 4. Source type only
        if (sourceType is not null)
        {
            return new DynamicDataAttribute(sourceName, sourceType.Value);
        }

        // 5. Source args only
        if (sourceArgs is not null)
        {
            return new DynamicDataAttribute(sourceName, sourceArgs);
        }

        // 6. Source name only
        return new DynamicDataAttribute(sourceName);
    }

    public IEnumerable<object?[]> GetData(MethodInfo testMethod)
    => _innerAttribute.GetData(testMethod);

    public string? GetDisplayName(MethodInfo testMethod, params object?[]? data)
    {
        ArgumentNullException.ThrowIfNull(testMethod);

        string? adatamiqName =
            data is { Length: > 0 } &&
            data[0] is string or INamedCase ?
                NamedCase.CreateDisplayName(testMethod.Name, data)
                : null;

        string mstestName =
            _innerAttribute.GetDisplayName(testMethod!, data)
            ?? string.Empty;

        return mstestName.FallbackIfNullOrEmpty(adatamiqName);
    }
}

public sealed class DynamicTestDataAttribute : DynamicTestDataAttributeBase
{
    public DynamicTestDataAttribute(string sourceName)
    : base(sourceName) { }

    public DynamicTestDataAttribute(string sourceName, DynamicDataSourceType sourceType)
    : base(sourceName, sourceType: sourceType) { }

    public DynamicTestDataAttribute(string sourceName, params object?[] sourceArgs)
    : base(sourceName, sourceArgs: sourceArgs) { }

    public DynamicTestDataAttribute(string sourceName, Type declaringType)
    : base(sourceName, declaringType: declaringType) { }

    public DynamicTestDataAttribute(string sourceName, Type declaringType, DynamicDataSourceType sourceType)
    : base(sourceName, declaringType: declaringType, sourceType: sourceType) { }

    public DynamicTestDataAttribute(string sourceName, Type declaringType, params object?[] sourceArgs)
    : base(sourceName, declaringType: declaringType, sourceArgs: sourceArgs) { }
}