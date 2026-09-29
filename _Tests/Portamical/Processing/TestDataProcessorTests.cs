// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.TestDataTypes;
using Portamical.Processing;

namespace Tests.Portamical.Processing;

[TestClass]
public class TestDataProcessorTests
{
#pragma warning disable CA1859
    private static ITestData CreateData(string definition, int arg = 1)
        => TestDataFactory.CreateTestData<int>(definition, "result", arg);
#pragma warning restore CA1859

    #region ProcessIfDistinct

    [TestMethod]
    public void ProcessIfDistinct_withNewItem_invokesProcess_andReturnsTrue()
    {
        var processor = new TestDataProcessor();
        var item = CreateData("single", 1);
        var processed = new List<ITestData>();

        var result = processor.ProcessIfDistinct(item, processed.Add);

        Assert.IsTrue(result);
        Assert.HasCount(1, processed);
        Assert.AreSame(item, processed[0]);
    }

    [TestMethod]
    public void ProcessIfDistinct_withAlreadySeenItem_doesNotInvokeProcessAgain_andReturnsFalse()
    {
        var processor = new TestDataProcessor();
        var item = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);
        var processed = new List<ITestData>();

        var firstResult = processor.ProcessIfDistinct(item, processed.Add);
        var secondResult = processor.ProcessIfDistinct(duplicate, processed.Add);

