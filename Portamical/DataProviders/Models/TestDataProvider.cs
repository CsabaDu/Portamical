// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

/// <summary>
/// Provides an abstract base class for test data providers that exposes <c>protected</c> constructors,
/// enabling derivation from outside the assembly while inheriting distinct row management from
/// <see cref="DistinctDataProviderBase{TTestData, TRow}"/>.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// </typeparam>
/// <typeparam name="TRow">
/// The target row type for the test framework (e.g., <c>object[]</c>, <c>TheoryDataRow</c>).
/// </typeparam>
/// <remarks>
/// <para>
/// This class serves as an intermediary layer between <see cref="DistinctDataProviderBase{TTestData, TRow}"/>
/// (which has <c>private protected</c> constructors) and external assemblies that need to derive custom
/// test data providers.
/// </para>
/// <para>
/// <strong>Constructor Forwarding:</strong> All constructors simply forward to the base class, making
/// the <c>private protected</c> base constructors accessible via <c>protected</c> wrappers.
/// </para>
/// <para>
/// <strong>Inheritance Pattern:</strong> Derive from this class when building domain-specific test
/// data providers in external assemblies, and override <see cref="DistinctDataProviderBase{TTestData, TRow}.ConvertRow"/>
/// to implement custom row conversion logic.
/// </para>
/// </remarks>
public abstract class TestDataProvider<TTestData, TRow>
: DistinctDataProviderBase<TTestData, TRow>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Initializes a new instance with an empty collection of test data rows.
    /// </summary>
    /// <remarks>
    /// This constructor forwards to <see cref="DistinctDataProviderBase{TTestData, TRow}"/>,
    /// making the parameterless constructor available to derived classes in external assemblies.
    /// </remarks>
    protected TestDataProvider()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance and adds a single test data row.
    /// </summary>
    /// <param name="testData">
    /// The initial test data to add. Will be converted to a row via <see cref="DistinctDataProviderBase{TTestData, TRow}.ConvertRow"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the test case name already exists in the collection.
    /// </exception>
    /// <remarks>
    /// This constructor forwards to <see cref="DistinctDataProviderBase{TTestData, TRow}"/>,
    /// making single-item initialization available to derived classes in external assemblies.
    /// </remarks>
    protected TestDataProvider(TTestData testData)
        : base(testData)
    {
    }

    /// <summary>
    /// Initializes a new instance and adds multiple test data rows.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to add. Each item will be converted to a row via <see cref="DistinctDataProviderBase{TTestData, TRow}.ConvertRow"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if any test case name in the collection is duplicated.
    /// </exception>
    /// <remarks>
    /// This constructor forwards to <see cref="DistinctDataProviderBase{TTestData, TRow}"/>,
    /// making bulk initialization available to derived classes in external assemblies.
    /// </remarks>
    protected TestDataProvider(IEnumerable<TTestData> testDataCollection)
        : base(testDataCollection)
    {
    }
}