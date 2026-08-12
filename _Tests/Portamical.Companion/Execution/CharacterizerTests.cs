// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Execution;

namespace Tests.Portamical.Companion.Execution;

/// <summary>Target used to exercise characterization against this test assembly.</summary>
public class CharacterizationTarget
{
    public static int Add(int a, int b) => a + b;

    public static int Divide(int a, int b)
    => b == 0 ? throw new DivideByZeroException("b must not be zero") : a / b;

    public string Echo(string input) => input;
}

[TestClass]
public class CharacterizerTests
{
    private static string TestAssemblyPath
    => typeof(CharacterizerTests).Assembly.Location;

    [TestMethod]
    public void Characterize_staticMethod_capturesReturnValue()
    {
        using var characterizer = new Characterizer(TestAssemblyPath);

        var result = characterizer.Characterize("CharacterizationTarget", "Add", ["2", "3"]);

        Assert.IsTrue(result.Succeeded);
        Assert.IsFalse(result.Threw);
        Assert.AreEqual("5", result.ReturnValue);
    }

    [TestMethod]
    public void Characterize_throwingInvocation_capturesExceptionType()
    {
        using var characterizer = new Characterizer(TestAssemblyPath);

        var result = characterizer.Characterize("CharacterizationTarget", "Divide", ["1", "0"]);

        Assert.IsTrue(result.Succeeded);
        Assert.IsTrue(result.Threw);
        Assert.AreEqual("DivideByZeroException", result.ExceptionTypeName);
        Assert.AreEqual("b must not be zero", result.ExceptionMessage);
    }

    [TestMethod]
    public void Characterize_instanceMethod_invokesOnDefaultInstance()
    {
        using var characterizer = new Characterizer(TestAssemblyPath);

        var result = characterizer.Characterize("CharacterizationTarget", "Echo", ["hello"]);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("hello", result.ReturnValue);
    }

    [TestMethod]
    public void Characterize_unknownType_throwsArgumentException()
    {
        using var characterizer = new Characterizer(TestAssemblyPath);

        Assert.ThrowsExactly<ArgumentException>(
            () => characterizer.Characterize("NoSuchType", "Add", []));
    }

    [TestMethod]
    public void Characterize_unknownMethod_throwsArgumentException()
    {
        using var characterizer = new Characterizer(TestAssemblyPath);

        Assert.ThrowsExactly<ArgumentException>(
            () => characterizer.Characterize("CharacterizationTarget", "NoSuchMethod", []));
    }

    [TestMethod]
    public void Characterize_afterDispose_throwsObjectDisposedException()
    {
        var characterizer = new Characterizer(TestAssemblyPath);
        characterizer.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => characterizer.Characterize("CharacterizationTarget", "Add", ["1", "2"]));
    }
}
