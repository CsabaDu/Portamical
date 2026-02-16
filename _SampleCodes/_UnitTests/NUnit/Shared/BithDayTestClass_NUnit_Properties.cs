// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Identity.Model;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases.ObjectArrayCollection;
using System.Reflection;
using static Portamical.Assertions.PortamicalAssert;

namespace Portamical.SampleCodes.UnitTests.NUnit.Shared;

[TestFixture]
public sealed class BithDayTestClass_NUnit_PropertiesArray : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => Convert(_dataSource.GetBirthDayConstructorValidArgs(), AsProperties);

    [Test, TestCaseSource(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(DateOnly dateOfBirth)
    {
        // Arrange
        string name = "valid name";

        // Act
        var actual = new BirthDay(name, dateOfBirth);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Name, Is.EqualTo(name));
            Assert.That(actual.DateOfBirth, Is.EqualTo(dateOfBirth));
        }
    }

    public static IEnumerable<object?[]> BirthDayConstructorInvalidArgs
    => Convert(_dataSource.GetBirthDayConstructorInvalidArgs(), AsProperties);

    [Test, TestCaseSource(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            ThrowsDetails(
                attempt,
                expected,
                catchException: att => Assert.Catch(() => att()),
                assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
                assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
                assertFail: Assert.Fail);
        }
    }

    public static IEnumerable<object?[]> CompareToArgs
    => Convert(_dataSource.GetCompareToArgs(), AsProperties);

    [Test, TestCaseSource(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(int expected, DateOnly dateOfBirth, BirthDay other)
    {
        // Arrange
        const string name = "valid name";
        BirthDay sut = new(name, dateOfBirth);

        // Act
        var actual = sut.CompareTo(other);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }
}
