// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Text;
using Portamical.Companion.Core;

namespace Portamical.Companion.Emit;

/// <summary>
/// Composes a complete, compilable test class file from a set of specs:
/// usings, namespace, TestData array field, data source member, and test method skeleton.
/// </summary>
public static class TestClassEmitter
{
    /// <summary>
    /// Emits a full test class source file for the given specs (which must share
    /// one TestData family and arity).
    /// </summary>
    public static string EmitTestClass(
        IReadOnlyList<TestCaseSpec> specs,
        TestFramework framework,
        string namespaceName,
        string className,
        string? testMethodName = null)
    {
        ArgumentNullException.ThrowIfNull(specs);
        ArgumentException.ThrowIfNullOrWhiteSpace(namespaceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

        if (specs.Count == 0)
        {
            throw new ArgumentException("At least one spec is required.", nameof(specs));
        }

        var distinct = SpecSet.Distinct(specs);
        string fieldName = "testCases";
        string dataSourceMember = "TestCases";

        var builder = new StringBuilder();

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using Portamical.Core.Factories;");
        builder.AppendLine("using Portamical.Core.Strategy;");
        builder.AppendLine("using Portamical.Core.TestDataTypes.Models.General;");
        builder.AppendLine("using Portamical.Core.TestDataTypes.Models.Specialized;");
        builder.AppendLine(GetFrameworkUsing(framework));
        builder.AppendLine();
        builder.Append("namespace ").Append(namespaceName).AppendLine(";");
        builder.AppendLine();
        builder.Append(GetClassAttribute(framework));
        builder.Append("public class ").AppendLine(className);
        builder.AppendLine("{");
        builder.AppendLine(TestDataEmitter.EmitTestDataArray(distinct, fieldName));
        builder.AppendLine();
        builder.AppendLine(TestMethodEmitter.EmitDataSource(fieldName, dataSourceMember));
        builder.AppendLine();
        builder.AppendLine(TestMethodEmitter.EmitTestMethod(
            distinct[0], dataSourceMember, framework, testMethodName));
        builder.AppendLine("}");

        return builder.ToString();
    }

    private static string GetFrameworkUsing(TestFramework framework)
    => framework switch
    {
        TestFramework.XUnitV3 => "using Xunit;",
        TestFramework.MSTest => "using Microsoft.VisualStudio.TestTools.UnitTesting;",
        _ => "using NUnit.Framework;",
    };

    private static string GetClassAttribute(TestFramework framework)
    => framework switch
    {
        TestFramework.MSTest => "[TestClass]" + Environment.NewLine,
        TestFramework.NUnit => "[TestFixture]" + Environment.NewLine,
        _ => string.Empty,
    };
}
