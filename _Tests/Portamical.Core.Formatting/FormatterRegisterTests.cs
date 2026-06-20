// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;

namespace Tests.Portamical.Core.Formatting;

/// <summary>
/// Unit tests for <see cref="FormatterRegister"/> custom formatter registration and management.
/// </summary>
[TestClass]
[DoNotParallelize] // Registry is a shared static resource; tests must run sequentially
public class FormatterRegisterTests
{
    [TestCleanup]
    public void Cleanup()
    {
        // Ensure registry is clean after each test to prevent test interference
        FormatterRegister.ClearFormatters();
    }

    #region RegisterFormatter - Basic Operations
#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void RegisterFormatter_withValidTypeAndFormatter_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        try
        {
            // Act
            var result = FormatterRegister.RegisterFormatter(typeof(CustomType), formatter);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(FormatterRegister.IsFormatterRegistered<CustomType>());
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }
#pragma warning restore CA2263

    [TestMethod]
    public void RegisterFormatter_generic_withValidFormatter_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        try
        {
            // Act
            var result = FormatterRegister.RegisterFormatter<CustomType>(formatter);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(FormatterRegister.IsFormatterRegistered<CustomType>());
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void RegisterFormatter_withAlreadyRegisteredType_returnsFalse()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new CustomTypeFormatter();

        try
        {
            FormatterRegister.RegisterFormatter<CustomType>(formatter1);

            // Act - Try to register again
            var result = FormatterRegister.RegisterFormatter<CustomType>(formatter2);

            // Assert
            Assert.IsFalse(result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void RegisterFormatter_withNullType_throwsArgumentNullException()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormatterRegister.RegisterFormatter(null!, formatter));
    }

    [TestMethod]
    public void RegisterFormatter_withNullFormatter_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormatterRegister.RegisterFormatter(typeof(CustomType), null!));
    }

    [TestMethod]
    public void RegisterFormatter_generic_withNullFormatter_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormatterRegister.RegisterFormatter<CustomType>(null!));
    }
    #endregion

    #region UnregisterFormatter - Basic Operations
#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void UnregisterFormatter_withRegisteredType_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        FormatterRegister.RegisterFormatter(typeof(CustomType), formatter);

        // Act
        var result = FormatterRegister.UnregisterFormatter<CustomType>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(FormatterRegister.IsFormatterRegistered<CustomType>());
    }
#pragma warning restore CA2263

    [TestMethod]
    public void UnregisterFormatter_generic_withRegisteredType_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(formatter);

        // Act
        var result = FormatterRegister.UnregisterFormatter<CustomType>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(FormatterRegister.IsFormatterRegistered<CustomType>());
    }

    [TestMethod]
    public void UnregisterFormatter_withUnregisteredType_returnsFalse()
    {
        // Act
        var result = FormatterRegister.UnregisterFormatter<CustomType>();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void UnregisterFormatter_withNullType_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormatterRegister.UnregisterFormatter(null!));
    }
    #endregion

    #region IsFormatterRegistered - Query Operations
