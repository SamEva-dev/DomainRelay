namespace DomainRelay.Mapping.Tests.Models;

public sealed class UserWithOrdersDto
{
    public List<OrderDto> Orders { get; set; } = new();
}