## Description
<!-- Provide a brief summary of the changes introduced by this PR and the reasoning behind them. -->

Fixes #(issue)

## Type of Change
- [ ] 🐛 Bug fix (non-breaking change fixing an issue)
- [ ] ✨ New feature (non-breaking change adding functionality)
- [ ] 💥 Breaking change (fix or feature causing existing functionality not to work as expected)
- [ ] ⚡ Performance optimization (allocation reduction, SIMD/Span improvements)
- [ ] 📝 Documentation update

## Architecture & Quality Checklist
- [ ] Adheres to the **Zero-Allocation** philosophy where applicable (critical paths avoid LINQ & heap allocations).
- [ ] Unit tests added / updated and all tests pass locally (`dotnet test`).
- [ ] BenchmarkDotNet results included (required for engine / parser performance modifications).
- [ ] XML Documentation added or updated for public APIs.
- [ ] Updated `CHANGELOG.md` under `[Unreleased]` (if applicable).

## Benchmark Results (if applicable)
<!-- Paste BenchmarkDotNet markdown table here if this PR affects performance -->
