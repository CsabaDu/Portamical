// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.DataProviders.Models;

/// <summary>
/// Provides an abstract base class for test data providers that exposes <c>protected</c> constructors,
/// enabling derivation from outside the assembly while inheriting distinct row management from
/// <see cref="DataProviderBase{TTestData, TRow}"/>.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// </typeparam>
/// <typeparam name="TRow">
/// The target row type for the test framework (e.g., <c>object[]</c>, <c>TheoryDataRow</c>).
/// </typeparam>
/// <remarks>
/// <para>
/// This class serves as an intermediary layer between <see cref="DataProviderBase{TTestData, TRow}"/>
/// (which has <c>private protected</c> constructors) and external assemblies that need to derive custom
/// test data providers.
/// </para>
/// <para>
/// <strong>Constructor Forwarding:</strong> All constructors simply forward to the base class, making
/// the <c>private protected</c> base constructors accessible via <c>protected</c> wrappers.
/// </para>
/// <para>
/// <strong>Inheritance Pattern:</strong> Derive from this class when building domain-specific test
/// data providers in external assemblies, and override <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/>
/// to implement custom row conversion logic.
/// </para>
/// </remarks>
public abstract class TestDataProvider<TTestData, TRow>
: DataProviderBase<TTestData, TRow>
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Initializes a new instance with an empty collection of test data rows.
    /// </summary>
    /// <remarks>
    /// This constructor forwards to <see cref="DataProviderBase{TTestData, TRow}"/>,
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
    /// The initial test data to addRange. It is stored unconverted; conversion via
    /// <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/> happens lazily whenever the
    /// corresponding row is subsequently read.
    /// </param>
    /// <remarks>
    /// This constructor forwards to <see cref="DataProviderBase{TTestData, TRow}"/>,
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
    /// The collection of test data to addRange. Each item is stored unconverted; conversion via
    /// <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/> happens lazily whenever the
    /// corresponding row is subsequently read. Duplicate test cases (per <see cref="NamedCase.Comparer"/>)
    /// are silently filtered out.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the collection is empty.
    /// </exception>
    /// <remarks>
    /// This constructor forwards to <see cref="DataProviderBase{TTestData, TRow}"/>,
    /// making bulk initialization available to derived classes in external assemblies.
    /// </remarks>
    protected TestDataProvider(IEnumerable<TTestData> testDataCollection)
    : base(testDataCollection)
    {
    }
}