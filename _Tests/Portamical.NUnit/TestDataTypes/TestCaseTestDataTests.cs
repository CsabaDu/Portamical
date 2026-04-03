// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Identity;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.Core.TestDataTypes.Patterns;
using Portamical.NUnit.TestDataTypes;

namespace Tests.Portamical.NUnit.TestDataTypes;

[TestClass]
public class TestCaseTestDataTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    // From factory

    [TestMethod]
    public void From_createsCorrectGenericType()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance);
        Assert.IsInstanceOfType<TestCaseTestData<ITestData>>(result);
    }

    [TestMethod]
    public void From_setsTestCaseName()
    {
        var testData = CreateData("myCaseName");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance);
        Assert.AreEqual(testData.TestCaseName, result.TestCaseName);
    }

    [TestMethod]
    public void From_withMethodName_setsTestNameWithPrefix()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance, "TestMethod");
        Assert.IsNotNull(result.TestName);
        Assert.StartsWith("TestMethod", result.TestName!);
        Assert.Contains("a", result.TestName!);
    }

    [TestMethod]
    public void From_withNullMethodName_testNameEqualsTestCaseName()
    {
        var testData = CreateData("myCaseName");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance, null);
        Assert.AreEqual(testData.TestCaseName, result.TestName);
    }

    [TestMethod]
    public void From_withReturnsData_setsExpectedResult()
    {
        var testData = TestDataFactory.CreateTestDataReturns<int, int>("a", 42, 1);
        var result = TestCaseTestData.From(testData, ArgsCode.Properties);
        Assert.AreEqual(42, result.ExpectedResult);
    }

    [TestMethod]
    public void From_instanceArgsCode_setsArgumentsToTestDataObject()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance);
        Assert.AreSame(testData, result.Arguments![0]);
    }

    [TestMethod]
    public void From_propertiesArgsCode_setsArgumentsFlattened()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.From(testData, ArgsCode.Properties);
        var expectedArgs = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        CollectionAssert.AreEqual(expectedArgs, result.Arguments);
    }

    // TestCaseDataArgsFrom

    [TestMethod]
    public void TestCaseDataArgsFrom_instanceArgsCode_returnsWrappedObject()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.TestCaseDataArgsFrom(testData, ArgsCode.Instance);
        Assert.AreSame(testData, result[0]);
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void TestCaseDataArgsFrom_propertiesArgsCode_returnsFlattened()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.TestCaseDataArgsFrom(testData, ArgsCode.Properties);
        var expected = testData.ToArgs(ArgsCode.Properties, PropsCode.TrimReturnsExpected);
        CollectionAssert.AreEqual(expected, result);
    }

    // GetDisplayName

    [TestMethod]
    public void GetDisplayName_withMethodName_returnsFormattedName()
    {
        var testData = CreateData("myCaseName");
        var testCase = TestCaseTestData.From(testData, ArgsCode.Instance);
        var displayName = testCase.GetDisplayName("TestMethod");
        Assert.AreEqual($"TestMethod(testData: {testCase.TestCaseName})", displayName);
    }

    [TestMethod]
    public void GetDisplayName_withNullMethodName_returnsNull()
    {
        var testData = CreateData("myCaseName");
        var testCase = TestCaseTestData.From(testData, ArgsCode.Instance);
        var displayName = testCase.GetDisplayName(null);
        Assert.IsNull(displayName);
    }

    // Equals

    [TestMethod]
    public void Equals_sameTestCaseName_returnsTrue()
    {
        var testData1 = CreateData("same");
        var testData2 = TestDataFactory.CreateTestData<int>("same", "result", 99);
        var tc1 = TestCaseTestData.From(testData1, ArgsCode.Instance);
        var tc2 = TestCaseTestData.From(testData2, ArgsCode.Instance);
        Assert.IsTrue(tc1.Equals(tc2));
    }

    [TestMethod]
    public void Equals_differentTestCaseName_returnsFalse()
    {
        var tc1 = TestCaseTestData.From(CreateData("a"), ArgsCode.Instance);
        var tc2 = TestCaseTestData.From(CreateData("b"), ArgsCode.Instance);
        Assert.IsFalse(tc1.Equals(tc2));
    }

    [TestMethod]
    public void Equals_null_returnsFalse()
    {
        var tc = TestCaseTestData.From(CreateData("a"), ArgsCode.Instance);
        Assert.IsFalse(tc.Equals((INamedCase?)null));
    }

    [TestMethod]
    public void Equals_object_sameTestCaseName_returnsTrue()
    {
        var testData1 = CreateData("same");
        var testData2 = TestDataFactory.CreateTestData<int>("same", "result", 99);
        var tc1 = TestCaseTestData.From(testData1, ArgsCode.Instance);
        var tc2 = TestCaseTestData.From(testData2, ArgsCode.Instance);
        Assert.IsTrue(tc1.Equals((object)tc2));
    }

    // GetHashCode

    [TestMethod]
    public void GetHashCode_sameTestCaseName_returnsSameHashCode()
    {
        var testData1 = CreateData("same");
        var testData2 = TestDataFactory.CreateTestData<int>("same", "result", 99);
        var tc1 = TestCaseTestData.From(testData1, ArgsCode.Instance);
        var tc2 = TestCaseTestData.From(testData2, ArgsCode.Instance);
        Assert.AreEqual(tc1.GetHashCode(), tc2.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_differentTestCaseName_returnsDifferentHashCode()
    {
        var tc1 = TestCaseTestData.From(CreateData("a"), ArgsCode.Instance);
        var tc2 = TestCaseTestData.From(CreateData("b"), ArgsCode.Instance);
        Assert.AreNotEqual(tc1.GetHashCode(), tc2.GetHashCode());
    }

    // ContainedBy

    [TestMethod]
    public void ContainedBy_containsThisCase_returnsTrue()
    {
        var tc = TestCaseTestData.From(CreateData("a"), ArgsCode.Instance);
        var collection = new[] { tc };
        Assert.IsTrue(tc.ContainedBy(collection));
    }

    [TestMethod]
    public void ContainedBy_doesNotContainThisCase_returnsFalse()
    {
        var tc1 = TestCaseTestData.From(CreateData("a"), ArgsCode.Instance);
        var tc2 = TestCaseTestData.From(CreateData("b"), ArgsCode.Instance);
        Assert.IsFalse(tc1.ContainedBy(new[] { tc2 }));
    }

    [TestMethod]
    public void ContainedBy_nullCollection_returnsFalse()
    {
        var tc = TestCaseTestData.From(CreateData("a"), ArgsCode.Instance);
        Assert.IsFalse(tc.ContainedBy(null));
    }

    // Internal constructor (accessible via InternalsVisibleTo)

    [TestMethod]
    public void InternalConstructor_accessible_createsInstance()
    {
        var testData = CreateData("a");
        var instance = new TestCaseTestData<ITestData>(testData, ArgsCode.Instance, null);
        Assert.IsNotNull(instance);
    }

    // HasFullName property

    [TestMethod]
    public void HasFullNameProperty_withMethodName_isTrue()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance, "TestMethod");
        Assert.IsTrue((bool)result.Properties.Get(TestCaseTestData.HasFullNameProperty)!);
    }

    [TestMethod]
    public void HasFullNameProperty_withoutMethodName_isFalse()
    {
        var testData = CreateData("a");
        var result = TestCaseTestData.From(testData, ArgsCode.Instance, null);
        Assert.IsFalse((bool)result.Properties.Get(TestCaseTestData.HasFullNameProperty)!);
    }

    // SetHasFullNameProperty

    [TestMethod]
    public void SetHasFullNameProperty_withMethodName_setsHasFullNameTrue()
    {
        var testData = CreateData("caseName");
        var tcd = new global::NUnit.Framework.TestCaseData();
        TestCaseTestData.SetHasFullNameProperty(tcd, testData, "TestMethod", out string testName);
        Assert.IsTrue((bool)tcd.Properties.Get(TestCaseTestData.HasFullNameProperty)!);
    }

    [TestMethod]
    public void SetHasFullNameProperty_withMethodName_outputsFormattedTestName()
    {
        var testData = CreateData("caseName");
        var tcd = new global::NUnit.Framework.TestCaseData();
        TestCaseTestData.SetHasFullNameProperty(tcd, testData, "TestMethod", out string testName);
        Assert.StartsWith("TestMethod", testName);
        Assert.Contains("caseName", testName);
    }

    [TestMethod]
    public void SetHasFullNameProperty_withoutMethodName_setsHasFullNameFalse()
    {
        var testData = CreateData("caseName");
        var tcd = new global::NUnit.Framework.TestCaseData();
        TestCaseTestData.SetHasFullNameProperty(tcd, testData, null, out string testName);
        Assert.IsFalse((bool)tcd.Properties.Get(TestCaseTestData.HasFullNameProperty)!);
    }

    [TestMethod]
    public void SetHasFullNameProperty_withoutMethodName_outputsTestCaseName()
    {
        var testData = CreateData("caseName");
        var tcd = new global::NUnit.Framework.TestCaseData();
        TestCaseTestData.SetHasFullNameProperty(tcd, testData, null, out string testName);
        Assert.AreEqual(testData.TestCaseName, testName);
    }

    // GetTypeArgs

    [TestMethod]
    public void GetTypeArgs_instanceArgsCode_returnsNull()
    {
        var testData = CreateData("a");
        var typeArgs = TestCaseTestData.GetTypeArgs(testData, ArgsCode.Instance);
        Assert.IsNull(typeArgs);
    }
}
