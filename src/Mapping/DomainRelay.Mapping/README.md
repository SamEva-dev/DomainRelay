# DomainRelay.Mapping

High-performance object mapping for the DomainRelay ecosystem.

`DomainRelay.Mapping` is a modular .NET mapping stack focused on:

- runtime object mapping
- explicit member configuration
- nested objects and collections
- queryable projection
- expression translation
- generated fast paths later in the roadmap

## Packages

- `DomainRelay.Mapping.Abstractions`
- `DomainRelay.Mapping`
- `DomainRelay.Mapping.DependencyInjection`
- `DomainRelay.Mapping.Expressions`

## Installation

```bash
dotnet add package DomainRelay.Mapping
dotnet add package DomainRelay.Mapping.DependencyInjection
```

For projection and translation:

```bash
dotnet add package DomainRelay.Mapping.Expressions
```

## Quick start

```csharp
using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

public sealed class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class UserProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<User, UserDto>();
    }
}

var services = new ServiceCollection();

services.AddDomainRelayMapping(builder =>
{
    builder.AddProfile<UserProfile>();
});

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IObjectMapper>();

var dto = mapper.Map<User, UserDto>(new User
{
    Id = Guid.NewGuid(),
    FirstName = "Sam",
    LastName = "Fokam"
});
```

## Features

### Runtime mapping
- convention-based mapping
- `ForMember`
- `Ignore`
- `MapFrom`
- `Condition`
- `NullSubstitute`
- `ConvertUsing`
- nested mapping
- collections
- dictionaries
- enums
- flattening
- simple reverse map
- constructor and record support

### Expressions
- `ProjectTo`
- expression translation
- `WhereTranslated`
- `OrderByTranslated`

## Documentation

- [Installation](docs/installation.md)
- [Getting started](docs/getting-started.md)
- [Configuration](docs/configuration.md)
- [Runtime mapping](docs/runtime-mapping.md)
- [Projection](docs/projection.md)
- [Expression translation](docs/expression-translation.md)
- [Migration from AutoMapper](docs/migration-from-automapper.md)
- [Limitations](docs/limitations.md)
- [Benchmarks](docs/benchmarks.md)
- [Versioning](docs/versioning.md)

## Important

Projection support and expression translation intentionally support a **strict subset** of runtime features.  
Do not assume that every runtime mapping rule is provider-translatable inside `IQueryable`.

## Status

Current milestone: `0.10.0`

This version includes:
- runtime mapping
- projection
- expression translation
- packaging baseline
- samples
- CI baseline

