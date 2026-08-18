// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Analysis;

namespace Tests.Portamical.Companion.Analysis;

[TestClass]
public class TargetAnalyzerTests
{
    private const string Source = """
        public class Calculator
        {
            public int Add(int a, int b) => a + b;

            public int Divide(int a, int b)
            {
                if (b == 0)
                {
                    throw new DivideByZeroException("b must not be zero");
                }

                return a / b;
            }

            private void Helper() { }
        }
        """;

    [TestMethod]
    public void Analyze_existingMethod_returnsSignature()
    {
        var targets = TargetAnalyzer.Analyze(Source, "Add");

        Assert.HasCount(1, targets);
        Assert.AreEqual("Add", targets[0].MethodName);
        Assert.AreEqual("Calculator", targets[0].ContainingType);
        Assert.AreEqual("int", targets[0].ReturnTypeName);
        Assert.HasCount(2, targets[0].Parameters);
        Assert.AreEqual(("a", "int"), targets[0].Parameters[0]);
    }

    [TestMethod]
    public void Analyze_methodWithThrow_detectsExceptionType()
    {
        var targets = TargetAnalyzer.Analyze(Source, "Divide");

        Assert.HasCount(1, targets);
        Assert.Contains("DivideByZeroException", targets[0].ThrownExceptionTypes);
    }

    [TestMethod]
    public void Analyze_missingMethod_returnsEmpty()
    {
        Assert.IsEmpty(TargetAnalyzer.Analyze(Source, "Subtract"));
    }

    [TestMethod]
    public void AnalyzeAll_returnsOnlyPublicMethods()
    {
        var targets = TargetAnalyzer.AnalyzeAll(Source);

        Assert.HasCount(2, targets);
        Assert.IsFalse(targets.Any(t => t.MethodName == "Helper"));
    }

    [TestMethod]
    public void Analyze_emptySource_throwsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => TargetAnalyzer.Analyze("", "Add"));
    }
}

[TestClass]
public class TestScannerTests
{
    private const string TestSource = """
        public class SomeTests
        {
            private static readonly string[] names =
            [
                "Adding positives => returns 5",
                "Null input => throws ArgumentNullException",
                "not a test case name",
                "Adding positives => returns 5",
            ];
        }
        """;

    [TestMethod]
    public void ExtractTestCaseNames_findsDistinctSeparatorLiterals()
    {
        var names = TestScanner.ExtractTestCaseNames(TestSource);

        Assert.HasCount(2, names);
        Assert.Contains("Adding positives => returns 5", names);
        Assert.Contains("Null input => throws ArgumentNullException", names);
    }

    [TestMethod]
    public void ExtractTestCaseNames_multipleSources_mergesDistinct()
    {
        var names = TestScanner.ExtractTestCaseNames([TestSource, TestSource]);

        Assert.HasCount(2, names);
    }

    [TestMethod]
    public void ExtractTestCaseNames_noLiterals_returnsEmpty()
    {
        Assert.IsEmpty(TestScanner.ExtractTestCaseNames("public class Empty { }"));
    }
}

[TestClass]
public class CompileCheckerTests
{
    [TestMethod]
    public void Check_validSource_succeeds()
    {
        var result = CompileChecker.Check(["public class Valid { public int X => 42; }"]);

        Assert.IsTrue(result.Success, string.Join('\n', result.Errors));
    }

    [TestMethod]
    public void Check_invalidSource_reportsErrors()
    {
        var result = CompileChecker.Check(["public class Broken { public UnknownType X; }"]);

        Assert.IsFalse(result.Success);
        Assert.IsNotEmpty(result.Errors);
    }
}
