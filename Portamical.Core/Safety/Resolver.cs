// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Diagnostics;
using System.Globalization;
using static Portamical.Core.Safety.Validator;

namespace Portamical.Core.Safety;

/// <summary>
/// Provides resolution and fallback utilities for handling missing or invalid test data values.
/// </summary>
/// <remarks>
/// <para>
/// This class offers methods for resolving test data values with automatic fallback to
/// generated labels when values are missing or invalid. It includes a thread-safe global
/// log counter for tracking fallback occurrences.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Static utility class with fallback resolution logic.
/// </para>
/// <para>
/// <strong>Thread Safety:</strong> All public methods are thread-safe. The internal log counter
/// uses atomic operations (<see cref="Interlocked"/>) to ensure thread-safe increments and resets.
/// </para>
/// <para>
/// <strong>Performance:</strong> The <see cref="ResetLogCounter"/> method is marked with
/// <see cref="MethodImplAttribute"/> using <see cref="MethodImplOptions.AggressiveInlining"/>
/// to eliminate method call overhead for atomic operations.
/// </para>
/// <para>
/// <strong>Key Features:</strong>
/// <list type="bullet">
///   <item>Automatic fallback label generation with unique indices</item>
///   <item>Diagnostic trace logging for fallback occurrences</item>
///   <item>Thread-safe global log counter</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Resolve test case name with fallback
/// string testCaseName = "TestCase".FallbackIfNullOrWhiteSpace(
///     actualValue: null,
///     methodName: nameof(GetTestCaseName));
/// // Result: "TestCase (1)" with trace warning logged
/// 
/// // Reset counter (typically in test cleanup)
/// long previousCount = Resolver.ResetLogCounter();
/// </code>
/// </example>
public static class Resolver
{
    private static long LogCounter;

    /// <summary>
    /// Returns the preferred value if it is not null, empty, or consists only of white-space characters; otherwise,
    /// returns a fallback label with a unique index appended.
    /// </summary>
    /// <param name="fallbackLabel">
    /// The fallback label to use if <paramref name="preferredValue"/> is null, empty, or white space. Cannot be null.
    /// </param>
    /// <param name="preferredValue">
    /// The value to return if it is not null, empty, or white space. May be null.
    /// </param>
    /// <param name="methodName">
    /// The name of the method requesting the value, used for logging purposes. Cannot be null.
    /// </param>
    /// <returns>
    /// The <paramref name="preferredValue"/> if it contains non-white-space characters; otherwise, the 
    /// <paramref name="fallbackLabel"/> with a unique index appended in the format <c>"{fallbackLabel} ({index})"</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="fallbackLabel"/> or <paramref name="methodName"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// If <paramref name="preferredValue"/> is null, empty, or white space, a warning is logged and
    /// the fallback label is returned with a unique index to aid in identifying fallback occurrences in logs and
    /// reports. This method is intended for use in test data scenarios where a meaningful value is required for
    /// reporting or logging.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This method is thread-safe. The unique index is generated
    /// using atomic operations (<see cref="Interlocked.Increment(ref long)"/>), ensuring that
    /// concurrent calls from multiple threads receive distinct indices.
    /// </para>
    /// <para>
    /// <strong>Trace Output:</strong> When a fallback occurs, a warning is written to the trace
    /// listeners via <see cref="Trace.TraceWarning(string)"/>. The message format is:
    /// <c>"Portamical log {index}: The '{methodName}' method of the test data object returned a null, empty, or whitespace value. Using indexed fallback label '{fallback}' in the test report."</c>
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> This method is NOT inlined because it contains complex logic
    /// (string operations, trace I/O). The overhead is acceptable since this is a fallback path
    /// (rare case in well-designed tests).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Valid value (no fallback)
    /// var result1 = "DefaultName".FallbackIfNullOrWhiteSpace("ActualName", "GetName");
    /// // Result: "ActualName" (no trace warning)
    /// 
    /// // Null value (fallback triggered)
    /// var result2 = "DefaultName".FallbackIfNullOrWhiteSpace(null, "GetName");
    /// // Result: "DefaultName (1)"
    /// // Trace: "Portamical log 1: The 'GetName' method... Using indexed fallback label 'DefaultName (1)'..."
    /// 
    /// // Whitespace value (fallback triggered)
    /// var result3 = "DefaultName".FallbackIfNullOrWhiteSpace("   ", "GetName");
    /// // Result: "DefaultName (2)"
    /// </code>
    /// </example>
    public static string FallbackIfNullOrWhiteSpace(
        this string fallbackLabel,
        string? preferredValue,
        string methodName)
    {
        _ = NotNull(fallbackLabel, nameof(fallbackLabel));
        _ = NotNull(methodName, nameof(methodName));

        if (string.IsNullOrWhiteSpace(preferredValue))
        {
            var logIndex = IncrementLogIndex(out string logPrefix);
            var indexedFallback = $"{fallbackLabel} ({logIndex})";

            Trace.TraceWarning(string.Create(
                CultureInfo.InvariantCulture,
                $"{logPrefix}The '{methodName}' method of the test data object " +
                $"returned a null, empty, or whitespace value. " +
                $"Using indexed fallback label '{indexedFallback}' in the test report."));

            return indexedFallback;
        }

        return preferredValue;
    }

