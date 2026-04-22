namespace DomainRelay.Mapping.Expressions.Tests.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}