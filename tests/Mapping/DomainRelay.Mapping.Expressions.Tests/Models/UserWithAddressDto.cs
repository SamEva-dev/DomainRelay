namespace DomainRelay.Mapping.Expressions.Tests.Models;

public sealed class UserWithAddressDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
}
