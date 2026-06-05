// Quick debug test
using Portamical.Core.Strategy;
using Portamical.Core.TestDataTypes.Models.Specialized;

var sut = new TestHelper("def", "expected", "arg1");
var args = sut.ToArgs(ArgsCode.Properties, PropsCode.TrimTestCaseName);
Console.WriteLine($"Count: {args.Length}");
for (int i = 0; i < args.Length; i++)
{
    Console.WriteLine($"  [{i}] = {args[i]}");
}

public sealed class TestHelper : TestDataExpected<string>
{
    public TestHelper(string definition, string expected, string? arg1)
        : base(definition, expected)
    {
        Arg1 = arg1;
    }

    public string? Arg1 { get; init; }

    public override string GetResult()
    => GetExpectedResult(Expected);

    public override string GetResultPrefix()
    => GetValidResultPrefix("results");

    protected override object?[] ToObjectArray(ArgsCode argsCode)
    => Extend(base.ToObjectArray, argsCode, Arg1);
}
