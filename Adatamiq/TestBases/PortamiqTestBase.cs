// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Adatamiq.Strategy;
using Adatamiq.Converters;
using Adatamiq.TestDataTypes;

namespace Adatamiq.TestBases;

public abstract class PortamiqTestBase
{
    protected PortamiqTestBase()
    {
    }
        
    protected PortamiqTestBase(ArgsCode argsCode) : this()
    {
        ArgsCode = argsCode.Defined(nameof(argsCode));
    }

    protected ArgsCode ArgsCode { get; private set; } = AsInstance;

    protected static readonly ArgsCode AsInstance = ArgsCode.Instance;
    protected static readonly ArgsCode AsProperties = ArgsCode.Properties;

    protected IEnumerable<object?[]> ConvertToObjectArrayCollection<TTestData>(
        IEnumerable<TTestData> testDataCollection)
    where TTestData : notnull, ITestData
    => testDataCollection.Convert(ArgsCode);
}
