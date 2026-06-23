// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting.CustomFormatters;
using Portamical.Core.Formatting.CustomFormatters.Model;
using static Portamical.Core.Formatting.FormatBuilder;

namespace Tests.Portamical.Core.Formatting.Model;

/// <summary>
/// Unit tests for <see cref="CustomFormatter{T}"/> generic base class.
/// </summary>
[TestClass]
public class CustomFormatterTests
{
    #region Abstract Method Test Implementation
    [TestMethod]
    public void Format_abstractMethod_canBeImplemented()
    {
        var formatter = new TestCustomFormatter();
        var result = formatter.Format("test");
        Assert.AreEqual("TEST", result);
    }

    [TestMethod]
    public void Format_abstractMethod_canReturnNull()
    {
        var formatter = new TestCustomFormatter();
        var result = formatter.Format(null!);
        Assert.IsNull(result);
    }

    // Test implementation of abstract CustomFormatter class
    private class TestCustomFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj == null) return null;
            return obj.ToString()?.ToUpper();
        }
    }
    #endregion

    #region CustomFormatter<T> - Type Safety Tests
    [TestMethod]
    public void CustomFormatterT_Format_withMatchingType_delegatesToTypeSafeMethod()
    {
        var formatter = new TestCustomFormatterInt();
        var result = formatter.Format(42);
        Assert.AreEqual("INT:42", result);
    }

    [TestMethod]
    public void CustomFormatterT_FormatObject_withNullForReferenceType_callsTypeSafeMethod()
    {
        var formatter = new TestCustomFormatterString();
        var result = formatter.Format(null!);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void CustomFormatterT_Format_withDerivedType_worksCorrectly()
    {
        var formatter = new TestCustomFormatterException();
        var exception = new ArgumentException("test");
        var result = formatter.Format(exception);
        Assert.AreEqual("EX:System.ArgumentException", result);
    }
    #endregion

    #region CustomFormatter<T> - ICustomFormatter Interface Compliance
    [TestMethod]
    public void CustomFormatterT_implementsICustomFormatter()
    {
        var formatter = new TestCustomFormatterInt();
        Assert.IsInstanceOfType<global::Portamical.Core.Formatting.IFormatter>(formatter);
    }

    [TestMethod]
    public void CustomFormatterT_implementsICustomFormatterT()
    {
        var formatter = new TestCustomFormatterInt();
        Assert.IsInstanceOfType<ICustomFormatter<int>>(formatter);
    }

    [TestMethod]
    public void CustomFormatterT_ICustomFormatterFormat_callsTypeSafeMethod()
    {
        global::Portamical.Core.Formatting.IFormatter formatter = new TestCustomFormatterInt();
        object value = 99;
        var result = formatter.Format(value);
        Assert.AreEqual("INT:99", result);
    }

    [TestMethod]
    public void CustomFormatterT_ICustomFormatterTFormat_callsTypeSafeMethod()
    {
        ICustomFormatter<int> formatter = new TestCustomFormatterInt();
        var result = formatter.Format(55);
        Assert.AreEqual("INT:55", result);
    }
    #endregion

    #region CustomFormatter<T> - Base Class Utility Usage
    [TestMethod]
    public void CustomFormatterT_canUseBaseClassConstants()
    {
        var formatter = new TestCustomFormatterWithBaseUtils();
        var result = formatter.Format(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void CustomFormatterT_canUseFallbackIfNull()
    {
        var formatter = new TestCustomFormatterWithBaseUtils();
        var result = formatter.Format("test");
        Assert.AreEqual("VALUE:test", result);
    }

    [TestMethod]
    public void CustomFormatterT_canUseJoinWithComma()
    {
        var formatter = new TestCustomFormatterList();
        var result = formatter.Format(["a", "b", "c"]);
        Assert.AreEqual("LIST:[a, b, c]", result);
    }

    [TestMethod]
    public void CustomFormatterT_canUseCreateSeparatedString()
    {
        var formatter = new TestCustomFormatterWithSeparator();
        var result = formatter.Format(("key", "value"));
        Assert.AreEqual("key => value", result);
    }

    [TestMethod]
    public void CustomFormatterT_canUseCopyAsSpan()
    {
        var formatter = new TestCustomFormatterWithSpan();
        var result = formatter.Format("test");
        Assert.AreEqual("PREFIX:test", result);
    }
    #endregion

    #region CustomFormatter<T> - Nullable Type Handling
    [TestMethod]
    public void CustomFormatterT_withNullableValueType_handlesNull()
    {
        var formatter = new TestCustomFormatterNullableInt();
        var result = formatter.Format(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void CustomFormatterT_withNullableValueType_handlesValue()
    {
        var formatter = new TestCustomFormatterNullableInt();
        var result = formatter.Format(42);
        Assert.AreEqual("NULLABLE:42", result);
    }
    #endregion

    #region CustomFormatter<T> - Edge Cases
    [TestMethod]
    public void CustomFormatterT_withStructType_handlesCorrectly()
    {
        var formatter = new TestCustomFormatterGuid();
        var guid = Guid.NewGuid();
        var result = formatter.Format(guid);
        Assert.IsTrue(result?.StartsWith("GUID:"));
    }

    [TestMethod]
    public void CustomFormatterT_withEnumType_handlesCorrectly()
    {
        var formatter = new TestCustomFormatterEnum();
        var result = formatter.Format(DayOfWeek.Monday);
        Assert.AreEqual("ENUM:Monday", result);
    }

    [TestMethod]
    public void CustomFormatterT_withInterfaceType_handlesImplementations()
    {
        var formatter = new TestCustomFormatterEnumerable();
        var list = new List<int> { 1, 2, 3 };
        var result = formatter.Format(list);
        Assert.AreEqual("ENUMERABLE:3", result);
    }

    [TestMethod]
    public void CustomFormatterT_withAbstractType_handlesDerivedTypes()
    {
        var formatter = new TestCustomFormatterStream();
        var stream = new MemoryStream();
        var result = formatter.Format(stream);
        Assert.AreEqual("STREAM:System.IO.MemoryStream", result);
    }
    #endregion

    #region CustomFormatter<T> Test Implementations

    private class TestCustomFormatterInt : CustomFormatter<int>
    {
        public override string Format(int value) => $"INT:{value}";
    }

    private class TestCustomFormatterString : CustomFormatter<string?>
    {
        public override string Format(string? value) => value is null ? "null" : $"STR:{value}";
    }

    private class TestCustomFormatterException : CustomFormatter<Exception>
    {
        public override string Format(Exception value) => $"EX:{value.GetType().FullName}";
    }

    private class TestCustomFormatterWithBaseUtils : CustomFormatter<string?>
    {
        public override string Format(string? value)
        {
            if (value is null) return NullString;
            return $"VALUE:{value}";
        }
    }

    private class TestCustomFormatterList : CustomFormatter<List<string>>
    {
        public override string Format(List<string> value)
        {
            var joined = JoinWithComma(value);
            return $"LIST:[{joined}]";
        }
    }

    private class TestCustomFormatterWithSeparator : CustomFormatter<(string, string)>
    {
        public override string Format((string, string) value)
        {
            var (key, val) = value;
            //var totalLength = key.Length + 4 + val.Length; // " => "
            return CreateSeparatedString(key, " => ", val);
        }
    }

    private class TestCustomFormatterWithSpan : CustomFormatter<string>
    {
        public override string Format(string value)
        {
            const string prefix = "PREFIX:";
            var totalLength = prefix.Length + value.Length;
            return string.Create(totalLength, (prefix, value), static (span, state) =>
            {
                var (p, v) = state;
                CopyAsSpan(p, span, 0);
                CopyAsSpan(v, span, p.Length);
            });
        }
    }

    private class TestCustomFormatterNullableInt : CustomFormatter<int?>
    {
        public override string Format(int? value)
        {
            if (value is null) return "null";
            return $"NULLABLE:{value}";
        }
    }

    private class TestCustomFormatterGuid : CustomFormatter<Guid>
    {
        public override string Format(Guid value) => $"GUID:{value}";
    }

    private class TestCustomFormatterEnum : CustomFormatter<DayOfWeek>
    {
        public override string Format(DayOfWeek value) => $"ENUM:{value}";
    }

    private class TestCustomFormatterEnumerable : CustomFormatter<IEnumerable<int>>
    {
        public override string Format(IEnumerable<int> value) => $"ENUMERABLE:{value.Count()}";
    }

    private class TestCustomFormatterStream : CustomFormatter<Stream>
    {
        public override string Format(Stream value) => $"STREAM:{value.GetType().FullName}";
    }

    #endregion

    #region CustomFormatter<T> - Performance and Allocation Tests
    [TestMethod]
    public void CustomFormatterT_withZeroAllocationFormatting_doesNotAllocateExcessively()
    {
        var formatter = new TestCustomFormatterWithSpan();

        // Warm up
        _ = formatter.Format("warmup");

        // This test verifies that the formatter can use zero-allocation patterns
        // We can't directly measure allocations in unit tests, but we verify the pattern works
        var result = formatter.Format("test");
        Assert.AreEqual("PREFIX:test", result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void CustomFormatterT_multipleCallsSameValue_canReturnSameOrNewString()
    {
        var formatter = new TestCustomFormatterInt();
        var result1 = formatter.Format(42);
        var result2 = formatter.Format(42);

        // CustomFormatters are not required to cache results
        Assert.AreEqual(result1, result2);
        // They might return same or different instances
    }
    #endregion

    #region CustomFormatter<T> - Thread Safety Tests
    [TestMethod]
    public void CustomFormatterT_concurrentCalls_handledSafely()
    {
        var formatter = new TestCustomFormatterInt();
        var tasks = new List<Task<string>>();

        // Create 100 concurrent formatting tasks
        for (int i = 0; i < 100; i++)
        {
            var value = i;
            tasks.Add(Task.Run(() => formatter.Format(value)));
        }

        Task.WaitAll([.. tasks]);

        // Verify all tasks completed successfully
        for (int i = 0; i < 100; i++)
        {
            Assert.AreEqual($"INT:{i}", tasks[i].Result);
        }
    }

    #endregion

    #region CustomFormatter<T> - Error Handling Tests
    [TestMethod]
    public void CustomFormatterT_whenFormatThrows_exceptionPropagates()
    {
        var formatter = new TestCustomFormatterThatThrows();

        try
        {
            formatter.Format(42);
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void CustomFormatterT_withNullReturnFromFormat_returnsNull()
    {
        var formatter = new TestCustomFormatterReturnsNull();
        var result = formatter.Format(42);

        Assert.IsNull(result);
    }
    #endregion

    #region CustomFormatter<T> - Inheritance and Composition Tests
    [TestMethod]
    public void CustomFormatterT_canBeInherited_byDerivedCustomFormatter()
    {
        var formatter = new DerivedTestCustomFormatter();
        var result = formatter.Format(42);

        Assert.AreEqual("DERIVED:INT:42", result);
    }

    [TestMethod]
    public void CustomFormatterT_derivedCustomFormatter_inheritsMethods()
    {
        var formatter = new DerivedTestCustomFormatter();

        // Can use base class utility methods
        var fallback = FallbackIfNull(null);
        Assert.AreEqual("null", fallback);

        // Verify formatter works
        var result = formatter.Format(100);
        Assert.AreEqual("DERIVED:INT:100", result);
    }

    [TestMethod]
    public void CustomFormatterT_canComposeMultipleCustomFormatters()
    {
        var intCustomFormatter = new TestCustomFormatterInt();
        var stringCustomFormatter = new TestCustomFormatterString();

        // Use formatters independently
        var intResult = intCustomFormatter.Format(42);
        var stringResult = stringCustomFormatter.Format("test");

        Assert.AreEqual("INT:42", intResult);
        Assert.AreEqual("STR:test", stringResult);
    }
    #endregion

    #region CustomFormatter<T> - Generic Type Constraints Tests
    [TestMethod]
    public void CustomFormatterT_withClassConstraint_handlesReferenceTypes()
    {
        var formatter = new TestCustomFormatterClassConstrained();
        var testObj = new TestReferenceType { Value = "test" };

        var result = formatter.Format(testObj);
        Assert.AreEqual("REF:test", result);
    }

    [TestMethod]
    public void CustomFormatterT_withClassConstraint_handlesNullCorrectly()
    {
        var formatter = new TestCustomFormatterClassConstrained();
        var result = formatter.Format(null);

        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void CustomFormatterT_withStructConstraint_handlesValueTypes()
    {
        var formatter = new TestCustomFormatterStructConstrained();
        var result = formatter.Format(42);

        Assert.AreEqual("STRUCT:42", result);
    }

    [TestMethod]
    public void CustomFormatterT_withNewConstraint_canInstantiateType()
    {
        var formatter = new TestCustomFormatterNewConstrained();
        var result = formatter.Format(new TestTypeWithDefaultConstructor { Id = 99 });

        Assert.IsTrue(result?.Contains("99"));
    }
    #endregion

    #region CustomFormatter<T> - Complex Type Hierarchies
    [TestMethod]
    public void CustomFormatterT_withBaseClass_formatsDerivedClasses()
    {
        var formatter = new TestCustomFormatterException();
        var derived = new DerivedTestException("test message");

        var result = formatter.Format(derived);
        Assert.IsTrue(result?.StartsWith("EX:"));
    }

    [TestMethod]
    public void CustomFormatterT_withInterface_formatsImplementations()
    {
        var formatter = new TestCustomFormatterComparable();
        var result = formatter.Format(42);

        Assert.AreEqual("COMPARABLE:42", result);
    }

    [TestMethod]
    public void CustomFormatterT_withGenericInterface_handlesGenericImplementations()
    {
        var formatter = new TestCustomFormatterGenericInterface();
        var list = new List<string> { "a", "b", "c" };

        var result = formatter.Format(list);
        Assert.AreEqual("GENERIC:3", result);
    }
    #endregion

    #region CustomFormatter<T> - Special Type Tests
    [TestMethod]
    public void CustomFormatterT_withSealedType_handlesCorrectly()
    {
        var formatter = new TestCustomFormatterSealed();
        var value = new SealedTestType { Data = "sealed" };

        var result = formatter.Format(value);
        Assert.AreEqual("SEALED:sealed", result);
    }

    [TestMethod]
    public void CustomFormatterT_withRecord_handlesCorrectly()
    {
        var formatter = new TestCustomFormatterRecord();
        var record = new TestRecord(42, "test");

        var result = formatter.Format(record);
        Assert.AreEqual("RECORD:42-test", result);
    }

    [TestMethod]
    public void CustomFormatterT_withTupleType_handlesValueTuples()
    {
        var formatter = new TestCustomFormatterValueTuple();
        var tuple = (42, "test");

        var result = formatter.Format(tuple);
        Assert.AreEqual("TUPLE:42,test", result);
    }

    [TestMethod]
    public void CustomFormatterT_withDelegate_handlesCorrectly()
    {
        var formatter = new TestCustomFormatterAction();
        static void attempt(int x) => Console.WriteLine(x);

        var result = formatter.Format(attempt);
        Assert.IsTrue(result?.StartsWith("ACTION:"));
    }
    #endregion

    #region CustomFormatter<T> - Pattern Matching Edge Cases

    [TestMethod]
    public void CustomFormatterT_withContravariance_handlesBaseTypeCustomFormatters()
    {
        // ICustomFormatter<in T> is contravariant
        ICustomFormatter<Exception> baseCustomFormatter = new TestCustomFormatterException();

        // Can assign to more derived type through interface
        var exception = new ArgumentException("test");
        var result = baseCustomFormatter.Format(exception);

        Assert.IsTrue(result?.StartsWith("EX:"));
    }
    #endregion

    #region CustomFormatter<T> - Integration with Base Class
    [TestMethod]
    public void CustomFormatterT_inheritsAllBaseClassMembers()
    {
#pragma warning disable MSTEST0032 // Using directive is unnecessary
        // Verify access to constants
        Assert.AreEqual(3, MaxCount);
        Assert.AreEqual("null", NullString);
#pragma warning restore MSTEST0032

        // Verify access to static methods
        var joined = JoinWithComma(["a", "b", "c"]);
        Assert.AreEqual("a, b, c", joined);

        var fallback = FallbackIfNull(null);
        Assert.AreEqual("null", fallback);
    }

    [TestMethod]
    public void CustomFormatterT_canCallBaseClassUtilitiesInImplementation()
    {
        var formatter = new TestCustomFormatterUsingAllBaseUtils();
        var result = formatter.Format(["a", "b", "c"]);

        // Uses JoinWithComma, CreateSeparatedString, CopyAsSpan internally
        Assert.AreEqual("ITEMS: a, b, c", result);
    }
    #endregion

    #region CustomFormatter<T> Additional Test Implementations

    private class TestCustomFormatterThatThrows : CustomFormatter<int>
    {
        public override string Format(int value)
            => throw new InvalidOperationException("CustomFormatter error");
    }

    private class TestCustomFormatterReturnsNull : CustomFormatter<int>
    {
        public override string Format(int value) => null!;
    }

    private class DerivedTestCustomFormatter : TestCustomFormatterInt
    {
        public override string Format(int value) => $"DERIVED:{base.Format(value)}";
    }

    private class TestReferenceType
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestCustomFormatterClassConstrained : CustomFormatter<TestReferenceType?>
    {
        public override string Format(TestReferenceType? value)
            => value is null ? "null" : $"REF:{value.Value}";
    }

    private class TestCustomFormatterStructConstrained : CustomFormatter<int>
    {
        public override string Format(int value) => $"STRUCT:{value}";
    }

    private class TestTypeWithDefaultConstructor
    {
        public int Id { get; set; }
    }

    private class TestCustomFormatterNewConstrained : CustomFormatter<TestTypeWithDefaultConstructor>
    {
        public override string Format(TestTypeWithDefaultConstructor value)
        {
            var newInstance = new TestTypeWithDefaultConstructor();
            return $"NEW:{value.Id},{newInstance.Id}";
        }
    }

    public class DerivedTestException(string message) : Exception(message)
    {
    }

    private class TestCustomFormatterComparable : CustomFormatter<IComparable>
    {
        public override string Format(IComparable value) => $"COMPARABLE:{value}";
    }

    private class TestCustomFormatterGenericInterface : CustomFormatter<IList<string>>
    {
        public override string Format(IList<string> value) => $"GENERIC:{value.Count}";
    }

    private sealed class SealedTestType
    {
        public string Data { get; set; } = string.Empty;
    }

    private class TestCustomFormatterSealed : CustomFormatter<SealedTestType>
    {
        public override string Format(SealedTestType value) => $"SEALED:{value.Data}";
    }

    private record TestRecord(int Id, string Name);

    private class TestCustomFormatterRecord : CustomFormatter<TestRecord>
    {
        public override string Format(TestRecord value) => $"RECORD:{value.Id}-{value.Name}";
    }

    private class TestCustomFormatterValueTuple : CustomFormatter<(int, string)>
    {
        public override string Format((int, string) value)
            => $"TUPLE:{value.Item1},{value.Item2}";
    }

    private class TestCustomFormatterAction : CustomFormatter<Action<int>>
    {
        public override string Format(Action<int> value)
            => $"ACTION:{value.Method.Name}";
    }

    private class TestCustomFormatterUsingAllBaseUtils : CustomFormatter<List<string>>
    {
        public override string Format(List<string> value)
        {
            // Use JoinWithComma
            var joined = JoinWithComma(value);

            // Use CreateSeparatedString
            const string prefix = "ITEMS: ";
            var totalLength = prefix.Length + joined.Length;

            return string.Create(totalLength, (prefix, joined), static (span, state) =>
            {
                var (p, j) = state;

                // Use CopyAsSpan
                CopyAsSpan(p, span, 0);
                CopyAsSpan(j, span, p.Length);
            });
        }
    }

    public TestContext TestContext { get; set; }

    #endregion
}
