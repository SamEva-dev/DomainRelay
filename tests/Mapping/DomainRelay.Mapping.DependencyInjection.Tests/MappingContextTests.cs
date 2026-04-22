using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Resolvers;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.DependencyInjection.Tests;

public sealed class MappingContextTests
{
    [Fact]
    public void Mapper_Should_Pass_Items_To_Context_Resolver()
    {
        var services = new ServiceCollection();

        services.AddSingleton<DisplayNameResolver>();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<PersonProfile>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IObjectMapper>();

        var result = mapper.Map<PersonSource, PersonDestination>(
            new PersonSource
            {
                FirstName = "Sam",
                LastName = "Fokam"
            },
            opts => { opts.Items["prefix"] = "Mr"; });

        result.DisplayName.Should().Be("Mr Sam Fokam");
    }

    private sealed class PersonProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<PersonSource, PersonDestination>()
                .ForMember(d => d.DisplayName, o => o.ResolveUsingContext<DisplayNameResolver>());
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

    private sealed class DisplayNameResolver : IContextValueResolver<PersonSource, PersonDestination, string?>
    {
        public string? Resolve(PersonSource source, PersonDestination destination, IMappingContext context)
        {
            var prefix = context.Items.TryGetValue("prefix", out var value) ? value?.ToString() : null;
            prefix = string.IsNullOrWhiteSpace(prefix) ? null : prefix!.Trim();

            return string.IsNullOrWhiteSpace(prefix)
                ? $"{source.FirstName} {source.LastName}".Trim()
                : $"{prefix} {source.FirstName} {source.LastName}".Trim();
        }
    }
}