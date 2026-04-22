using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Abstractions.Profiles;

public abstract class MappingProfile
{
    public abstract void Configure(IMappingConfiguration configuration);
}