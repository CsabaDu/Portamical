// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.TestDataTypes.Patterns;

/// <summary>
/// Marker interface for test cases validating exception throwing behavior.
/// Inherits from <see cref="IExpected"/> and marks test data designed to throw an exception.
/// </summary>
/// <remarks>
/// Identifies tests verifying error handling and exceptional execution paths.
/// </remarks>
public interface IThrows
: IExpected;

/// <summary>
/// Represents an object that specifies an expected exception of a particular type that may be thrown during execution.
/// </summary>
/// <typeparam name="TException">The type of exception that is expected to be thrown. Must derive from <see cref="Exception"/>.</typeparam>
public interface IThrows<out TException>
: IExpected<TException>,
IThrows
where TException : Exception;