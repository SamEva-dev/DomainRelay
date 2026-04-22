using DomainRelay.Mapping.Tests.Models;

namespace DomainRelay.Mapping.Tests.Models;

public sealed class UserWithAddressDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
}