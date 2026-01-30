// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)using System;

using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;

namespace Portamical.MSTest.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class DynamicTestDataAttributeBase(
    string sourceName,
    Type? declaringType = null,
    DynamicDataSourceType? sourceType = null,
    object?[]? sourceArgs = null)
: Attribute, ITestDataSource
{
    private readonly DynamicDataAttribute _innerAttribute =
        Create(sourceName, declaringType, sourceType, sourceArgs);

    private static DynamicDataAttribute Create(
        string sourceName,
        Type? declaringType,
        DynamicDataSourceType? sourceType,
        object?[]? sourceArgs)
    {
        if (declaringType is not null && sourceType is not null)
        {
            return new DynamicDataAttribute(sourceName, declaringType, sourceType.Value);
        }

        if (declaringType is not null && sourceArgs is not null)
        {
            return new DynamicDataAttribute(sourceName, declaringType, sourceArgs);
        }

        if (declaringType is not null)
        {
            return new DynamicDataAttribute(sourceName, declaringType);
        }

        if (sourceType is not null)
        {
            return new DynamicDataAttribute(sourceName, sourceType.Value);
        }

        if (sourceArgs is not null)
        {
            return new DynamicDataAttribute(sourceName, sourceArgs);
        }

        return new DynamicDataAttribute(sourceName);
    }

    public IEnumerable<object?[]> GetData(MethodInfo testMethod)
    => _innerAttribute.GetData(testMethod);

    public string? GetDisplayName(MethodInfo testMethod, params object?[]? data)
    {
        ArgumentNullException.ThrowIfNull(testMethod);

        string? displayName =
            data is { Length: > 0 } &&
            data[0] is string or INamedCase ?
                NamedCase.CreateDisplayName(testMethod.Name, data)
                : null;

        string defaultName =
            _innerAttribute.GetDisplayName(testMethod!, data)
            ?? string.Empty;

        return defaultName.FallbackIfNullOrEmpty(displayName);
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