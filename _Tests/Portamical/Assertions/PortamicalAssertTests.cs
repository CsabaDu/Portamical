// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Assertions;

namespace Tests.Portamical.Assertions;

[TestClass]
public class PortamicalAssertTests
{
    private sealed class ConcreteAssert : PortamicalAssert
    {
        public static string? ExposedGetTypeFullName(object? obj) => GetTypeFullName(obj);
        public static InvalidOperationException ExposedGetAssertionFailedException(string msg)
            => GetAssertionFailedException(msg);
        public static string ExposedGetNotExpectedTypeExceptionThrownMessage(Type expectedType, Type? actualType)
            => GetNotExpectedTypeExceptionThrownMessage(expectedType, actualType);
        public static string ExposedGetNotExpectedValueMessage(object expected, object? actual)
            => GetNotExpectedValueMessage(expected, actual);
        public static string ExposedGetFullName(Type? obj)
            => GetFullName(obj);
    }

    #region CatchException

    [TestMethod]
    public void CatchException_nullAttempt_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.CatchException(null!));

    [TestMethod]
    public void CatchException_actionDoesNotThrow_returnsNull()
    {
        var result = PortamicalAssert.CatchException(() => { });
        Assert.IsNull(result);
    }

    [TestMethod]
    public void CatchException_actionThrows_returnsException()
    {
        var thrown = new InvalidOperationException("test");
        var result = PortamicalAssert.CatchException(() => throw thrown);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public void CatchException_actionThrowsArgumentException_preservesType()
    {
#pragma warning disable S3928
        var thrown = new ArgumentException("bad arg", "param");
#pragma warning restore S3928
        var result = PortamicalAssert.CatchException(() => throw thrown);
        Assert.IsInstanceOfType<ArgumentException>(result);
        Assert.AreSame(thrown, result);
    }

    #endregion

    #region DoesNotThrow

    [TestMethod]
    public void DoesNotThrow_nullAttempt_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.DoesNotThrow(null!, _ => { }));

    [TestMethod]
    public void DoesNotThrow_nullAssertFail_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.DoesNotThrow(() => { }, null!));

    [TestMethod]
    public void DoesNotThrow_noException_doesNotCallAssertFail()
    {
        bool called = false;
        PortamicalAssert.DoesNotThrow(() => { }, _ => called = true);
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void DoesNotThrow_exceptionThrown_callsAssertFail()
    {
        bool called = false;
        PortamicalAssert.DoesNotThrow(
            () => throw new InvalidOperationException("oops"),
            _ => called = true);
        Assert.IsTrue(called);
    }

    [TestMethod]
    public void DoesNotThrow_exceptionThrown_messageContainsTypeName()
    {
        string? message = null;
        PortamicalAssert.DoesNotThrow(
            () => throw new InvalidOperationException("oops"),
            msg => message = msg);
        Assert.IsNotNull(message);
#pragma warning disable MSTEST0046
        StringAssert.Contains(message, typeof(InvalidOperationException).FullName!);
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void DoesNotThrow_exceptionThrown_messageContainsExceptionMessage()
    {
        string? message = null;
        PortamicalAssert.DoesNotThrow(
            () => throw new InvalidOperationException("specific error text"),
            msg => message = msg);
        Assert.IsNotNull(message);
#pragma warning disable MSTEST0046
        StringAssert.Contains(message, "specific error text");
#pragma warning restore MSTEST0046
    }

    #endregion

    #region DoesNotThrowAsync

    [TestMethod]
    public async Task DoesNotThrowAsync_noException_doesNotCallAssertFailAsync()
    {
        bool called = false;
        await PortamicalAssert.DoesNotThrowAsync(
            () => Task.CompletedTask,
            _ =>
            {
                called = true;
                return ValueTask.CompletedTask;
            });
        Assert.IsFalse(called);
    }

    [TestMethod]
    public async Task DoesNotThrowAsync_exceptionThrown_callsAssertFailAsyncWithMessage()
    {
        string? message = null;
        await PortamicalAssert.DoesNotThrowAsync(
            () => Task.FromException(new InvalidOperationException("async oops")),
            msg =>
            {
                message = msg;
                return ValueTask.CompletedTask;
            });
        Assert.IsNotNull(message);
#pragma warning disable MSTEST0046
        StringAssert.Contains(message, typeof(InvalidOperationException).FullName!);
        StringAssert.Contains(message, "async oops");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task DoesNotThrowAsync_nullAssertFailAsync_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.DoesNotThrowAsync(() => Task.CompletedTask, null!));

    #endregion

    #region IsTypeOf

    [TestMethod]
    public void IsTypeOf_nullExpected_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.IsTypeOf(null!, new object(), (_, _) => { }));

    [TestMethod]
    public void IsTypeOf_nullActual_passesNullTypeToAssertEquality()
    {
        Type? capturedActualType = null;
        bool equalityCalled = false;
        PortamicalAssert.IsTypeOf(
            typeof(string), 
            null, 
            (e, a) => { equalityCalled = true; capturedActualType = a; });
        Assert.IsTrue(equalityCalled);
        Assert.IsNull(capturedActualType);
    }

    [TestMethod]
    public void IsTypeOf_nullAssertEquality_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.IsTypeOf(typeof(string), "hello", null!));

    [TestMethod]
    public void IsTypeOf_callsAssertEqualityWithExpectedTypeAndActualGetType()
    {
        Type? capturedExpected = null;
        Type? capturedActual = null;
        PortamicalAssert.IsTypeOf(
            typeof(string),
            "hello",
            (e, a) => { capturedExpected = e; capturedActual = a; });
        Assert.AreEqual(typeof(string), capturedExpected);
        Assert.AreEqual(typeof(string), capturedActual);
    }

    [TestMethod]
    public void IsTypeOf_passesActualRuntimeType_notDeclaredType()
    {
        Type? capturedActual = null;
        object obj = new ArgumentException("test"); // declared as object
        PortamicalAssert.IsTypeOf(
            typeof(ArgumentException),
            obj,
            (_, a) => capturedActual = a);
        Assert.AreEqual(typeof(ArgumentException), capturedActual);
    }

    #endregion



    #region ThrowsDetails

    [TestMethod]
    public void ThrowsDetails_nullCatchException_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(
#pragma warning disable S3928
                () => throw new ArgumentException(),
                new ArgumentException(),
#pragma warning restore S3928
                null!,
                (_, _) => { },
                (_, _) => { },
                _ => { }));

    [TestMethod]
    public void ThrowsDetails_nullAssertIsType_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(
#pragma warning disable S3928
                () => throw new ArgumentException(),
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                null!,
                (_, _) => { },
                _ => { }));

    [TestMethod]
    public void ThrowsDetails_nullAssertEquality_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(
#pragma warning disable S3928
                () => throw new ArgumentException(),
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                null!,
                _ => { }));

    [TestMethod]
    public void ThrowsDetails_nullAssertFail_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(
#pragma warning disable S3928
                () => throw new ArgumentException(),
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                null!));

    [TestMethod]
    public void ThrowsDetails_noExceptionThrown_callsAssertFail()
    {
        bool failCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { },
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => failCalled = true));
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void ThrowsDetails_noExceptionThrown_messageContainsNotThrown()
    {
        string? message = null;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { },
                new ArgumentException("expected"),
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => message = msg));
        Assert.IsNotNull(message);