#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void IsFormatterRegistered_withRegisteredType_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        try
        {
            FormatterRegister.RegisterFormatter(typeof(CustomType), formatter);

            // Act
            var result = FormatterRegister.IsFormatterRegistered<CustomType>();

            // Assert
            Assert.IsTrue(result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }
#pragma warning restore CA2263

    [TestMethod]
    public void IsFormatterRegistered_generic_withRegisteredType_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        try
        {
            FormatterRegister.RegisterFormatter<CustomType>(formatter);

            // Act
            var result = FormatterRegister.IsFormatterRegistered<CustomType>();

            // Assert
            Assert.IsTrue(result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void IsFormatterRegistered_withUnregisteredType_returnsFalse()
    {
        // Act
        var result = FormatterRegister.IsFormatterRegistered(typeof(CustomType));

        // Assert
        Assert.IsFalse(result);
    }
#pragma warning restore CA2263

    [TestMethod]
    public void IsFormatterRegistered_generic_withUnregisteredType_returnsFalse()
    {
        // Act
        var result = FormatterRegister.IsFormatterRegistered<CustomType>();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsFormatterRegistered_withNullType_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormatterRegister.IsFormatterRegistered(null!));
    }
    #endregion

    #region ClearFormatters - Registry Management
    [TestMethod]
    public void ClearFormatters_removesAllRegisteredFormatters()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new AnotherTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(formatter1);
        FormatterRegister.RegisterFormatter<AnotherCustomType>(formatter2);

        // Act
        FormatterRegister.ClearFormatters();

        // Assert
        Assert.IsFalse(FormatterRegister.IsFormatterRegistered<CustomType>());
        Assert.IsFalse(FormatterRegister.IsFormatterRegistered<AnotherCustomType>());
        Assert.AreEqual(0, FormatterRegister.RegisteredFormatterCount);
    }

    [TestMethod]
    public void ClearFormatters_whenRegistryEmpty_doesNotThrow()
    {
        // Arrange
        FormatterRegister.ClearFormatters();

        // Act & Assert - Should not throw
        FormatterRegister.ClearFormatters();
        Assert.AreEqual(0, FormatterRegister.RegisteredFormatterCount);
    }
    #endregion

    #region RegisteredFormatterCount - Registry State
    [TestMethod]
    public void RegisteredFormatterCount_withOneFormatter_returnsOne()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        try
        {
            FormatterRegister.RegisterFormatter<CustomType>(formatter);

            // Act
            var count = FormatterRegister.RegisteredFormatterCount;

            // Assert
            Assert.AreEqual(1, count);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void RegisteredFormatterCount_withMultipleFormatters_returnsCorrectCount()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new AnotherTypeFormatter();
        var formatter3 = new CustomStringFormatter();

        try
        {
            FormatterRegister.RegisterFormatter<CustomType>(formatter1);
            FormatterRegister.RegisterFormatter<AnotherCustomType>(formatter2);
            FormatterRegister.RegisterFormatter<string>(formatter3);

            // Act
            var count = FormatterRegister.RegisteredFormatterCount;

            // Assert
            Assert.AreEqual(3, count);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
            FormatterRegister.UnregisterFormatter<AnotherCustomType>();
            FormatterRegister.UnregisterFormatter<string>();
        }
    }

    [TestMethod]
    public void RegisteredFormatterCount_afterUnregister_decrements()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new AnotherTypeFormatter();

        try
        {
            FormatterRegister.RegisterFormatter<CustomType>(formatter1);
            FormatterRegister.RegisterFormatter<AnotherCustomType>(formatter2);
            var initialCount = FormatterRegister.RegisteredFormatterCount;

            // Act
            FormatterRegister.UnregisterFormatter<CustomType>();
            var finalCount = FormatterRegister.RegisteredFormatterCount;

            // Assert
            Assert.AreEqual(2, initialCount);
            Assert.AreEqual(1, finalCount);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<AnotherCustomType>();
        }
    }
    #endregion

    #region Registry - Internal State Access
    [TestMethod]
    public void Registry_returnsNonNullDictionary()
    {
        // Act
        var registry = FormatterRegister.Registry;

        // Assert
        Assert.IsNotNull(registry);
    }

    [TestMethod]
    public void Registry_afterRegistration_containsRegisteredType()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        try
        {
            FormatterRegister.RegisterFormatter<CustomType>(formatter);

            // Act
            var registry = FormatterRegister.Registry;

            // Assert
            Assert.IsTrue(registry.ContainsKey(typeof(CustomType)));
            Assert.AreSame(formatter, registry[typeof(CustomType)]);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Registry_afterUnregistration_doesNotContainType()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(formatter);
        FormatterRegister.UnregisterFormatter<CustomType>();

        // Act
        var registry = FormatterRegister.Registry;

        // Assert
        Assert.IsFalse(registry.ContainsKey(typeof(CustomType)));
    }

    [TestMethod]
    public void Registry_afterClear_isEmpty()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new AnotherTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(formatter1);
        FormatterRegister.RegisterFormatter<AnotherCustomType>(formatter2);
        FormatterRegister.ClearFormatters();

        // Act
        var registry = FormatterRegister.Registry;

        // Assert
        Assert.IsEmpty(registry);
    }
    #endregion

    #region Thread Safety Tests
    [TestMethod]
    public void FormatterRegistration_threadSafety_handlesMultipleThreads()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        var tasks = new List<Task<bool>>();
        var threadCount = 10;

        try
        {
            // Act - Multiple threads trying to register the same type
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Run(() =>
                    FormatterRegister.RegisterFormatter<CustomType>(formatter)));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert - Only one should succeed
            var successCount = tasks.Count(t => t.Result);
            Assert.AreEqual(1, successCount);
            Assert.AreEqual(1, FormatterRegister.RegisteredFormatterCount);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }
    #endregion

    #region Reregistration Tests
    [TestMethod]
    public void FormatterUnregistration_afterRegistration_allowsReregistration()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new CustomTypeFormatter();

        try
        {
            // Act - Register, unregister, then register again
            var firstReg = FormatterRegister.RegisterFormatter<CustomType>(formatter1);
            var unreg = FormatterRegister.UnregisterFormatter<CustomType>();
            var secondReg = FormatterRegister.RegisterFormatter<CustomType>(formatter2);

            // Assert
            Assert.IsTrue(firstReg);
            Assert.IsTrue(unreg);
            Assert.IsTrue(secondReg);
            Assert.IsTrue(FormatterRegister.IsFormatterRegistered<CustomType>());
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }
    #endregion

    #region GetFormatter - Formatter Retrieval
