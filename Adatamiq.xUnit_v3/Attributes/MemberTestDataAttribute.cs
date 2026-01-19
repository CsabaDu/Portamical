// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.xUnit_v3.Converters;
using Adatamiq.xUnit_v3.TestDataTypes;
using Adatamiq.xUnit_v3.TestDataTypes.Model;

namespace Adatamiq.xUnit_v3.Attributes;

/// <summary>
/// Provides a data source for a theory test, with the data coming from a member of the test class.
/// Extends <see cref="MemberDataAttributeBase"/> with additional functionality.
/// </summary>
public abstract class MemberTestDataAttributeBase
: MemberDataAttributeBase
{
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTestDataAttributeBase"/> class.
    /// </summary>
    /// <param name="memberName">The name of the public member that will provide the test data.</param>
    /// <param name="arguments">The arguments to be passed to the member (only supported for static members).</param>
    private protected MemberTestDataAttributeBase(
        string memberName,
        params object[] arguments)
    : base(memberName, arguments)
    {
        DisableDiscoveryEnumeration = true;
    }
    #endregion

    #region Methods

    /// <inheritdoc/>
    public override async ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var testMethodName = testMethod.Name;
        var theoryDataRowCollection =
            await base.GetData(testMethod, disposalTracker)
            .ConfigureAwait(false);

        if (theoryDataRowCollection is null)
        {
            return [];
        }

        if (string.IsNullOrEmpty(testMethodName))
        {
            return theoryDataRowCollection;
        }

        var runtimeGenericType = theoryDataRowCollection
            .GetType()
            .GetGenericArguments()?[0];

        if (runtimeGenericType?.IsAssignableTo(typeof(ITheoryTestDataRow)) != true)
        {
            return theoryDataRowCollection;
        }

        HashSet<ITheoryTestDataRow> ttdrCollection = new(NamedCase.Comparer);

        foreach (var item in theoryDataRowCollection)
        {
            var ttdr = (item as ITheoryTestDataRow)!;

            if (string.IsNullOrEmpty(ttdr.TestDisplayName))
            {
                ttdr = new TheoryTestDataRow(ttdr, testMethodName);
            }

            ttdrCollection.Add(ttdr);
        }

        return ttdrCollection.CastOrToReadOnlyCollection();
    }

    /// <inheritdoc/>
    protected override ITheoryDataRow ConvertDataRow(object dataRow)
    {
        if (dataRow is not ITheoryTestDataRow theoryTestDataRow)
        {
            if (dataRow is not ITestData testData)
            {
                return base.ConvertDataRow(dataRow);
            }

            if (Arguments is not [ArgsCode argsCode])
            {
                argsCode = ArgsCode.Instance;
            }

            return testData.ToTheoryTestDataRow(argsCode);
        }

        var ttdrTraits = theoryTestDataRow.Traits;
        var traits = new Dictionary<string, HashSet<string>>(
            StringComparer.OrdinalIgnoreCase);

        if (ttdrTraits is not null)
        {
            foreach (var kvp in ttdrTraits)
            {
                traits.AddOrGet(kvp.Key).AddRange(kvp.Value);
            }
        }

        TestIntrospectionHelper.MergeTraitsInto(traits, Traits);

        return new TheoryTestDataRow(theoryTestDataRow, null);
    }
    #endregion
}

public sealed class MemberTestDataAttribute(
    string memberName,
    params object[] arguments)
: MemberTestDataAttributeBase(
    memberName,
    arguments);
