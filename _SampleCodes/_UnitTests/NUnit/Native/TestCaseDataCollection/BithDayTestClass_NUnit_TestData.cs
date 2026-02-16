// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.TestDataTypes.Models.General;
using Portamical.Core.TestDataTypes.Models.Specialized;
using Portamical.NUnit.Assertions;
using Portamical.NUnit.Attributes;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases.TestDataCollection;

namespace Portamical.SampleCodes.UnitTests.NUnit.Native.TestCaseDataCollection;

[TestFixture]
public sealed class BithDayTestClass_NUnit_TestDatas : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static IEnumerable<TestData<DateOnly>> BirthDayConstructorValidArgs
    => Convert( _dataSource.GetBirthDayConstructorValidArgs());

    [Test, TestCaseDataSource(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(TestData<DateOnly> testData)
    {
        // Arrange
        string name = "valid name";
        DateOnly dateOfBirth = testData.Arg1;

        // Act
        var actual = new BirthDay(name, dateOfBirth);

        // Assert
        PortamicalAssert.AssertMultiple(() =>
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Name, Is.EqualTo(name));
            Assert.That(actual.DateOfBirth, Is.EqualTo(dateOfBirth));
        });
    }

    public static IEnumerable<TestDataThrows<ArgumentException, string>>? BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs());

    [Test, TestCaseDataSource(nameof(BirthDayConstructorInvalidArgs))]
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

    public static IEnumerable<TestDataReturns<int, DateOnly, BirthDay>>? CompareToArgs
    => Convert(_dataSource.GetCompareToArgs());

    [Test, TestCaseDataSource(nameof(CompareToArgs))]
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
        Assert.That(actual, Is.EqualTo(testData.Expected));
    }
}
