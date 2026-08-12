// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;

namespace Portamical.Companion.Execution;

/// <summary>
/// Observed outcome of executing the method under test with a spec's arguments.
/// </summary>
/// <param name="Succeeded">Whether the invocation completed (returned or threw an expected-style exception).</param>
/// <param name="ReturnValue">String representation of the returned value, if the method returned.</param>
/// <param name="ExceptionTypeName">Short type name of the thrown exception, if any.</param>
/// <param name="ExceptionMessage">Message of the thrown exception, if any.</param>
public sealed record CharacterizationResult(
    bool Succeeded,
    string? ReturnValue,
    string? ExceptionTypeName,
    string? ExceptionMessage)
{
    /// <summary>Whether the invocation threw an exception.</summary>
    public bool Threw => ExceptionTypeName is not null;
}

/// <summary>
/// Characterization mode: executes the method under test in an isolated, collectible
/// <see cref="AssemblyLoadContext"/> to observe the *actual* result or exception for a
/// proposed test case, so expected values are verified rather than assumed.
/// </summary>
public sealed class Characterizer : IDisposable
{
    private readonly AssemblyLoadContext _context;
    private readonly Assembly _assembly;
    private bool _disposed;

    /// <summary>
    /// Loads the target assembly into an isolated, collectible load context.
    /// </summary>
    /// <param name="assemblyPath">Full path of the assembly containing the method under test.</param>
    public Characterizer(string assemblyPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

        _context = new AssemblyLoadContext($"Characterizer:{Path.GetFileName(assemblyPath)}", isCollectible: true);
        _assembly = _context.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
    }

    /// <summary>
    /// Invokes <paramref name="methodName"/> on <paramref name="typeName"/> with the given
    /// argument strings (converted to parameter types) and captures the outcome.
    /// Static methods are invoked directly; instance methods on a default-constructed instance.
    /// </summary>
    /// <param name="typeName">Full or simple name of the declaring type.</param>
    /// <param name="methodName">Name of the method under test.</param>
    /// <param name="argValues">Argument values as strings ("null" for null); converted via <see cref="Convert.ChangeType(object?, Type)"/>.</param>
    /// <param name="timeout">Guard timeout; defaults to 5 seconds.</param>
    public CharacterizationResult Characterize(
        string typeName,
        string methodName,
        IReadOnlyList<string?> argValues,
        TimeSpan? timeout = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
        ArgumentNullException.ThrowIfNull(argValues);

        var type = FindType(typeName)
            ?? throw new ArgumentException($"Type '{typeName}' not found in assembly.", nameof(typeName));

        var method = type
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == argValues.Count)
            ?? throw new ArgumentException(
                $"Method '{methodName}' with {argValues.Count} parameter(s) not found on '{typeName}'.",
                nameof(methodName));

        object?[] args = ConvertArgs(method.GetParameters(), argValues);
        object? instance = method.IsStatic ? null : Activator.CreateInstance(type);

        var task = Task.Run<(object? Result, Exception? Exception)>(() =>
        {
            try
            {
                return (method.Invoke(instance, args), null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                return (null, ex.InnerException);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        });

        if (!task.Wait(timeout ?? TimeSpan.FromSeconds(5)))
        {
            return new CharacterizationResult(false, null, "Timeout", "Invocation exceeded the guard timeout.");
        }

        (object? result, Exception? exception) = task.GetAwaiter().GetResult();

        return exception is null
            ? new CharacterizationResult(true, FormatValue(result), null, null)
            : new CharacterizationResult(true, null, exception.GetType().Name, exception.Message);
    }

    private Type? FindType(string typeName)
    => _assembly.GetType(typeName)
        ?? _assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);

    private static object?[] ConvertArgs(
        ParameterInfo[] parameters,
        IReadOnlyList<string?> argValues)
    {
        object?[] args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            string? value = argValues[i];

            if (value is null or "null")
            {
                args[i] = null;
                continue;
            }

            Type targetType = Nullable.GetUnderlyingType(parameters[i].ParameterType)
                ?? parameters[i].ParameterType;

            args[i] = targetType.IsEnum
                ? Enum.Parse(targetType, value, ignoreCase: true)
                : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        return args;
    }

    private static string? FormatValue(object? value)
    => value switch
    {
        null => "null",
        string s => s,
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _context.Unload();
    }
}
