using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Resolvers;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.DependencyInjection.Tests;

public sealed class ServiceResolverTests
{
    [Fact]
    public void Mapper_Should_Use_Resolver_From_DI()
    {
        var services = new ServiceCollection();

        services.AddSingleton<FullNameResolver>();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<PersonProfile>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IObjectMapper>();

        var result = mapper.Map<PersonSource, PersonDestination>(new PersonSource
        {
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.DisplayName.Should().Be("Sam Fokam");
    }

    private sealed class PersonProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<PersonSource, PersonDestination>()
                .ForMember(d => d.DisplayName, o => o.ResolveUsing<FullNameResolver>());
        }
    }

    public sealed class PersonSource
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public sealed class PersonDestination
    {
        public string? DisplayName { get; set; }
    }

    private sealed class FullNameResolver : IValueResolver<PersonSource, PersonDestination, string?>
    {
        public string? Resolve(PersonSource source, PersonDestination destination)
        {
            return $"{source.FirstName} {source.LastName}".Trim();
        }
    }
}