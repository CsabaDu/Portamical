// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Patterns;

namespace Portamical.Core.TestDataTypes.Models.Specialized;

public abstract class TestDataReturns<TStruct>
: TestDataExpected<TStruct>,
IReturns<TStruct>
where TStruct : struct
{
    private protected TestDataReturns(
        string definition,
        TStruct expected)
    : base(definition, expected)
    {
    }

    private const string ReturnsString = "returns";

    /// <inheritdoc/>
    public override sealed string GetResultPrefix()
    => GetResultPrefix(ReturnsString);

    /// <summary>
    /// Gets the TrimTestCaseName return value as an object.
    /// </summary>
    /// <returns>The string representation of the boxed 'TrimTestCaseName' value.</returns>
    public override sealed string GetResult()
    => GetExpectedResult(Expected.ToString());

    /// <inheritdoc/>
    public override sealed object?[] ToArgs(
        ArgsCode argsCode,
        PropsCode propsCode)
    => Trim(base.ToArgs, argsCode, propsCode,
        propsCode == PropsCode.TrimReturnsExpected);
}

//#region Concrete types
///// <summary>
///// Test data implementation for return-value tests with 1 additional argument.
///// </summary>
///// <inheritdoc cref="TestDataReturns{TStruct}"/>
///// <typeparam name="T1">Type of the first test argument.</typeparam>
///// <param name="arg1">First test argument value.</param>
//public class TestDataReturns<TStruct, T1>
//: TestDataReturns<TStruct>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1)
//    : base(definition, expected)
//    {
//        Arg1 = arg1;
//    }

//    public T1? Arg1 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg1);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1}" />
///// <typeparam name="T2">The type of the second argument.</typeparam>
///// <param name="arg2">The second argument.</param>
//public class TestDataReturns<TStruct, T1, T2>
//: TestDataReturns<TStruct, T1>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2)
//    : base(definition, expected, arg1)
//    {
//        Arg2 = arg2;
//    }

//    public T2? Arg2 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg2);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2}" />
///// <typeparam name="T3">The type of the third argument.</typeparam>
///// <param name="arg3">The third argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3>
//: TestDataReturns<TStruct, T1, T2>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3)
//    : base(definition, expected, arg1, arg2)
//    {
//        Arg3 = arg3;
//    }

//    public T3? Arg3 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg3);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2, T3}" />
///// <typeparam name="T4">The type of the fourth argument.</typeparam>
///// <param name="arg4">The fourth argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3, T4>
//: TestDataReturns<TStruct, T1, T2, T3>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4)
//    : base(definition, expected, arg1, arg2, arg3)
//    {
//        Arg4 = arg4;
//    }

//    public T4? Arg4 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg4);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2, T3, T4}" />
///// <typeparam name="T5">The type of the fifth argument.</typeparam>
///// <param name="arg5">The fifth argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3, T4, T5>
//: TestDataReturns<TStruct, T1, T2, T3, T4>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5)
//    : base(definition, expected, arg1, arg2, arg3, arg4)
//    {
//        Arg5 = arg5;
//    }

//    public T5? Arg5 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg5);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2, T3, T4, T5}" />
///// <typeparam name="T6">The type of the sixth argument.</typeparam>
///// <param name="arg6">The sixth argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6>
//: TestDataReturns<TStruct, T1, T2, T3, T4, T5>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6)
//    : base(definition, expected, arg1, arg2, arg3, arg4, arg5)
//    {
//        Arg6 = arg6;
//    }

//    public T6? Arg6 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg6);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2, T3, T4, T5, T6}" />
///// <typeparam name="T7">The type of the seventh argument.</typeparam>
///// <param name="arg7">The seventh argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6, T7>
//: TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7)
//    : base(definition, expected, arg1, arg2, arg3, arg4, arg5, arg6)
//    {
//        Arg7 = arg7;
//    }

//    public T7? Arg7 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg7);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2, T3, T4, T5, T6, T7}" />
///// <typeparam name="T8">The type of the eighth argument.</typeparam>
///// <param name="arg8">The eighth argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6, T7, T8>
//: TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6, T7>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8)
//    : base(definition, expected, arg1, arg2, arg3, arg4, arg5, arg6, arg7)
//    {
//        Arg8 = arg8;
//    }

//    public T8? Arg8 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg8);
//}

///// <inheritdoc cref="TestDataReturns{TStruct, T1, T2, T3, T4, T5, T6, T7, T8}" />
///// <typeparam name="T9">The type of the ninth argument.</typeparam>
///// <param name="arg9">The ninth argument.</param>
//public class TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6, T7, T8, T9>
//: TestDataReturns<TStruct, T1, T2, T3, T4, T5, T6, T7, T8>
//where TStruct : struct
//{
//    internal TestDataReturns(
//        string definition,
//        TStruct expected,
//        T1? arg1, T2? arg2, T3? arg3, T4? arg4, T5? arg5, T6? arg6, T7? arg7, T8? arg8, T9? arg9)
//    : base(definition, expected, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
//    {
//        Arg9 = arg9;
//    }

//    public T9? Arg9 { get; init; }

//    /// <inheritdoc cref="TestDataBase.ToObjectArray(ArgsCode)" />
//    protected override object?[] ToObjectArray(ArgsCode argsCode)
//    => Extend(base.ToObjectArray, argsCode, Arg9);
//}
//#endregion