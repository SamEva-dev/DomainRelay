# DomainRelay.Validation

FluentValidation integration for DomainRelay via a pipeline behavior.

## Install

- `DomainRelay.Validation`

## Register

```csharp
using DomainRelay.Validation;

services.AddDomainRelayValidation();
```

## Register validators

```csharp
using FluentValidation;

services.AddTransient<IValidator<CreateUser>, CreateUserValidator>();
```
