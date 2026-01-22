// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.Strategy;

/// <summary>
/// Specifies which properties of an <see cref="ITestData"/> instance should be included in the test data object array
/// when <see cref="ArgsCode.Properties"/> is used. This works in conjunction with <see cref="IDataStrategy"/>.
/// </summary>
public enum PropsCode
{
    /// <summary>
    /// Includes all properties of the <see cref="TestDataTypes.ITestData"/> instance in the test data object array,
    /// including the <see cref="Identity.INamedCase.TestCaseName"/>.
    /// This is the most comprehensive inclusion option.
    /// For MSTest: 'DynamicDataAttribute.DynamicDataDisplayName' can use
    /// <see cref="Identity.Model.NamedCase.CreateDisplayName(string?, object?[]?)"/>
    /// to construct the display name.
    /// </summary>
    All,

    /// <summary>
    /// Includes all properties of the <see cref="TestDataTypes.ITestData"/> instanc
    /// except the <see cref="Identity.INamedCase.TestCaseName"/> property.
    /// Most common case: Useful when the test case name isn't needed to be contained 
    /// by the test data object array.
    /// </summary>
    TrimName,

    /// <summary>
    /// Excludes also the <see cref="TestDataTypes.Patterns.IExpected{TExpected}.Expected"/> property
    /// if the <see cref="TestDataTypes.ITestData"/> instance implements
    /// <see cref="TestDataTypes.Patterns.IReturns"/>. Otherwise, the 'TrimName' property is included.
    // Useful for NUnit/TestNG style tests returning values.
    /// </summary>
    TrimReturned,

    /// <summary>
    /// Excludes the also <see cref="TestDataTypes.Patterns.IExpected{TExpected}.Expected"/> property
    /// if the <see cref="TestDataTypes.ITestData"/> instance implements
    /// <see cref="TestDataTypes.Patterns.IThrows"/>. Otherwise, the TrimName property is included.
    /// </summary>
    TrimThrown,
}