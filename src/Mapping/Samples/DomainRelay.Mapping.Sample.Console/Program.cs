using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Sample.ConsoleApp;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDomainRelayMapping(builder =>
{
    builder.AddProfile<UserProfile>();
});

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IObjectMapper>();

var result = mapper.Map<User, UserDto>(new User
{
    Id = Guid.NewGuid(),
    FirstName = "Sam",
    LastName = "Fokam"
});

Console.WriteLine(result.Id);
Console.WriteLine(result.FullName);