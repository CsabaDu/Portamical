// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.xUnit_v3.Assertions;
using Portamical.xUnit_v3.Attributes;
using Portamical.TestBases.TestDataCollection;

namespace Portamical.SampleCodes.UnitTests.xUnit_v3.Specific.TheoryDataRowCollection;

public sealed class BithDayTestClass_xUnit_v3_TestData : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static TheoryData<TestData<DateOnly>> BirthDayConstructorValidArgs
    => [.. Convert(_dataSource.GetBirthDayConstructorValidArgs())];

    [Theory, MemberTestData(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(TestData<DateOnly> testData)
    {
        // Arrange
        string name = "valid name";
        DateOnly dateOfBirth = testData.Arg1;

        // Act
        var actual = new BirthDay(name, dateOfBirth);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(name, actual.Name);
        Assert.Equal(dateOfBirth, actual.DateOfBirth);
    }

    public static TheoryData<TestDataThrows<ArgumentException, string>>? BirthDayConstructorInvalidArgs
    => [.. Convert(_dataSource.GetBirthDayConstructorInvalidArgs())];

    [Theory, MemberTestData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(TestDataThrows<ArgumentException, string> testData)
    {
        // Arrange
        string? name = testData.Arg1;
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        PortamicalAssert.ThrowsDetails(attempt, testData.Expected);
    }

    public static TheoryData<TestDataReturns<int, DateOnly, BirthDay>>? CompareToArgs
    => [.. Convert(_dataSource.GetCompareToArgs())];

    [Theory, MemberTestData(nameof(CompareToArgs))]
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
        Assert.Equal(testData.Expected, actual);
    }
}