#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void GetFormatter_withRegisteredType_returnsCustomFormatter()
    {
        // Arrange
        var customFormatter = new CustomTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(customFormatter);

        try
        {
            // Act
            var formatter = FormatterRegister.GetFormatter(typeof(CustomType));

            // Assert
            Assert.IsNotNull(formatter);
            Assert.IsInstanceOfType<IFormatter>(formatter);

            // Verify it uses the custom formatter
            var obj = new CustomType(42);
            var result = formatter.Format(obj);
            Assert.AreEqual("Custom:42", result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }
#pragma warning restore CA2263

    [TestMethod]
    public void GetFormatter_generic_withRegisteredType_returnsCustomFormatter()
    {
        // Arrange
        var customFormatter = new CustomTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(customFormatter);

        try
        {
            // Act
            var formatter = FormatterRegister.GetFormatter<CustomType>();

            // Assert
            Assert.IsNotNull(formatter);

            // Verify it uses the custom formatter
            var obj = new CustomType(42);
            var result = formatter.Format(obj);
            Assert.AreEqual("Custom:42", result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void GetFormatter_withUnregisteredType_returnsDefaultFormatter()
    {
        // Act
        var formatter = FormatterRegister.GetFormatter(typeof(CustomType));

        // Assert
        Assert.IsNotNull(formatter);
        Assert.AreSame(DefaultFormatter.Instance, formatter);
    }
#pragma warning restore CA2263

    [TestMethod]
    public void GetFormatter_generic_withUnregisteredType_returnsDefaultFormatter()
    {
        // Act
        var formatter = FormatterRegister.GetFormatter<CustomType>();

        // Assert
        Assert.IsNotNull(formatter);
        Assert.AreSame(DefaultFormatter.Instance, formatter);
    }

    [TestMethod]
    public void GetFormatter_withNullType_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormatterRegister.GetFormatter(null!));
    }
    #endregion

    #region Format - Convenience Formatting
    [TestMethod]
    public void Format_withRegisteredFormatter_usesCustomFormatter()
    {
        // Arrange
        var customFormatter = new CustomTypeFormatter();
        FormatterRegister.RegisterFormatter<CustomType>(customFormatter);

        try
        {
            var obj = new CustomType(99);

            // Act
            var result = FormatterRegister.Format(obj);

            // Assert
            Assert.AreEqual("Custom:99", result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Format_withUnregisteredType_usesDefaultFormatter()
    {
        // Arrange
        var obj = new CustomType(42);

        // Act
        var result = FormatterRegister.Format(obj);

        // Assert - Should use default ToString()
        Assert.AreEqual("CustomType:42", result);
    }

    [TestMethod]
    public void Format_withNull_returnsNull()
    {
        // Act
        var obj = (CustomType)null!;
        var result = FormatterRegister.Format(obj);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_withPrimitiveType_usesDefaultFormatter()
    {
        // Act
        var result = FormatterRegister.Format(42);

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Format_withString_usesDefaultFormatter()
    {
        // Act
        var result = FormatterRegister.Format("test");

        // Assert - DefaultFormatter quotes strings
        Assert.AreEqual("\"test\"", result);
    }

    [TestMethod]
    public void Format_withRegisteredStringFormatter_overridesDefault()
    {
        // Arrange
        var stringFormatter = new CustomStringFormatter();
        FormatterRegister.RegisterFormatter<string>(stringFormatter);

        try
        {
            // Act
            var result = FormatterRegister.Format("hello");

            // Assert - Should use custom formatter
            Assert.AreEqual("[hello]", result);
        }
        finally
        {
            FormatterRegister.UnregisterFormatter<string>();
        }
    }
    #endregion

    #region Test Helper Types
    private class CustomType(int value = 0)
    {
        public int Value { get; } = value;
        public override string ToString() => $"CustomType:{Value}";
    }

    private class AnotherCustomType(string name = "")
    {
        public string Name { get; } = name;
        public override string ToString() => $"Another:{Name}";
    }

    private class CustomTypeFormatter : IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is CustomType custom)
                return $"Custom:{custom.Value}";
            return null;
        }
    }

    private class AnotherTypeFormatter : IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is AnotherCustomType another)
                return $"Another_Custom:{another.Name}";
            return null;
        }
    }

    private class CustomStringFormatter : IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is string str)
                return $"[{str}]";
            return null;
        }
    }
    #endregion
}
