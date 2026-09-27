using BankSharp;
using BankSharp.Formatting;
using BankSharp.Parsing;
using BenchmarkDotNet.Running;

namespace BankSharp.Benchmarks;

public class Program
{
    //public static void Main(string[] args)
    //{
    //    // Uncomment this line later when you want to run BenchmarkDotNet:
    //    // BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

    //    // Replace with your IBAN here:
    //    string myIban = "DE57165667457111362456";

    //    Console.WriteLine($"--- Testing IBAN: {myIban} ---");

    //    // 1. Normalize and format
    //    string normalized = IbanFormatter.Normalize(myIban);
    //    Console.WriteLine($"Formatted:        {IbanFormatter.ToPrintFormat(normalized)}");

    //    // 2. Validate
    //    var result = IbanValidator.Validate(normalized);

    //    if (result.IsValid)
    //    {
    //        Console.WriteLine("Status:           [VALID] ✅");

    //        // 3. Parse
    //        ParsedIban parsed = IbanParser.Parse(normalized);
    //        Console.WriteLine($"Country Code:     {parsed.CountryCode}");
    //        Console.WriteLine($"Bank Code:        {parsed.BankCode}");
    //        Console.WriteLine($"Account Number:   {parsed.AccountNumber}");
    //        Console.WriteLine($"Trimmed Account:  {parsed.AccountNumber.TrimStart('0')}");
    //    }
    //    else
    //    {
    //        Console.WriteLine($"Status:           [INVALID] ❌");
    //        Console.WriteLine($"Error Detail:     {result.Error}");
    //    }

    //    Console.WriteLine("\nPress any key to exit...");
    //    Console.ReadKey();
    //}

    //static void Main(string[] args)
    //{
    //    Console.WriteLine("--- BankSharp Test Data Generator ---");

    //    string myIban = IbanBuilder.GenerateRandom("IR");
    //    Console.WriteLine($"Generated IR IBAN: {myIban}");

    //    string deIban = IbanBuilder.GenerateRandom("DE");
    //    Console.WriteLine($"Generated DE IBAN: {deIban}");
    //}

   static void Main(string[] args)
    {
        BenchmarkRunner.Run<IbanNetComparisonBenchmark>();
    }
}

