// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Analysis;
using Portamical.Companion.Core;
using Portamical.Companion.Emit;

namespace Tests.Portamical.Companion.Emit;

[TestClass]
public class AritySelectorTests
{
    private static TestCaseSpec Spec(ResultKind kind, int argCount) => new()
    {
        Definition = "def",
        Kind = kind,
        ExpectedTypeName = kind == ResultKind.Throws ? "ArgumentNullException" : "int",
        ExpectedValueLiteral = "5",
        ExpectedDisplay = "5",
        Args = [.. Enumerable.Range(1, argCount).Select(i => new ArgSpec($"a{i}", "int", i.ToString()))],
    };

    [TestMethod]
    public void Select_returnsKind_selectsTestDataReturns()
    {
        var selection = AritySelector.Select(Spec(ResultKind.Returns, 2));

        Assert.AreEqual("TestDataReturns", selection.TestDataTypeName);
        Assert.AreEqual("CreateTestDataReturns", selection.FactoryMethodName);
        Assert.AreEqual("TestDataReturns<int, int, int>", selection.ConstructedTypeName);
    }

    [TestMethod]
    public void Select_throwsKind_selectsTestDataThrows()
    {
        var selection = AritySelector.Select(Spec(ResultKind.Throws, 1));

        Assert.AreEqual("TestDataThrows<ArgumentNullException, int>", selection.ConstructedTypeName);
    }

    [TestMethod]
    public void Select_customKind_selectsTestDataWithoutExpectedType()
    {
        var selection = AritySelector.Select(Spec(ResultKind.Custom, 3));

        Assert.AreEqual("TestData<int, int, int>", selection.ConstructedTypeName);
    }

    [TestMethod]
    public void Select_maxArity_succeeds()
    {
        var selection = AritySelector.Select(Spec(ResultKind.Returns, AritySelector.MaxArity));

        Assert.HasCount(AritySelector.MaxArity + 1, selection.GenericTypeArguments);
    }

    [TestMethod]
    public void Select_zeroArgs_throwsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => AritySelector.Select(Spec(ResultKind.Returns, 0)));
    }

    [TestMethod]
    public void Select_tooManyArgs_throwsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => AritySelector.Select(Spec(ResultKind.Returns, AritySelector.MaxArity + 1)));
    }

    [TestMethod]
    public void Select_returnsWithoutExpectedType_throwsArgumentException()
    {
        var spec = Spec(ResultKind.Returns, 1) with { ExpectedTypeName = null };

        Assert.ThrowsExactly<ArgumentException>(() => AritySelector.Select(spec));
    }
}

[TestClass]
public class TestDataEmitterTests
{
    private static TestCaseSpec AddSpec => new()
    {
        Definition = "Adding two positives",
        Kind = ResultKind.Returns,
        ExpectedTypeName = "int",
        ExpectedValueLiteral = "5",
        ExpectedDisplay = "5",
        Args = [new ArgSpec("a", "int", "2"), new ArgSpec("b", "int", "3")],
        TargetMethod = "Add",
    };

    [TestMethod]
    public void EmitFactoryCall_returnsSpec_emitsFactoryInvocation()
    {
        string code = TestDataEmitter.EmitFactoryCall(AddSpec);

        StringAssert.Contains(code, "TestDataFactory.CreateTestDataReturns<int, int, int>(");
        StringAssert.Contains(code, "\"Adding two positives\"");
        StringAssert.Contains(code, "2, 3)");
    }

    [TestMethod]
    public void EmitFactoryCall_customSpec_emitsResultString()
    {
        var spec = new TestCaseSpec
        {
            Definition = "Process data",
            Kind = ResultKind.Custom,
            ExpectedDisplay = "succeeds with warnings",
            Args = [new ArgSpec("input", "string", "\"x\"")],
        };

        string code = TestDataEmitter.EmitFactoryCall(spec);

        StringAssert.Contains(code, "TestDataFactory.CreateTestData<string>(");
        StringAssert.Contains(code, "\"succeeds with warnings\"");
    }

    [TestMethod]
    public void EmitTestDataArray_mixedFamilies_throwsArgumentException()
    {
        var throwsSpec = new TestCaseSpec
        {
            Definition = "Null input",
            Kind = ResultKind.Throws,
            ExpectedTypeName = "ArgumentNullException",
            ExpectedValueLiteral = "new ArgumentNullException()",
            Args = [new ArgSpec("a", "int", "1"), new ArgSpec("b", "int", "2")],
        };

        Assert.ThrowsExactly<ArgumentException>(
            () => TestDataEmitter.EmitTestDataArray([AddSpec, throwsSpec], "cases"));
    }

