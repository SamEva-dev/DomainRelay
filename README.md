# DomainRelay

DomainRelay is a lightweight mediator for Clean Architecture:
- Commands/Queries via `Send`
- Domain Events / Notifications via `Publish`
- Pipeline behaviors (validation, transactions, logging, diagnostics)
- No external dependency in core packages

## Packages
- DomainRelay.Abstractions
- DomainRelay
- DomainRelay.DependencyInjection
- DomainRelay.Diagnostics (optional)
- DomainRelay.Validation (optional, FluentValidation integration)
- DomainRelay.EFCore (optional, EF Core transaction behavior)

## Quick start
See `docs/getting-started.md`.
