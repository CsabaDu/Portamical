// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;

namespace Portamical.Core.TestDataTypes.Models.General;

/// <summary>
/// Provides an abstract base class for representing test data with a definition and an associated result.
/// </summary>
/// <remarks>This class is intended to be inherited by types that encapsulate test data scenarios, supplying both
/// the test definition and its expected result. It extends TestDataBase and enforces a consistent contract for
/// retrieving result values and argument representations.</remarks>
public abstract class TestData
: TestDataBase
{
    private protected TestData(
        string definition,
        string result)
    : base(definition)
    {
        _result = result;
    }

    private readonly string _result;
    private const string ResultString = "result";

    /// <summary>
    /// Returns the result string, or a fallback value if the result is null or empty.
    /// </summary>
    /// <returns>A string containing the result. If the result is null or empty, a fallback value is returned instead.</returns>
    public override sealed string GetResult()
    => ResultString.FallbackIfNullOrEmpty(_result);

    /// <summary>
    /// Returns an array of argument values based on the specified argument and property codes.
    /// </summary>
    /// <param name="argsCode">A value that specifies which arguments to include in the returned array.</param>
    /// <param name="propsCode">A value that specifies which properties to include in the returned array.</param>
    /// <returns>An array of objects containing the argument values corresponding to the specified codes. The array may be empty
    /// if no arguments match the criteria.</returns>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode != PropsCode.All);
}

//#region Concrete types
///// <summary>
///// Represents a general purpose strongly typed test data.
///// </summary>
///// <typeparam name="T1">The type of the first test iargument.</typeparam>
///// <param name="arg1">The first test argument value.</param>
//public class TestData<T1>
//: TestData
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1)
//    : base(definition, result)
//    {
//        Arg1 = arg1;
//    }

//    /// <summary>
//    /// Gets the first test argument value.
//    /// </summary>
//    public T1? Arg1 { get; init; }

//    /// <inheritdoc/>
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg1);
//}

///// <summary>
///// <inheritdoc cref="TestData{T1}"/>
///// <typeparam name="T2">Type of the second test argument.</typeparam>
///// <param name="arg2">Second test argument value.</param>

//public class TestData<T1, T2>
//: TestData<T1>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2)
//    : base(definition, result, arg1)
//    {
//        Arg2 =  arg2;
//    }

//    /// <summary>
//    /// Gets the second test argument value.
//    /// </summary>
//    public T2? Arg2 { get; init; }

//    /// <inheritdoc/>
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg2);
//}

///// <inheritdoc cref="TestData{T1, T2}"/>
///// <typeparam name="T3">Type of the third test argument.</typeparam>
///// <param name="arg3">Third test argument value.</param>
//public class TestData<T1, T2, T3>
//: TestData<T1, T2>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3)
//    : base(definition, result, arg1, arg2)
//    {
//        Arg3 =  arg3;
//    }

//    /// <summary>
//    /// Gets the third test argument value.
//    /// </summary>
//    public T3? Arg3 { get; init; }

//    /// <inheritdoc/>
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg3);
//}

///// <inheritdoc cref="TestData{T1, T2, T3}" />
///// <typeparam name="T4">The type of the fourth argument.</typeparam>
///// <param name="arg4">The fourth test argument value.</param>
//public class TestData<T1, T2, T3, T4>
//: TestData<T1, T2, T3>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4)
//    : base(definition, result, arg1, arg2, arg3)
//    {
//        Arg4 = arg4;
//    }

//    /// <summary>
//    /// Gets the fourth test argument value.
//    /// </summary>
//    public T4? Arg4 { get; init; }

//    /// <inheritdoc />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg4);
//}

///// <inheritdoc cref="TestData{T1, T2, T3, T4}" />
///// <typeparam name="T5">The type of the fifth argument.</typeparam>
///// <param name="arg5">The fifth test argument value.</param>
//public class TestData<T1, T2, T3, T4, T5>
//: TestData<T1, T2, T3, T4>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5)
//    : base(definition, result, arg1, arg2, arg3, arg4)
//    {
//        Arg5 = arg5;
//    }

//    /// <summary>
//    /// Gets the fifth test argument value.
//    /// </summary>
//    public T5? Arg5 { get; init; }

//    /// <inheritdoc />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg5);
//}

///// <inheritdoc cref="TestData{T1, T2, T3, T4, T5}" />
///// <typeparam name="T6">The type of the sixth argument.</typeparam>
///// <param name="arg6">The sixth test argument value.</param>
//public class TestData<T1, T2, T3, T4, T5, T6>
//: TestData<T1, T2, T3, T4, T5>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6)
//    : base(definition, result, arg1, arg2, arg3, arg4, arg5)
//    {
//        Arg6 = arg6;
//    }

//    /// <summary>
//    /// Gets the sixth test argument value.
//    /// </summary>
//    public T6? Arg6 { get; init; }

//    /// <inheritdoc />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg6);
//}

//// <inheritdoc cref="TestData{T1, T2, T3, T4, T5, T6}" />
///// <typeparam name="T7">The type of the seventh argument.</typeparam>
///// <param name="arg7">The seventh test argument value.</param>
//public class TestData<T1, T2, T3, T4, T5, T6, T7>
//: TestData<T1, T2, T3, T4, T5, T6>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7)
//    : base(definition, result, arg1, arg2, arg3, arg4, arg5, arg6)
//    {
//        Arg7 = arg7; 
//    }

//    /// <summary>
//    /// Gets the seventh test argument value.
//    /// </summary>
//    public T7? Arg7 { get; init; }

//    /// <inheritdoc cref="TestData.ToArgs(argsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg7);
//}

///// <inheritdoc cref="TestData{T1, T2, T3, T4, T5, T6, T7}" />
///// <typeparam name="T8">The type of the eighth argument.</typeparam>
///// <param name="arg8">The eighth test argument value.</param>
//public class TestData<T1, T2, T3, T4, T5, T6, T7, T8>
//: TestData<T1, T2, T3, T4, T5, T6, T7>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8)
//    : base(definition, result, arg1, arg2, arg3, arg4, arg5, arg6, arg7)
//    {
//        Arg8 = arg8;
//    }

//    /// <summary>
//    /// Gets the eighth test argument value.
//    /// </summary>
//    public T8? Arg8 { get; init; }

//    /// <inheritdoc cref="TestData.ToArgs(argsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg8);
//}

///// <inheritdoc cref="TestData{T1, T2, T3, T4, T5, T6, T7, T8}" />
///// <typeparam name="T9">The type of the ninth argument.</typeparam>
///// <param name="arg9">The ninth test argument value.</param>
//public class TestData<T1, T2, T3, T4, T5, T6, T7, T8, T9>
//: TestData<T1, T2, T3, T4, T5, T6, T7, T8>
//{
//    internal TestData(
//        string definition,
//        string result,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9)
//    : base(definition, result, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
//    {
//        Arg9 = arg9;
//    }

//    /// <summary>
//    /// Gets the ninth test argument value.
//    /// </summary>
//    public T9? Arg9 { get; init; }

//    /// <inheritdoc cref="TestData.ToArgs(argsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg9);
//}
//#endregion