    [TestMethod]
    public void EmitTestDataArray_emptySpecs_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TestDataEmitter.EmitTestDataArray([], "cases"));
    }

    [TestMethod]
    public void EmitTestDataArray_emittedCode_compilesAgainstPortamicalCore()
    {
        string array = TestDataEmitter.EmitTestDataArray([AddSpec], "cases");
        string dataSource = TestMethodEmitter.EmitDataSource("cases", "TestCases");

        string source = $$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using Portamical.Core.Factories;
            using Portamical.Core.Strategy;
            using Portamical.Core.TestDataTypes.Models.Specialized;

            namespace Generated;

            public static class Wrapper
            {
            {{array}}

            {{dataSource}}
            }
            """;

        string coreDll = typeof(global::Portamical.Core.Strategy.ArgsCode).Assembly.Location;

        var result = CompileChecker.Check([source], [coreDll]);

        Assert.IsTrue(result.Success, string.Join('\n', result.Errors));
    }
}

[TestClass]
public class TestMethodEmitterTests
{
    private static TestCaseSpec AddSpec => new()
    {
        Definition = "Adding two positives",
        Kind = ResultKind.Returns,
        ExpectedTypeName = "int",
        ExpectedValueLiteral = "5",
        ExpectedDisplay = "5",
        Args = [new ArgSpec("a", "int", "2"), new ArgSpec("b", "int", "3")],
        TargetMethod = "Add",
    };

    [TestMethod]
    public void EmitDataSource_emitsToArgsProjection()
    {
        string code = TestMethodEmitter.EmitDataSource("cases", "TestCases");

        StringAssert.Contains(code, "public static IEnumerable<object?[]> TestCases");
        StringAssert.Contains(code, "cases.Select(td => td.ToArgs(ArgsCode.Instance))");
    }

    [TestMethod]
    public void EmitTestMethod_xunit_usesTheoryAndMemberData()
    {
        string code = TestMethodEmitter.EmitTestMethod(AddSpec, "TestCases", TestFramework.XUnitV3);

        StringAssert.Contains(code, "[Theory]");
        StringAssert.Contains(code, "[MemberData(nameof(TestCases))]");
        StringAssert.Contains(code, "Assert.Equal(testData.Expected, actual);");
        StringAssert.Contains(code, "Add(testData.Arg1, testData.Arg2)");
    }

    [TestMethod]
    public void EmitTestMethod_mstest_usesDynamicData()
    {
        string code = TestMethodEmitter.EmitTestMethod(AddSpec, "TestCases", TestFramework.MSTest);

        StringAssert.Contains(code, "[TestMethod]");
        StringAssert.Contains(code, "[DynamicData(nameof(TestCases))]");
        StringAssert.Contains(code, "Assert.AreEqual(testData.Expected, actual);");
    }

    [TestMethod]
    public void EmitTestMethod_throwsSpec_emitsThrowsAssert()
    {
        var spec = AddSpec with
        {
            Kind = ResultKind.Throws,
            ExpectedTypeName = "DivideByZeroException",
            ExpectedValueLiteral = "new DivideByZeroException()",
        };

        string code = TestMethodEmitter.EmitTestMethod(spec, "TestCases", TestFramework.XUnitV3);

        StringAssert.Contains(code, "Assert.Throws<DivideByZeroException>");
    }
}

[TestClass]
public class FormatterEmitterTests
{
    [TestMethod]
    public void EmitFormatter_emitsFormatterSubclassAndRegistrationHint()
    {
        string code = FormatterEmitter.EmitFormatter("ProductId", "$\"PROD-{value.Id:D6}\"");

        StringAssert.Contains(code, "class ProductIdFormatter : Formatter<ProductId>");
        StringAssert.Contains(code, "$\"PROD-{value.Id:D6}\"");
        StringAssert.Contains(code, "RegisterFormatter<ProductId>");
    }

    [TestMethod]
    public void EmitFormatter_noExpression_emitsTodoBody()
    {
        StringAssert.Contains(FormatterEmitter.EmitFormatter("Money"), "TODO");
    }
}

[TestClass]
public class TestClassEmitterTests
{
    private static TestCaseSpec AddSpec => new()
    {
        Definition = "Adding two positives",
        Kind = ResultKind.Returns,
        ExpectedTypeName = "int",
        ExpectedValueLiteral = "5",
        ExpectedDisplay = "5",
        Args = [new ArgSpec("a", "int", "2"), new ArgSpec("b", "int", "3")],
        TargetMethod = "Add",
    };

    [TestMethod]
    public void EmitTestClass_composesFieldDataSourceAndMethod()
    {
        string source = TestClassEmitter.EmitTestClass(
            [AddSpec], TestFramework.XUnitV3, "Tests.Generated", "CalculatorTests");

        StringAssert.Contains(source, "namespace Tests.Generated;");
        StringAssert.Contains(source, "public class CalculatorTests");
        StringAssert.Contains(source, "private static readonly TestDataReturns<int, int, int>[] testCases");
        StringAssert.Contains(source, "[Theory]");
    }

    [TestMethod]
    public void EmitTestClass_deduplicatesSpecs()
    {
        string source = TestClassEmitter.EmitTestClass(
            [AddSpec, AddSpec], TestFramework.XUnitV3, "Tests.Generated", "CalculatorTests");

        int occurrences = source.Split("\"Adding two positives\"").Length - 1;

        Assert.AreEqual(1, occurrences);
    }
}