#pragma warning disable MSTEST0046
        StringAssert.Contains(message, "was not thrown");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_callsAssertFail()
    {
        bool failCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => failCalled = true));
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_messageContainsTypeInfo()
    {
        string? message = null;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => message = msg));
        Assert.IsNotNull(message);
#pragma warning disable MSTEST0046
        StringAssert.Contains(message, nameof(ArgumentException));
        StringAssert.Contains(message, nameof(InvalidOperationException));
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void ThrowsDetails_correctException_callsAssertIsType()
    {
        bool isTypeCalled = false;
        Type? capturedType = null;
        Exception? capturedException = null;
#pragma warning disable S3928
        var thrown = new ArgumentException("test msg", "p");
        var expected = new ArgumentException("test msg", "p");
#pragma warning restore S3928

#pragma warning disable S1481
        var result = PortamicalAssert.ThrowsDetails(
#pragma warning restore S1481
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (t, e) => { isTypeCalled = true; capturedType = t; capturedException = e; },
            (_, _) => { },
            msg => throw new InvalidOperationException(msg));

        Assert.IsTrue(isTypeCalled);
        Assert.AreEqual(typeof(ArgumentException), capturedType);
        Assert.AreSame(thrown, capturedException);
    }

    [TestMethod]
    public void ThrowsDetails_correctException_callsAssertEquality()
    {
        bool equalityCalled = false;
#pragma warning disable S3928
        var thrown = new ArgumentException("test msg", "p");
        var expected = new ArgumentException("test msg", "p");
#pragma warning restore S3928

#pragma warning disable S1481
        var result = PortamicalAssert.ThrowsDetails(
#pragma warning restore S1481
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => equalityCalled = true,
            msg => throw new InvalidOperationException(msg));

        Assert.IsTrue(equalityCalled);
    }

    [TestMethod]
    public void ThrowsDetails_correctException_returnsTypedActual()
    {
#pragma warning disable S3928
        var thrown = new ArgumentException("test msg", "p");
        var expected = new ArgumentException("test msg", "p");
#pragma warning restore S3928
        var result = PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (t, e) => { },
            (e, a) => { },
            msg => throw new InvalidOperationException(msg));
        Assert.AreSame(thrown, result);
    }

    #endregion

    #region ThrowsDetailsAsync

    [TestMethod]
    public async Task ThrowsDetailsAsync_correctException_returnsTypedActual()
    {
#pragma warning disable S3928
        var thrown = new ArgumentException("async test msg", "p");
        var expected = new ArgumentException("async test msg", "p");
#pragma warning restore S3928
        var result = await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_argumentException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
#pragma warning disable S3928
        var thrown = new ArgumentException("async message", "paramName");
        var expected = new ArgumentException("async message", "paramName");
#pragma warning restore S3928

        await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            },
            _ => ValueTask.CompletedTask);

        var callsText = string.Join(Environment.NewLine, calls);
