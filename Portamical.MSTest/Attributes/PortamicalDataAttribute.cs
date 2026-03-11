// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.MSTest.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class PortamicalBaseDataAttribute(
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
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

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

    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    => _innerAttribute.GetData(methodInfo);

    public string? GetDisplayName(MethodInfo methodInfo, params object?[]? data)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        string? displayName =
            data is { Length: > 0 } &&
            data[0] is string or INamedCase ?
                NamedCase.CreateDisplayName(methodInfo, data)
                : null;

        return displayName
            ?? _innerAttribute.GetDisplayName(methodInfo, data);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class PortamicalDataAttribute : PortamicalBaseDataAttribute
{
    public PortamicalDataAttribute(string sourceName)
    : base(sourceName) { }

    public PortamicalDataAttribute(string sourceName, DynamicDataSourceType sourceType)
    : base(sourceName, sourceType: sourceType) { }

    public PortamicalDataAttribute(string sourceName, params object?[] sourceArgs)
    : base(sourceName, sourceArgs: sourceArgs) { }

    public PortamicalDataAttribute(string sourceName, Type declaringType)
    : base(sourceName, declaringType: declaringType) { }

    public PortamicalDataAttribute(string sourceName, Type declaringType, DynamicDataSourceType sourceType)
    : base(sourceName, declaringType: declaringType, sourceType: sourceType) { }

    public PortamicalDataAttribute(string sourceName, Type declaringType, params object?[] sourceArgs)
    : base(sourceName, declaringType: declaringType, sourceArgs: sourceArgs) { }
}