// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.MSTest.TestBases;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using static Portamical.MSTest.Assertions.PortamicalAssert;

namespace Portamical.SampleCodes.UnitTests.MSTest.Specific;

[TestClass]
public sealed class BithDayTestClass_MSTest_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    private static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs(), AsProperties);

    [TestMethod, DynamicData(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(string ignore, DateOnly dateOfBirth)
    {
        // Arrange
        string name = "valid name";

        // Act
        var actual = new BirthDay(name, dateOfBirth);

        // Assert
        Assert.IsNotNull(actual);
        Assert.AreEqual(name, actual.Name);
        Assert.AreEqual(dateOfBirth, actual.DateOfBirth);
    }

    private static IEnumerable<object?[]> BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs(), AsProperties);

    [TestMethod, DynamicData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(string ignore, ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        ThrowsDetails(attempt, expected);
    }

    private static IEnumerable<object?[]> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs(), AsProperties);

    [TestMethod, DynamicData(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(string ignore, int expected, DateOnly dateOfBirth, BirthDay other)
    {
        // Arrange
        const string name = "valid name";
        BirthDay sut = new(name, dateOfBirth);

        // Act
        var actual = sut.CompareTo(other);

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
