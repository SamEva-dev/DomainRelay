# DomainRelay.Mapping

> Package README for the DomainRelay.Mapping NuGet packages. Place this file at the repository root as `README.md`. The mapping `.csproj` files include `..\..\..\README.md` as the NuGet `PackageReadmeFile`, so the root README is the one published on NuGet.


High-performance object mapping for the DomainRelay ecosystem.

`DomainRelay.Mapping` is a lightweight mapping stack for .NET applications that need explicit, testable and predictable object mapping without depending on AutoMapper.

It supports:

- simple object-to-object mapping
- custom member mapping with lambda expressions
- enum mapping
- nested object mapping
- list, array and collection mapping
- dictionary mapping
- constructor and record mapping
- reverse mapping
- conditional mapping
- null substitution
- custom resolvers and converters
- queryable projection
- destination-to-source expression translation
- optional source-generated fast paths for simple mappings

---

## Packages

Core packages:

```bash
dotnet add package DomainRelay.Mapping.Abstractions
dotnet add package DomainRelay.Mapping
dotnet add package DomainRelay.Mapping.DependencyInjection
```

Optional packages:

```bash
dotnet add package DomainRelay.Mapping.Expressions
dotnet add package DomainRelay.Mapping.SourceGen
```

In SDK-style projects, the package is packed as a Roslyn analyzer under `analyzers/dotnet/cs`. No runtime reference is required from your application code.

Package roles:

| Package | Role |
|---|---|
| `DomainRelay.Mapping.Abstractions` | Public contracts: `IObjectMapper`, `MappingProfile`, configuration APIs, resolvers, converters. |
| `DomainRelay.Mapping` | Runtime mapping engine. |
| `DomainRelay.Mapping.DependencyInjection` | `IServiceCollection` integration. |
| `DomainRelay.Mapping.Expressions` | `ProjectTo`, `WhereTranslated`, `OrderByTranslated`. |
| `DomainRelay.Mapping.SourceGen` | Optional Roslyn source generator for very simple same-name/same-type mappings. |

Supported target frameworks:

- Runtime packages: `net8.0`, `net9.0`
- Source generator package: `netstandard2.0` analyzer package

---

## Quick start

### 1. Create your source and destination models

```csharp
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
```

### 2. Create a mapping profile

```csharp
using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;

public sealed class UserProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<User, UserDto>();
    }
}
```

### 3. Register DomainRelay.Mapping

```csharp
using DomainRelay.Mapping.DependencyInjection.Extensions;

builder.Services.AddDomainRelayMapping(mapping =>
{
    mapping.AddProfile<UserProfile>();

    // Recommended in production startup/tests: fail fast when a mapping is invalid.
    mapping.ValidateConfigurationOnBuild();
});
```

You can also scan profiles from an assembly:

```csharp
builder.Services.AddDomainRelayMapping(mapping =>
{
    mapping.AddProfilesFromAssemblyContaining<UserProfile>();
    mapping.ValidateConfigurationOnBuild();
});
```

### 4. Use `IObjectMapper`

```csharp
using DomainRelay.Mapping.Abstractions.Services;

public sealed class UserService
{
    private readonly IObjectMapper _mapper;

    public UserService(IObjectMapper mapper)
    {
        _mapper = mapper;
    }

    public UserDto GetDto(User user)
    {
        return _mapper.Map<User, UserDto>(user);
    }
}
```

---

## API overview

`IObjectMapper` exposes strongly typed and dynamic mapping methods:

```csharp
TDestination Map<TDestination>(object source);
TDestination Map<TSource, TDestination>(TSource source);
TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

object? Map(object? source, Type sourceType, Type destinationType);
object? Map(object? source, object destination, Type sourceType, Type destinationType);
```

Each method also has an overload accepting mapping operation options:

```csharp
var dto = mapper.Map<User, UserDto>(user, options =>
{
    options.Items["CurrentUserId"] = currentUserId;
});
```

---


## Minimal model used in examples

Some snippets below reuse these models and add extra properties when needed:

```csharp
public sealed class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string InternalComment { get; set; } = string.Empty;
    public Guid? MappedByUserId { get; set; }
}
```

# Runtime mapping examples

## 1. Simple mapping

If source and destination members have the same name and compatible types, no custom configuration is required.

```csharp
public sealed class Property
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
}

public sealed class PropertyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyRent { get; set; }
}

public sealed class PropertyProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<Property, PropertyDto>();
    }
}

var dto = mapper.Map<Property, PropertyDto>(property);
```

