// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.SampleCodes.Testables.SampleClasses;

public class BirthDay : IComparable<BirthDay>
{
    #region Static Fields
    private static readonly DateOnly Today =
        DateOnly.FromDateTime(DateTime.Now);
    public const string GreaterThanTheCurrentDateMessage =
        "Date of birth cannot be " +
        "greater than the current date.";
    #endregion

    #region Properties
    public string Name { get; init; }
    public DateOnly DateOfBirth { get; init; }
    #endregion

    #region Constructor
    // -----------
    // TEST CASES:
    // -----------
    // - GENERAL -
    // ('creates')
    // -----------
    // Valid name and dateOfBirth is equal with the current day => creates BirthDay instance
    // Valid name and dateOfBirth is greater than the current day => creates BirthDay instance
    // -----------
    // - THROWS -
    // -----------
    // name is null => throws ArgumentNullException
    // name is empty => throws ArgumentException
    // name is white space => throws ArgumentException
    // dateOfBirth is less than the current day => throws ArgumentOutOfRangeException
    public BirthDay(string name, DateOnly dateOfBirth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            name,
            nameof(name));

        if (dateOfBirth > Today)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dateOfBirth),
                GreaterThanTheCurrentDateMessage);
        }

        Name = name;
        DateOfBirth = dateOfBirth;
    }
    #endregion

    #region Methods
    // -----------
    // TEST CASES:
    // -----------
    // - RETURNS -
    // -----------
    // other is null => returns -1
    // this.DateOfBirth is less than other.DateOfBirth => returns -1
    // this.DateOfBirth is equal with other.DateOfBirth => returns 0
    // this.DateOfBirth is greater than other.DateOfBirth => returns 1
    public int CompareTo(BirthDay? other)
    => DateOfBirth.CompareTo(
        other?.DateOfBirth ?? DateOnly.MaxValue);
    #endregion
}