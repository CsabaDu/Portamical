// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Analysis;
using Portamical.Companion.Core;

if (args.Length == 0)
{
    PrintUsage();
    return 0;
}

return args[0] switch
{
    "scan" => Scan(args[1..]),
    "gaps" => Gaps(args[1..]),
    _ => PrintUsage(),
};

static int PrintUsage()
{
    Console.WriteLine("""
        portamical-companion — CI companion for Portamical data-driven tests

        Commands:
          scan <file-or-dir>...
              Extract existing "definition => result" test case names from test sources.

          gaps <proposals-file> <file-or-dir>...
              Diff proposed test case lines (one per line in proposals-file) against
              existing test sources. Exit code 1 when gaps are found (CI gate).
        """);
    return 0;
}

static IEnumerable<string> ResolveSourceFiles(IEnumerable<string> paths)
{
    foreach (string path in paths)
    {
        if (Directory.Exists(path))
        {
            foreach (string file in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            {
                yield return file;
            }
        }
        else if (File.Exists(path))
        {
            yield return path;
        }
        else
        {
            Console.Error.WriteLine($"warning: path not found: {path}");
        }
    }
}

static int Scan(string[] paths)
{
    if (paths.Length == 0)
    {
        Console.Error.WriteLine("scan: at least one file or directory is required.");
        return 2;
    }

    var names = TestScanner.ExtractTestCaseNames(
        ResolveSourceFiles(paths).Select(File.ReadAllText));

    foreach (string name in names)
    {
        Console.WriteLine(name);
    }

    Console.Error.WriteLine($"{names.Count} test case name(s) found.");
    return 0;
}

static int Gaps(string[] paths)
{
    if (paths.Length < 2)
    {
        Console.Error.WriteLine("gaps: usage: gaps <proposals-file> <file-or-dir>...");
        return 2;
    }

    var proposed = new List<TestCaseSpec>();

    foreach (string line in File.ReadAllLines(paths[0]))
    {
        if (NamingSemantics.TryParse(line, out var spec))
        {
            proposed.Add(spec!);
        }
    }

    var existing = TestScanner.ExtractTestCaseNames(
        ResolveSourceFiles(paths[1..]).Select(File.ReadAllText));

    var result = SpecSet.AnalyzeGaps(existing, proposed);

    Console.WriteLine($"Existing test cases: {existing.Count}");
    Console.WriteLine($"Covered proposals:   {result.Covered.Count}");
    Console.WriteLine($"Missing proposals:   {result.Missing.Count}");

    foreach (var spec in result.Missing)
    {
        Console.WriteLine($"  MISSING: {spec.TestCaseName}");
    }

    return result.Missing.Count > 0 ? 1 : 0;
}
