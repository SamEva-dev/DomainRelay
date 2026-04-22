using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;

namespace DomainRelay.Mapping.Sample.AutoMapperMigration;

public sealed class SourceUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public sealed class DestinationUser
{
    public string FullName { get; set; } = string.Empty;
}

public sealed class MigrationProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<SourceUser, DestinationUser>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));
    }
}