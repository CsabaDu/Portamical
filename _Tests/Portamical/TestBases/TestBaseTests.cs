// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Core.Factories;
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes;
using Portamical.TestBases;

namespace Tests.Portamical.TestBases;

[TestClass]
public class TestBaseTests
{
    private sealed class ConcreteBase : TestBase
    {
        public static ArgsCode GetAsInstance() => AsInstance;
        public static ArgsCode GetAsProperties() => AsProperties;
        public static PropsCode GetWithTestCaseName() => WithTestCaseName;

        public static T Invoke3Args<TTestData, T>(
            Func<IEnumerable<TTestData>, ArgsCode, string?, T> convert,
            IEnumerable<TTestData> collection,
            string? name)
        where TTestData : notnull, ITestData
            => ConvertAsInstance(convert, collection, name);

        public static T Invoke2Args<TTestData, T>(
            Func<IEnumerable<TTestData>, ArgsCode, T> convert,
            IEnumerable<TTestData> collection)
        where TTestData : notnull, ITestData
            => ConvertAsInstance(convert, collection);
    }

    #region Constants

    [TestMethod]
    public void AsInstance_returnsArgsCodeInstance()
        => Assert.AreEqual(ArgsCode.Instance, ConcreteBase.GetAsInstance());

    [TestMethod]
    public void AsProperties_returnsArgsCodeProperties()
        => Assert.AreEqual(ArgsCode.Properties, ConcreteBase.GetAsProperties());

    [TestMethod]
    public void WithTestCaseName_returnsPropsCodeAll()
        => Assert.AreEqual(PropsCode.All, ConcreteBase.GetWithTestCaseName());

    #endregion

    #region ConvertAsInstance (3-arg)

    [TestMethod]
    public void ConvertAsInstance_3args_callsConvertWithArgsCodeInstance()
    {
        ArgsCode? captured = null;
        Func<IEnumerable<ITestData>, ArgsCode, string?, object> convert =
            (_, ac, _) => { captured = ac; return new object(); };

        ConcreteBase.Invoke3Args(convert, Array.Empty<ITestData>(), null);

        Assert.AreEqual(ArgsCode.Instance, captured);
    }

    [TestMethod]
    public void ConvertAsInstance_3args_callsConvertWithTestMethodName()
    {
        string? capturedName = null;
        Func<IEnumerable<ITestData>, ArgsCode, string?, object> convert =
            (_, _, name) => { capturedName = name; return new object(); };

        ConcreteBase.Invoke3Args(convert, Array.Empty<ITestData>(), "MyTest");

        Assert.AreEqual("MyTest", capturedName);
    }

    [TestMethod]
    public void ConvertAsInstance_3args_nullTestMethodName_passesNull()
    {
        string? capturedName = "sentinel";
        Func<IEnumerable<ITestData>, ArgsCode, string?, object> convert =
            (_, _, name) => { capturedName = name; return new object(); };

        ConcreteBase.Invoke3Args(convert, Array.Empty<ITestData>(), null);

        Assert.IsNull(capturedName);
    }

    [TestMethod]
    public void ConvertAsInstance_3args_passesCollectionThrough()
    {
        IEnumerable<ITestData>? capturedCollection = null;
        var item = TestDataFactory.CreateTestData<int>("def", "result", 1);
        ITestData[] collection = [item];
        Func<IEnumerable<ITestData>, ArgsCode, string?, object> convert =
            (col, _, _) => { capturedCollection = col; return new object(); };

        ConcreteBase.Invoke3Args(convert, collection, null);

        Assert.AreSame(collection, capturedCollection);
    }

    [TestMethod]
    public void ConvertAsInstance_3args_nullConvert_throwsArgumentNullException()
    {
        Func<IEnumerable<ITestData>, ArgsCode, string?, object> nullConvert = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ConcreteBase.Invoke3Args(nullConvert, Array.Empty<ITestData>(), null));
    }

    #endregion

    #region ConvertAsInstance (2-arg)

    [TestMethod]
    public void ConvertAsInstance_2args_callsConvertWithArgsCodeInstance()
    {
        ArgsCode? captured = null;
        Func<IEnumerable<ITestData>, ArgsCode, object> convert =
            (_, ac) => { captured = ac; return new object(); };

        ConcreteBase.Invoke2Args(convert, Array.Empty<ITestData>());

        Assert.AreEqual(ArgsCode.Instance, captured);
    }

    [TestMethod]
    public void ConvertAsInstance_2args_passesCollectionThrough()
    {
        IEnumerable<ITestData>? capturedCollection = null;
        var item = TestDataFactory.CreateTestData<int>("def", "result", 1);
        ITestData[] collection = [item];
        Func<IEnumerable<ITestData>, ArgsCode, object> convert =
            (col, _) => { capturedCollection = col; return new object(); };

        ConcreteBase.Invoke2Args(convert, collection);

        Assert.AreSame(collection, capturedCollection);
    }

    [TestMethod]
    public void ConvertAsInstance_2args_nullConvert_throwsArgumentNullException()
    {
        Func<IEnumerable<ITestData>, ArgsCode, object> nullConvert = null!;
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ConcreteBase.Invoke2Args(nullConvert, Array.Empty<ITestData>()));
    }

    #endregion
}
