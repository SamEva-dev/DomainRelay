
## `docs/getting-started.md`

```md id="h2i3j4"
# Getting started

## 1. Create models

```csharp
public sealed class User
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class UserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

2. Create a profile
public sealed class UserProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<User, UserDto>();
    }
}
3. Register DI
services.AddDomainRelayMapping(builder =>
{
    builder.AddProfile<UserProfile>();
});
4. Map
var dto = mapper.Map<User, UserDto>(user);