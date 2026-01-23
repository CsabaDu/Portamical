// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.MSTest.Attributes;
using Portamical.MSTest.TestBases;
using Portamical.SampleCodes.DataSources;
using Portamical.SampleCodes.Testables;
using Portamical.TestBases;
using Portamical.TestDataTypes.Models.General;
using Portamical.TestDataTypes.Models.Specialized;
using System.Data.Common;

namespace Portamical.SampleCodes.UnitTests.MSTest;

[TestClass]
public sealed class BithDayTestClass_MSTest_TestDatas
: TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    private static IEnumerable<TestData<DateOnly>> BirthDayConstructorValidArgs
    => _dataSource.GetBirthDayConstructorValidArgs();

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

    private static IEnumerable<TestDataThrows<ArgumentException, string>>? BirthDayConstructorInvalidArgs
    => _dataSource.GetBirthDayConstructorInvalidArgs();

    [TestMethod, DynamicTestData(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(TestDataThrows<ArgumentException, string> testData)
    {
        // Arrange
        string? name = testData.Arg1;
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        TestBase_MSTest.AssertThrowsDetails(attempt, testData.Expected);
    }

    private static IEnumerable<TestDataReturns<int, DateOnly, BirthDay>>? CompareToArgs
    => _dataSource.GetCompareToArgs();

    [TestMethod, DynamicTestData(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(TestDataReturns<int, DateOnly, BirthDay> testData)
    {
        // Arrange
        string name = "valid name";
        DateOnly dateOfBirth = testData.Arg1;
        BirthDay? other = testData.Arg2;
        BirthDay sut = new(name, dateOfBirth);

        // Act
        var actual = sut.CompareTo(other);

        // Assert
        Assert.AreEqual(testData.Expected, actual);
    }
}