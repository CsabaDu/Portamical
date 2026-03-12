// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;

namespace Portamical.TestBases.TestDataCollection;

/// <summary>
/// Provides an abstract base class for test implementations that require standardized handling of test data
/// collections with automatic deduplication.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="TestBases.TestBase"/> to provide specialized utilities for test scenarios
/// involving collections of test data. It ensures that all test data collections are processed to contain
/// only distinct elements, preventing issues caused by duplicate test cases.
/// </para>
/// <para>
/// <strong>When to Use:</strong>
/// <list type="bullet">
///   <item>Test classes that aggregate test data from multiple sources</item>
///   <item>Scenarios where duplicate test cases could cause confusion or test failures</item>
///   <item>Tests requiring guaranteed unique test data elements</item>
/// </list>
/// </para>
/// <para>
/// <strong>Deduplication:</strong> Uses equality comparison based on <see cref="ITestData"/> equality semantics.
/// Test data objects with identical <see cref="ITestData.TestCaseName"/> or equivalent definitions are
/// considered duplicates and only one instance is retained.
/// </para>
/// <para>
/// <strong>Inheritance Hierarchy:</strong>
/// <list type="bullet">
///   <item><see cref="TestBases.TestBase"/> - General test base class</item>
///   <item><see cref="TestBase"/> (this class) - Specialized for distinct test data collections</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class CalculatorTests : TestBase
/// {
///     public static IEnumerable&lt;object[]&gt; TestCases
///     {
///         get
///         {
///             // Collect from multiple sources
///             var source1 = GetTestDataFromSource1();
///             var source2 = GetTestDataFromSource2();
///             var combined = source1.Concat(source2);
///             
///             // Convert to distinct collection (removes duplicates)
///             var distinct = Convert(combined);
///             
///             return distinct.Select(td =&gt; new object[] { td });
///         }
///     }
///     
///     [Theory, MemberData(nameof(TestCases))]
///     public void Test(TestDataReturns&lt;int&gt; testData)
///     {
///         // Test with guaranteed unique test data
///     }
/// }
/// </code>
/// </example>
public abstract class TestBase : TestBases.TestBase
{
    /// <summary>
    /// Converts an enumerable collection of test data into a distinct read-only collection.
    /// </summary>
    /// <typeparam name="TTestData">
    /// The type of test data elements. Must be non-nullable and implement <see cref="ITestData"/>.
    /// </typeparam>
    /// <param name="testDataCollection">
    /// The source collection of test data that may contain duplicates.
    /// </param>
    /// <returns>
    /// A read-only collection containing only distinct test data elements from <paramref name="testDataCollection"/>.
    /// Duplicates are removed based on equality comparison.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method ensures that the returned collection contains no duplicate test data elements.
    /// Deduplication is performed using the default equality comparer for <typeparamref name="TTestData"/>,
    /// which typically compares test data based on <see cref="ITestData.TestCaseName"/> or equivalent
    /// properties.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> This method materializes the collection into an array, so it is
    /// not suitable for infinite or very large sequences. For typical test data scenarios (hundreds
    /// or thousands of test cases), performance is excellent.
    /// </para>
    /// <para>
    /// <strong>Order Preservation:</strong> The order of elements is preserved from the source collection,
    /// with the first occurrence of each duplicate retained.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Scenario: Combining test data from multiple sources
    /// var basicTests = GetBasicCalculatorTests();
    /// var edgeTests = GetEdgeCaseTests();
    /// var combined = basicTests.Concat(edgeTests);
    /// 
    /// // Remove duplicates (e.g., if both sources include "Add(0,0) => returns 0")
    /// var distinct = Convert(combined);
    /// 
    /// // Result: Each test case appears exactly once
    /// foreach (var test in distinct)
    /// {
    ///     Console.WriteLine(test.TestCaseName);
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="testDataCollection"/> is <c>null</c>.
    /// </exception>
    protected static IReadOnlyCollection<TTestData> Convert<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.ToDistinctArray();
}
