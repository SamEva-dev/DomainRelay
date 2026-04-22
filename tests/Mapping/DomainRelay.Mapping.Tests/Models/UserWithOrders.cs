namespace DomainRelay.Mapping.Tests.Models;

public sealed class UserWithOrders
{
    public List<Order> Orders { get; set; } = new();
}