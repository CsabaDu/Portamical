// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

namespace Portamical.Core.TestDataTypes.Patterns;

/// <summary>
/// Marker interface for test cases validating method return values.
/// Inherits from <see cref="IExpected"/> and marks test data designed to return a value.
/// </summary>
/// <remarks>
/// Serves as a semantic indicator for tests verifying successful execution paths with return values.
/// </remarks>
public interface IReturns
: IExpected;

/// <summary>
/// Defines a contract for objects that provide a result value of a specified type.
/// </summary>
/// <remarks>This interface is typically used to represent operations or expectations that yield a result. It
/// supports covariance, allowing assignment to compatible result types.</remarks>
/// <typeparam name="TResult">The type of the result value returned by the implementation. Must not be null.</typeparam>
public interface IReturns<out TResult>
: IExpected<TResult>,
IReturns
where TResult : notnull;