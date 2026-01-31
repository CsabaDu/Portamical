// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Assertions;
using Portamical.NUnit.Attributes;
using Portamical.NUnit.TestBases;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;

namespace Portamical.SampleCodes.UnitTests.NUnit.Specific;

[TestFixture]
public sealed class BithDayTestClass_NUnit_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    private static IEnumerable<TestCaseData> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs(), AsProperties, nameof(Ctor_validArgs_createInstance));

    [Test, TestCaseDataSource(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(DateOnly dateOfBirth)
    {
        // Arrange
        string name = "valid name";

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

    private static IEnumerable<TestCaseData> BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs(), AsProperties, nameof(Ctor_invalidArgs_throwsArgumentException));

    [Test, TestCaseDataSource(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        PortamicalAssert.ThrowsDetails(attempt, expected);
    }

    private static IEnumerable<TestCaseData> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs(), AsProperties, nameof(CompareTo_validArgs_returnsExpected));

    [Test, TestCaseDataSource(nameof(CompareToArgs))]
    public int CompareTo_validArgs_returnsExpected(DateOnly dateOfBirth, BirthDay other)
    {
        // Arrange
        const string name = "valid name";
        BirthDay sut = new(name, dateOfBirth);

        // Act
        var actual = sut.CompareTo(other);

        // Act & Assert
        return sut.CompareTo(other);
    }
}
