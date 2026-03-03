// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)using System;

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
        ArgumentException.ThrowIfNullOrEmpty(sourceName, nameof(sourceName));

        return (declaringType, sourceType, sourceArgs) switch
        {
            (_, not null, not null) => throw new ArgumentException(
                "Cannot specify both sourceType and sourceArgs"),

            (not null, not null, null) => new(sourceName, declaringType, sourceType.Value),
            (not null, null, not null) => new(sourceName, declaringType, sourceArgs),
            (not null, null, null)     => new(sourceName, declaringType),
            (null, not null, null)     => new(sourceName, sourceType.Value),
            (null, null, not null)     => new(sourceName, sourceArgs),
            _                          => new(sourceName),
        };
    }

    public IEnumerable<object?[]> GetData(MethodInfo testMethod)
    => _innerAttribute.GetData(testMethod);

    public string? GetDisplayName(MethodInfo testMethod, params object?[]? data)
    {
        ArgumentNullException.ThrowIfNull(testMethod);

        string? displayName =
            data is { Length: > 0 } &&
            data[0] is string or INamedCase ?
                NamedCase.CreateDisplayName(testMethod, data)
                : null;

        return displayName
            ?? _innerAttribute.GetDisplayName(testMethod, data);
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