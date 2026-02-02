// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases.ObjectArray;
using static Portamical.Assertions.PortamicalAssertBase;

namespace Portamical.SampleCodes.UnitTests.xUnit_v3.Native;

public sealed class BithDayTestClass_xUnit_v3_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs(), AsProperties);

    [Theory, MemberData(nameof(BirthDayConstructorValidArgs))]
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

    public static IEnumerable<object?[]> BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs(), AsProperties);

    [Theory, MemberData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        ThrowsDetails(
            attempt,
            expected,
            assertIsType: Assert.IsType,
            assertEquality: Assert.Equal,
            assertFail: Assert.Fail,
            catchException: Record.Exception);
    }

    public static IEnumerable<object?[]> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs(), AsProperties);

    [Theory, MemberData(nameof(CompareToArgs))]
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
