// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.xUnit_v3.Assertions;
using Portamical.xUnit_v3.Attributes;
using Portamical.xUnit_v3.DataProviders.Model;
using Portamical.xUnit_v3.TestBases;

namespace Portamical.SampleCodes.UnitTests.xUnit.Specific;

public sealed class BithDayTestClass_xUnit_v3_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static TheoryTestData<TestData<DateOnly>> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs(),
        AsProperties,
        nameof(Ctor_validArgs_createInstance));

    [Theory, MemberTestData(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(DateOnly dateOfBirth)
    {
        // Arrange
        string name = "valid name";

        // Act
        var actual = new BirthDay(name, dateOfBirth);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(name, actual.Name);
        Assert.Equal(dateOfBirth, actual.DateOfBirth);
    }

    public static TheoryTestData<TestDataThrows<ArgumentException, string>> BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs(),
        AsProperties,
        nameof(Ctor_invalidArgs_throwsArgumentException));

    [Theory, MemberTestData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        PortamicalAssert.ThrowsDetails(attempt, expected);
    }

    public static TheoryTestData<TestDataReturns<int, DateOnly, BirthDay>> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs(),
        AsProperties,
        nameof(CompareTo_validArgs_returnsExpected));

    [Theory, MemberTestData(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(int expected, DateOnly dateOfBirth, BirthDay other)
    {
        // Arrange
        const string name = "valid name";
        BirthDay sut = new(name, dateOfBirth);

        // Act
        var actual = sut.CompareTo(other);

        // Assert
        Assert.Equal(expected, actual);
    }
}
