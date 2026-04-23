// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.NUnit.Assertions;

namespace Tests.Portamical.NUnit.Assertions;

[TestClass]
public class PortamicalAssertTests
{
    private sealed class TestableAssert : PortamicalAssert { }

    [TestMethod]
    public void AssertMultiple_executesAction()
    {
        bool executed = false;
        PortamicalAssert.AssertMultiple(() => executed = true);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void AssertMultiple_propagatesNonAssertionException()
        => Assert.ThrowsExactly<InvalidOperationException>(
            () => PortamicalAssert.AssertMultiple(
                () => throw new InvalidOperationException("test")));

    [TestMethod]
    public async Task AssertMultipleAsync_executesAsyncAction()
    {
        bool executed = false;
        await PortamicalAssert.AssertMultipleAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void AssertMultipleAsync_returnsTask()
    {
        var task = PortamicalAssert.AssertMultipleAsync(() => Task.CompletedTask);
        Assert.IsNotNull(task);
    }

    [TestMethod]
    public void DoesNotThrow_noException_doesNotFail()
        => PortamicalAssert.DoesNotThrow(() => { });

    [TestMethod]
    public void DoesNotThrow_exceptionThrown_throwsAssertionException()
        => Assert.Throws<Exception>(
            () => PortamicalAssert.DoesNotThrow(
                () => throw new InvalidOperationException("oops")));

    [TestMethod]
    public void IsTypeOf_matchingType_doesNotFail()
        => PortamicalAssert.IsTypeOf(typeof(string), "hello");

    [TestMethod]
    public void IsTypeOf_mismatchedType_throwsAssertionException()
        => Assert.Throws<Exception>(
            () => PortamicalAssert.IsTypeOf(typeof(int), "hello"));

    [TestMethod]
    public void ThrowsDetails_correctTypeAndMessage_returnsException()
    {
#pragma warning disable S3928
        var thrown = new ArgumentException("test message", "param1");
        var template = new ArgumentException("test message", "param1");
#pragma warning restore S3928
        var result = PortamicalAssert.ThrowsDetails(() => throw thrown, template);
        Assert.AreSame(thrown, result);
    }

    [TestMethod]
    public void ThrowsDetails_noException_propagatesFailure()
        => Assert.Throws<Exception>(
            () => PortamicalAssert.ThrowsDetails(
                () => { }, new ArgumentException("msg")));

    [TestMethod]
    public void ThrowsDetails_wrongExceptionType_propagatesFailure()
        => Assert.Throws<Exception>(
            () => PortamicalAssert.ThrowsDetails(
                () => throw new InvalidOperationException("oops"),
                new ArgumentException("different")));

    [TestMethod]
    public void PortamicalAssert_isAbstract()
        => Assert.IsTrue(typeof(PortamicalAssert).IsAbstract);

    [TestMethod]
    public void TestableAssert_canBeInstantiated()
        => Assert.IsNotNull(new TestableAssert());
}
