// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.xUnit_v3.Assertions;

namespace Tests.Portamical.xUnit_v3.Assertions;

[TestClass]
public class PortamicalAssertTests
{
    private sealed class TestableAssert : PortamicalAssert { }

    [TestMethod]
    public void AssertMultiple_executesAction()
    {
        bool executed = false;
        TestableAssert.AssertMultiple(() => executed = true);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void AssertMultiple_noException_doesNotThrow()
        => TestableAssert.AssertMultiple(() => { });

    [TestMethod]
    public void AssertMultiple_actionThrows_propagatesException()
        => Assert.ThrowsExactly<InvalidOperationException>(
            () => TestableAssert.AssertMultiple(
                () => throw new InvalidOperationException("test")));

    [TestMethod]
    public async Task AssertMultipleAsync_executesAction()
    {
        bool executed = false;
        await TestableAssert.AssertMultipleAsync(async () =>
        {
            await Task.Yield();
            executed = true;
        });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task AssertMultipleAsync_noException_doesNotThrow()
        => await TestableAssert.AssertMultipleAsync(async () => await Task.Yield());

    [TestMethod]
    public void DoesNotThrow_noException_doesNotThrow()
        => TestableAssert.DoesNotThrow(() => { });

    [TestMethod]
    public void DoesNotThrow_actionThrows_failsWithXunitException()
        => Assert.Throws<Exception>(
            () => TestableAssert.DoesNotThrow(
                () => throw new InvalidOperationException("oops")));

    [TestMethod]
    public void IsTypeOf_correctType_passes()
        => TestableAssert.IsTypeOf(typeof(string), "hello");

    [TestMethod]
    public void IsTypeOf_wrongType_throwsXunitException()
        => Assert.Throws<Exception>(
            () => TestableAssert.IsTypeOf(typeof(int), "hello"));

    [TestMethod]
    public void ThrowsDetails_actionThrowsExpectedType_returnsException()
    {
        var thrown = new ArgumentException("test message", "param1");
        var template = new ArgumentException("test message", "param1");
        var sut_result = TestableAssert.ThrowsDetails(() => throw thrown, template);
        Assert.AreSame(thrown, sut_result);
    }

    [TestMethod]
    public void ThrowsDetails_actionDoesNotThrow_failsWithXunitException()
        => Assert.Throws<Exception>(
            () => TestableAssert.ThrowsDetails(
                () => { }, new ArgumentException("msg")));

    [TestMethod]
    public void ThrowsDetails_actionThrowsWrongType_failsWithXunitException()
        => Assert.Throws<Exception>(
            () => TestableAssert.ThrowsDetails(
                () => throw new InvalidOperationException("oops"),
                new ArgumentException("different")));
}
