
## `docs/configuration.md`

```md id="k5l6m7"
# Configuration

## Supported runtime configuration

- `CreateMap<TSource, TDestination>()`
- `ForMember(...)`
- `Ignore(...)`
- `MapFrom(...)`
- `Condition(...)`
- `NullSubstitute(...)`
- `ConvertUsing(...)`
- `ConstructUsing(...)`
- `BeforeMap(...)`
- `AfterMap(...)`
- `ReverseMap()` simple

## Example

```csharp
configuration.CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName))
    .ForMember(d => d.Nickname, o =>
    {
        o.MapFrom(s => s.Nickname);
        o.NullSubstitute("Unknown");
    })
    .BeforeMap((src, dest) => dest.Trace = "before")
    .AfterMap((src, dest) => dest.Trace += "|after");