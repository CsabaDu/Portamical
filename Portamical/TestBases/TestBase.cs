// SPDX-License-Identifier: MIT
// Copyright (c) 2025. Csaba Dudas (CsabaDu)

namespace Portamical.TestBases;

/// <summary>
/// Provides a base class for test implementations, offering common functionality for managing test case arguments and
/// log counters.
/// </summary>
/// <remarks>This class implements <see cref="IDisposable"/>, ensuring proper resource management. Derived classes
/// should call <see cref="Dispose"/> to reset the log counter and restore the default argument state. The static
/// properties provide access to various argument configurations relevant to tests.</remarks>
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