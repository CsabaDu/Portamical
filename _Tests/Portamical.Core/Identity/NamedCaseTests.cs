// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Reflection;
using Portamical.Core.Identity;
using Portamical.Core.Identity.Model;

namespace Tests.Portamical.Core.Identity;

[TestClass]
public sealed class NamedCaseTests
{
    private sealed class TestableNamedCase(string testCaseName) : NamedCase
    {
        public override string TestCaseName { get; init; } = testCaseName;
    }

    private static TestableNamedCase Case(string name) => new(name);

    // -------------------------------------------------------------------------
    // Comparer.Equals
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Comparer_Equals_sameInstance_returnsTrue()
    {
        var instance = Case("test => passes");

        Assert.IsTrue(NamedCase.Comparer.Equals(instance, instance));
    }

    [TestMethod]
    public void Comparer_Equals_differentInstanceSameName_returnsTrue()
    {
        var x = Case("test => passes");
        var y = Case("test => passes");

        Assert.IsTrue(NamedCase.Comparer.Equals(x, y));
    }

    [TestMethod]
    public void Comparer_Equals_differentName_returnsFalse()
    {
        var x = Case("test1 => passes");
        var y = Case("test2 => passes");

        Assert.IsFalse(NamedCase.Comparer.Equals(x, y));
    }

    [TestMethod]
    public void Comparer_Equals_caseSensitive_returnsFalse()
    {
        var x = Case("Test => passes");
        var y = Case("test => passes");

        Assert.IsFalse(NamedCase.Comparer.Equals(x, y));
    }

    [TestMethod]
    public void Comparer_Equals_bothNull_returnsTrue()
    {
        Assert.IsTrue(NamedCase.Comparer.Equals(null, null));
    }

    [TestMethod]
    public void Comparer_Equals_xIsNull_returnsFalse()
    {
        var y = Case("test => passes");

        Assert.IsFalse(NamedCase.Comparer.Equals(null, y));
    }

    [TestMethod]
    public void Comparer_Equals_yIsNull_returnsFalse()
    {
        var x = Case("test => passes");

        Assert.IsFalse(NamedCase.Comparer.Equals(x, null));
    }

    // -------------------------------------------------------------------------
    // Comparer.GetHashCode
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Comparer_GetHashCode_sameName_returnsSameHash()
    {
        var x = Case("test => passes");
        var y = Case("test => passes");

        Assert.AreEqual(
            NamedCase.Comparer.GetHashCode(x),
            NamedCase.Comparer.GetHashCode(y));
    }

