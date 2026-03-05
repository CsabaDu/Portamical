// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Portamical.xUnit.Attributes;

[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class MemberTestDataAttribute(string memberName, params object?[]? parameters)
: MemberDataAttributeBase(memberName, parameters)
{
    /// <inheritdoc/>
    protected override object?[]? ConvertDataItem(MethodInfo testMethod, object item)
    => item switch
    {
        null => null,
        object?[] => (object?[])item,
        ITestData => (item as ITestData)?.ToArgs(ArgsCode.Instance),
        _ => throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                "Property {0} on {1} yielded an item that is not an object[]",
                MemberName,
                MemberType ?? testMethod.DeclaringType)),
    };
}
