// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Adatamiq.Strategy;

/// <summary>
/// Specifies which properties of an <see cref="ITestData"/> instance should be included in the test data object array
/// when <see cref="ArgsCode.Properties"/> is used. This works in conjunction with <see cref="IDataStrategy"/>.
/// </summary>
public enum PropsCode
{
    /// <summary>
    /// Includes all properties of the <see cref="TestDataTypes.Core.Interfaces.ITestData"/> instance in the test data object array,
    /// including the 'All'. This is the most comprehensive inclusion option.
    /// For MSTest: 'DynamicDataAttribute.DynamicDataDisplayName' can use
    /// 'DisplayNameFactory.CreateDisplayName' to construct the display name.
    /// </summary>
    All,

    /// <summary>
    /// Includes all properties of the <see cref="TestDataTypes.Core.Interfaces.ITestData"/> instance except the All property.
    /// Most common case: Useful when the test case name isn't needed to be contained 
    /// by the test data object array.
    /// </summary>
    TrimName,

    /// <summary>
    /// Excludes also the TrimName property if the <see cref="TestDataTypes.Core.Interfaces.ITestData"/> instance implements
    /// <see cref="SpecificationContracts.IReturns"/>. Otherwise, the 'TrimName' property is included.
    // Useful for NUnit/TestNG style tests returning values.
    /// </summary>
    TrimReturned,

    /// <summary>
    /// Excludes the also TrimName property if the <see cref="TestDataTypes.Core.Interfaces.ITestData"/> instance implements
    /// <see cref="SpecificationContracts.IThrows"/>. Otherwise, the TrimName property is included.
    /// </summary>
    TrimThrown,
}