---

## 2. Custom member mapping with lambda expression

Use `ForMember(...).MapFrom(...)` when the destination member does not map directly by convention.

```csharp
public sealed class Tenant
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class TenantDto
{
    public string FullName { get; set; } = string.Empty;
}

public sealed class TenantProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<Tenant, TenantDto>()
            .ForMember(d => d.FullName, o =>
                o.MapFrom(s => s.FirstName + " " + s.LastName));
    }
}
```

---

## 3. Ignore a destination member

Use `Ignore(...)` when a destination property must not be mapped.

```csharp
public sealed class UserProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<User, UserDto>()
            .Ignore(d => d.InternalComment);
    }
}
```

Equivalent form:

```csharp
configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.InternalComment, o => o.Ignore());
```

---

## 4. Null substitution

Use `NullSubstitute(...)` to replace `null` with a default value.

```csharp
configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.DisplayName, o =>
    {
        o.MapFrom(s => s.Nickname);
        o.NullSubstitute("Unknown");
    });
```

---

## 5. Conditional mapping

`PreCondition` runs before resolving the destination value.

`Condition` runs with the source and destination instances.

```csharp
configuration.CreateMap<Property, PropertyDto>()
    .ForMember(d => d.MonthlyRent, o =>
    {
        o.PreCondition(s => s.MonthlyRent > 0);
        o.MapFrom(s => s.MonthlyRent);
    })
    .ForMember(d => d.Name, o =>
    {
        o.Condition((source, destination) => !string.IsNullOrWhiteSpace(source.Name));
        o.MapFrom(s => s.Name);
    });
```

---

## 6. Enum mapping

DomainRelay.Mapping registers built-in enum converters:

- enum to enum
- enum to string
- string to enum by name
- number to enum

```csharp
public enum LeaseStatus
{
    Draft = 0,
    Active = 1,
    Terminated = 2
}

public enum LeaseStatusDto
{
    Draft = 0,
    Active = 1,
    Terminated = 2
}

public sealed class Lease
{
    public LeaseStatus Status { get; set; }
}

public sealed class LeaseDto
{
    public LeaseStatusDto Status { get; set; }
}

configuration.CreateMap<Lease, LeaseDto>();

var dto = mapper.Map<Lease, LeaseDto>(new Lease
{
    Status = LeaseStatus.Active
});
```

Enum to string:

```csharp
public sealed class LeaseStatusView
{
    public string Status { get; set; } = string.Empty;
}

configuration.CreateMap<Lease, LeaseStatusView>();
```

String to enum:

```csharp
public sealed class UpdateLeaseStatusRequest
{
    public string Status { get; set; } = "Active";
}

public sealed class UpdateLeaseStatusCommand
{
    public LeaseStatus Status { get; set; }
}

configuration.CreateMap<UpdateLeaseStatusRequest, UpdateLeaseStatusCommand>();
```

---

## 7. Nested object mapping

When a member is another complex object, create a map for the nested type too.

```csharp
public sealed class Address
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class AddressDto
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class Property
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}

public sealed class PropertyDto
{
    public string Name { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
}

public sealed class PropertyProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<Address, AddressDto>();
        configuration.CreateMap<Property, PropertyDto>();
    }
}
```

Usage:

```csharp
var dto = mapper.Map<Property, PropertyDto>(property);
```

---

## 8. List, array and collection mapping

DomainRelay.Mapping can map collections when it knows how to map the element type.

```csharp
configuration.CreateMap<Tenant, TenantDto>();

List<TenantDto> dtos = mapper.Map<List<Tenant>, List<TenantDto>>(tenants);
TenantDto[] array = mapper.Map<List<Tenant>, TenantDto[]>(tenants);
IEnumerable<TenantDto> enumerable = mapper.Map<List<Tenant>, IEnumerable<TenantDto>>(tenants);
```

Mapping into an existing list clears and refills the destination list:

```csharp
var destination = new List<TenantDto>();
mapper.Map<List<Tenant>, List<TenantDto>>(tenants, destination);
```

---

## 9. Dictionary mapping

### Object to dictionary

```csharp
var dictionary = mapper.Map<User, Dictionary<string, object?>>(user);

Console.WriteLine(dictionary["FirstName"]);
Console.WriteLine(dictionary["LastName"]);
```

