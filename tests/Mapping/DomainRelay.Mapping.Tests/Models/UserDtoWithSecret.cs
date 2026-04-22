namespace DomainRelay.Mapping.Tests.Models;

public sealed class UserDtoWithSecret
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string InternalSecret { get; set; } = string.Empty;
}