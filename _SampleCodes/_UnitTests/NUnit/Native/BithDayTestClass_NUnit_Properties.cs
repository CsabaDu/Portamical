// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Core.Identity.Model;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases;
using System.Reflection;
using static Portamical.Assertions.PortamicalAssertBase;

namespace Portamical.SampleCodes.UnitTests.NUnit.Native;

[TestFixture]
public sealed class BithDayTestClass_NUnit_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => _dataSource.GetBirthDayConstructorValidArgs().ToDistinctReadOnly(AsProperties, WithTestCaseName);

    [Test, TestCaseSource(nameof(BirthDayConstructorValidArgs))]
    public void Ctor_validArgs_createInstance(string ignore, DateOnly dateOfBirth)
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
    => _dataSource.GetBirthDayConstructorInvalidArgs().ToDistinctReadOnly(AsProperties, WithTestCaseName);

    [Test, TestCaseSource(nameof(BirthDayConstructorInvalidArgs))]
    public void Ctor_invalidArgs_throwsArgumentException(string ignore, ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            ThrowsDetails(
                expected,
                attempt,
                assertIsType: (e, a) => Assert.That(a, Is.TypeOf(e)),
                assertEquality: (e, a) => Assert.That(a, Is.EqualTo(e)),
                assertFail: Assert.Fail,
                catchException: att => Assert.Catch(() => att()));
        }
    }

    public static IEnumerable<object?[]> CompareToArgs
    => _dataSource.GetCompareToArgs().ToDistinctReadOnly(AsProperties, WithTestCaseName);

    [Test, TestCaseSource(nameof(CompareToArgs))]
    public void CompareTo_validArgs_returnsExpected(string ignore, int expected, DateOnly dateOfBirth, BirthDay other)
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