### Dictionary to object

```csharp
var values = new Dictionary<string, object?>
{
    ["Id"] = Guid.NewGuid(),
    ["FirstName"] = "Sam",
    ["LastName"] = "Fokam"
};

var user = mapper.Map<Dictionary<string, object?>, User>(values);
```

Dictionary keys are matched against destination property names using case-insensitive lookup.

---

## 10. Complex mapping example

This example combines custom lambdas, nested objects, lists, enum conversion, ignored members and lifecycle hooks.

```csharp
public enum BookingStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public sealed class Booking
{
    public Guid Id { get; set; }
    public BookingStatus Status { get; set; }
    public Tenant Tenant { get; set; } = new();
    public Address PropertyAddress { get; set; } = new();
    public List<BookingLine> Lines { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class BookingLine
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class BookingDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TenantFullName { get; set; } = string.Empty;
    public AddressDto PropertyAddress { get; set; } = new();
    public List<BookingLineDto> Lines { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Trace { get; set; } = string.Empty;
}

public sealed class BookingLineDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class BookingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<Tenant, TenantDto>()
            .ForMember(d => d.FullName, o =>
                o.MapFrom(s => s.FirstName + " " + s.LastName));

        configuration.CreateMap<Address, AddressDto>();
        configuration.CreateMap<BookingLine, BookingLineDto>();

        configuration.CreateMap<Booking, BookingDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.TenantFullName, o =>
                o.MapFrom(s => s.Tenant.FirstName + " " + s.Tenant.LastName))
            .ForMember(d => d.TotalAmount, o =>
                o.MapFrom(s => s.Lines.Sum(line => line.Amount)))
            .BeforeMap((source, destination) => destination.Trace = "before")
            .AfterMap((source, destination) => destination.Trace += "|after");
    }
}
```

---

## 11. Mapping to an existing destination object

Use this when updating an existing DTO/model instance.

```csharp
var existingDto = new UserDto
{
    Id = user.Id,
    FirstName = "Old",
    LastName = "Value"
};

mapper.Map<User, UserDto>(user, existingDto);
```

---

## 12. Constructor and record mapping

Use `ConstructUsing(...)` when the destination type is immutable or requires custom construction.

```csharp
public sealed record UserSummaryDto(Guid Id, string FullName);

configuration.CreateMap<User, UserSummaryDto>()
    .ConstructUsing(s => new UserSummaryDto(
        s.Id,
        s.FirstName + " " + s.LastName));
```

Use `ForCtorParam(...)` when you want to bind constructor parameters individually.

```csharp
public sealed record UserSummary(Guid Id, string FullName);

configuration.CreateMap<User, UserSummary>()
    .ForCtorParam("id", o => o.MapFrom(s => s.Id))
    .ForCtorParam("fullName", o => o.MapFrom(s => s.FirstName + " " + s.LastName));
```

---

## 13. Reverse mapping

Use `ReverseMap()` for simple reversible mappings.

```csharp
configuration.CreateMap<User, UserDto>()
    .ReverseMap();
```

For complex mappings, prefer explicit maps in both directions:

```csharp
configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, o =>
        o.MapFrom(s => s.FirstName + " " + s.LastName));

configuration.CreateMap<UserDto, User>()
    .ForMember(d => d.FirstName, o =>
        o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]))
    .ForMember(d => d.LastName, o =>
        o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty));
```

---

## 14. Custom value converter

Use `IValueConverter<TSourceMember>` when the conversion is reusable for a member value.

```csharp
using DomainRelay.Mapping.Abstractions.Converters;

public sealed class UpperCaseConverter : IValueConverter<string>
{
    public object? Convert(string sourceMember)
    {
        return sourceMember.ToUpperInvariant();
    }
}

configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.FirstName, o =>
    {
        o.MapFrom(s => s.FirstName);
        o.ConvertUsing(new UpperCaseConverter());
    });
```

---

## 15. Custom resolver

Use `IValueResolver<TSource, TDestination, TDestMember>` when the mapping needs both the source and destination objects.

```csharp
using DomainRelay.Mapping.Abstractions.Resolvers;

public sealed class FullNameResolver : IValueResolver<User, UserDto, string>
{
    public string Resolve(User source, UserDto destination)
    {
        return $"{source.FirstName} {source.LastName}".Trim();
    }
}

configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, o =>
        o.ResolveUsing(new FullNameResolver()));
```

