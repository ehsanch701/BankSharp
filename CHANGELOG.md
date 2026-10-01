# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](httpskeepachangelog.comen1.1.0),
and this project adheres to [Semantic Versioning](httpssemver.orgspecv2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-09-30
### Added
- Core IBAN domain types (`Iban`, `IbanBuilder`, `IbanParser`, `IbanFormatter`).
- High-performance, zero-allocation `Mod97Engine` using SIMDSpan optimizations.
- Multi-targeting support for `.NET 8.0`, `.NET 9.0`, and `.NET 10.0`.
- Native FluentValidation integration (`BankSharp.FluentValidation`).
- Entity Framework Core Value Converters and ModelBuilder extensions (`BankSharp.EntityFrameworkCore`).
- Microsoft Dependency Injection extensions (`BankSharp.DependencyInjection`).
- OpenAPI schema transformation support for Swashbuckle and Microsoft.AspNetCore.OpenApi (`BankSharp.OpenApi`).
