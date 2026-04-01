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
        var thrown = new ArgumentException("bad arg", "param");
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
        Assert.IsTrue(message?.Contains(typeof(InvalidOperationException).FullName!));
    }

    [TestMethod]
    public void DoesNotThrow_exceptionThrown_messageContainsExceptionMessage()
    {
        string? message = null;
        PortamicalAssert.DoesNotThrow(
            () => throw new InvalidOperationException("specific error text"),
            msg => message = msg);
        Assert.IsTrue(message?.Contains("specific error text"));
    }

    #endregion

    #region IsTypeOf

    [TestMethod]
    public void IsTypeOf_nullExpected_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.IsTypeOf(null!, new object(), (_, _) => { }));

    [TestMethod]
    public void IsTypeOf_nullActual_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.IsTypeOf(typeof(string), null!, (_, _) => { }));

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

    #region ThrowsActualType

    [TestMethod]
    public void ThrowsActualType_nullAssertIsType_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsActualType(
                new ArgumentException(), null, null!, _ => { }));

    [TestMethod]
    public void ThrowsActualType_nullAssertFail_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsActualType(
                new ArgumentException(), null, (_, _) => { }, null!));

    [TestMethod]
    public void ThrowsActualType_nullActual_callsAssertFailAndThrows()
    {
        bool failCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsActualType(
                new ArgumentException(),
                null,
                (_, _) => { },
                _ => failCalled = true));
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void ThrowsActualType_nullActual_assertFailMessageMentionsNotThrown()
    {
        string? message = null;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsActualType(
                new ArgumentException(),
                null,
                (_, _) => { },
                msg => message = msg));
        Assert.IsTrue(message?.Contains("was not thrown"));
    }

    [TestMethod]
    public void ThrowsActualType_wrongActualType_callsAssertFailAndThrows()
    {
        bool failCalled = false;
        Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.ThrowsActualType(
                new ArgumentException(),
                new InvalidOperationException("wrong"),
                (_, _) => { },
                _ => failCalled = true));
        Assert.IsTrue(failCalled);
    }

    [TestMethod]
    public void ThrowsActualType_correctType_callsAssertIsTypeAndReturnsTyped()
    {
        bool isTypeCalled = false;
        var expected = new ArgumentException("test", "p");
        var thrown = new ArgumentException("test", "p");
        var result = PortamicalAssert.ThrowsActualType(
            expected,
            thrown,
            (t, e) => isTypeCalled = true,
            msg => throw new InvalidOperationException(msg));
        Assert.IsTrue(isTypeCalled);
        Assert.AreSame(thrown, result);
    }

    #endregion

    #region ThrowsMetadataEquality

    [TestMethod]
    public void ThrowsMetadataEquality_nullAssertEquality_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsMetadataEquality(
                new InvalidOperationException("msg"),
                new InvalidOperationException("msg"),
                null!));

    [TestMethod]
    public void ThrowsMetadataEquality_regularException_assertsMessage()
    {
        int callCount = 0;
        var expected = new InvalidOperationException("some message");
        var actual = new InvalidOperationException("some message");
        PortamicalAssert.ThrowsMetadataEquality(expected, actual, (e, a) => callCount++);
        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public void ThrowsMetadataEquality_regularException_passesCorrectMessageStrings()
    {
        string? capturedExpected = null;
        string? capturedActual = null;
        var expected = new InvalidOperationException("expected msg");
        var actual = new InvalidOperationException("actual msg");
        PortamicalAssert.ThrowsMetadataEquality(
            expected, actual, (e, a) => { capturedExpected = e; capturedActual = a; });
        Assert.AreEqual("expected msg", capturedExpected);
        Assert.AreEqual("actual msg", capturedActual);
    }

    [TestMethod]
    public void ThrowsMetadataEquality_argumentExceptionWithParamName_assertsMessageAndParamName()
    {
        int callCount = 0;
        var expected = new ArgumentException("arg message", "myParam");
        var actual = new ArgumentException("arg message", "myParam");
        PortamicalAssert.ThrowsMetadataEquality(expected, actual, (e, a) => callCount++);
        Assert.AreEqual(2, callCount); // message + paramName
    }

    [TestMethod]
    public void ThrowsMetadataEquality_argumentExceptionWithParamName_passesParamName()
    {
        string? capturedParamExpected = null;
        string? capturedParamActual = null;
        int callIndex = 0;
        var expected = new ArgumentException("msg", "myParam");
        var actual = new ArgumentException("msg", "actualParam");
        PortamicalAssert.ThrowsMetadataEquality(expected, actual, (e, a) =>
        {
            if (callIndex++ == 1) { capturedParamExpected = e; capturedParamActual = a; }
        });
        Assert.AreEqual("myParam", capturedParamExpected);
        Assert.AreEqual("actualParam", capturedParamActual);
    }

    [TestMethod]
    public void ThrowsMetadataEquality_argumentExceptionNullParamName_onlyAssertsMessage()
    {
        int callCount = 0;
        var expected = new ArgumentException("arg message");
        var actual = new ArgumentException("arg message");
        PortamicalAssert.ThrowsMetadataEquality(expected, actual, (e, a) => callCount++);
        Assert.AreEqual(1, callCount); // message only
    }

    [TestMethod]
    public void ThrowsMetadataEquality_argumentExceptionWithGuardMessage_skipsAllAssertions()
    {
        int callCount = 0;
        const string guardMsg = "The value cannot be an empty string";
        var expected = new ArgumentException(guardMsg);
        var actual = new ArgumentException(guardMsg);
        PortamicalAssert.ThrowsMetadataEquality(expected, actual, (e, a) => callCount++);
        Assert.AreEqual(0, callCount);
    }

    [TestMethod]
    public void ThrowsMetadataEquality_returnsActual()
    {
        var expected = new InvalidOperationException("msg");
        var thrown = new InvalidOperationException("msg");
        var result = PortamicalAssert.ThrowsMetadataEquality(expected, thrown, (_, _) => { });
        Assert.AreSame(thrown, result);
    }

    #endregion

    #region ThrowsDetails

    [TestMethod]
    public void ThrowsDetails_nullCatchException_throwsArgumentNullException()
        => Assert.ThrowsExactly<ArgumentNullException>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new ArgumentException(),
                new ArgumentException(),
                null!,
                (_, _) => { },
                (_, _) => { },
                _ => { }));

    [TestMethod]
    public void ThrowsDetails_correctException_returnsTypedActual()
    {
        var thrown = new ArgumentException("test msg", "p");
        var expected = new ArgumentException("test msg", "p");
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

    #region Protected helpers

    [TestMethod]
    public void GetTypeFullName_nonNull_returnsFullName()
    {
        var result = ConcreteAssert.ExposedGetTypeFullName(new ArgumentException());
        Assert.AreEqual(typeof(ArgumentException).FullName, result);
    }

    [TestMethod]
    public void GetTypeFullName_null_returnsNull()
    {
        var result = ConcreteAssert.ExposedGetTypeFullName(null);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAssertionFailedException_returnsInvalidOperationExceptionWithMessage()
    {
        var ex = ConcreteAssert.ExposedGetAssertionFailedException("something went wrong");
        Assert.IsInstanceOfType<InvalidOperationException>(ex);
        Assert.Contains("something went wrong", ex.Message);
    }

    #endregion
}