Resolver through dependency injection:

```csharp
services.AddTransient<FullNameResolver>();

configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, o =>
        o.ResolveUsing<FullNameResolver>());
```

---

## 16. Context-aware resolver

Use `IContextValueResolver<TSource, TDestination, TDestMember>` when the mapping needs operation-specific data.

```csharp
using DomainRelay.Mapping.Abstractions.Resolvers;
using DomainRelay.Mapping.Abstractions.Services;

public sealed class CurrentUserResolver : IContextValueResolver<User, UserDto, Guid?>
{
    public Guid? Resolve(User source, UserDto destination, IMappingContext context)
    {
        return context.Items.TryGetValue("CurrentUserId", out var value)
            ? value as Guid?
            : null;
    }
}

configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.MappedByUserId, o =>
        o.ResolveUsingContext<CurrentUserResolver>());

var dto = mapper.Map<User, UserDto>(user, options =>
{
    options.Items["CurrentUserId"] = currentUserId;
});
```

---

# Projection and expression translation

Projection support is intentionally stricter than runtime mapping. Use it for provider-translatable mappings over `IQueryable`, for example with EF Core.

## Register expression services

```csharp
using DomainRelay.Mapping.Expressions.Extensions;

builder.Services.AddDomainRelayMapping(mapping =>
{
    mapping.AddProfilesFromAssemblyContaining<UserProfile>();
});

builder.Services.AddDomainRelayMappingExpressions();
```

## ProjectTo

```csharp
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Queryable;

public sealed class UserQueryService
{
    private readonly AppDbContext _db;
    private readonly IProjectionBuilder _projectionBuilder;

    public UserQueryService(AppDbContext db, IProjectionBuilder projectionBuilder)
    {
        _db = db;
        _projectionBuilder = projectionBuilder;
    }

    public Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return _db.Users
            .Where(u => u.IsActive)
            .ProjectTo<User, UserDto>(_projectionBuilder)
            .ToListAsync(cancellationToken);
    }
}
```

## WhereTranslated

Use `WhereTranslated` when your API receives a predicate over the DTO but the database query must run against the entity.

```csharp
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Queryable;

Expression<Func<UserDto, bool>> dtoPredicate = dto => dto.FirstName.StartsWith("S");

var users = await _db.Users
    .WhereTranslated<User, UserDto>(dtoPredicate, translator)
    .ToListAsync(cancellationToken);
```

## OrderByTranslated

```csharp
var users = await _db.Users
    .OrderByTranslated<User, UserDto, string>(dto => dto.LastName, translator)
    .ToListAsync(cancellationToken);
```

Important: projection and expression translation support a strict subset of runtime mapping features. Avoid runtime-only constructs such as custom resolvers, arbitrary method calls or complex non-translatable logic inside queryable projections.

---

# Source generation

`DomainRelay.Mapping.SourceGen` is optional. It generates fast mapping methods for very simple mappings where source and destination properties have the same name and exactly the same type.

## Install

```bash
dotnet add package DomainRelay.Mapping.SourceGen
```

## Add a mapping hint

```csharp
using DomainRelay.Mapping.Abstractions.Generation;

[GenerateMapping(typeof(User), typeof(UserDto))]
public sealed partial class MappingHints
{
}
```

The generator creates:

- `DomainRelay.Mapping.Generated.GeneratedMappings`
- `DomainRelay.Mapping.Generated.GeneratedMappingRegistry`

## Use generated mapping directly

```csharp
using DomainRelay.Mapping.Generated;

var dto = GeneratedMappings.Map_User_To_UserDto(user);
```

## Register generated mappings with runtime mapper

```csharp
builder.Services.AddDomainRelayMapping(mapping =>
{
    mapping.AddProfile<UserProfile>();
    mapping.AddGeneratedMappingsFromAssemblyContaining<MappingHints>();
});
```

Then the normal runtime API can use the generated fast path when available:

```csharp
var dto = mapper.Map<User, UserDto>(user);
```

Current source generator limitation: it only handles simple same-name/same-type property assignments. Use runtime mapping for custom members, nested conversions, resolvers, dictionaries and complex rules.

---

# Recommended usage in Clean Architecture

A common structure is:

```text
YourApp.Domain
YourApp.Application
YourApp.Infrastructure
YourApp.Api
```

Recommended placement:

