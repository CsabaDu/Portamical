// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases;
using static Portamical.Assertions.PortamicalAssertBase;

namespace Portamical.SampleCodes.UnitTests.MSTest.Native;

[TestClass]
public sealed class BithDayTestClass_MSTest_InstanceArray : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    private static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => _dataSource.GetBirthDayConstructorValidArgs().ToDistinctReadOnly(AsInstance);

    [TestMethod, DynamicData(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(TestData<DateOnly> testData)
    {
        // Arrange
        string name = "valid name";
        DateOnly dateOfBirth = testData.Arg1;

        // Act
        var actual = new BirthDay(name, dateOfBirth);

        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual(name, actual.Name);
        Assert.AreEqual(dateOfBirth, actual.DateOfBirth);
    }

    private static IEnumerable<object?[]> BirthDayConstructorInvalidArgs
    => _dataSource.GetBirthDayConstructorInvalidArgs().ToDistinctReadOnly(AsInstance);

    [TestMethod, DynamicData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(TestDataThrows<ArgumentException, string> testData)
    {
        // Arrange
        string? name = testData.Arg1;
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        ArgumentException expected = testData.Expected;

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        ThrowsDetails(
            expected,
            attempt,
            assertIsType: (e, a) => Assert.AreEqual(e, a.GetType()),
            assertEquality: (e, a) => Assert.AreEqual(e, a),
            assertFail: Assert.Fail);
    }

    private static IEnumerable<object?[]> CompareToArgs
    => _dataSource.GetCompareToArgs().ToDistinctReadOnly(AsInstance);

    [TestMethod, DynamicData(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(TestDataReturns<int, DateOnly, BirthDay> testData)
    {
        // Arrange
        const string name = "valid name";
        DateOnly dateOfBirth = testData.Arg1;
        BirthDay? other = testData.Arg2;
        BirthDay sut = new(name, dateOfBirth);

        // Act
        var actual = sut.CompareTo(other);

        // Assert
        Assert.AreEqual(testData.Expected, actual);
    }
}