    [TestMethod]
    public void Comparer_GetHashCode_nullObj_throwsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => NamedCase.Comparer.GetHashCode(null!));
    }

    // -------------------------------------------------------------------------
    // ContainedBy
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ContainedBy_collectionContainsInstance_returnsTrue()
    {
        var subject = Case("test => passes");
        IEnumerable<INamedCase> collection = [Case("other => fails"), Case("test => passes")];

        Assert.IsTrue(subject.ContainedBy(collection));
    }

    [TestMethod]
    public void ContainedBy_collectionDoesNotContainInstance_returnsFalse()
    {
        var subject = Case("test => passes");
        IEnumerable<INamedCase> collection = [Case("other => fails")];

        Assert.IsFalse(subject.ContainedBy(collection));
    }

    [TestMethod]
    public void ContainedBy_nullCollection_returnsFalse()
    {
        var subject = Case("test => passes");

        Assert.IsFalse(subject.ContainedBy(null));
    }

    [TestMethod]
    public void ContainedBy_emptyCollection_returnsFalse()
    {
        var subject = Case("test => passes");

        Assert.IsFalse(subject.ContainedBy([]));
    }

    // -------------------------------------------------------------------------
    // GetDisplayName
    // -------------------------------------------------------------------------

    [TestMethod]
    public void GetDisplayName_withMethodName_returnsFormattedDisplayName()
    {
        var namedCase = Case("scenario => outcome");

        var result = namedCase.GetDisplayName("MyTestMethod");

        Assert.AreEqual("MyTestMethod(testData: scenario => outcome)", result);
    }

    [TestMethod]
    public void GetDisplayName_withNullMethodName_returnsNull()
    {
        var namedCase = Case("scenario => outcome");

        Assert.IsNull(namedCase.GetDisplayName(null));
    }

    [TestMethod]
    public void GetDisplayName_withEmptyMethodName_returnsNull()
    {
        var namedCase = Case("scenario => outcome");

        Assert.IsNull(namedCase.GetDisplayName(string.Empty));
    }

    // -------------------------------------------------------------------------
    // Equals(INamedCase?)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Equals_INamedCase_sameName_returnsTrue()
    {
        var x = Case("test => passes");
        var y = Case("test => passes");

        Assert.IsTrue(x.Equals((INamedCase)y));
    }

    [TestMethod]
    public void Equals_INamedCase_differentName_returnsFalse()
    {
        var x = Case("test1 => passes");
        var y = Case("test2 => passes");

        Assert.IsFalse(x.Equals((INamedCase)y));
    }

    [TestMethod]
    public void Equals_INamedCase_null_returnsFalse()
    {
        var x = Case("test => passes");

        Assert.IsFalse(x.Equals((INamedCase?)null));
    }

    // -------------------------------------------------------------------------
    // Equals(object?)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Equals_object_INamedCaseWithSameName_returnsTrue()
    {
        var x = Case("test => passes");
        object y = Case("test => passes");

        Assert.IsTrue(x.Equals(y));
    }

    [TestMethod]
    public void Equals_object_nonINamedCaseType_returnsFalse()
    {
        var x = Case("test => passes");

        Assert.IsFalse(x.Equals("test => passes"));
    }

    [TestMethod]
    public void Equals_object_null_returnsFalse()
    {
        var x = Case("test => passes");

        Assert.IsFalse(x.Equals((object?)null));
    }

    // -------------------------------------------------------------------------
    // GetHashCode
    // -------------------------------------------------------------------------

    [TestMethod]
    public void GetHashCode_sameNameInstances_returnsSameHashCode()
    {
        var x = Case("test => passes");
        var y = Case("test => passes");

        Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
    }

    // -------------------------------------------------------------------------
    // ToString
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ToString_returnsTestCaseName()
    {
        const string name = "scenario => outcome";
        var namedCase = Case(name);

        Assert.AreEqual(name, namedCase.ToString());
    }

    // -------------------------------------------------------------------------
    // implicit operator string?
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ImplicitConversion_nonNullInstance_returnsTestCaseName()
    {
        const string name = "scenario => outcome";
        TestableNamedCase namedCase = Case(name);

        string? result = namedCase;

        Assert.AreEqual(name, result);
    }

    [TestMethod]
    public void ImplicitConversion_nullInstance_returnsNull()
    {
        TestableNamedCase? namedCase = null;

        string? result = namedCase;

        Assert.IsNull(result);
    }

    // -------------------------------------------------------------------------
    // CreateDisplayName(string?, params object?[]?)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CreateDisplayName_string_validMethodNameAndCaseName_returnsFormattedName()
    {
        var result = NamedCase.CreateDisplayName("MyMethod", "scenario => outcome");

        Assert.AreEqual("MyMethod(testData: scenario => outcome)", result);
    }

    [TestMethod]
    public void CreateDisplayName_string_nullMethodName_returnsNull()
    {
        var result = NamedCase.CreateDisplayName((string?)null, "scenario => outcome");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_string_emptyMethodName_returnsNull()
    {
        var result = NamedCase.CreateDisplayName(string.Empty, "scenario => outcome");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_string_nullArgs_returnsNull()
    {
        var result = NamedCase.CreateDisplayName("MyMethod", (object?[]?)null);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_string_emptyArgs_returnsNull()
    {
        var result = NamedCase.CreateDisplayName("MyMethod", []);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_string_nullFirstArg_returnsNull()
    {
        var result = NamedCase.CreateDisplayName("MyMethod", (object?)null);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_string_emptyFirstArg_returnsNull()
    {
        var result = NamedCase.CreateDisplayName("MyMethod", string.Empty);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_string_INamedCaseFirstArg_returnsFormattedName()
    {
        INamedCase namedCase = Case("scenario => outcome");

        var result = NamedCase.CreateDisplayName("MyMethod", namedCase);

        Assert.AreEqual("MyMethod(testData: scenario => outcome)", result);
    }

    // -------------------------------------------------------------------------
    // CreateDisplayName(MethodInfo?, params object?[]?)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void CreateDisplayName_methodInfo_stringFirstArg_returnsFormattedName()
    {
        MethodInfo method = typeof(NamedCaseTests).GetMethod(
            nameof(CreateDisplayName_methodInfo_stringFirstArg_returnsFormattedName))!;

        var result = NamedCase.CreateDisplayName(method, "scenario => outcome");

        Assert.AreEqual(
            $"{method.Name}(testData: scenario => outcome)",
            result);
    }

    [TestMethod]
    public void CreateDisplayName_methodInfo_INamedCaseFirstArg_returnsFormattedName()
    {
        MethodInfo method = typeof(NamedCaseTests).GetMethod(
            nameof(CreateDisplayName_methodInfo_INamedCaseFirstArg_returnsFormattedName))!;
        INamedCase namedCase = Case("scenario => outcome");

        var result = NamedCase.CreateDisplayName(method, namedCase);

        Assert.AreEqual(
            $"{method.Name}(testData: scenario => outcome)",
            result);
    }

    [TestMethod]
    public void CreateDisplayName_methodInfo_nonStringFirstArg_returnsNull()
    {
        MethodInfo method = typeof(NamedCaseTests).GetMethod(
            nameof(CreateDisplayName_methodInfo_nonStringFirstArg_returnsNull))!;

        var result = NamedCase.CreateDisplayName(method, 42);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_methodInfo_nullArgs_returnsNull()
    {
        MethodInfo method = typeof(NamedCaseTests).GetMethod(
            nameof(CreateDisplayName_methodInfo_nullArgs_returnsNull))!;

        var result = NamedCase.CreateDisplayName(method, (object?[]?)null);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void CreateDisplayName_methodInfo_nullMethod_returnsNull()
    {
        MethodInfo? nullMethod = null;
        var result = NamedCase.CreateDisplayName(nullMethod, "scenario => outcome");

        Assert.IsNull(result);
    }

    // -------------------------------------------------------------------------
    // Contains(INamedCase, IEnumerable<INamedCase>?)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Contains_nullCollection_returnsFalse()
    {
        var namedCase = Case("test => passes");

        Assert.IsFalse(NamedCase.Contains(namedCase, null));
    }

    [TestMethod]
    public void Contains_collectionContainsMatchingCase_returnsTrue()
    {
        var namedCase = Case("test => passes");
        INamedCase[] collection = [Case("other => fails"), Case("test => passes")];

        Assert.IsTrue(NamedCase.Contains(namedCase, collection));
    }

    [TestMethod]
    public void Contains_collectionDoesNotContainMatchingCase_returnsFalse()
    {
        var namedCase = Case("test => passes");
        INamedCase[] collection = [Case("other => fails")];

        Assert.IsFalse(NamedCase.Contains(namedCase, collection));
    }

    [TestMethod]
    public void Contains_nonArrayEnumerable_worksCorrectly()
    {
        var namedCase = Case("test => passes");
        IEnumerable<INamedCase> collection =
        [
            Case("other => fails"),
            Case("test => passes"),
        ];

        Assert.IsTrue(NamedCase.Contains(namedCase, collection));
    }
}
