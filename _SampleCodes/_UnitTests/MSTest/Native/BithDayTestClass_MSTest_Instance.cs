// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.MSTest.Attributes;
using Portamical.MSTest.TestBases;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using static Portamical.MSTest.Assertions.PortamicalAssert;

namespace Portamical.SampleCodes.UnitTests.MSTest.Native;

[TestClass]
public sealed class BithDayTestClass_MSTest_Instance : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    private static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs());

    [TestMethod, DynamicTestData(nameof(BirthDayConstructorValidArgs))]
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
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs());

    [TestMethod, DynamicTestData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(TestDataThrows<ArgumentException, string> testData)
    {
        // Arrange
        string? name = testData.Arg1;
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        ArgumentException expected = testData.Expected;

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        ThrowsDetails(attempt, expected);
    }

    private static IEnumerable<object?[]> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs());

    [TestMethod, DynamicTestData(nameof(CompareToArgs))]
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
