// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Converters;
using Portamical.Core.Identity.Model;
using Portamical.SampleCodes.DataSources.TestDataSources;
using Portamical.SampleCodes.Testables.SampleClasses;
using Portamical.TestBases;
using System.Reflection;
using static Portamical.Assertions.PortamicalAssertBase;

namespace Portamical.SampleCodes.UnitTests.MSTest.Native;

[TestClass]
public sealed class BithDayTestClass_MSTest_Properties : TestBase
{
    private static readonly BirthDayDataSource _dataSource = new();

    public static string? GetDisplayName(MethodInfo testMethod, object?[]? args)
    => NamedCase.CreateDisplayName(testMethod, args);

    private const string DisplayName = nameof(GetDisplayName);

    private static IEnumerable<object?[]> BirthDayConstructorValidArgs
    => _dataSource.GetBirthDayConstructorValidArgs().ToDistinctReadOnly(AsProperties, WithTestCaseName);

    [TestMethod, DynamicData(nameof(BirthDayConstructorValidArgs), DynamicDataDisplayName = DisplayName)]
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
    => _dataSource.GetBirthDayConstructorInvalidArgs().ToDistinctReadOnly(AsProperties, WithTestCaseName);

    [TestMethod, DynamicData(nameof(BirthDayConstructorInvalidArgs), DynamicDataDisplayName = DisplayName)]
    public void Ctor_invalidArgs_throwsArgumentException(string ignore, ArgumentException expected, string name)
    {
        // Arrange
        DateOnly dateOfBirth = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

        // Act
        void attempt() => _ = new BirthDay(name!, dateOfBirth);

        // Assert
        ThrowsDetails(
            attempt,
            expected,
            catchException: CatchException,
            assertIsType: (e, a) => Assert.AreEqual(e, a.GetType()),
            assertEquality: (e, a) => Assert.AreEqual(e, a),
            assertFail: Assert.Fail);
    }

    private static IEnumerable<object?[]> CompareToArgs
    => _dataSource.GetCompareToArgs().ToDistinctReadOnly(AsProperties, WithTestCaseName);

    [TestMethod, DynamicData(nameof(CompareToArgs), DynamicDataDisplayName = DisplayName)]
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
