using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Sample.AutoMapperMigration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDomainRelayMapping(builder =>
{
    builder.AddProfile<MigrationProfile>();
});

var provider = services.BuildServiceProvider();
var mapper = provider.GetRequiredService<IObjectMapper>();

var result = mapper.Map<SourceUser, DestinationUser>(new SourceUser
{
    FirstName = "Sam",
    LastName = "Fokam"
});

Console.WriteLine(result.FullName);