#pragma warning disable MSTEST0046
        StringAssert.Contains(callsText, "async message");
        StringAssert.Contains(callsText, "paramName");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_objectDisposedException_currently_assertsMessage()
    {
        var calls = new List<string>();
        var thrown = new ObjectDisposedException("resource");
        var expected = new ObjectDisposedException("resource");

        await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            },
            _ => ValueTask.CompletedTask);

        Assert.HasCount(1, calls);
#pragma warning disable MSTEST0046
        StringAssert.Contains(calls[0], "Cannot access a disposed object.");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_noException_assertFailDoesNotThrow_throwsFallbackException()
    {
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => { },
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "Assertion failed");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_wrongType_assertFailDoesNotThrow_throwsFallbackException()
    {
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new InvalidOperationException(),
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "Assertion failed");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_correctException_returnsTypedActual()
    {
#pragma warning disable S3928
        var thrown = new ArgumentException("async func task msg", "asyncParam");
        var expected = new ArgumentException("async func task msg", "asyncParam");
#pragma warning restore S3928
        var result = await PortamicalAssert.ThrowsDetailsAsync(
            async () =>
            {
                await Task.Delay(1);
                throw thrown;
            },
            expected,
            PortamicalAssert.CatchException,
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_argumentException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
#pragma warning disable S3928
        var thrown = new ArgumentException("async func message", "asyncParamName");
        var expected = new ArgumentException("async func message", "asyncParamName");
#pragma warning restore S3928

        await PortamicalAssert.ThrowsDetailsAsync(
            async () =>
            {
                await Task.Delay(1);
                throw thrown;
            },
            expected,
            PortamicalAssert.CatchException,
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            },
            _ => ValueTask.CompletedTask);

        var callsText = string.Join(Environment.NewLine, calls);
#pragma warning disable MSTEST0046
        StringAssert.Contains(callsText, "async func message");
        StringAssert.Contains(callsText, "asyncParamName");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_noException_assertFailDoesNotThrow_throwsFallbackException()
    {
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                async () => await Task.Delay(1),
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "Assertion failed");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_wrongType_assertFailDoesNotThrow_throwsFallbackException()
    {
        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                async () =>
                {
                    await Task.Delay(1);
                    throw new InvalidOperationException();
                },
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "Assertion failed");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_nullAttempt_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                (Func<Task>)null!,
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_trulyAsyncException_capturesCorrectly()
    {
#pragma warning disable S3928
        var thrown = new InvalidOperationException("truly async error");
        var expected = new InvalidOperationException("truly async error");
#pragma warning restore S3928
        var result = await PortamicalAssert.ThrowsDetailsAsync(
            async () =>
            {
                await Task.Yield(); // Ensure true async behavior
                await Task.Delay(5);
                throw thrown;
            },
            expected,
            PortamicalAssert.CatchException,
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask);
        Assert.AreSame(thrown, result);
    }

    #endregion

    #region Equality

    [TestMethod]
    public void Equality_generic_nullEquals_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.Equality(1, 1, null!, _ => { }, "msg"));

    [TestMethod]
    public void Equality_generic_nullAssertFail_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.Equality(1, 1, (a, b) => a == b, null!, "msg"));

    [TestMethod]
    public void Equality_generic_equalValues_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(42, 42, (a, b) => a == b, _ => failCalled = true, "msg");
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_generic_unequalValues_callsAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(42, 99, (a, b) => a == b, _ => failCalled = true, "msg");
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_generic_unequalValues_passesMessage()
    {
        string? capturedMessage = null;
        PortamicalAssert.Equality(42, 99, (a, b) => a == b, msg => capturedMessage = msg, "custom message");
        Assert.AreEqual("custom message", capturedMessage);
    }

    [TestMethod]
    public void Equality_object_nullAssertFail_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.Equality(1, 1, null!));

    [TestMethod]
    public void Equality_object_equalIntegers_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(42, 42, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_unequalIntegers_callsAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(42, 99, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalStrings_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality("hello", "hello", () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalDoublesWithinTolerance_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0, 1.0 + 1e-11, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_unequalDoublesOutsideTolerance_callsAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0, 2.0, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    #endregion

    #region EqualityAsync

    [TestMethod]
    public async Task EqualityAsync_generic_equalValues_doesNotCallAssertFailAsync()
    {
        bool failCalled = false;
        await PortamicalAssert.EqualityAsync(
            42,
            42,
            (a, b) => a == b,
            _ =>
            {
                failCalled = true;
                return ValueTask.CompletedTask;
            },
            "msg");
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public async Task EqualityAsync_generic_unequalValues_callsAssertFailAsyncWithMessage()
    {
        string? message = null;
        await PortamicalAssert.EqualityAsync(
            42,
            99,
            (a, b) => a == b,
            msg =>
            {
                message = msg;
                return ValueTask.CompletedTask;
            },
            "async message");
        Assert.AreEqual("async message", message);
    }

    [TestMethod]
    public async Task EqualityAsync_object_unequalValues_callsAssertFailAsync()
    {
        bool failCalled = false;
        await PortamicalAssert.EqualityAsync(
            42,
            99,
            () =>
            {
                failCalled = true;
                return ValueTask.CompletedTask;
            });
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public async Task EqualityAsync_object_equalNestedCollections_doesNotCallAssertFailAsync()
    {
        bool failCalled = false;
        object?[] expected = [new[] { 1, 2 }, new[] { 3, 4 }];
        object?[] actual = [new[] { 1, 2 }, new[] { 3, 4 }];
        await PortamicalAssert.EqualityAsync(
            expected,
            actual,
            () =>
            {
                failCalled = true;
                return ValueTask.CompletedTask;
            });
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public async Task EqualityAsync_object_nullAssertFailAsync_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.EqualityAsync(1, 1, null!));

    #endregion

    #region Protected helpers

    [TestMethod]
    public void GetTypeFullName_nonNull_returnsFullName()
    {
#pragma warning disable S3928
        var result = ConcreteAssert.ExposedGetTypeFullName(new ArgumentException());
#pragma warning restore S3928
        Assert.AreEqual(typeof(ArgumentException).FullName, result);
    }

    [TestMethod]
    public void GetTypeFullName_null_returnsNullString()
    {
        var result = ConcreteAssert.ExposedGetTypeFullName(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void GetAssertionFailedException_returnsInvalidOperationExceptionWithMessage()
    {
        var ex = ConcreteAssert.ExposedGetAssertionFailedException("something went wrong");
        Assert.IsInstanceOfType<InvalidOperationException>(ex);
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "something went wrong");
#pragma warning restore MSTEST0046
    }

    #endregion

    #region Equality - Additional Type Coverage

    [TestMethod]
    public void Equality_object_equalGuids_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var guid = Guid.NewGuid();
        PortamicalAssert.Equality(guid, guid, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalDateTimes_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var dt = DateTime.Now;
        PortamicalAssert.Equality(dt, dt, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalBooleans_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(true, true, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalDecimals_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(123.45m, 123.45m, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalFloats_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0f, 1.0f, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalFloatsWithinTolerance_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0f, 1.0f + 1e-7f, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_unequalFloatsOutsideTolerance_callsAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0f, 2.0f, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_customTolerance_usesSpecifiedValue()
    {
        bool failCalled = false;
        // With tolerance 0.1, these should be equal
        PortamicalAssert.Equality(1.0, 1.05, () => failCalled = true, floatingPointTolerance: 0.1);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_customTolerance_failsOutsideTolerance()
    {
        bool failCalled = false;
        // With tolerance 0.01, these should NOT be equal
        PortamicalAssert.Equality(1.0, 1.05, () => failCalled = true, floatingPointTolerance: 0.01);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalByteArrays_doesNotCallAssertFail()
    {
        bool failCalled = false;
        byte[] arr1 = [1, 2, 3];
        byte[] arr2 = [1, 2, 3];
        PortamicalAssert.Equality(arr1, arr2, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_unequalByteArrays_callsAssertFail()
    {
        bool failCalled = false;
        byte[] arr1 = [1, 2, 3];
        byte[] arr2 = [1, 2, 4];
        PortamicalAssert.Equality(arr1, arr2, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_nullBothSides_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(null!, null, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_nullExpected_callsAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(null!, 42, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    #endregion

    #region ThrowsDetails - ArgumentException Metadata

    [TestMethod]
    public void ThrowsDetails_argumentExceptionWithParamName_assertsParamName()
    {
        var calls = new List<string>();
#pragma warning disable S3928
        var thrown = new ArgumentException("test message", "paramName");
        var expected = new ArgumentException("test message", "paramName");
#pragma warning restore S3928

#pragma warning disable S1481
        var result = PortamicalAssert.ThrowsDetails(
#pragma warning restore S1481
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        // Should assert both message and paramName
        Assert.IsGreaterThanOrEqualTo(calls.Count, 2);
        var callsText = string.Join(Environment.NewLine, calls);
#pragma warning disable MSTEST0046
        StringAssert.Contains(callsText, "test message");
        StringAssert.Contains(callsText, "paramName");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void ThrowsDetails_argumentExceptionWithoutParamName_assertsMessageOnly()
    {
        var calls = new List<string>();
        var thrown = new ArgumentException("test message");
        var expected = new ArgumentException("test message");

#pragma warning disable S1481
        var result = PortamicalAssert.ThrowsDetails(
#pragma warning restore S1481
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        // Should assert message only (paramName is null)
        Assert.IsGreaterThanOrEqualTo(calls.Count, 1);
        var callsText = string.Join(Environment.NewLine, calls);
#pragma warning disable MSTEST0046
        StringAssert.Contains(callsText, "test message");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void ThrowsDetails_argumentExceptionWithGuardMessage_skipsMessageAssertion()
    {
        var calls = new List<string>();
        var thrown = new ArgumentException("The value cannot be an empty string");
        var expected = new ArgumentException("The value cannot be an empty string");

#pragma warning disable S1481
        var result = PortamicalAssert.ThrowsDetails(
#pragma warning restore S1481
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        // Guard message should skip message assertion
        var callsText = string.Join(Environment.NewLine, calls);
        Assert.AreEqual(
            -1,
            callsText.IndexOf("The value cannot be an empty string", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThrowsDetails_invalidOperationException_assertsMessage()
    {
        var calls = new List<string>();
        var thrown = new InvalidOperationException("operation failed");
        var expected = new InvalidOperationException("operation failed");

#pragma warning disable S1481
        var result = PortamicalAssert.ThrowsDetails(
#pragma warning restore S1481
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        var callsText = string.Join(Environment.NewLine, calls);
#pragma warning disable MSTEST0046
        StringAssert.Contains(callsText, "operation failed");
#pragma warning restore MSTEST0046
    }

    #endregion

    #region ThrowsDetails - Exception Type Validation

    [TestMethod]
    public void ThrowsDetails_derivedException_failsOnExactTypeMatch()
    {
        bool failCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
#pragma warning disable S3928
                () => throw new ArgumentNullException("param"),
                new ArgumentException("expected"),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => failCalled = true));
        Assert.IsTrue(failCalled);
    }

    #endregion

    #region Equality - Special Floating Point Values

    [TestMethod]
    public void Equality_object_doubleNaN_treatsAllNaNAsEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(double.NaN, double.NaN, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatNaN_treatsAllNaNAsEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.NaN, float.NaN, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_doublePositiveInfinity_equal()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(double.PositiveInfinity, double.PositiveInfinity, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_doubleNegativeInfinity_equal()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(double.NegativeInfinity, double.NegativeInfinity, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_doublePositiveAndNegativeInfinity_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(double.PositiveInfinity, double.NegativeInfinity, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatPositiveInfinity_equal()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.PositiveInfinity, float.PositiveInfinity, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_doubleZeroPositiveAndNegative_equal()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(0.0, -0.0, () => failCalled = true);
        Assert.IsFalse(failCalled); // Both are bitwise equal in IEEE 754 representation
    }

    [TestMethod]
    public void Equality_object_doubleNaNAndRegular_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(double.NaN, 1.0, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    #endregion

    #region Equality - More Type Coverage

    [TestMethod]
    public void Equality_object_equalTimeSpans_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var ts = TimeSpan.FromHours(1);
        PortamicalAssert.Equality(ts, ts, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalDateOnly_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var date = new DateOnly(2024, 1, 1);
        PortamicalAssert.Equality(date, date, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalTimeOnly_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var time = new TimeOnly(12, 30, 45);
        PortamicalAssert.Equality(time, time, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalDateTimeOffset_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var dto = DateTimeOffset.Now;
        PortamicalAssert.Equality(dto, dto, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalChars_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality('A', 'A', () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalBytes_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality((byte)255, (byte)255, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalSBytes_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality((sbyte)-128, (sbyte)-128, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalShorts_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality((short)12345, (short)12345, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalUShorts_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality((ushort)65535, (ushort)65535, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalUInts_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(4294967295u, 4294967295u, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalLongs_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(9223372036854775807L, 9223372036854775807L, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalULongs_doesNotCallAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(18446744073709551615UL, 18446744073709551615UL, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalIntPtrs_doesNotCallAssertFail()
    {
        bool failCalled = false;
        nint value = 12345;
        PortamicalAssert.Equality(value, value, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_equalUIntPtrs_doesNotCallAssertFail()
    {
        bool failCalled = false;
        nuint value = 12345;
        PortamicalAssert.Equality(value, value, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    #endregion

    #region Protected helpers - Additional Coverage

    [TestMethod]
    public void GetFullName_nullType_returnsNullString()
    {
        var result = ConcreteAssert.ExposedGetFullName(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void GetFullName_nonNullType_returnsFullName()
    {
        var result = ConcreteAssert.ExposedGetFullName(typeof(ArgumentException));
        Assert.AreEqual(typeof(ArgumentException).FullName, result);
    }

    [TestMethod]
    public void GetNotExpectedTypeExceptionThrownMessage_createsCorrectMessage()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedTypeExceptionThrownMessage(
            typeof(ArgumentException), 
            typeof(InvalidOperationException));
#pragma warning disable MSTEST0046
        StringAssert.Contains(result, nameof(ArgumentException));
        StringAssert.Contains(result, nameof(InvalidOperationException));
        StringAssert.Contains(result, "Expected exception");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void GetNotExpectedTypeExceptionThrownMessage_nullActualType_includesNull()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedTypeExceptionThrownMessage(
            typeof(ArgumentException), 
            null);
#pragma warning disable MSTEST0046
        StringAssert.Contains(result, nameof(ArgumentException));
        StringAssert.Contains(result, "null");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void GetNotExpectedValueMessage_formatsCorrectly()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedValueMessage(42, 99);
#pragma warning disable MSTEST0046
        StringAssert.Contains(result, "42");
        StringAssert.Contains(result, "99");
        StringAssert.Contains(result, "Expected");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void GetNotExpectedValueMessage_nullActual_includesNullString()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedValueMessage(42, null);
#pragma warning disable MSTEST0046
        StringAssert.Contains(result, "42");
        StringAssert.Contains(result, "null");
#pragma warning restore MSTEST0046
    }

    #endregion

    #region Fallback Exception Behavior

    [TestMethod]
    public void ThrowsDetails_assertFailDoesNotThrow_throwsFallbackException()
    {
        // When assertFail doesn't throw, a fallback InvalidOperationException should be thrown
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { }, // No exception thrown
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => { } // assertFail that doesn't throw
            ));
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "Assertion failed");
#pragma warning restore MSTEST0046
    }

    [TestMethod]
    public void ThrowsDetails_wrongType_assertFailDoesNotThrow_throwsFallbackException()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),
#pragma warning disable S3928
                new ArgumentException(),
#pragma warning restore S3928
                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => { } // assertFail that doesn't throw
            ));
#pragma warning disable MSTEST0046
        StringAssert.Contains(ex.Message, "Assertion failed");
#pragma warning restore MSTEST0046
    }

    #endregion

    #region CatchException - Edge Cases

    [TestMethod]
    public void CatchException_nestedExceptions_returnsOuterException()
    {
        var inner = new InvalidOperationException("inner");
        var outer = new ArgumentException("outer", inner);
        var result = PortamicalAssert.CatchException(() => throw outer);
        Assert.AreSame(outer, result);
        Assert.AreSame(inner, result?.InnerException);
    }

    #endregion

    #region IsTypeOfAsync

    [TestMethod]
    public async Task IsTypeOfAsync_passesExpectedAndActualRuntimeType()
    {
        Type? capturedExpected = null;
        Type? capturedActual = null;
        object actual = new ArgumentException("runtime");

        await PortamicalAssert.IsTypeOfAsync(
            typeof(ArgumentException),
            actual,
            (expected, type) =>
            {
                capturedExpected = expected;
                capturedActual = type;
                return ValueTask.CompletedTask;
            });

        Assert.AreEqual(typeof(ArgumentException), capturedExpected);
        Assert.AreEqual(typeof(ArgumentException), capturedActual);
    }

    [TestMethod]
    public async Task IsTypeOfAsync_nullAssertEqualityAsync_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.IsTypeOfAsync(typeof(string), "hello", null!));

    #endregion
}
