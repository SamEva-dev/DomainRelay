namespace DomainRelay.Mapping.Tests.Models;

public sealed class UserWithAddress
{
    public string FirstName { get; set; } = string.Empty;
    public Address? Address { get; set; }
}