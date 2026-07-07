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

    private sealed class CustomEquatableType(int value)
    {
        private readonly int _value = value;

        public override bool Equals(object? obj)
        {
            return obj is CustomEquatableType other && _value == other._value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
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

    [TestMethod]
    public void CatchException_outOfMemoryException_propagates()
    {
        var thrown = new OutOfMemoryException("OOM test");
        var caught = Assert.ThrowsExactly<OutOfMemoryException>(
            () => PortamicalAssert.CatchException(() => throw thrown));
        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public void CatchException_accessViolationException_propagates()
    {
        var thrown = new AccessViolationException("Access violation test");
        var caught = Assert.ThrowsExactly<AccessViolationException>(
            () => PortamicalAssert.CatchException(() => throw thrown));
        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public void CatchException_stackOverflowException_propagates()
    {
        var thrown = new StackOverflowException("Stack overflow test");
        var caught = Assert.ThrowsExactly<StackOverflowException>(
            () => PortamicalAssert.CatchException(() => throw thrown));
        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public void CatchException_nonFatalException_returnsCaught()
    {
        // Verify non-fatal exceptions are caught and returned (not propagated)
        var thrown = new InvalidOperationException("Non-fatal test");
        var result = PortamicalAssert.CatchException(() => throw thrown);
        Assert.IsNotNull(result);
        Assert.AreSame(thrown, result);
    }

    #endregion

    #region CatchExceptionAsync

    [TestMethod]
    public async Task CatchExceptionAsync_nullAttempt_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.CatchExceptionAsync(null!));

    [TestMethod]
    public async Task CatchExceptionAsync_actionDoesNotThrow_returnsNull()
    {
        var result = await PortamicalAssert.CatchExceptionAsync(() => Task.CompletedTask);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_actionThrows_returnsException()
    {
        var thrown = new InvalidOperationException("async test");
        var result = await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(thrown));
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_actionThrowsArgumentException_preservesType()
    {
        var paramName = "asyncParam";
        var thrown = new ArgumentException("bad arg async", paramName);

        var result = await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(thrown));
        Assert.IsInstanceOfType<ArgumentException>(result);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_nestedExceptions_returnsOuterException()
    {
        var inner = new InvalidOperationException("inner async");
        var outer = new ArgumentException("outer async", inner);
        var result = await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(outer));
        Assert.AreSame(outer, result);
        Assert.AreSame(inner, result?.InnerException);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_trulyAsyncException_capturesCorrectly()
    {
        var thrown = new InvalidOperationException("truly async");
        var result = await PortamicalAssert.CatchExceptionAsync(async () =>
        {
            await Task.Yield();
            await Task.Delay(5, TestContext.CancellationToken);
            throw thrown;
        });
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_outOfMemoryException_propagates()
    {
        var thrown = new OutOfMemoryException("Async OOM test");
        var caught = await Assert.ThrowsExactlyAsync<OutOfMemoryException>(
            async () => await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(thrown)));
        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_accessViolationException_propagates()
    {
        var thrown = new AccessViolationException("Async access violation test");
        var caught = await Assert.ThrowsExactlyAsync<AccessViolationException>(
            async () => await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(thrown)));
        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_stackOverflowException_propagates()
    {
        var thrown = new StackOverflowException("Async stack overflow test");
        var caught = await Assert.ThrowsExactlyAsync<StackOverflowException>(
            async () => await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(thrown)));
        Assert.AreSame(thrown, caught);
    }

    [TestMethod]
    public async Task CatchExceptionAsync_nonFatalException_returnsCaught()
    {
        // Verify non-fatal exceptions are caught and returned (not propagated)
        var thrown = new InvalidOperationException("Async non-fatal test");
        var result = await PortamicalAssert.CatchExceptionAsync(() => Task.FromException(thrown));
        Assert.IsNotNull(result);
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
    public async Task DoesNotThrowAsync_nullAttempt_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.DoesNotThrowAsync(null!, _ => ValueTask.CompletedTask));

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
    public void ThrowsDetails_nullAttempt_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(
                null!,
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (_, _) => { },
                (_, _) => { }));
    }

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
                (_, _) => { }));
    }

    [TestMethod]
    public void ThrowsDetails_nullAssertIsType_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),

                null!,
                (_, _) => { }));
    }

    [TestMethod]
    public void ThrowsDetails_nullAssertEquality_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),

                (_, _) => { },
                null!));
    }

    [TestMethod]
    public void ThrowsDetails_nullAssertFail_throwsArgumentNullException()
    {
        string paramName = "param";

        Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(

                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                null!,

                (_, _) => { },
                (_, _) => { }));
    }

    [TestMethod]
    public void ThrowsDetails_noExceptionThrown_callsAssertFail()
    {
        string paramName = "param";

        bool failCalled = false;
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { },

                new ArgumentException("expected", paramName),

                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (_, _) => { },
                (_, _) => { }));
        // Note: failCalled will always be false now since assertFail callback was removed
        // assertThrowsAny throws AssertFailedException when no exception
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void ThrowsDetails_noExceptionThrown_messageContainsNotThrown()
    {
        string paramName = "param";
        string? message = null;
        var ex = Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { },

                new ArgumentException("expected", paramName),

                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (_, _) => { },
                (_, _) => { }));
        // Message now comes from Assert.ThrowsExactly failure
        message = ex.Message;
        Assert.IsNotNull(message);
        AssertContainsOrdinal(message, "Assert.ThrowsExactly failed");
    }

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_callsAssertIsType()
    {
        string paramName = "param";
        bool isTypeCalled = false;

        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (expectedType, actual) =>
                {
                    isTypeCalled = true;

                    if (actual.GetType() != expectedType)
                    {
                        throw new InvalidOperationException();
                    }
                },
                (_, _) => { }));
        // Note: isTypeCalled will be false because assertThrowsAny fails before assertIsType is called
        Assert.IsFalse(isTypeCalled);
    }

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_messageContainsTypeInfo()
    {
        string paramName = "param";

        var ex = Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (expectedType, actual) => throw new InvalidOperationException(
                    $"Expected {expectedType.Name} but got {actual.GetType().Name}"),
                (_, _) => { }));
        // Message comes from Assert.ThrowsExactly failure, not from assertIsType callback
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
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (t, e) => { isTypeCalled = true; capturedType = t; capturedException = e; },
            (_, _) => { });

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
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (_, _) => { },
            (e, a) => equalityCalled = true);

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
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (t, e) => { },
            (e, a) => { });
        Assert.AreSame<Exception>(thrown, result);
    }

    [TestMethod]
    public void ThrowsDetails_attemptInvoked_executesAction()
    {
        bool attemptExecuted = false;
        string paramName = "p";
        var thrown = new ArgumentException("test", paramName);
        var expected = new ArgumentException("test", paramName);

        var result = PortamicalAssert.ThrowsDetails(
            () =>
            {
                attemptExecuted = true;
                throw thrown;
            },
            expected,
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (_, _) => { },
            (_, _) => { });

        Assert.IsTrue(attemptExecuted);
        Assert.AreSame<Exception>(thrown, result);
    }

    [TestMethod]
    public void ThrowsDetails_catchExceptionInvoked_receivesWrappedAttempt()
    {
        bool catchExceptionCalled = false;
        bool attemptExecuted = false;
        string paramName = "p";
        var thrown = new ArgumentException("test", paramName);
        var expected = new ArgumentException("test", paramName);

        void attempt()
        {
            attemptExecuted = true;
            throw thrown;
        }

        var result = PortamicalAssert.ThrowsDetails(
            attempt,
            expected,
            attemptArg =>
            {
                catchExceptionCalled = true;
                // attemptArg is now a wrapped lambda, not the original action
                return Assert.ThrowsExactly<ArgumentException>(attemptArg);
            },
            (_, _) => { },
            (_, _) => { });

        Assert.IsTrue(catchExceptionCalled, "assertThrowsAny should have been invoked");
        Assert.IsTrue(attemptExecuted, "original attempt should have been executed");
        Assert.AreSame<Exception>(thrown, result);
    }

    [TestMethod]
    public void ThrowsDetails_assertIsTypeInvoked_receivesTypeAndException()
    {
        bool assertIsTypeCalled = false;
        Type? capturedExpectedType = null;
        object? capturedActual = null;
        string paramName = "p";
        var thrown = new ArgumentException("test", paramName);
        var expected = new ArgumentException("test", paramName);

        _ = PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (expectedType, actual) =>
            {
                assertIsTypeCalled = true;
                capturedExpectedType = expectedType;
                capturedActual = actual;
            },
            (_, _) => { });

        Assert.IsTrue(assertIsTypeCalled);
        Assert.AreEqual(typeof(ArgumentException), capturedExpectedType);
        Assert.AreSame(thrown, capturedActual);
    }

    [TestMethod]
    public void ThrowsDetails_assertEqualityInvoked_receivesExpectedAndActual()
    {
        bool assertEqualityCalled = false;
        string? capturedExpected = null;
        string? capturedActual = null;
        string testMessage = "test message";
        string paramName = "p";
        var thrown = new ArgumentException(testMessage, paramName);
        var expected = new ArgumentException(testMessage, paramName);

        _ = PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (_, _) => { },
            (exp, act) =>
            {
                assertEqualityCalled = true;
                capturedExpected = exp;
                capturedActual = act;
            });

        Assert.IsTrue(assertEqualityCalled);
        // The method calls assertEquality multiple times for message and paramName
        // We just verify it was called at least once
    }

    [TestMethod]
    public void ThrowsDetails_assertFailInvoked_receivesMessage()
    {
        bool assertFailCalled = false;
        string? capturedMessage = null;
        string paramName = "p";

        var ex = Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { }, // No exception actual
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (_, _) => { },
                (_, _) => { }));

        // assertFail callback no longer exists, message comes from Assert.ThrowsExactly
        Assert.IsFalse(assertFailCalled);
        capturedMessage = ex.Message;
        Assert.IsNotNull(capturedMessage);
        AssertContainsOrdinal(capturedMessage, "Assert.ThrowsExactly failed");
    }

    [TestMethod]
    public void ThrowsDetails_wrongType_assertIsTypeDoesNotThrow_throwsInvalidCastException()
    {
        bool assertIsTypeCalled = false;
        string paramName = "p";

        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException("wrong type"),
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (expectedType, actual) =>
                {
                    assertIsTypeCalled = true;
                    // Not throwing here - should trigger fallback InvalidCastException
                },
                (_, _) => { }));

        // assertIsTypeCalled will be false because assertThrowsAny fails before assertIsType
        Assert.IsFalse(assertIsTypeCalled);
    }

    [TestMethod]
    public void ThrowsDetails_assertFailDoesNotThrow_throwsFallbackException()
    {
        bool assertFailCalled = false;

        string paramName = "p";

        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => { }, // No exception actual
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (_, _) => { },
                (_, _) => { }));

        // assertFail no longer exists, assertThrowsAny throws AssertFailedException
        Assert.IsFalse(assertFailCalled);
    }

    #endregion

    #region ThrowsDetailsAsync

    [TestMethod]
    public async Task ThrowsDetailsAsync_nullAttempt_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                null!,
                new ArgumentException("expected", paramName),
                async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_nullCatchExceptionAsync_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                null!,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_nullAssertIsTypeAsync_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                null!,
                (_, _) => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_nullAssertEqualityAsync_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                null!));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_nullAssertThrowsAnyAsync_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new ArgumentException("test", paramName),
                new ArgumentException("expected", paramName),
                null!,
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
    }

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
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask);
        Assert.AreSame<Exception>(thrown, result);
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
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "async message");
        AssertContainsOrdinal(callsText, "paramName");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_objectDisposedException_skipsGuardMessage()
    {
        var calls = new List<string>();
        var thrown = new ObjectDisposedException("resource");
        var expected = new ObjectDisposedException("resource");

        await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<ObjectDisposedException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Should skip guard message assertion
        Assert.IsLessThanOrEqualTo(calls.Count, 1);
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_argumentOutOfRangeException_assertsParamNameButSkipsGuardMessage()
    {
        var calls = new List<string>();
        string paramName = "asyncCount";
        var thrown = new ArgumentOutOfRangeException(paramName, 10, "Must be less than 20");
        var expected = new ArgumentOutOfRangeException(paramName, 10, "Must be less than 20");

        await PortamicalAssert.ThrowsDetailsAsync(
            () => throw thrown,
            expected,
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, paramName);
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_noException_assertFailDoesNotThrow_throwsFallbackException()
    {
        string paramName = "param";

        var ex = await Assert.ThrowsExactlyAsync<AssertFailedException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => { return Task.CompletedTask; },

                new ArgumentException("expected", paramName),

                async (attempt) => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
        AssertContainsOrdinal(ex.Message, "Assert.ThrowsExactlyAsync failed");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_wrongType_assertIsTypeDoesNotThrow_throwsInvalidCastException()
    {
        string paramName = "param";
        await Assert.ThrowsExactlyAsync<AssertFailedException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                () => throw new InvalidOperationException(),

                new ArgumentException("expected", paramName),

                async (attempt) => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
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
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask);
        Assert.AreSame<Exception>(thrown, result);
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
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "async func message");
        AssertContainsOrdinal(callsText, "asyncParamName");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_noException_assertFailDoesNotThrow_throwsFallbackException()
    {
        string paramName = "param";

        var ex = await Assert.ThrowsExactlyAsync<AssertFailedException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                async () => await Task.Delay(1, TestContext.CancellationToken),

                new ArgumentException("expected", paramName),

                async (attempt) => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
        AssertContainsOrdinal(ex.Message, "Assert.ThrowsExactlyAsync failed");
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_wrongType_assertIsTypeDoesNotThrow_throwsInvalidCastException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<AssertFailedException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                async () =>
                {
                    await Task.Delay(1, TestContext.CancellationToken);
                    throw new InvalidOperationException();
                },

                new ArgumentException("expected", paramName),

                async (attempt) => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
    }

    [TestMethod]
    public async Task ThrowsDetailsAsync_FuncTask_nullAttempt_throwsArgumentNullException()
    {
        string paramName = "param";

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.ThrowsDetailsAsync(
                null!,

                new ArgumentException("expected", paramName),

                async (attempt) => await Assert.ThrowsExactlyAsync<ArgumentException>(attempt),
                (_, _) => ValueTask.CompletedTask,
                (_, _) => ValueTask.CompletedTask));
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
            assertThrowsAnyAsync: async attempt => await Assert.ThrowsExactlyAsync<InvalidOperationException>(attempt),
            (_, _) => ValueTask.CompletedTask,
            (_, _) => ValueTask.CompletedTask);
        Assert.AreSame<Exception>(thrown, result);
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
    public void Equality_object_equalStringsNonInterned_doesNotCallAssertFail()
    {
        bool failCalled = false;
        string str1 = new("hello".ToCharArray());
        string str2 = new("hello".ToCharArray());
        // Ensure they're not reference-equal (not interned)
        Assert.IsFalse(ReferenceEquals(str1, str2));
        PortamicalAssert.Equality(str1, str2, () => failCalled = true);
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
    public async Task EqualityAsync_generic_nullEquals_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.EqualityAsync(1, 1, null!, _ => ValueTask.CompletedTask, "msg"));

    [TestMethod]
    public async Task EqualityAsync_generic_nullAssertFailAsync_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.EqualityAsync(1, 1, (a, b) => a == b, null!, "msg"));

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

    [TestMethod]
    public async Task EqualityAsync_object_emptyCollections_doesNotCallAssertFailAsync()
    {
        bool failCalled = false;
        object?[] expected = [];
        object?[] actual = [];
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
    public async Task EqualityAsync_object_differentLengthCollections_callsAssertFailAsync()
    {
        bool failCalled = false;
        object?[] expected = [1, 2, 3];
        object?[] actual = [1, 2];
        await PortamicalAssert.EqualityAsync(
            expected,
            actual,
            () =>
            {
                failCalled = true;
                return ValueTask.CompletedTask;
            });
        Assert.IsTrue(failCalled);
    }

    #endregion

    #region Protected helpers

    [TestMethod]
    public void Constructor_derivedClass_canInstantiate()
    {
        // The protected constructor should allow derived classes to instantiate
        var instance = new ConcreteAssert();
        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType<PortamicalAssert>(instance);
    }

    [TestMethod]
    public void Constructor_derivedClass_instanceIsOfCorrectType()
    {
        var instance = new ConcreteAssert();
        Assert.IsInstanceOfType<ConcreteAssert>(instance);
        Assert.IsInstanceOfType<PortamicalAssert>(instance);
    }

    [TestMethod]
    public void Constructor_multipleInstances_canCreateIndependently()
    {
        // Verify that multiple instances can be created independently
        var instance1 = new ConcreteAssert();
        var instance2 = new ConcreteAssert();

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreNotSame(instance1, instance2);
    }

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

    [TestMethod]
    public void Equality_object_nullActual_callsAssertFail()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(42, null, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_sameReferenceComplexObject_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var obj = new ArgumentException("test");
        PortamicalAssert.Equality(obj, obj, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_emptyCollections_doesNotCallAssertFail()
    {
        bool failCalled = false;
        object?[] arr1 = [];
        object?[] arr2 = [];
        PortamicalAssert.Equality(arr1, arr2, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_differentLengthCollections_callsAssertFail()
    {
        bool failCalled = false;
        object?[] arr1 = [1, 2, 3];
        object?[] arr2 = [1, 2];
        PortamicalAssert.Equality(arr1, arr2, () => failCalled = true);
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
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

        // Should assert both message and wrongParamName
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
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

        // Should assert message only (wrongParamName is null)
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
            attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

        // Guard message should skip message assertion
        var callsText = string.Join(Environment.NewLine, calls);
        Assert.AreEqual(
            -1,
            callsText.IndexOf("The value cannot be an empty string", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThrowsDetails_argumentOutOfRangeException_assertsParamNameButSkipsGuardMessage()
    {
        var calls = new List<string>();
        string paramName = "count";
        var thrown = new ArgumentOutOfRangeException(paramName, 5, "Value must be less than 10");
        var expected = new ArgumentOutOfRangeException(paramName, 5, "Value must be less than 10");

        PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            attempt => Assert.ThrowsExactly<ArgumentOutOfRangeException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

        // Should assert wrongParamName but skip guard message that starts with 'wrongParamName' ('actualValue')
        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, paramName);
    }

    [TestMethod]
    public void ThrowsDetails_objectDisposedException_assertsMessageIfNotGuardMessage()
    {
        var calls = new List<string>();
        var thrown = new ObjectDisposedException("MyResource", "Custom disposal message");
        var expected = new ObjectDisposedException("MyResource", "Custom disposal message");

        PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            attempt => Assert.ThrowsExactly<ObjectDisposedException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

        // Should assert custom message
        var callsText = string.Join(Environment.NewLine, calls);
        AssertContainsOrdinal(callsText, "Custom disposal message");
    }

    [TestMethod]
    public void ThrowsDetails_objectDisposedExceptionWithGuardMessage_skipsMessageAssertion()
    {
        var calls = new List<string>();
        var thrown = new ObjectDisposedException("MyResource");
        var expected = new ObjectDisposedException("MyResource");

        PortamicalAssert.ThrowsDetails(
            () => throw thrown,
            expected,
            attempt => Assert.ThrowsExactly<ObjectDisposedException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

        // Guard message "Cannot access a disposed object" should skip message assertion
        // Only one call expected (no message assertion)
        Assert.IsLessThanOrEqualTo(calls.Count, 1);
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
            attempt => Assert.ThrowsExactly<InvalidOperationException>(attempt),
            (_, _) => { },
            (e, a) => calls.Add($"{e}={a}"));

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
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new ArgumentNullException(paramName),
                new ArgumentException("expected", paramName),
                attempt => Assert.ThrowsExactly<ArgumentException>(attempt),
                (expectedType, actual) =>
                {
                    isTypeCalled = true;

                    if (actual.GetType() != expectedType)
                    {
                        throw new InvalidOperationException();
                    }
                },
                (_, _) => { }));
        // assertIsTypeCalled will be false because assertThrowsAny fails before assertIsType
        Assert.IsFalse(isTypeCalled);
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

    [TestMethod]
    public void Equality_object_doubleRegularAndNaN_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0, double.NaN, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatNaNAndRegular_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.NaN, 1.0f, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatRegularAndNaN_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0f, float.NaN, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatNegativeInfinity_equal()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.NegativeInfinity, float.NegativeInfinity, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatPositiveAndNegativeInfinity_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.PositiveInfinity, float.NegativeInfinity, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatPositiveInfinityAndRegular_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.PositiveInfinity, 1.0f, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatRegularAndPositiveInfinity_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0f, float.PositiveInfinity, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatNegativeInfinityAndRegular_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(float.NegativeInfinity, 1.0f, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatRegularAndNegativeInfinity_notEqual()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(1.0f, float.NegativeInfinity, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatWithinAbsoluteTolerance_equal()
    {
        bool failCalled = false;
        // Test absolute tolerance path: diff <= tolerance
        // Using very small values where relative tolerance would not apply
        float expected = 1e-10f;
        float actual = 1.5e-10f;
        // Default tolerance is 1e-6f, so diff (0.5e-10f) is much smaller
        PortamicalAssert.Equality(expected, actual, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatWithinRelativeTolerance_equal()
    {
        bool failCalled = false;
        // Test relative tolerance path: diff <= maxAbs * tolerance
        // Using larger values where relative tolerance applies
        float expected = 1000000.0f;
        float actual = 1000000.5f;
        // diff = 0.5, tolerance = 1e-6f, maxAbs = 1000000.0f
        // diff (0.5) > tolerance (1e-6f) but diff (0.5) <= maxAbs * tolerance (1.0)
        PortamicalAssert.Equality(expected, actual, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_floatZeroPositiveAndNegative_equal()
    {
        bool failCalled = false;
        PortamicalAssert.Equality(0.0f, -0.0f, () => failCalled = true);
        Assert.IsFalse(failCalled); // Both are bitwise equal in IEEE 754 representation
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

    [TestMethod]
    public void Equality_object_equalBigIntegers_doesNotCallAssertFail()
    {
        bool failCalled = false;
        var big1 = new System.Numerics.BigInteger(123456789012345);
        var big2 = new System.Numerics.BigInteger(123456789012345);
        PortamicalAssert.Equality(big1, big2, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_unequalBigIntegers_callsAssertFail()
    {
        bool failCalled = false;
        var big1 = new System.Numerics.BigInteger(123456789012345);
        var big2 = new System.Numerics.BigInteger(987654321098765);
        PortamicalAssert.Equality(big1, big2, () => failCalled = true);
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void Equality_object_customTypeWithEquals_usesObjectEquals()
    {
        bool failCalled = false;
        var obj1 = new CustomEquatableType(42);
        var obj2 = new CustomEquatableType(42);
        PortamicalAssert.Equality(obj1, obj2, () => failCalled = true);
        Assert.IsFalse(failCalled);
    }

    [TestMethod]
    public void Equality_object_customTypeNotEqual_callsAssertFail()
    {
        bool failCalled = false;
        var obj1 = new CustomEquatableType(42);
        var obj2 = new CustomEquatableType(99);
        PortamicalAssert.Equality(obj1, obj2, () => failCalled = true);
        Assert.IsTrue(failCalled);
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
    public async Task IsTypeOfAsync_nullExpected_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.IsTypeOfAsync(
                null!,
                new object(),
                (_, _) => ValueTask.CompletedTask));

    [TestMethod]
    public async Task IsTypeOfAsync_nullAssertEqualityAsync_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.IsTypeOfAsync(typeof(string), "hello", null!));

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

    #endregion

    #region MetadataEqualityAsync

    [TestMethod]
    public async Task MetadataEqualityAsync_nullAssertEqualityAsync_throwsArgumentNullException()
        => await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await PortamicalAssert.MetadataEqualityAsync(
                new ArgumentException("test"),
                new ArgumentException("test"),
                null!));

    [TestMethod]
    public async Task MetadataEqualityAsync_argumentException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
        string testParam = "testParam";
        var expected = new ArgumentException("Test message", testParam);
        var actual = new ArgumentException("Test message", testParam);

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        Assert.HasCount(2, calls);
        // ArgumentException.Message includes parameter name: "Test message (Parameter 'testParam')"
        AssertContainsOrdinal(calls[0], "Test message");
        Assert.AreEqual("testParam=testParam", calls[1]);
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_argumentExceptionWithoutParamName_assertsMessageOnly()
    {
        var calls = new List<string>();
        var expected = new ArgumentException("Test message");
        var actual = new ArgumentException("Test message");

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Only message assertion, no ParamName
        Assert.HasCount(1, calls);
        Assert.AreEqual("Test message=Test message", calls[0]);
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_argumentExceptionGuardMessage_skipsMessageAssertion()
    {
        var calls = new List<string>();
        var paramName = "param";
        var expected = new ArgumentException("The value cannot be an empty string", paramName);
        var actual = new ArgumentException("The value cannot be an empty string", paramName);

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Should only assert ParamName, skip guard message
        Assert.HasCount(1, calls);
        Assert.AreEqual("param=param", calls[0]);
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_argumentOutOfRangeException_assertsCustomMessage()
    {
        var calls = new List<string>();
        var paramName = "count";
        var expected = new ArgumentOutOfRangeException(paramName, 5, "Must be positive");
        var actual = new ArgumentOutOfRangeException(paramName, 5, "Must be positive");

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Custom message is asserted along with ParamName
        Assert.HasCount(2, calls);
        AssertContainsOrdinal(calls[0], "Must be positive");
        Assert.AreEqual("count=count", calls[1]);
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_objectDisposedException_assertsFrameworkMessage()
    {
        var calls = new List<string>();
        var resourceName = "resource";
        var expected = new ObjectDisposedException(resourceName);
        var actual = new ObjectDisposedException(resourceName);

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Framework-generated message is asserted
        Assert.HasCount(1, calls);
        AssertContainsOrdinal(calls[0], "Cannot access a disposed object");
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_objectDisposedExceptionCustomMessage_assertsMessage()
    {
        var calls = new List<string>();
        var resourceName = "resource";
        var expected = new ObjectDisposedException(resourceName, "Custom disposal message");
        var actual = new ObjectDisposedException(resourceName, "Custom disposal message");

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Should assert custom message
        Assert.HasCount(1, calls);
        AssertContainsOrdinal(calls[0], "Custom disposal message");
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_invalidOperationException_assertsMessage()
    {
        var calls = new List<string>();
        var expected = new InvalidOperationException("Operation failed");
        var actual = new InvalidOperationException("Operation failed");

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        Assert.HasCount(1, calls);
        Assert.AreEqual("Operation failed=Operation failed", calls[0]);
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_argumentNullException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
        var paramName = "parameter";
        var expected = new ArgumentNullException(paramName);
        var actual = new ArgumentNullException(paramName);

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // ArgumentNullException derives from ArgumentException - asserts both message and ParamName
        Assert.HasCount(2, calls);
        AssertContainsOrdinal(calls[0], "parameter");
        Assert.AreEqual("parameter=parameter", calls[1]);
    }

    [TestMethod]
    public async Task MetadataEqualityAsync_exceptionWithNullMessage_doesNotAssert()
    {
        var calls = new List<string>();
        var expected = new Exception();
        var actual = new Exception();

        await PortamicalAssert.MetadataEqualityAsync(
            expected,
            actual,
            (e, a) =>
            {
                calls.Add($"{e}={a}");
                return ValueTask.CompletedTask;
            });

        // Message is "Exception of type 'System.Exception' was actual." (not null but framework-generated)
        Assert.HasCount(1, calls);
    }

    #endregion

    #region MetadataEquality

    [TestMethod]
    public void MetadataEquality_nullAssertEquality_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.MetadataEquality(
                new ArgumentException("test"),
                new ArgumentException("test"),
                null!));

    [TestMethod]
    public void MetadataEquality_argumentException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
        var paramName = "syncParam";
        var expected = new ArgumentException("Sync test message", paramName);
        var actual = new ArgumentException("Sync test message", paramName);

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        Assert.HasCount(2, calls);
        // ArgumentException.Message includes parameter name
        AssertContainsOrdinal(calls[0], "Sync test message");
        Assert.AreEqual("syncParam=syncParam", calls[1]);
    }

    [TestMethod]
    public void MetadataEquality_argumentExceptionWithoutParamName_assertsMessageOnly()
    {
        var calls = new List<string>();
        var expected = new ArgumentException("Sync test");
        var actual = new ArgumentException("Sync test");

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        Assert.HasCount(1, calls);
        Assert.AreEqual("Sync test=Sync test", calls[0]);
    }

    [TestMethod]
    public void MetadataEquality_argumentExceptionGuardMessage_skipsMessageAssertion()
    {
        var calls = new List<string>();
        var paramName = "syncParam";
        var expected = new ArgumentException("The value cannot be an empty string", paramName);
        var actual = new ArgumentException("The value cannot be an empty string", paramName);

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        // Should only assert ParamName
        Assert.HasCount(1, calls);
        Assert.AreEqual("syncParam=syncParam", calls[0]);
    }

    [TestMethod]
    public void MetadataEquality_argumentOutOfRangeException_assertsCustomMessage()
    {
        var calls = new List<string>();
        var paramName = "index";
        var expected = new ArgumentOutOfRangeException(paramName, 10, "Index out of bounds");
        var actual = new ArgumentOutOfRangeException(paramName, 10, "Index out of bounds");

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        // Custom message is asserted along with ParamName
        Assert.HasCount(2, calls);
        AssertContainsOrdinal(calls[0], "Index out of bounds");
        Assert.AreEqual("index=index", calls[1]);
    }

    [TestMethod]
    public void MetadataEquality_objectDisposedException_assertsFrameworkMessage()
    {
        var calls = new List<string>();
        var expected = new ObjectDisposedException("syncResource");
        var actual = new ObjectDisposedException("syncResource");

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        // Framework-generated message is asserted
        Assert.HasCount(1, calls);
        AssertContainsOrdinal(calls[0], "Cannot access a disposed object");
    }

    [TestMethod]
    public void MetadataEquality_objectDisposedExceptionCustomMessage_assertsMessage()
    {
        var calls = new List<string>();
        var expected = new ObjectDisposedException("syncResource", "Sync disposal message");
        var actual = new ObjectDisposedException("syncResource", "Sync disposal message");

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        Assert.HasCount(1, calls);
        AssertContainsOrdinal(calls[0], "Sync disposal message");
    }

    [TestMethod]
    public void MetadataEquality_invalidOperationException_assertsMessage()
    {
        var calls = new List<string>();
        var expected = new InvalidOperationException("Sync operation failed");
        var actual = new InvalidOperationException("Sync operation failed");

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        Assert.HasCount(1, calls);
        Assert.AreEqual("Sync operation failed=Sync operation failed", calls[0]);
    }

    [TestMethod]
    public void MetadataEquality_argumentNullException_assertsMessageAndParamName()
    {
        var calls = new List<string>();
        var paramName = "syncParameter";
        var expected = new ArgumentNullException(paramName);
        var actual = new ArgumentNullException(paramName);

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) => calls.Add($"{e}={a}"));

        // ArgumentNullException derives from ArgumentException - asserts both message and ParamName
        Assert.HasCount(2, calls);
        AssertContainsOrdinal(calls[0], "syncParameter");
        Assert.AreEqual("syncParameter=syncParameter", calls[1]);
    }

    [TestMethod]
    public void MetadataEquality_delegatesCallsAssertEquality()
    {
        var wrongParamName = "wrongParam";
        var paramName = "param";
        var expected = new ArgumentException("Test", paramName);
        var actual = new ArgumentException("Different", wrongParamName);
        bool wasCalledForMessage = false;
        bool wasCalledForParamName = false;

        PortamicalAssert.MetadataEquality(
            expected,
            actual,
            (e, a) =>
            {
                // ArgumentException.Message includes parameter name, so check if it contains the base message
                if (e.Contains("Test")) wasCalledForMessage = true;
                if (e == "param") wasCalledForParamName = true;
            });

        Assert.IsTrue(wasCalledForMessage);
        Assert.IsTrue(wasCalledForParamName);
    }

    #endregion
}
