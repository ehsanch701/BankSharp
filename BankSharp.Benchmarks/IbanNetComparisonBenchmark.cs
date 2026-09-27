using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BankSharp.Formatting;
using IbanNet;

namespace BankSharp.Benchmarks;

/// <summary>
/// Head-to-head performance and memory allocation comparison benchmarks
/// between BankSharp and IbanNet across validation, normalization, and formatting.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class IbanNetComparisonBenchmark
{
    private const int MaxIbanLength = 34;
    private const int MaxPrintFormatLength = 42;

    private static readonly global::IbanNet.IIbanValidator _ibanNetValidator = new global::IbanNet.IbanValidator();
    private readonly global::IbanNet.IIbanParser _ibanNetParser = new global::IbanNet.IbanParser(_ibanNetValidator);

    /// <summary>
    /// Parameterized IBAN input strings covering various formats and lengths.
    /// </summary>
    [Params(
        "DE89370400440532013000",
        "GB82 WEST 1234 5698 7654 32",
        "FR14 2004 1010 0505 0001 3M02 606"
    )]
    public string IbanInput = default!;

    // -------------------------------------------------------------------------
    // 1. Validation Head-to-Head
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates an IBAN string using BankSharp validation engine (baseline).
    /// </summary>
    [Benchmark(Baseline = true)]
    public bool BankSharp_Validate()
    {
        return IbanValidator.Validate(IbanInput).IsValid;
    }

    /// <summary>
    /// Validates an IBAN string using IbanNet validator.
    /// </summary>
    [Benchmark]
    public bool IbanNet_Validate()
    {
        return _ibanNetValidator.Validate(IbanInput).IsValid;
    }

    // -------------------------------------------------------------------------
    // 2. Normalization / Electronic Parsing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Strips whitespaces/hyphens and normalizes casing using BankSharp.
    /// </summary>
    [Benchmark]
    public string BankSharp_Normalize()
    {
        return IbanFormatter.Normalize(IbanInput);
    }

    /// <summary>
    /// Parses and converts an IBAN to standard electronic representation using IbanNet.
    /// </summary>
    [Benchmark]
    public string IbanNet_Parse_ToElectronic()
    {
        return _ibanNetParser.Parse(IbanInput).ToString();
    }

    // -------------------------------------------------------------------------
    // 3. Formatting - Print Format
    // -------------------------------------------------------------------------

    /// <summary>
    /// Formats an IBAN string into 4-character separated print format using BankSharp.
    /// </summary>
    [Benchmark]
    public string BankSharp_ToPrintFormat()
    {
        return IbanFormatter.ToPrintFormat(IbanInput);
    }

    /// <summary>
    /// Parses and formats an IBAN into print format using IbanNet.
    /// </summary>
    [Benchmark]
    public string IbanNet_ParseAndToPrintFormat()
    {
        var iban = _ibanNetParser.Parse(IbanInput);
        return iban.ToString(IbanFormat.Print);
    }

    // -------------------------------------------------------------------------
    // 4. Zero-Allocation TryToPrintFormat (Stack-allocated buffer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// High-performance, zero-allocation IBAN print formatting using Span and stackalloc.
    /// </summary>
    [Benchmark]
    public bool BankSharp_ZeroAlloc_TryToPrintFormat()
    {
        Span<char> buffer = stackalloc char[MaxPrintFormatLength];
        return IbanFormatter.TryToPrintFormat(IbanInput.AsSpan(), buffer, out _);
    }

    // -------------------------------------------------------------------------
    // 5. Zero-Allocation TryNormalize (Stack-allocated buffer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// High-performance, zero-allocation IBAN normalization using Span and stackalloc.
    /// </summary>
    [Benchmark]
    public bool BankSharp_ZeroAlloc_TryNormalize()
    {
        Span<char> buffer = stackalloc char[MaxIbanLength];
        return IbanFormatter.TryNormalize(IbanInput.AsSpan(), buffer, out _);
    }
}
