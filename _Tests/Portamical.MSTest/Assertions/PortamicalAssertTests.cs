// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.MSTest.Assertions;

namespace Tests.Portamical.MSTest.Assertions;

[TestClass]
public class PortamicalAssertTests
{
    private sealed class ConcreteAssert : PortamicalAssert { }

    [TestMethod]
    public void AssertMultiple_executesAction()
    {
        bool executed = false;
        PortamicalAssert.AssertMultiple(() => { executed = true; });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public void AssertMultiple_whenActionFails_propagatesException()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.AssertMultiple(() => Assert.Fail("forced")));
    }

    [TestMethod]
    public async Task AssertMultipleAsync_executesAction()
    {
        bool executed = false;
        await PortamicalAssert.AssertMultipleAsync(async () =>
        {
            executed = true;
            await Task.CompletedTask;
        });
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task AssertMultipleAsync_whenActionFails_propagatesException()
    {
        await Assert.ThrowsExactlyAsync<AssertFailedException>(
            async () => await PortamicalAssert.AssertMultipleAsync(async () =>
            {
                await Task.CompletedTask;
                Assert.Fail("forced");
            }));
    }

    [TestMethod]
    public void DoesNotThrow_noException_succeeds()
    {
        PortamicalAssert.DoesNotThrow(() => { });
    }

    [TestMethod]
    public void DoesNotThrow_throwingAction_throwsAssertFailedException()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.DoesNotThrow(() => throw new InvalidOperationException("oops")));
    }

    [TestMethod]
    public void IsTypeOf_matchingType_succeeds()
    {
        PortamicalAssert.IsTypeOf(typeof(string), "hello");
    }

    [TestMethod]
    public void IsTypeOf_mismatchedType_throwsAssertFailedException()
    {
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.IsTypeOf(typeof(int), "not an int"));
    }

    [TestMethod]
    public void ThrowsDetails_correctException_returnsException()
    {
        var expectedEx = new ArgumentException("value is null", "param1");
        var thrownEx = PortamicalAssert.ThrowsDetails<ArgumentException>(
            () => throw new ArgumentException("value is null", "param1"),
            expectedEx);
        Assert.IsNotNull(thrownEx);
        Assert.AreEqual(typeof(ArgumentException), thrownEx.GetType());
    }

    [TestMethod]
    public void ThrowsDetails_noException_throwsAssertFailedException()
    {
        var expectedEx = new InvalidOperationException("msg");
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails<InvalidOperationException>(
                () => { },
                expectedEx));
    }

    [TestMethod]
    public void ThrowsDetails_wrongType_throwsAssertFailedException()
    {
        var expectedEx = new InvalidOperationException("msg");
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails<InvalidOperationException>(
                () => throw new ArgumentException("msg"),
                expectedEx));
    }

    [TestMethod]
    public void ThrowsDetails_wrongMessage_throwsAssertFailedException()
    {
        var expectedEx = new InvalidOperationException("expected message");
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails<InvalidOperationException>(
                () => throw new InvalidOperationException("different message"),
                expectedEx));
    }

    [TestMethod]
    public void ThrowsDetails_wrongParamName_throwsAssertFailedException()
    {
        var expectedEx = new ArgumentException("msg", "correctParam");
        Assert.ThrowsExactly<AssertFailedException>(
            () => PortamicalAssert.ThrowsDetails<ArgumentException>(
                () => throw new ArgumentException("msg", "wrongParam"),
                expectedEx));
    }
}
