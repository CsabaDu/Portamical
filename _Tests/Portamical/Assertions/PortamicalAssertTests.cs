// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Assertions;

namespace Tests.Portamical.Assertions;

[TestClass]
public class PortamicalAssertTests
{
    public TestContext TestContext { get; set; } = null!;

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

    private static void AssertContainsOrdinal(string value, string substring)
        => Assert.Contains(substring, value);

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

        var paramName = "param";
        var thrown = new ArgumentException("bad arg", paramName);

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
        AssertContainsOrdinal(message, typeof(InvalidOperationException).FullName!);
    }

    [TestMethod]
    public void DoesNotThrow_exceptionThrown_messageContainsExceptionMessage()
    {
        string? message = null;
        PortamicalAssert.DoesNotThrow(
            () => throw new InvalidOperationException("specific error text"),
            msg => message = msg);
        Assert.IsNotNull(message);
        AssertContainsOrdinal(message, "specific error text");
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
        AssertContainsOrdinal(message, typeof(InvalidOperationException).FullName!);
        AssertContainsOrdinal(message, "async oops");
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
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),

                null!,
                (_, _) => { },
                (_, _) => { },
                _ => { }));
    }

    [TestMethod]
    public void ThrowsDetails_nullAssertIsType_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                attempt => PortamicalAssert.CatchException(attempt),

                null!,
                (_, _) => { },
                _ => { }));
    }

    [TestMethod]
    public void ThrowsDetails_nullAssertEquality_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                attempt => PortamicalAssert.CatchException(attempt),

                (_, _) => { },
                null!,
                _ => { }));
    }

    [TestMethod]
    public void ThrowsDetails_nullAssertFail_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                attempt => PortamicalAssert.CatchException(attempt),

                (_, _) => { },
                (_, _) => { },
                null!));
    }

    [TestMethod]
    public void ThrowsDetails_noExceptionThrown_callsAssertFail()
    {
        string paramName = "param";

        bool failCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { },

                new ArgumentException("expected", paramName),

                attempt => PortamicalAssert.CatchException(attempt),
                (_, _) => { },
                (_, _) => { },
                msg => failCalled = true));
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void ThrowsDetails_noExceptionThrown_messageContainsNotThrown()
    {
        string paramName = "param";
        string? message = null;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { },

                new ArgumentException("expected", paramName),

                attempt => PortamicalAssert.CatchException(attempt),
                (_, _) => { },
                (_, _) => { },
                msg => message = msg));
        Assert.IsNotNull(message);
        AssertContainsOrdinal(message, "was not thrown");
    }

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_callsAssertIsType()
    {
        string paramName = "param";
        bool isTypeCalled = false;

        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                PortamicalAssert.CatchException,
                (expectedType, actual) =>
                {
                    isTypeCalled = true;

                    if (actual.GetType() != expectedType)
                    {
                        throw new InvalidOperationException();
                    }
                },
                (_, _) => { },
                _ => { }));
        Assert.IsTrue(isTypeCalled);
    }

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_messageContainsTypeInfo()
    {
        string paramName = "param";

        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                PortamicalAssert.CatchException,
                (expectedType, actual) => throw new InvalidOperationException(
                    $"Expected {expectedType.Name} but got {actual.GetType().Name}"),
                (_, _) => { },
                _ => { }));
        AssertContainsOrdinal(ex.Message, nameof(ArgumentException));
        AssertContainsOrdinal(ex.Message, nameof(InvalidOperationException));
    }

    [TestMethod]
    public void ThrowsDetails_correctException_callsAssertIsType()
    {
        bool isTypeCalled = false;
        Type? capturedType = null;
        object? capturedException = null;
        string testMessage = "test msg";
        string paramName = "p";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);

        PortamicalAssert.ThrowsDetails(
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
        string testMessage = "test msg";
        string paramName = "p";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);

        PortamicalAssert.ThrowsDetails(
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
        string testMessage = "test msg";
        string paramName = "p";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);
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
        string testMessage = "async test msg";
        string paramName = "p";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);
        var result = await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            catchExceptionAsync: PortamicalAssert.CatchExceptionAsync,
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_argumentException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
        string testMessage = "async message";
        string paramName = "paramName";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);

        await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            catchExceptionAsync: PortamicalAssert.CatchExceptionAsync,
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            },
            _ => ValueTask.CompletedTask);

        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "async message");
        AssertContainsOrdinal(callsText, "paramName");
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
            catchExceptionAsync: PortamicalAssert.CatchExceptionAsync,
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            },
            _ => ValueTask.CompletedTask);

        Assert.HasCount(1, calls);
        AssertContainsOrdinal(calls[0], "Cannot access a disposed object.");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_noException_assertFailDoesNotThrow_throwsFallbackException()
    {
        string paramName = "param";

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => { return Task.CompletedTask; },

                new ArgumentException("expected", paramName),

                async (attempt) => await PortamicalAssert.CatchExceptionAsync(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
        AssertContainsOrdinal(ex.Message, "Assertion failed");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_wrongType_assertIsTypeDoesNotThrow_throwsInvalidCastException()
    {
        string paramName = "param";
        await Assert.ThrowsExactlyAsync<InvalidCastException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                async (attempt) => await PortamicalAssert.CatchExceptionAsync(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_correctException_returnsTypedActual()
    {
        string testMessage = "async func task msg";
        string paramName = "asyncParam";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);
        var result = await PortamicalAssert.ThrowsDetailsAsync(
            async () =>
            {
                await Task.Delay(1, TestContext.CancellationToken);
                throw thrown;
            },
            expected,
            catchExceptionAsync: PortamicalAssert.CatchExceptionAsync,
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask,
            _ => ValueTask.CompletedTask);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_argumentException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
        string testMessage = "async func message";
        string paramName = "asyncParamName";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);

        await PortamicalAssert.ThrowsDetailsAsync(
            async () =>
            {
                await Task.Delay(1, TestContext.CancellationToken);
                throw thrown;
            },
            expected,
            catchExceptionAsync: PortamicalAssert.CatchExceptionAsync,
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            },
            _ => ValueTask.CompletedTask);

        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "async func message");
        AssertContainsOrdinal(callsText, "asyncParamName");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_noException_assertFailDoesNotThrow_throwsFallbackException()
    {
        string paramName = "param";

        var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                async () => await Task.Delay(1, TestContext.CancellationToken),

                new ArgumentException("expected", paramName),

                async (attempt) => await PortamicalAssert.CatchExceptionAsync(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
        AssertContainsOrdinal(ex.Message, "Assertion failed");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_wrongType_assertIsTypeDoesNotThrow_throwsInvalidCastException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<InvalidCastException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                async () =>
                {
                    await Task.Delay(1, TestContext.CancellationToken);
                    throw new InvalidOperationException();
                },

                new ArgumentException("expected", paramName),

                async (attempt) => await PortamicalAssert.CatchExceptionAsync(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_nullAttempt_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                null!,

                new ArgumentException("expected", paramName),

                async (attempt) => await PortamicalAssert.CatchExceptionAsync(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask,
                _ => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_trulyAsyncException_capturesCorrectly()
    {
        var thrown = new InvalidOperationException("truly async error");
        var expected = new InvalidOperationException("truly async error");
        var result = await PortamicalAssert.ThrowsDetailsAsync(
            async () =>
            {
                await Task.Yield(); // Ensure true async behavior
                await Task.Delay(5, TestContext.CancellationToken);
                throw thrown;
            },
            expected,
            catchExceptionAsync: PortamicalAssert.CatchExceptionAsync,
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
        string paramName = "param";

        var result = ConcreteAssert.ExposedGetTypeFullName(new ArgumentException("test", paramName));

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
        AssertContainsOrdinal(ex.Message, "something went wrong");
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
        string testMessage = "test message";
        string paramName = "paramName";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);

        PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        // Should assert both message and paramName
        Assert.IsGreaterThanOrEqualTo(calls.Count, 2);
        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "test message");
        AssertContainsOrdinal(callsText, "paramName");
    }

    [TestMethod]
    public void ThrowsDetails_argumentExceptionWithoutParamName_assertsMessageOnly()
    {
        var calls = new List<string>();
        var thrown = new ArgumentException("test message");
        var expected = new ArgumentException("test message");

        PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        // Should assert message only (paramName is null)
        Assert.IsGreaterThanOrEqualTo(calls.Count, 1);
        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "test message");
    }

    [TestMethod]
    public void ThrowsDetails_argumentExceptionWithGuardMessage_skipsMessageAssertion()
    {
        var calls = new List<string>();
        var thrown = new ArgumentException("The value cannot be an empty string");
        var expected = new ArgumentException("The value cannot be an empty string");

        PortamicalAssert.ThrowsDetails(
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

        PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            PortamicalAssert.CatchException,
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"),
            msg => throw new InvalidOperationException(msg));

        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "operation failed");
    }

    #endregion

    #region ThrowsDetails - Exception Type Validation

    [TestMethod]
    public void ThrowsDetails_derivedException_failsOnExactTypeMatch()
    {
        string paramName = "param";
        bool isTypeCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new ArgumentNullException(paramName),
                new ArgumentException("expected", paramName),
                PortamicalAssert.CatchException,
                (expectedType, actual) =>
                {
                    isTypeCalled = true;

                    if (actual.GetType() != expectedType)
                    {
                        throw new InvalidOperationException();
                    }
                },
                (_, _) => { },
                _ => { }));
        Assert.IsTrue(isTypeCalled);
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
        AssertContainsOrdinal(result, nameof(ArgumentException));
        AssertContainsOrdinal(result, nameof(InvalidOperationException));
        AssertContainsOrdinal(result, "Expected exception");
    }

    [TestMethod]
    public void GetNotExpectedTypeExceptionThrownMessage_nullActualType_includesNull()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedTypeExceptionThrownMessage(
            typeof(ArgumentException), 
            null);
        AssertContainsOrdinal(result, nameof(ArgumentException));
        AssertContainsOrdinal(result, "null");
    }

    [TestMethod]
    public void GetNotExpectedValueMessage_formatsCorrectly()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedValueMessage(42, 99);
        AssertContainsOrdinal(result, "42");
        AssertContainsOrdinal(result, "99");
        AssertContainsOrdinal(result, "Expected");
    }

    [TestMethod]
    public void GetNotExpectedValueMessage_nullActual_includesNullString()
    {
        var result = ConcreteAssert.ExposedGetNotExpectedValueMessage(42, null);
        AssertContainsOrdinal(result, "42");
        AssertContainsOrdinal(result, "null");
    }

    #endregion

    #region Fallback Exception Behavior

    [TestMethod]
    public void ThrowsDetails_assertFailDoesNotThrow_throwsFallbackException()
    {
        string paramName = "param";

        // When assertFail doesn't throw, a fallback InvalidOperationException should be thrown
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { }, // No exception thrown

                new ArgumentException("expected", paramName),

                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => { } // assertFail that doesn't throw
            ));
        AssertContainsOrdinal(ex.Message, "Assertion failed");
    }

    [TestMethod]
    public void ThrowsDetails_wrongType_assertIsTypeDoesNotThrow_throwsInvalidCastException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<InvalidCastException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                PortamicalAssert.CatchException,
                (_, _) => { },
                (_, _) => { },
                msg => { } // assertFail that doesn't throw
            ));
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
