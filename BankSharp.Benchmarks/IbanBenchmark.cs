using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BankSharp.Formatting;
using BankSharp.Parsing;

namespace BankSharp.Benchmarks;

/// <summary>
/// Official performance benchmarks for the BankSharp pipeline.
/// Measures execution time (ns) and heap allocations (B) across validation, parsing, and formatting.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class IbanBenchmark
{
    private const string ValidIban = "IR020120000000001083758362";
    private const string InvalidChecksumIban = "IR000120000000001083758362";
    private readonly string _instanceIban = new(ValidIban.ToCharArray());

    [Benchmark(Baseline = true)]
    public bool Validate_Span()
    {
        // Zero-allocation path using ReadOnlySpan<char>
        return IbanValidator.Validate(ValidIban.AsSpan()).IsValid;
    }

    [Benchmark]
    public bool Validate_String()
    {
        // Standard string path used by consumers
        return IbanValidator.Validate(_instanceIban).IsValid;
    }

    [Benchmark]
    public bool Validate_InvalidChecksum()
    {
        // Worst-case scenario: passes length checks but fails Mod-97 at the end
        return IbanValidator.Validate(InvalidChecksumIban.AsSpan()).IsValid;
    }

    [Benchmark]
    public bool Parse_ValidIban()
    {
        // High-performance parsing
        return IbanParser.TryParse(ValidIban.AsSpan(), out _);
    }

    [Benchmark]
    public string Format_ToPrint_String()
    {
        // ISO 13616 print format (4-character blocks) via string allocation
        return IbanFormatter.ToPrintFormat(ValidIban);
    }

    [Benchmark]
    public bool Format_TryFormat_Print_Span()
    {
        // Zero-allocation print formatting via Span and stackalloc buffer
        Span<char> destination = stackalloc char[32];
        return IbanFormatter.TryFormat(ValidIban.AsSpan(), destination, out _, "P");
    }

    [Benchmark]
    public bool Format_TryFormat_General_Span()
    {
        // Zero-allocation canonical/general formatting via Span and stackalloc buffer
        Span<char> destination = stackalloc char[26];
        return IbanFormatter.TryFormat(ValidIban.AsSpan(), destination, out _, "G");
    }
}
