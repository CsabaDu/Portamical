// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.DataProviders.CustomRow;

namespace Portamical.DataProviders.Models.CustomRow;

/// <summary>
/// Provides an abstract base implementation of <see cref="ITestDataProvider{TTestData, TRow}"/> that
/// converts test data into custom typed rows with configurable argument conversion and test method association.
/// </summary>
/// <typeparam name="TTestData">
/// The test data type that implements <see cref="ITestData"/>. Must be a non-nullable reference type.
/// </typeparam>
/// <typeparam name="TRow">
/// The custom row type produced by the provider. This can be any type suitable for the target test framework,
/// such as xUnit v3's strongly-typed <c>TheoryDataRow&lt;T1, T2, ...&gt;</c>, tuple types, or custom record types.
/// </typeparam>
/// <remarks>
/// <para>
/// This abstract class provides the foundation for building custom typed row providers. Derived classes
/// must implement <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/> to define how
/// test data is transformed into the target <typeparamref name="TRow"/> type.
/// </para>
/// <para>
/// <strong>Constructor Accessibility:</strong> The parameterless constructor is marked <c>private</c>
/// to enforce that <see cref="ArgsCode"/> and <see cref="TestMethodName"/> are always explicitly specified
/// during construction. All protected constructors require these parameters, ensuring consistent configuration.
/// </para>
/// <para>
/// <strong>Use Cases:</strong>
/// </para>
/// <list type="bullet">
///   <item>xUnit v3 strongly-typed <c>TheoryDataRow&lt;T1, T2, ...&gt;</c> providers</item>
///   <item>Custom test frameworks with domain-specific row types</item>
///   <item>Scenarios requiring type-safe row representations</item>
///   <item>Building specialized providers with rich row metadata</item>
/// </list>
/// <para>
/// <strong>Framework Compatibility:</strong> The generic <typeparamref name="TRow"/> parameter makes this
/// class adaptable to any test framework or custom row format requirement.
/// </para>
/// <para>
/// <strong>Constructor Ordering Contract:</strong> The constructors that accept <c>testData</c> or
/// <c>testDataCollection</c> chain to <c>this(argsCode, testMethodName)</c> before calling
/// <see cref="DataProviderBase{TTestData, TRow}.AddRow"/> or
/// <see cref="DataProviderBase{TTestData, TRow}.AddRange"/>, guaranteeing that <see cref="ArgsCode"/>
/// and <see cref="TestMethodName"/> are fully initialized before any row conversion occurs. Derived classes
/// that introduce additional constructor parameters or fields consumed by their
/// <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/> override <strong>must preserve this
/// ordering</strong>: any state referenced by <c>ConvertRow</c> has to be assigned via constructor chaining
/// (e.g. <c>: this(...)</c> or <c>: base(...)</c>) before invoking <c>AddRow</c>/<c>AddRange</c>, since those
/// methods trigger <c>ConvertRow</c> immediately and will otherwise observe uninitialized (default) values.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Example derived class for xUnit v3
/// public class XUnit3Provider&lt;T1, T2, TConvertedRows&gt; 
///     : TestDataProvider&lt;TestDataReturns&lt;TConvertedRows&gt;, TheoryDataRow&lt;T1, T2, TConvertedRows&gt;&gt;
/// {
///     public XUnit3Provider(ArgsCode argsCode, string testMethodName)
///         : base(argsCode, testMethodName) { }
/// 
///     public override TheoryDataRow&lt;T1, T2, TConvertedRows&gt; ConvertRow(TestDataReturns&lt;TConvertedRows&gt; testData)
///     {
///         // Convert test data to strongly-typed row
///         return new TheoryDataRow&lt;T1, T2, TConvertedRows&gt;(
///             (T1)testData.Args[0],
///             (T2)testData.Args[1],
///             testData.Expected);
///     }
/// }
/// </code>
/// </example>
public abstract class TestDataProvider<TTestData, TRow>
: DataProviderBase<TTestData, TRow>,
ITestDataProvider<TTestData, TRow> 
where TTestData : notnull, ITestData
{
    /// <summary>
    /// Initializes a new instance with an empty collection. Private to enforce explicit
    /// <see cref="ArgsCode"/> and <see cref="TestMethodName"/> configuration.
    /// </summary>
    private TestDataProvider()
    : base()
    {
    }

    /// <summary>
    /// Initializes a new instance with specified conversion strategy and test method association.
    /// </summary>
    /// <param name="argsCode">
    /// The argument code determining the conversion strategy.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method associated with this provider, or <see langword="null"/> if not applicable.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="argsCode"/> is undefined or invalid.
    /// </exception>
    /// <remarks>
    /// Use this constructor when building the test data collection incrementally via
    /// <see cref="DataProviderBase{TTestData, TRow}.AddRow"/> or
    /// <see cref="DataProviderBase{TTestData, TRow}.AddRange"/>.
    /// </remarks>
    protected TestDataProvider(ArgsCode argsCode, string? testMethodName)
    : this()
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
        TestMethodName = testMethodName;
    }

    /// <summary>
    /// Initializes a new instance with a single test data item, conversion strategy, and test method association.
    /// </summary>
    /// <param name="testData">
    /// The initial test data to addRange to the provider.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the conversion strategy.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method associated with this provider, or <see langword="null"/> if not applicable.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="argsCode"/> is undefined or invalid, or if the test case name conflicts.
    /// </exception>
    /// <remarks>
    /// The test data is converted and added immediately during construction via <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/>.
    /// </remarks>
    protected TestDataProvider(TTestData testData, ArgsCode argsCode, string? testMethodName)
    : this(argsCode, testMethodName)
    {
        AddRow(testData);
    }

    /// <summary>
    /// Initializes a new instance with a collection of test data items, conversion strategy, and test method association.
    /// </summary>
    /// <param name="testDataCollection">
    /// The collection of test data to addRange to the provider.
    /// </param>
    /// <param name="argsCode">
    /// The argument code determining the conversion strategy.
    /// </param>
    /// <param name="testMethodName">
    /// The name of the test method associated with this provider, or <see langword="null"/> if not applicable.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="testDataCollection"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="argsCode"/> is undefined or invalid, if the collection is empty,
    /// or if it contains duplicate test case names.
    /// </exception>
    /// <remarks>
    /// All items are converted and added during construction.
    /// </remarks>
    protected TestDataProvider(IEnumerable<TTestData> testDataCollection, ArgsCode argsCode, string? testMethodName)
    : this(argsCode, testMethodName)
    {
        AddRange(testDataCollection);
    }

    /// <summary>
    /// Gets the argument code that determines how test data is converted into row format.
    /// </summary>
    /// <value>
    /// An <see cref="ArgsCode"/> value set during construction, controlling the conversion strategy
    /// used by derived class implementations of <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/>.
    /// </value>
    /// <remarks>
    /// This property is immutable after construction (init-only). Derived classes should use this
    /// value in their <see cref="DataProviderBase{TTestData, TRow}.ConvertRow"/> implementation
    /// to determine whether to pass instance or flatten properties.
    /// </remarks>
    public ArgsCode ArgsCode { get; init; }

    /// <summary>
    /// Gets the name of the test method associated with this provider instance.
    /// </summary>
    /// <value>
    /// The test method name set during construction, or <see langword="null"/> if no specific test method is associated.
    /// </value>
    /// <remarks>
    /// This property is immutable after construction (init-only). It can be used by test frameworks
    /// for method association, logging, or metadata purposes.
    /// </remarks>
    public string? TestMethodName { get; init; }
}
