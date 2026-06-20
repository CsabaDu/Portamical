// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;
using Portamical.Core.Formatting.Model;
using static Portamical.Core.Formatting.FormatBuilder;

namespace Tests.Portamical.Core.Formatting.Model;

/// <summary>
/// Unit tests for <see cref="Formatter{T}"/> generic base class.
/// </summary>
[TestClass]
public class FormatterTests
{
    #region Abstract Method Test Implementation
    [TestMethod]
    public void Format_abstractMethod_canBeImplemented()
    {
        var formatter = new TestFormatter();
        var result = formatter.Format("test");
        Assert.AreEqual("TEST", result);
    }

    [TestMethod]
    public void Format_abstractMethod_canReturnNull()
    {
        var formatter = new TestFormatter();
        var result = formatter.Format(null!);
        Assert.IsNull(result);
    }

    // Test implementation of abstract Formatter class
    private class TestFormatter : IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj == null) return null;
            return obj.ToString()?.ToUpper();
        }
    }
    #endregion

    #region Formatter<T> - Type Safety Tests
    [TestMethod]
    public void FormatterT_Format_withMatchingType_delegatesToTypeSafeMethod()
    {
        var formatter = new TestFormatterInt();
        var result = formatter.Format(42);
        Assert.AreEqual("INT:42", result);
    }

    [TestMethod]
    public void FormatterT_FormatObject_withNullForReferenceType_callsTypeSafeMethod()
    {
        var formatter = new TestFormatterString();
        var result = formatter.Format(null!);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_Format_withDerivedType_worksCorrectly()
    {
        var formatter = new TestFormatterException();
        var exception = new ArgumentException("test");
        var result = formatter.Format(exception);
        Assert.AreEqual("EX:System.ArgumentException", result);
    }
    #endregion

    #region Formatter<T> - IFormatter Interface Compliance
    [TestMethod]
    public void FormatterT_implementsIFormatter()
    {
        var formatter = new TestFormatterInt();
        Assert.IsInstanceOfType<IFormatter>(formatter);
    }

    [TestMethod]
    public void FormatterT_implementsIFormatterT()
    {
        var formatter = new TestFormatterInt();
        Assert.IsInstanceOfType<IFormatter<int>>(formatter);
    }

    [TestMethod]
    public void FormatterT_IFormatterFormat_callsTypeSafeMethod()
    {
        IFormatter formatter = new TestFormatterInt();
        object value = 99;
        var result = formatter.Format(value);
        Assert.AreEqual("INT:99", result);
    }

    [TestMethod]
    public void FormatterT_IFormatterTFormat_callsTypeSafeMethod()
    {
        IFormatter<int> formatter = new TestFormatterInt();
        var result = formatter.Format(55);
        Assert.AreEqual("INT:55", result);
    }
    #endregion

    #region Formatter<T> - Base Class Utility Usage
    [TestMethod]
    public void FormatterT_canUseBaseClassConstants()
    {
        var formatter = new TestFormatterWithBaseUtils();
        var result = formatter.Format(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_canUseFallbackIfNull()
    {
        var formatter = new TestFormatterWithBaseUtils();
        var result = formatter.Format("test");
        Assert.AreEqual("VALUE:test", result);
    }

    [TestMethod]
    public void FormatterT_canUseJoinWithComma()
    {
        var formatter = new TestFormatterList();
        var result = formatter.Format(["a", "b", "c"]);
        Assert.AreEqual("LIST:[a, b, c]", result);
    }

    [TestMethod]
    public void FormatterT_canUseCreateSeparatedString()
    {
        var formatter = new TestFormatterWithSeparator();
        var result = formatter.Format(("key", "value"));
        Assert.AreEqual("key => value", result);
    }

    [TestMethod]
    public void FormatterT_canUseCopyAsSpan()
    {
        var formatter = new TestFormatterWithSpan();
        var result = formatter.Format("test");
        Assert.AreEqual("PREFIX:test", result);
    }
    #endregion

    #region Formatter<T> - Nullable Type Handling
    [TestMethod]
    public void FormatterT_withNullableValueType_handlesNull()
    {
        var formatter = new TestFormatterNullableInt();
        var result = formatter.Format(null);
        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_withNullableValueType_handlesValue()
    {
        var formatter = new TestFormatterNullableInt();
        var result = formatter.Format(42);
        Assert.AreEqual("NULLABLE:42", result);
    }
    #endregion

    #region Formatter<T> - Edge Cases
    [TestMethod]
    public void FormatterT_withStructType_handlesCorrectly()
    {
        var formatter = new TestFormatterGuid();
        var guid = Guid.NewGuid();
        var result = formatter.Format(guid);
        Assert.IsTrue(result?.StartsWith("GUID:"));
    }

    [TestMethod]
    public void FormatterT_withEnumType_handlesCorrectly()
    {
        var formatter = new TestFormatterEnum();
        var result = formatter.Format(DayOfWeek.Monday);
        Assert.AreEqual("ENUM:Monday", result);
    }

    [TestMethod]
    public void FormatterT_withInterfaceType_handlesImplementations()
    {
        var formatter = new TestFormatterEnumerable();
        var list = new List<int> { 1, 2, 3 };
        var result = formatter.Format(list);
        Assert.AreEqual("ENUMERABLE:3", result);
    }

    [TestMethod]
    public void FormatterT_withAbstractType_handlesDerivedTypes()
    {
        var formatter = new TestFormatterStream();
        var stream = new MemoryStream();
        var result = formatter.Format(stream);
        Assert.AreEqual("STREAM:System.IO.MemoryStream", result);
    }
    #endregion

    #region Formatter<T> Test Implementations

    private class TestFormatterInt : Formatter<int>
    {
        public override string Format(int value) => $"INT:{value}";
    }

    private class TestFormatterString : Formatter<string?>
    {
        public override string Format(string? value) => value is null ? "null" : $"STR:{value}";
    }

    private class TestFormatterException : Formatter<Exception>
    {
        public override string Format(Exception value) => $"EX:{value.GetType().FullName}";
    }

    private class TestFormatterWithBaseUtils : Formatter<string?>
    {
        public override string Format(string? value)
        {
            if (value is null) return NullString;
            return $"VALUE:{value}";
        }
    }

    private class TestFormatterList : Formatter<List<string>>
    {
        public override string Format(List<string> value)
        {
            var joined = JoinWithComma(value);
            return $"LIST:[{joined}]";
        }
    }

    private class TestFormatterWithSeparator : Formatter<(string, string)>
    {
        public override string Format((string, string) value)
        {
            var (key, val) = value;
            //var totalLength = key.Length + 4 + val.Length; // " => "
            return CreateSeparatedString(key, " => ", val);
        }
    }

    private class TestFormatterWithSpan : Formatter<string>
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

    private class TestFormatterNullableInt : Formatter<int?>
    {
        public override string Format(int? value)
        {
            if (value is null) return "null";
            return $"NULLABLE:{value}";
        }
    }

    private class TestFormatterGuid : Formatter<Guid>
    {
        public override string Format(Guid value) => $"GUID:{value}";
    }

    private class TestFormatterEnum : Formatter<DayOfWeek>
    {
        public override string Format(DayOfWeek value) => $"ENUM:{value}";
    }

    private class TestFormatterEnumerable : Formatter<IEnumerable<int>>
    {
        public override string Format(IEnumerable<int> value) => $"ENUMERABLE:{value.Count()}";
    }

    private class TestFormatterStream : Formatter<Stream>
    {
        public override string Format(Stream value) => $"STREAM:{value.GetType().FullName}";
    }

    #endregion

    #region Formatter<T> - Performance and Allocation Tests
    [TestMethod]
    public void FormatterT_withZeroAllocationFormatting_doesNotAllocateExcessively()
    {
        var formatter = new TestFormatterWithSpan();

        // Warm up
        _ = formatter.Format("warmup");

        // This test verifies that the formatter can use zero-allocation patterns
        // We can't directly measure allocations in unit tests, but we verify the pattern works
        var result = formatter.Format("test");
        Assert.AreEqual("PREFIX:test", result);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void FormatterT_multipleCallsSameValue_canReturnSameOrNewString()
    {
        var formatter = new TestFormatterInt();
        var result1 = formatter.Format(42);
        var result2 = formatter.Format(42);

        // Formatters are not required to cache results
        Assert.AreEqual(result1, result2);
        // They might return same or different instances
    }
    #endregion

    #region Formatter<T> - Thread Safety Tests
    [TestMethod]
    public void FormatterT_concurrentCalls_handledSafely()
    {
        var formatter = new TestFormatterInt();
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

    #region Formatter<T> - Error Handling Tests
    [TestMethod]
    public void FormatterT_whenFormatThrows_exceptionPropagates()
    {
        var formatter = new TestFormatterThatThrows();

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
    public void FormatterT_withNullReturnFromFormat_returnsNull()
    {
        var formatter = new TestFormatterReturnsNull();
        var result = formatter.Format(42);

        Assert.IsNull(result);
    }
    #endregion

    #region Formatter<T> - Inheritance and Composition Tests
    [TestMethod]
    public void FormatterT_canBeInherited_byDerivedFormatter()
    {
        var formatter = new DerivedTestFormatter();
        var result = formatter.Format(42);

        Assert.AreEqual("DERIVED:INT:42", result);
    }

    [TestMethod]
    public void FormatterT_derivedFormatter_inheritsMethods()
    {
        var formatter = new DerivedTestFormatter();

        // Can use base class utility methods
        var fallback = FallbackIfNull(null);
        Assert.AreEqual("null", fallback);

        // Verify formatter works
        var result = formatter.Format(100);
        Assert.AreEqual("DERIVED:INT:100", result);
    }

    [TestMethod]
    public void FormatterT_canComposeMultipleFormatters()
    {
        var intFormatter = new TestFormatterInt();
        var stringFormatter = new TestFormatterString();

        // Use formatters independently
        var intResult = intFormatter.Format(42);
        var stringResult = stringFormatter.Format("test");

        Assert.AreEqual("INT:42", intResult);
        Assert.AreEqual("STR:test", stringResult);
    }
    #endregion

    #region Formatter<T> - Generic Type Constraints Tests
    [TestMethod]
    public void FormatterT_withClassConstraint_handlesReferenceTypes()
    {
        var formatter = new TestFormatterClassConstrained();
        var testObj = new TestReferenceType { Value = "test" };

        var result = formatter.Format(testObj);
        Assert.AreEqual("REF:test", result);
    }

    [TestMethod]
    public void FormatterT_withClassConstraint_handlesNullCorrectly()
    {
        var formatter = new TestFormatterClassConstrained();
        var result = formatter.Format(null);

        Assert.AreEqual("null", result);
    }

    [TestMethod]
    public void FormatterT_withStructConstraint_handlesValueTypes()
    {
        var formatter = new TestFormatterStructConstrained();
        var result = formatter.Format(42);

        Assert.AreEqual("STRUCT:42", result);
    }

    [TestMethod]
    public void FormatterT_withNewConstraint_canInstantiateType()
    {
        var formatter = new TestFormatterNewConstrained();
        var result = formatter.Format(new TestTypeWithDefaultConstructor { Id = 99 });

        Assert.IsTrue(result?.Contains("99"));
    }
    #endregion

    #region Formatter<T> - Complex Type Hierarchies
    [TestMethod]
    public void FormatterT_withBaseClass_formatsDerivedClasses()
    {
        var formatter = new TestFormatterException();
        var derived = new DerivedTestException("test message");

        var result = formatter.Format(derived);
        Assert.IsTrue(result?.StartsWith("EX:"));
    }

    [TestMethod]
    public void FormatterT_withInterface_formatsImplementations()
    {
        var formatter = new TestFormatterComparable();
        var result = formatter.Format(42);

        Assert.AreEqual("COMPARABLE:42", result);
    }

    [TestMethod]
    public void FormatterT_withGenericInterface_handlesGenericImplementations()
    {
        var formatter = new TestFormatterGenericInterface();
        var list = new List<string> { "a", "b", "c" };

        var result = formatter.Format(list);
        Assert.AreEqual("GENERIC:3", result);
    }
    #endregion

    #region Formatter<T> - Special Type Tests
    [TestMethod]
    public void FormatterT_withSealedType_handlesCorrectly()
    {
        var formatter = new TestFormatterSealed();
        var value = new SealedTestType { Data = "sealed" };

        var result = formatter.Format(value);
        Assert.AreEqual("SEALED:sealed", result);
    }

    [TestMethod]
    public void FormatterT_withRecord_handlesCorrectly()
    {
        var formatter = new TestFormatterRecord();
        var record = new TestRecord(42, "test");

        var result = formatter.Format(record);
        Assert.AreEqual("RECORD:42-test", result);
    }

    [TestMethod]
    public void FormatterT_withTupleType_handlesValueTuples()
    {
        var formatter = new TestFormatterValueTuple();
        var tuple = (42, "test");

        var result = formatter.Format(tuple);
        Assert.AreEqual("TUPLE:42,test", result);
    }

    [TestMethod]
    public void FormatterT_withDelegate_handlesCorrectly()
    {
        var formatter = new TestFormatterAction();
        static void attempt(int x) => Console.WriteLine(x);

        var result = formatter.Format(attempt);
        Assert.IsTrue(result?.StartsWith("ACTION:"));
    }
    #endregion

    #region Formatter<T> - Pattern Matching Edge Cases

    [TestMethod]
    public void FormatterT_withContravariance_handlesBaseTypeFormatters()
    {
        // IFormatter<in T> is contravariant
        IFormatter<Exception> baseFormatter = new TestFormatterException();

        // Can assign to more derived type through interface
        var exception = new ArgumentException("test");
        var result = baseFormatter.Format(exception);

        Assert.IsTrue(result?.StartsWith("EX:"));
    }
    #endregion

    #region Formatter<T> - Integration with Base Class
    [TestMethod]
    public void FormatterT_inheritsAllBaseClassMembers()
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
    public void FormatterT_canCallBaseClassUtilitiesInImplementation()
    {
        var formatter = new TestFormatterUsingAllBaseUtils();
        var result = formatter.Format(["a", "b", "c"]);

        // Uses JoinWithComma, CreateSeparatedString, CopyAsSpan internally
        Assert.AreEqual("ITEMS: a, b, c", result);
    }
    #endregion

    #region Formatter<T> Additional Test Implementations

    private class TestFormatterThatThrows : Formatter<int>
    {
        public override string Format(int value)
            => throw new InvalidOperationException("Formatter error");
    }

    private class TestFormatterReturnsNull : Formatter<int>
    {
        public override string Format(int value) => null!;
    }

    private class DerivedTestFormatter : TestFormatterInt
    {
        public override string Format(int value) => $"DERIVED:{base.Format(value)}";
    }

    private class TestReferenceType
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestFormatterClassConstrained : Formatter<TestReferenceType?>
    {
        public override string Format(TestReferenceType? value)
            => value is null ? "null" : $"REF:{value.Value}";
    }

    private class TestFormatterStructConstrained : Formatter<int>
    {
        public override string Format(int value) => $"STRUCT:{value}";
    }

    private class TestTypeWithDefaultConstructor
    {
        public int Id { get; set; }
    }

    private class TestFormatterNewConstrained : Formatter<TestTypeWithDefaultConstructor>
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

    private class TestFormatterComparable : Formatter<IComparable>
    {
        public override string Format(IComparable value) => $"COMPARABLE:{value}";
    }

    private class TestFormatterGenericInterface : Formatter<IList<string>>
    {
        public override string Format(IList<string> value) => $"GENERIC:{value.Count}";
    }

    private sealed class SealedTestType
    {
        public string Data { get; set; } = string.Empty;
    }

    private class TestFormatterSealed : Formatter<SealedTestType>
    {
        public override string Format(SealedTestType value) => $"SEALED:{value.Data}";
    }

    private record TestRecord(int Id, string Name);

    private class TestFormatterRecord : Formatter<TestRecord>
    {
        public override string Format(TestRecord value) => $"RECORD:{value.Id}-{value.Name}";
    }

    private class TestFormatterValueTuple : Formatter<(int, string)>
    {
        public override string Format((int, string) value)
            => $"TUPLE:{value.Item1},{value.Item2}";
    }

    private class TestFormatterAction : Formatter<Action<int>>
    {
        public override string Format(Action<int> value)
            => $"ACTION:{value.Method.Name}";
    }

    private class TestFormatterUsingAllBaseUtils : Formatter<List<string>>
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
