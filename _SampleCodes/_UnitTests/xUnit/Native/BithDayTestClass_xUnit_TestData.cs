// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Assertions;
using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases;

namespace Portamical.SampleCodes.UnitTests.xUnit.Native;

public sealed class BithDayTestClass_xUnit_TestData : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static TheoryData<TestData<DateOnly>> BirthDayConstructorValidArgs
    => [.. _dataSource.GetBirthDayConstructorValidArgs()];

    [Theory, MemberData(nameof(BirthDayConstructorValidArgs))]
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
    => [.. _dataSource.GetBirthDayConstructorInvalidArgs()];

    [Theory, MemberData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(TestDataThrows<ArgumentException, string> testData)
    {
        // Arrange
        string? name = testData.Arg1;
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        ArgumentException expected = testData.Expected;

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        PortamicalAssertBase.ThrowsDetails(
            attempt,
            expected,
            assertIsType: Assert.IsType,
            assertEquality: Assert.Equal,
            assertFail: Assert.Fail,
            catchException: Record.Exception);

    }

    public static TheoryData<TestDataReturns<int, DateOnly, BirthDay>>? CompareToArgs
    => [.. _dataSource.GetCompareToArgs()];

    [Theory, MemberData(nameof(CompareToArgs))]
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
