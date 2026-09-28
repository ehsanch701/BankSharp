# Contributing to BankSharp

Hi there! We are thrilled that you're interested in contributing to **BankSharp**. Our mission is to build the fastest, most accurate, and most memory-efficient IBAN validation library for the .NET ecosystem.

By contributing to this project, you help us maintain high standards. Please take a moment to review this guide before submitting a Pull Request.

## 🚀 Our Core Philosophy

We are obsessed with performance and precision. If you're planning to contribute code, please keep these principles in mind:

1. **Zero-Allocation First:** Our primary goal is to minimize pressure on the Garbage Collector (GC). Whenever possible, use `Span<T>`, `ReadOnlySpan<T>`, and `Memory<T>`.
2. **Avoid LINQ in Hot Paths:** Performance is our top priority. Do not use LINQ in the critical path (validation/parsing logic). Stick to simple `for` loops and `Span` manipulations.
3. **Clean Architecture & DDD:** BankSharp follows Domain-Driven Design principles. Keep the Domain logic pure and decoupled from infrastructure.
4. **Well-Documented Code:** We have `TreatWarningsAsErrors` enabled to ensure code quality. All public classes and members **must** have XML documentation. Please don't ignore the `CS1591` warnings!

## 🛠 Development Environment

* **SDK:** The project targets .NET 8.0, 9.0, and 10.0. Please ensure you have the latest SDKs installed.
* **Editor:** We recommend **Visual Studio 2022** or **JetBrains Rider**.

## 🔄 Development Workflow

1. **Fork:** Fork this repository to your GitHub account.
2. **Branch:** Create a new branch (e.g., `feature/add-new-country` or `fix/mod97-logic`).
3. **Code & Test:** Implement your changes and **always** add Unit Tests. We use **xUnit** for testing.
4. **Conventional Commits:** Please follow the [Conventional Commits](https://www.conventionalcommits.org/) specification for your commit messages.
   * Examples:
     * `feat: add support for Turkish IBAN`
     * `fix: resolve overflow in Mod97 calculation`
     * `perf: reduce allocations in parser`
5. **Pull Request:** Open a Pull Request, describe your changes, and link any related issues.

## 🧪 Testing

Before submitting your PR, run all tests to ensure stability:

```bash
dotnet test
```

We expect all tests to pass and zero warnings to be present in the project.

---

Thank you for contributing to BankSharp! Let’s build the fastest IBAN validator in the .NET world together! ⚡️
