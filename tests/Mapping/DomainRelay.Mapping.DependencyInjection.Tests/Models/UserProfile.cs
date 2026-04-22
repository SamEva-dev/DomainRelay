using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;

namespace DomainRelay.Mapping.DependencyInjection.Tests.Models;

public sealed class UserProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<User, UserDto>();
    }
}