| Layer | Recommended usage |
|---|---|
| Domain | No dependency on mapping. Keep entities and value objects pure. |
| Application | Define DTOs, commands, queries and mapping profiles. Inject `IObjectMapper` in handlers/services if needed. |
| Infrastructure | Usually no mapping dependency unless mapping persistence models to domain/application models. |
| Api | Register packages and profiles. Map request/response models if needed. |

Example handler:

```csharp
public sealed record GetUserQuery(Guid Id) : IRequest<UserDto>;

public sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _repository;
    private readonly IObjectMapper _mapper;

    public GetUserQueryHandler(IUserRepository repository, IObjectMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return _mapper.Map<User, UserDto>(user);
    }
}
```

---

# Configuration validation

Use validation at startup to detect missing or invalid mappings early.

```csharp
builder.Services.AddDomainRelayMapping(mapping =>
{
    mapping.AddProfilesFromAssemblyContaining<UserProfile>();
    mapping.ValidateConfigurationOnBuild();
});
```

You can also validate manually if you inject `IMappingConfiguration`:

```csharp
using DomainRelay.Mapping.Abstractions.Configuration;

var configuration = serviceProvider.GetRequiredService<IMappingConfiguration>();
configuration.AssertConfigurationIsValid();
```

---

# Error handling

DomainRelay.Mapping exposes mapping-specific exceptions through `DomainRelay.Mapping.Abstractions.Exceptions`:

- `MappingException`
- `MappingConfigurationException`
- `MappingExecutionException`
- `MappingValidationException`

Recommended practice:

```csharp
try
{
    var dto = mapper.Map<User, UserDto>(user);
}
catch (MappingException ex)
{
    // Log mapping failure with source/destination context.
    throw;
}
```

---

# Feature matrix

| Feature | Runtime mapping | Projection / expression translation | Source generator |
|---|---:|---:|---:|
| Same-name property mapping | Yes | Yes | Yes |
| `ForMember(...).MapFrom(...)` | Yes | Limited | No |
| `Ignore(...)` | Yes | Limited | No |
| `NullSubstitute(...)` | Yes | Limited | No |
| `Condition(...)` / `PreCondition(...)` | Yes | No / limited | No |
| Nested object mapping | Yes | Limited | No |
| Collections / arrays | Yes | No / limited | No |
| Dictionaries | Yes | No | No |
| Enum conversion | Yes | Limited | No |
| Custom resolver | Yes | No | No |
| Context-aware resolver | Yes | No | No |
| Constructor mapping | Yes | Limited | No |
| Reverse mapping | Simple cases | No | No |

---

# Migration from AutoMapper

AutoMapper:

```csharp
CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, o =>
        o.MapFrom(s => s.FirstName + " " + s.LastName));
```

DomainRelay.Mapping:

```csharp
configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, o =>
        o.MapFrom(s => s.FirstName + " " + s.LastName));
```

AutoMapper injection:

```csharp
private readonly IMapper _mapper;
```

DomainRelay.Mapping injection:

```csharp
private readonly IObjectMapper _mapper;
```

AutoMapper map:

```csharp
var dto = _mapper.Map<UserDto>(user);
```

DomainRelay.Mapping map:

```csharp
var dto = _mapper.Map<User, UserDto>(user);
```

---

# Best practices

1. Prefer explicit profiles per bounded context or module.
2. Keep domain entities independent from mapping concerns.
3. Use `ValidateConfigurationOnBuild()` in application startup and tests.
4. Use runtime mapping for commands, DTOs and rich object transformations.
5. Use projection only for database queries where the expression must be translated by the LINQ provider.
6. Prefer explicit reverse maps for complex transformations.
7. Use resolvers for reusable business mapping logic, but avoid putting domain rules inside mapping profiles.
8. Keep source generation for simple high-volume maps only.

---

# Complete minimal example

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
    public string FullName { get; set; } = string.Empty;
}

public sealed class UserProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, o =>
                o.MapFrom(s => s.FirstName + " " + s.LastName));
    }
}

var services = new ServiceCollection();

services.AddDomainRelayMapping(mapping =>
{
    mapping.AddProfile<UserProfile>();
    mapping.ValidateConfigurationOnBuild();
});

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IObjectMapper>();

var dto = mapper.Map<User, UserDto>(new User
{
    Id = Guid.NewGuid(),
    FirstName = "Sam",
    LastName = "Fokam"
});

Console.WriteLine(dto.FullName);
```