        Assert.IsTrue(firstResult);
        Assert.IsFalse(secondResult);
        Assert.HasCount(1, processed);
        Assert.AreSame(item, processed[0]);
    }

    [TestMethod]
    public void ProcessIfDistinct_withNewItemAndNullProcess_doesNotThrow_registersItem_andReturnsNull()
    {
        var processor = new TestDataProcessor();
        var item = CreateData("single", 1);

        var result = processor.ProcessIfDistinct(item, null);

        Assert.IsNull(result);

        // The item should now be registered as seen, so a later call with the same identity
        // and a non-null process should be skipped.
        var processed = new List<ITestData>();
        var secondResult = processor.ProcessIfDistinct(CreateData("single", 2), processed.Add);

        Assert.IsFalse(secondResult);
        Assert.HasCount(0, processed);
    }

    [TestMethod]
    public void ProcessIfDistinct_withAlreadySeenItemAndNullProcess_returnsFalse()
    {
        var processor = new TestDataProcessor();
        var item = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);

        processor.ProcessIfDistinct(item, null);
        var result = processor.ProcessIfDistinct(duplicate, null);

        // Once already seen, the null-process branch is never reached; false takes precedence.
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ProcessIfDistinct_withDistinctItems_invokesProcessForEach_andReturnsTrueForEach()
    {
        var processor = new TestDataProcessor();
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var processed = new List<ITestData>();

        var firstResult = processor.ProcessIfDistinct(first, processed.Add);
        var secondResult = processor.ProcessIfDistinct(second, processed.Add);

        Assert.IsTrue(firstResult);
        Assert.IsTrue(secondResult);
        Assert.HasCount(2, processed);
        CollectionAssert.Contains(processed, first);
        CollectionAssert.Contains(processed, second);
    }

    #endregion

    #region ProcessCollection - Argument Validation

    [TestMethod]
    public void ProcessCollection_withNullCollection_throwsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => TestDataProcessor.ProcessCollection<ITestData>(
                null!, _ => { }, removeDuplicates: false, skipFirst: false));
    }

    [TestMethod]
    public void ProcessCollection_withEmptyCollection_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => TestDataProcessor.ProcessCollection(
                Array.Empty<ITestData>(), _ => { }, removeDuplicates: false, skipFirst: false));
    }

    #endregion

    #region ProcessCollection - No Deduplication

    [TestMethod]
    public void ProcessCollection_withoutDuplicatesOrSkip_processesAllItemsInOrder()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var processed = new List<ITestData>();

        TestDataProcessor.ProcessCollection(
            [first, second], processed.Add, removeDuplicates: false, skipFirst: false);

        CollectionAssert.AreEqual(new[] { first, second }, processed);
    }

    [TestMethod]
    public void ProcessCollection_withSkipFirst_excludesFirstItem()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var processed = new List<ITestData>();

        TestDataProcessor.ProcessCollection(
            [first, second], processed.Add, removeDuplicates: false, skipFirst: true);

        CollectionAssert.AreEqual(new[] { second }, processed);
    }

    [TestMethod]
    public void ProcessCollection_withNullProcess_doesNotThrow()
    {
        var first = CreateData("first", 1);
        bool doesnotThrow = false;

        try
        {
            TestDataProcessor.ProcessCollection(
                [first], null, removeDuplicates: false, skipFirst: false);

            doesnotThrow = true;
        }
        catch (Exception ex)
        {
            throw new AssertFailedException("ProcessCollection threw an exception when process was null.", ex);
        }

        Assert.IsTrue(doesnotThrow);
    }

    [TestMethod]
    public void ProcessCollection_withSingleItem_removeDuplicatesIgnored_processesItem()
    {
        // Deduplication is only applied when count > 1, so with a single item it should
        // still be processed even when removeDuplicates is true.
        var item = CreateData("single", 1);
        var processed = new List<ITestData>();

        TestDataProcessor.ProcessCollection(
            [item], processed.Add, removeDuplicates: true, skipFirst: false);

        CollectionAssert.AreEqual(new[] { item }, processed);
    }

    #endregion

    #region ProcessCollection - Deduplication

    [TestMethod]
    public void ProcessCollection_withDuplicatesAndRemoveDuplicates_processesOnlyDistinctItems()
    {
        var first = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);
        var second = CreateData("second", 3);
        var processed = new List<ITestData>();

        TestDataProcessor.ProcessCollection(
            [first, duplicate, second], processed.Add, removeDuplicates: true, skipFirst: false);

        // 'first' and 'duplicate' share the same TestCaseName, so equality-based assertions would
        // treat them as interchangeable. Verify identity (reference) instead.
        Assert.HasCount(2, processed);
        Assert.Contains(first, processed);
        Assert.Contains(second, processed);
    }

    [TestMethod]
    public void ProcessCollection_withDuplicatesAndWithoutRemoveDuplicates_processesAllItems()
    {
        var first = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);
        var processed = new List<ITestData>();

        TestDataProcessor.ProcessCollection(
            [first, duplicate], processed.Add, removeDuplicates: false, skipFirst: false);

        CollectionAssert.AreEqual(new[] { first, duplicate }, processed);
    }

    [TestMethod]
    public void ProcessCollection_withRemoveDuplicatesAndSkipFirst_registersFirstItemAsSeen_withoutProcessingIt()
    {
        var first = CreateData("duplicate", 1);
        var duplicate = CreateData("duplicate", 2);
        var second = CreateData("second", 3);
        var processed = new List<ITestData>();

        TestDataProcessor.ProcessCollection(
            [first, duplicate, second], processed.Add, removeDuplicates: true, skipFirst: true);

        // 'first' is skipped from processing, but its identity is registered as seen,
        // so 'duplicate' (same test case name) is also excluded.
        Assert.HasCount(1, processed);
        Assert.Contains(second, processed);
        Assert.DoesNotContain(first, processed);
        Assert.DoesNotContain(duplicate, processed);
    }

    [TestMethod]
    public void ProcessCollection_withRemoveDuplicatesSkipFirstAndNullProcess_doesNotThrow()
    {
        var first = CreateData("first", 1);
        var second = CreateData("second", 2);
        var count = 0;

        TestDataProcessor.ProcessCollection(
            [first, second], (_) => count++, removeDuplicates: true, skipFirst: false);

        Assert.AreEqual(2, count);
    }

    #endregion
}
