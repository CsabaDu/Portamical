// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Formatting;

namespace Tests.Portamical.Core.Formatting;

/// <summary>
/// Unit tests for <see cref="Formatter"/> custom formatter registration and management.
/// </summary>
[TestClass]
[DoNotParallelize] // Registry is a shared static resource; tests must run sequentially
public class FormatterTests
{
    [TestCleanup]
    public void Cleanup()
    {
        // Ensure registry is clean after each test to prevent test interference
        Formatter.ClearFormatters();
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
            var result = Formatter.RegisterFormatter(typeof(CustomType), formatter);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Formatter.IsFormatterRegistered<CustomType>());
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
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
            var result = Formatter.RegisterFormatter<CustomType>(formatter);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Formatter.IsFormatterRegistered<CustomType>());
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
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
            Formatter.RegisterFormatter<CustomType>(formatter1);

            // Act - Try to register again
            var result = Formatter.RegisterFormatter<CustomType>(formatter2);

            // Assert
            Assert.IsFalse(result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void RegisterFormatter_withNullType_throwsArgumentNullException()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Formatter.RegisterFormatter(null!, formatter));
    }

    [TestMethod]
    public void RegisterFormatter_withNullFormatter_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Formatter.RegisterFormatter(typeof(CustomType), null!));
    }

    [TestMethod]
    public void RegisterFormatter_generic_withNullFormatter_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Formatter.RegisterFormatter<CustomType>(null!));
    }
    #endregion

    #region UnregisterFormatter - Basic Operations
#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void UnregisterFormatter_withRegisteredType_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        Formatter.RegisterFormatter(typeof(CustomType), formatter);

        // Act
        var result = Formatter.UnregisterFormatter<CustomType>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(Formatter.IsFormatterRegistered<CustomType>());
    }
