# BankSharp

[![Build & Test](https://github.com/ehsanch701/BankSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/ehsanch701/BankSharp/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![Native AOT Compatible](https://img.shields.io/badge/Native%20AOT-compatible-blue)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)

**BankSharp** is an ultra-fast ISO 13616 International Bank Account Number (IBAN) validation, parsing, and formatting library for .NET, engineered for high throughput with zero-allocation hot paths and full **Native AOT** compatibility.

---

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Ecosystem Integrations](#ecosystem-integrations)
  - [Dependency Injection](#dependency-injection)
  - [ASP.NET Core OpenAPI](#aspnet-core-openapi)
  - [Entity Framework Core](#entity-framework-core)
  - [FluentValidation](#fluentvalidation)
- [Extending Validation Rules](#extending-validation-rules)
- [Performance & Benchmarks](#performance--benchmarks)
- [Security & Hardening](#security--hardening)
- [Supported Countries](#supported-countries)
- [Contributing & License](#contributing--license)

---

## Features

- 🚀 **Allocation-Free Core:** Core validation (`IbanValidator.Validate(ReadOnlySpan<char>)`) and formatting APIs (`TryToPrintFormat`, `TryNormalize`, `TryToMaskedFormat`, `TryFormat`) allocate **0 bytes** on the managed heap on successful hot paths.
- ⚡ **Span-First:** Heavily utilizes `ReadOnlySpan<char>`, `stackalloc`, `SearchValues<char>`, and `FrozenDictionary` for lookup.
- 📦 **Modern .NET:** Multi-targeted for **`.NET 8.0`**, **`.NET 9.0`**, and **`.NET 10.0`**.
- 🛡️ **Native AOT Ready:** Trim-safe, no runtime reflection, ready for serverless and container deployments (`<IsAotCompatible>true</IsAotCompatible>`).
- 🧩 **Modular Architecture:** Opt-in extensions for EF Core, FluentValidation, OpenAPI/Swagger, and Dependency Injection.
- 🇮🇷 **First-Class Iranian Sheba Support:** Strict 26-character national format (`IR`) with bank routing rules built-in.

---

## Installation

Install the core package:

```bash
dotnet add package BankSharp
```

Or pick individual modular extensions:

```bash
dotnet add package BankSharp.DependencyInjection
dotnet add package BankSharp.OpenApi
dotnet add package BankSharp.EntityFrameworkCore
dotnet add package BankSharp.FluentValidation
```

> **Note:** All packages are versioned in lockstep (e.g., `1.0.0` across the suite).

---

## Quick Start

```csharp
using BankSharp;

// 1. Parsing & Validation (Strongly-typed Iban value object)
if (Iban.TryParse("GB82 WEST 1234 5698 7654 32", out Iban iban))
{
    // Canonical electronic format (e.g. GB82WEST12345698765432)
    string electronic = iban.Value;              // or iban.ToString()
    
    // Human-readable print format with 4-char chunks
    string printable = iban.ToPrintFormat();     // GB82 WEST 1234 5698 7654 32
    
    // Masked format for logging/UI (hides middle digits)
    string masked = iban.ToMaskedFormat();       // GB82 **** **** **** **** 32
    
    Console.WriteLine($"Country: {iban.CountryCode}");   // GB
    Console.WriteLine($"Check Digits: {iban.CheckDigits}"); // 82
    Console.WriteLine($"Printable: {printable}");
}

// 2. High-Performance Span Validation (0 B allocated on successful paths)
ReadOnlySpan<char> ibanSpan = "DE89370400440532013000".AsSpan();
var result = IbanValidator.Validate(ibanSpan);  // Returns IbanValidationResult

if (result.IsValid)
{
    Console.WriteLine($"Valid {result.CountryCode} IBAN");
}

// 3. Iranian Sheba (IR) — 26 chars, strict national format
Iban.TryParse("IR020120000000001234567890", out var sheba);
// sheba.CountryCode == "IR", sheba.Value == "IR020120000000001234567890"
```

> `Iban` is a `readonly record struct` implementing `IParsable<Iban>`, `ISpanParsable<Iban>`, `IFormattable`, `IEquatable<Iban>`, and `IComparable<Iban>`. It works seamlessly with `string.Create()`, `IParsable.Parse()`, and standard formatting.

---

## Ecosystem Integrations

### Dependency Injection

Register BankSharp with the .NET service collection:

```csharp
// Program.cs
builder.Services.AddBankSharp();  // Registers IIbanValidator as Singleton
```

Inject and use `IIbanValidator`:

```csharp
public class PaymentService
{
    private readonly IIbanValidator _ibanValidator;

    public PaymentService(IIbanValidator ibanValidator)
    {
        _ibanValidator = ibanValidator;
    }

    public bool Process(string ibanString)
    {
        return _ibanValidator.IsValid(ibanString);
    }
    
    public bool TryParse(string input, out Iban iban)
    {
        return _ibanValidator.TryParse(input, out iban);
    }
    
    public IbanValidationResult Validate(string input)
    {
        return _ibanValidator.Validate(input);
    }
}
```

---

### ASP.NET Core OpenAPI

Register OpenAPI schema transformers to automatically describe `Iban` types with correct patterns, regex, and examples in Swagger/Scalar:

```csharp
// Program.cs (.NET 9 / .NET 10 - Microsoft.AspNetCore.OpenApi)
builder.Services.AddOpenApi(options =>
{
    options.AddIbanSupport();  // or options.AddIbanSchemaTransformer()
});

// Program.cs (.NET 8 - Swashbuckle.AspNetCore)
builder.Services.AddSwaggerGen(options =>
{
    options.AddIbanSupport();  // or options.AddIbanSchemaTransformer()
});
```

This applies:
- `format: "iban"` with `minLength: 15`, `maxLength: 34`
- Regex pattern per country (e.g., `^IR\d{24}$` for Iran)
- Example value: `IR660170000000123456789012`

---

### Entity Framework Core

Use strongly-typed `Iban` value objects directly in your domain entities:

```csharp
using BankSharp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BankAccount
{
    public int Id { get; set; }
    public Iban AccountIban { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BankAccount> Accounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Option 1: Auto-apply IbanValueConverter to ALL Iban properties
        modelBuilder.UseIbanConversions();

        // Option 2: Manual configuration for specific properties
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.Property(e => e.AccountIban)
                  .HasConversion<IbanValueConverter>()
                  .HasMaxLength(34);
        });
    }
}
```

---

### FluentValidation

Validate models fluently with zero string allocations on the hot path:

```csharp
using BankSharp.FluentValidation;
using FluentValidation;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        // General IBAN validation (any supported country)
        RuleFor(x => x.TargetIban)
            .MustBeValidIban()
            .WithMessage("Invalid IBAN provided.");

        // Restrict to specific countries (e.g., Iranian Sheba only)
        RuleFor(x => x.DomesticSheba)
            .MustBeValidIban(allowedCountries: ["IR"])
            .WithMessage("Only Iranian Sheba accounts are accepted.");

        // Works with string, Iban, or Iban? property types
        RuleFor(x => x.OptionalIban)
            .MustBeValidIban(allowedCountries: ["DE", "FR", "GB"]);
    }
}
```

---

## Extending Validation Rules

Custom BBAN validation rules can be added per country using `CountryRule`. The registry is **lock-free** and thread-safe (atomic CAS via `Interlocked.CompareExchange` on a volatile snapshot).

```csharp
using BankSharp.Registry;

var customRule = new CountryRule
{
    CountryCode = "XX",
    TotalLength = 20,
    Pattern = @"^XX\d{2}[A-Z0-9]{16}$",  // Full IBAN regex: XX + 2 check digits + 16-char BBAN
    BankCodeOffset = 4,
    BankCodeLength = 4,
    AccountNumberOffset = 8,
    AccountNumberLength = 12
    // BranchCodeOffset/Length optional (set to 0 if not used)
};

IbanRegistry.RegisterCustomRule(customRule);

// Now validation works for the new country (valid Mod97 check digits = 03)
var result = IbanValidator.Validate("XX03ABCD123456789012");
// result.IsValid == true
```

> **Required fields for parsing:** `BankCodeLength > 0` enables `IbanParser` / `Iban.Deconstruct()` support.

---

## Performance & Benchmarks

> **Environment:** BenchmarkDotNet **v0.15.8**, **.NET 10.0.12** (x64), Intel Core i7-10510U CPU 1.80GHz (Max 2.30GHz), Windows 10 22H2.

### Validation & Parsing (German IBAN, 22 chars)

| Method | Mean | Ratio | Allocated |
| :--- | :---: | :---: | :---: |
| **BankSharp `Validate(ReadOnlySpan<char>)`** | **89.02 ns** | **1.00x** | **0 B** |
| BankSharp `Validate(string)` | 92.55 ns | 1.04x | 0 B |
| **IbanNet `Validate`** | 150.30 ns | 1.69x | 56 B |
| **IbanNet `Parse` (ToElectronic)** | 222.03 ns | 2.49x | 160 B |

### Print Formatting (German IBAN, 22 chars)

| Method | Mean | Ratio | Allocated |
| :--- | :---: | :---: | :---: |
| **BankSharp `IbanFormatter.TryToPrintFormat` (Span, zero-alloc)** | **91.75 ns** | **1.00x** | **0 B** |
| **BankSharp `ToPrintFormat()` (allocating)** | **108.62 ns** | **1.18x** | **80 B** |
| **IbanNet `ToPrintFormat`** | 526.20 ns | 5.74x | 1,008 B |

### Key Takeaways

- **Validation:** BankSharp is **~1.7x faster** than IbanNet with **zero allocations** (vs 56 B).
- **Formatting:** BankSharp's **zero-allocation `TryToPrintFormat` is ~5.7x faster** than IbanNet's allocating equivalent, and even the allocating `ToPrintFormat()` is **~4.8x faster** with **~12.6x less memory** (80 B vs 1,008 B).
- **Longer IBANs (FR, 27 chars normalized):** Gap widens — BankSharp `Validate` at **27 ns** vs IbanNet at **82 ns** (3x faster).

[View full benchmark artifact →](BankSharp.Benchmarks/BenchmarkDotNet.Artifacts/results/BankSharp.Benchmarks.IbanNetComparisonBenchmark-report-github.md)

### Run Benchmarks Locally

```bash
dotnet run -c Release --project BankSharp.Benchmarks/BankSharp.Benchmarks.csproj
```

---

### Performance Characteristics

- **Zero-Allocation Hot Paths:** `IbanValidator.Validate(ReadOnlySpan<char>)` allocates 0 B on successful execution paths. Failure paths involving an unsupported country code may allocate via `ToString()`.
- **Allocation-Free Formatting:** `IbanFormatter.TryNormalize`, `IbanFormatter.TryToPrintFormat`, `IbanFormatter.TryToMaskedFormat`, and `IbanFormatter.TryFormat` write directly to destination spans with zero heap allocation.
- **Allocating APIs:** Convenience methods returning a new `string` (such as `IbanParser.Parse`, `IbanFormatter.ToPrintFormat`, and `IbanFormatter.Normalize`) allocate heap memory to construct the returned string.


## Security & Hardening

| Mechanism | Implementation |
|-----------|----------------|
| **Bounded Stack Buffers** | All `stackalloc` buffers capped at `MaxIbanLength = 34` — prevents `StackOverflowException`. |
| **DoS Input Truncation** | Public entry points enforce `MaxAllowedInputLength = 128` — guards payment gateways against massive payloads. |
| **ReDoS Immunity** | Mod97 calculation and validation bypass regular expressions entirely on hot paths; regex used only in `IbanBuilder.Build()` for generated IBAN verification. |
| **Native AOT & Trimming** | Explicitly tagged with `<IsAotCompatible>true</IsAotCompatible>` and `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`. No reflection, no `DynamicCode`. |
| **Input Validation** | Early-exit checks for null, whitespace, minimum length (5), valid ASCII letters/digits before registry lookup. |

---

## Supported Countries

BankSharp includes a high-speed `FrozenDictionary` registry for key global and regional economic corridors:

| Country | Code | Length | Example |
|---------|------|--------|---------|
| **Iran (Sheba)** | `IR` | 26 | `IR020120000000001234567890` |
| Germany | `DE` | 22 | `DE89370400440532013000` |
| United Kingdom | `GB` | 22 | `GB29NWBK60161331926819` |
| France | `FR` | 27 | `FR1420041010050500013M02606` |
| Italy | `IT` | 27 | `IT60X0542811101000000123456` |
| Spain | `ES` | 24 | `ES9121000418450200051332` |
| Netherlands | `NL` | 18 | `NL91ABNA0417164300` |
| Portugal | `PT` | 25 | `PT50000201231234567890154` |
| Switzerland | `CH` | 21 | `CH9300762011623852957` |
| Turkey | `TR` | 26 | `TR330006100519786457841326` |
| United Arab Emirates | `AE` | 23 | `AE070331234567890123456` |

> Need a country not listed? Use `IbanRegistry.RegisterCustomRule()` (see [Extending Validation Rules](#extending-validation-rules)).

---

## Contributing & License

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines (coding style, test requirements, benchmark verification).

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

---

**BankSharp** — High-performance IBAN engine with zero-allocation hot paths for the modern .NET stack. ⚡