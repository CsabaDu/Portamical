// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.xUnit.Attributes;
using Portamical.xUnit.DataProviders;
using Portamical.xUnit.TestBases;
using static Portamical.xUnit.Assertions.PortamicalAssert;

namespace Portamical.SampleCodes.UnitTests.xUnit.Native;

public sealed class BithDayTestClass_xUnit_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static TestDataProvider<TestData<DateOnly>> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs(), AsProperties);

    [Theory, PortamicalData(nameof(BirthDayConstructorValidArgs))]
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

    public static TestDataProvider<TestDataThrows<ArgumentException, string>> BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs(), AsProperties);

    [Theory, PortamicalData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        ThrowsDetails(attempt, expected);
    }

    public static TestDataProvider<TestDataReturns<int, DateOnly, BirthDay>> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs(), AsProperties);

    [Theory, PortamicalData(nameof(CompareToArgs))]
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
