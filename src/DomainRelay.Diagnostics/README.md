# DomainRelay.Diagnostics

Adds an `ActivitySource`-based diagnostics pipeline behavior for DomainRelay.

## Install

- `DomainRelay.Diagnostics`

## Register

```csharp
using DomainRelay.Diagnostics;

services.AddDomainRelayDiagnostics();
```

## What you get

- An `Activity` around each `Send` (and additional tags around handler execution)
- Tags typically include request type, response type, success/failure, and exception metadata

This is compatible with OpenTelemetry (exporters configured in your application).
