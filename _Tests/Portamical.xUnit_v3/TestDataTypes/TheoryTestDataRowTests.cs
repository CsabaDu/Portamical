// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes;
using Portamical.xUnit_v3.TestDataTypes.Model;

namespace Tests.Portamical.xUnit_v3.TestDataTypes;

[TestClass]
public class TheoryTestDataRowTests
{
    private static ITestData CreateData(string def, int arg = 1)
        => TestDataFactory.CreateTestData<int>(def, "result", arg);

    [TestMethod]
    public void Constructor_setsTestCaseName()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        Assert.AreEqual(testData.TestCaseName, row.TestCaseName);
    }

    [TestMethod]
    public void Constructor_withMethodName_setsFormattedDisplayName()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, "TestMethod");
        Assert.Contains("TestMethod", row.TestDisplayName!);
        Assert.Contains(testData.TestCaseName, row.TestDisplayName!);
    }

    [TestMethod]
    public void Constructor_withoutMethodName_displayNameIsNull()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        Assert.IsNull(row.TestDisplayName);
    }

    [TestMethod]
    public void Constructor_instanceArgsCode_getDataReturnsWrappedObject()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        var data = ((Xunit.ITheoryDataRow)row).GetData();
        Assert.HasCount(1, data);
        Assert.AreSame(testData, data[0]);
    }

    [TestMethod]
    public void Constructor_propertiesArgsCode_getDataReturnsFlattenedArgs()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Properties, null);
        var data = ((Xunit.ITheoryDataRow)row).GetData();
        var sut_expected = testData.ToArgs(ArgsCode.Properties);
        CollectionAssert.AreEqual(sut_expected, data);
    }

    [TestMethod]
    public void CopyConstructor_copiesTestCaseName()
    {
        var testData = CreateData("a");
        var original = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        var copy = new TheoryTestDataRow(original, null);
        Assert.AreEqual(original.TestCaseName, copy.TestCaseName);
    }

    [TestMethod]
    public void CopyConstructor_withMethodName_overridesDisplayName()
    {
        var testData = CreateData("a");
        var original = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        var copy = new TheoryTestDataRow(original, "NewMethod");
        Assert.Contains("NewMethod", copy.TestDisplayName!);
        Assert.Contains(testData.TestCaseName, copy.TestDisplayName!);
    }

    [TestMethod]
    public void CopyConstructor_withNullMethodName_preservesOriginalDisplayName()
    {
        var testData = CreateData("a");
        var original = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, "OriginalMethod");
        var copy = new TheoryTestDataRow(original, null);
        Assert.AreEqual(original.TestDisplayName, copy.TestDisplayName);
    }

    [TestMethod]
    public void CopyConstructor_preservesGetData()
    {
        var testData = CreateData("a");
        var original = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        var copy = new TheoryTestDataRow(original, null);
        var originalData = ((Xunit.ITheoryDataRow)original).GetData();
        var copyData = ((Xunit.ITheoryDataRow)copy).GetData();
        CollectionAssert.AreEqual(originalData, copyData);
    }

    [TestMethod]
    public void Equals_sameTestCaseName_returnsTrue()
    {
        var testData1 = CreateData("same");
        var testData2 = CreateData("same");
        var row1 = new TheoryTestDataRow<ITestData>(testData1, ArgsCode.Instance, null);
        var row2 = new TheoryTestDataRow<ITestData>(testData2, ArgsCode.Instance, null);
        Assert.IsTrue(row1.Equals(row2));
    }

    [TestMethod]
    public void Equals_differentTestCaseName_returnsFalse()
    {
        var row1 = new TheoryTestDataRow<ITestData>(CreateData("a"), ArgsCode.Instance, null);
        var row2 = new TheoryTestDataRow<ITestData>(CreateData("b"), ArgsCode.Instance, null);
        Assert.IsFalse(row1.Equals(row2));
    }

    [TestMethod]
    public void Equals_null_returnsFalse()
    {
        var row = new TheoryTestDataRow<ITestData>(CreateData("a"), ArgsCode.Instance, null);
        Assert.IsFalse(row.Equals(null));
    }

    [TestMethod]
    public void GetHashCode_sameTestCaseName_returnsSameHash()
    {
        var testData1 = CreateData("same");
        var testData2 = CreateData("same");
        var row1 = new TheoryTestDataRow<ITestData>(testData1, ArgsCode.Instance, null);
        var row2 = new TheoryTestDataRow<ITestData>(testData2, ArgsCode.Instance, null);
        Assert.AreEqual(row1.GetHashCode(), row2.GetHashCode());
    }

    [TestMethod]
    public void GenericVariant_isInstanceOfNonGeneric()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        Assert.IsInstanceOfType<TheoryTestDataRow>(row);
    }

    [TestMethod]
    public void GenericVariant_implementsITheoryTestDataRow()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        Assert.IsInstanceOfType<ITheoryTestDataRow>(row);
    }

    [TestMethod]
    public void GenericVariant_implementsITheoryDataRow()
    {
        var testData = CreateData("a");
        var row = new TheoryTestDataRow<ITestData>(testData, ArgsCode.Instance, null);
        Assert.IsInstanceOfType<Xunit.ITheoryDataRow>(row);
    }
}
