using DomainRelay.Mapping.Abstractions.Generation;

namespace DomainRelay.Mapping.Sample.SourceGen;

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

[GenerateMapping(typeof(User), typeof(UserDto))]
public sealed partial class MappingHints
{
}