#pragma warning restore CA2263

    [TestMethod]
    public void UnregisterFormatter_generic_withRegisteredType_returnsTrue()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        Formatter.RegisterFormatter<CustomType>(formatter);

        // Act
        var result = Formatter.UnregisterFormatter<CustomType>();

        // Assert
        Assert.IsTrue(result);
        Assert.IsFalse(Formatter.IsFormatterRegistered<CustomType>());
    }

    [TestMethod]
    public void UnregisterFormatter_withUnregisteredType_returnsFalse()
    {
        // Act
        var result = Formatter.UnregisterFormatter<CustomType>();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void UnregisterFormatter_withNullType_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Formatter.UnregisterFormatter(null!));
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
            Formatter.RegisterFormatter(typeof(CustomType), formatter);

            // Act
            var result = Formatter.IsFormatterRegistered<CustomType>();

            // Assert
            Assert.IsTrue(result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
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
            Formatter.RegisterFormatter<CustomType>(formatter);

            // Act
            var result = Formatter.IsFormatterRegistered<CustomType>();

            // Assert
            Assert.IsTrue(result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void IsFormatterRegistered_withUnregisteredType_returnsFalse()
    {
        // Act
        var result = Formatter.IsFormatterRegistered(typeof(CustomType));

        // Assert
        Assert.IsFalse(result);
    }
#pragma warning restore CA2263

    [TestMethod]
    public void IsFormatterRegistered_generic_withUnregisteredType_returnsFalse()
    {
        // Act
        var result = Formatter.IsFormatterRegistered<CustomType>();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsFormatterRegistered_withNullType_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Formatter.IsFormatterRegistered(null!));
    }
    #endregion

    #region ClearFormatters - Registry Management
    [TestMethod]
    public void ClearFormatters_removesAllRegisteredFormatters()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new AnotherTypeFormatter();
        Formatter.RegisterFormatter<CustomType>(formatter1);
        Formatter.RegisterFormatter<AnotherCustomType>(formatter2);

        // Act
        Formatter.ClearFormatters();

        // Assert
        Assert.IsFalse(Formatter.IsFormatterRegistered<CustomType>());
        Assert.IsFalse(Formatter.IsFormatterRegistered<AnotherCustomType>());
        Assert.IsEmpty(Formatter.Registry);
    }

    [TestMethod]
    public void ClearFormatters_whenRegistryEmpty_doesNotThrow()
    {
        // Arrange
        Formatter.ClearFormatters();

        // Act & Assert - Should not throw
        Formatter.ClearFormatters();
        Assert.IsEmpty(Formatter.Registry);
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
            Formatter.RegisterFormatter<CustomType>(formatter);

            // Act & Assert
            Assert.HasCount(1, Formatter.Registry);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
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
            Formatter.RegisterFormatter<CustomType>(formatter1);
            Formatter.RegisterFormatter<AnotherCustomType>(formatter2);
            Formatter.RegisterFormatter<string>(formatter3);

            // Act & Assert
            Assert.HasCount(3, Formatter.Registry);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
            Formatter.UnregisterFormatter<AnotherCustomType>();
            Formatter.UnregisterFormatter<string>();
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
            Formatter.RegisterFormatter<CustomType>(formatter1);
            Formatter.RegisterFormatter<AnotherCustomType>(formatter2);
            Assert.HasCount(2, Formatter.Registry);

            // Act
            Formatter.UnregisterFormatter<CustomType>();

            // Assert
            Assert.HasCount(1, Formatter.Registry);
        }
        finally
        {
            Formatter.UnregisterFormatter<AnotherCustomType>();
        }
    }
    #endregion

    #region Registry - Internal State Access
    [TestMethod]
    public void Registry_returnsNonNullDictionary()
    {
        // Act
        var registry = Formatter.Registry;

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
            Formatter.RegisterFormatter<CustomType>(formatter);

            // Act
            var registry = Formatter.Registry;

            // Assert
            Assert.IsTrue(registry.ContainsKey(typeof(CustomType)));
            Assert.AreSame(formatter, registry[typeof(CustomType)]);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Registry_afterUnregistration_doesNotContainType()
    {
        // Arrange
        var formatter = new CustomTypeFormatter();
        Formatter.RegisterFormatter<CustomType>(formatter);
        Formatter.UnregisterFormatter<CustomType>();

        // Act
        var registry = Formatter.Registry;

        // Assert
        Assert.IsFalse(registry.ContainsKey(typeof(CustomType)));
    }

    [TestMethod]
    public void Registry_afterClear_isEmpty()
    {
        // Arrange
        var formatter1 = new CustomTypeFormatter();
        var formatter2 = new AnotherTypeFormatter();
        Formatter.RegisterFormatter<CustomType>(formatter1);
        Formatter.RegisterFormatter<AnotherCustomType>(formatter2);
        Formatter.ClearFormatters();

        // Act
        var registry = Formatter.Registry;

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
                    Formatter.RegisterFormatter<CustomType>(formatter)));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert - Only one should succeed
            var successCount = tasks.Count(t => t.Result);
            Assert.AreEqual(1, successCount);
            Assert.HasCount(1, Formatter.Registry);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
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
            var firstReg = Formatter.RegisterFormatter<CustomType>(formatter1);
            var unreg = Formatter.UnregisterFormatter<CustomType>();
            var secondReg = Formatter.RegisterFormatter<CustomType>(formatter2);

            // Assert
            Assert.IsTrue(firstReg);
            Assert.IsTrue(unreg);
            Assert.IsTrue(secondReg);
            Assert.IsTrue(Formatter.IsFormatterRegistered<CustomType>());
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
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
        Formatter.RegisterFormatter<CustomType>(customFormatter);

        try
        {
            // Act
            var formatter = Formatter.GetFormatter(typeof(CustomType));

            // Assert
            Assert.IsNotNull(formatter);
            Assert.IsInstanceOfType<global::Portamical.Core.Formatting.IFormatter>(formatter);

            // Verify it uses the custom formatter
            var obj = new CustomType(42);
            var result = formatter.Format(obj);
            Assert.AreEqual("Custom:42", result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }
#pragma warning restore CA2263

    [TestMethod]
    public void GetFormatter_generic_withRegisteredType_returnsCustomFormatter()
    {
        // Arrange
        var customFormatter = new CustomTypeFormatter();
        Formatter.RegisterFormatter<CustomType>(customFormatter);

        try
        {
            // Act
            var formatter = Formatter.GetFormatter<CustomType>();

            // Assert
            Assert.IsNotNull(formatter);

            // Verify it uses the custom formatter
            var obj = new CustomType(42);
            var result = formatter.Format(obj);
            Assert.AreEqual("Custom:42", result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

#pragma warning disable CA2263 // Prefer generic overload - Intentionally testing non-generic overload
    [TestMethod]
    public void GetFormatter_withUnregisteredType_returnsDefaultFormatter()
    {
        // Act
        var formatter = Formatter.GetFormatter(typeof(CustomType));

        // Assert
        Assert.IsNotNull(formatter);
        Assert.AreSame(DefaultFormatter.Instance, formatter);
    }
#pragma warning restore CA2263

    [TestMethod]
    public void GetFormatter_generic_withUnregisteredType_returnsDefaultFormatter()
    {
        // Act
        var formatter = Formatter.GetFormatter<CustomType>();

        // Assert
        Assert.IsNotNull(formatter);
        Assert.AreSame(DefaultFormatter.Instance, formatter);
    }

    [TestMethod]
    public void GetFormatter_withNullType_throwsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            Formatter.GetFormatter(null!));
    }
    #endregion

    #region Format - Convenience Formatting
    [TestMethod]
    public void Format_withRegisteredFormatter_usesCustomFormatter()
    {
        // Arrange
        var customFormatter = new CustomTypeFormatter();
        Formatter.RegisterFormatter<CustomType>(customFormatter);

        try
        {
            var obj = new CustomType(99);

            // Act
            var result = Formatter.Format(obj);

            // Assert
            Assert.AreEqual("Custom:99", result);
        }
        finally
        {
            Formatter.UnregisterFormatter<CustomType>();
        }
    }

    [TestMethod]
    public void Format_withUnregisteredType_usesDefaultFormatter()
    {
        // Arrange
        var obj = new CustomType(42);

        // Act
        var result = Formatter.Format(obj);

        // Assert - Should use default ToString()
        Assert.AreEqual("CustomType:42", result);
    }

    [TestMethod]
    public void Format_withNull_returnsNull()
    {
        // Act
        var obj = (CustomType)null!;
        var result = Formatter.Format(obj);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Format_withPrimitiveType_usesDefaultFormatter()
    {
        // Act
        var result = Formatter.Format(42);

        // Assert
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public void Format_withString_usesDefaultFormatter()
    {
        // Act
        var result = Formatter.Format("test");

        // Assert - DefaultFormatter quotes strings
        Assert.AreEqual("\"test\"", result);
    }

    [TestMethod]
    public void Format_withRegisteredStringFormatter_overridesDefault()
    {
        // Arrange
        var stringFormatter = new CustomStringFormatter();
        Formatter.RegisterFormatter<string>(stringFormatter);

        try
        {
            // Act
            var result = Formatter.Format("hello");

            // Assert - Should use custom formatter
            Assert.AreEqual("[hello]", result);
        }
        finally
        {
            Formatter.UnregisterFormatter<string>();
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

    private class CustomTypeFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is CustomType custom)
                return $"Custom:{custom.Value}";
            return null;
        }
    }

    private class AnotherTypeFormatter : global::Portamical.Core.Formatting.IFormatter
    {
        public string? Format(object? obj)
        {
            if (obj is AnotherCustomType another)
                return $"Another_Custom:{another.Name}";
            return null;
        }
    }

    private class CustomStringFormatter : global::Portamical.Core.Formatting.IFormatter
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
