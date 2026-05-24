using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.Tests;

public sealed class DateOnlyMappingTests
{
    private sealed class Source
    {
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public DateOnly? OptionalDate { get; set; }
        public TimeOnly? OptionalTime { get; set; }
    }

    private sealed class Destination
    {
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public DateOnly? OptionalDate { get; set; }
        public TimeOnly? OptionalTime { get; set; }
    }

    [Fact]
    public void Should_map_DateOnly_and_TimeOnly_by_value()
    {
        var services = new ServiceCollection()
            .AddDomainRelayMapping(configuration =>
            {
                configuration.AddProfile<DateOnlyProfile>();
            })
            .BuildServiceProvider();

        var mapper = services.GetRequiredService<IObjectMapper>();

        var source = new Source
        {
            Date = new DateOnly(2026, 5, 24),
            Time = new TimeOnly(14, 30),
            OptionalDate = new DateOnly(2026, 6, 1),
            OptionalTime = new TimeOnly(9, 15)
        };

        var result = mapper.Map<Destination>(source);

        Assert.Equal(source.Date, result.Date);
        Assert.Equal(source.Time, result.Time);
        Assert.Equal(source.OptionalDate, result.OptionalDate);
        Assert.Equal(source.OptionalTime, result.OptionalTime);
    }

    private sealed class DateOnlyProfile : MappingProfile
    {
        public override void Configure(DomainRelay.Mapping.Abstractions.Configuration.IMappingConfiguration configuration)
        {
            configuration.CreateMap<Source, Destination>();
        }
    }
}