    /// <summary>
    /// Resets the log counter to zero and returns the previous value of the log counter.
    /// </summary>
    /// <returns>The previous value of the log counter before it was reset.</returns>
    /// <remarks>
    /// <para>
    /// This method is thread-safe and can be called concurrently from multiple threads. It uses
    /// atomic operations (<see cref="Interlocked.Exchange(ref long, long)"/>) to ensure that the 
    /// log counter is reset without race conditions.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> This method is marked for aggressive inlining because it wraps
    /// a single atomic operation (<see cref="Interlocked.Exchange(ref long, long)"/>). Inlining eliminates method
    /// call overhead while preserving thread safety.
    /// </para>
    /// <para>
    /// <strong>Typical Usage:</strong> Call this method in test cleanup or between test runs
    /// to reset the fallback counter to zero. This is useful for:
    /// <list type="bullet">
    ///   <item>Ensuring consistent fallback indices across test runs</item>
    ///   <item>Detecting how many fallbacks occurred during a test session</item>
    ///   <item>Cleaning up state between integration tests</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [TestCleanup]
    /// public void Cleanup()
    /// {
    ///     long fallbackCount = Resolver.ResetLogCounter();
    ///     Console.WriteLine($"Test run had {fallbackCount} fallback occurrences.");
    /// }
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ResetLogCounter()
    => Interlocked.Exchange(ref LogCounter, 0L);

    /// <summary>
    /// Atomically increments the global log counter and generates a log prefix.
    /// </summary>
    /// <param name="logPrefix">
    /// When this method returns, contains the formatted log prefix in the form "Portamical log {index}: ".
    /// </param>
    /// <returns>The incremented log counter value.</returns>
    /// <remarks>
    /// <para>
    /// This method is thread-safe. It uses <see cref="Interlocked.Increment(ref long)"/> to ensure
    /// atomic increment without race conditions.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong> This method is NOT inlined because:
    /// <list type="bullet">
    ///   <item>It has only a single call site (from <see cref="FallbackIfNullOrWhiteSpace"/>)</item>
    ///   <item>It allocates a string via interpolation (dominant cost)</item>
    ///   <item>It's only called on the fallback path (rare case)</item>
    /// </list>
    /// Inlining would provide no benefit and would increase code size unnecessarily.
    /// </para>
    /// </remarks>
    private static long IncrementLogIndex(out string logPrefix)
    {
        var logIndex = Interlocked.Increment(ref LogCounter);
        logPrefix = $"Portamical log {logIndex}: ";

        return logIndex;
    }

    /// <summary>
    /// Creates a snapshot array of the specified sequence, or returns <see langword="null"/> if the sequence is null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="enumerable">
    /// The sequence to convert to an array, or <see langword="null"/>.
    /// </param>
    /// <returns>
    /// An array containing all elements from <paramref name="enumerable"/> if it is not null;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Optimization:</strong> If <paramref name="enumerable"/> is already a <typeparamref name="T"/> array,
    /// it is returned directly without creating a new allocation. Otherwise, a new array is created
    /// using collection expressions ([.. enumerable]).
    /// </para>
    /// <para>
    /// <strong>Purpose:</strong> This internal helper prevents multiple enumeration by creating a
    /// snapshot of the sequence. It is used by validation methods like <see cref="NotNullOrEmpty{T}(IEnumerable{T}, string)"/>
    /// to safely check properties (like <see cref="Array.Length"/>) without enumerating the source multiple times.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// <list type="bullet">
    ///   <item>Null input: O(1) - immediate return</item>
    ///   <item>Already an array: O(1) - cast and return</item>
    ///   <item>Other sequences: O(n) - enumerates once to create array</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static T[]? SnapshotOrNull<T>(IEnumerable<T>? enumerable)
    {
        if (enumerable is null) return null;

        return enumerable as T[] ?? [.. enumerable];
    }
}