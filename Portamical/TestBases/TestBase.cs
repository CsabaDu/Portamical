// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

using Portamical.Core.Safety;

namespace Portamical.TestBases;

public abstract class TestBase : IDisposable
{
    protected static ArgsCode ArgsCode { get; set; } = AsInstance;

    protected static ArgsCode AsInstance => ArgsCode.Instance;
    protected static ArgsCode AsProperties => ArgsCode.Properties;
    protected static PropsCode WithTestCaseName => PropsCode.All;

    protected static long ResetLogCounter()
    => Resolver.ResetLogCounter();

    public virtual void Dispose()
    {
        ArgsCode = AsInstance;
        ResetLogCounter();

        GC.SuppressFinalize(this);
    }
}