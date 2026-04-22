using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

BenchmarkRunner.Run<SimpleObjectMappingBenchmarks>();

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SimpleObjectMappingBenchmarks
{
    private SourceSimple _source = null!;
    private SourceNested _nestedSource = null!;

    private IMapper _autoMapper = null!;
    private IObjectMapper _domainRelayMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        _source = new SourceSimple
        {
            Id = Guid.NewGuid(),
            Name = "Sam",
            Age = 40
        };

        _nestedSource = new SourceNested
        {
            Id = Guid.NewGuid(),
            Name = "Sam",
            Address = new SourceAddress
            {
                City = "Paris",
                Street = "10 rue A"
            }
        };

        var autoConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceSimple, DestinationSimple>();
            cfg.CreateMap<SourceAddress, DestinationAddress>();
            cfg.CreateMap<SourceNested, DestinationNested>();
        });

        _autoMapper = autoConfig.CreateMapper();

        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<SimpleBenchmarkProfile>();
            builder.AddProfile<NestedBenchmarkProfile>();
            builder.ValidateConfigurationOnBuild();
        });

        var provider = services.BuildServiceProvider();
        _domainRelayMapper = provider.GetRequiredService<IObjectMapper>();
    }

    [Benchmark(Baseline = true)]
    public DestinationSimple Manual_Simple()
    {
        return new DestinationSimple
        {
            Id = _source.Id,
            Name = _source.Name,
            Age = _source.Age
        };
    }

    [Benchmark]
    public DestinationSimple AutoMapper_Simple()
    {
        return _autoMapper.Map<DestinationSimple>(_source);
    }

    [Benchmark]
    public DestinationSimple DomainRelay_Simple_PublicApi()
    {
        return _domainRelayMapper.Map<SourceSimple, DestinationSimple>(_source);
    }

    [Benchmark]
    public DestinationNested Manual_Nested()
    {
        return new DestinationNested
        {
            Id = _nestedSource.Id,
            Name = _nestedSource.Name,
            Address = new DestinationAddress
            {
                City = _nestedSource.Address.City,
                Street = _nestedSource.Address.Street
            }
        };
    }

    [Benchmark]
    public DestinationNested AutoMapper_Nested()
    {
        return _autoMapper.Map<DestinationNested>(_nestedSource);
    }

    [Benchmark]
    public DestinationNested DomainRelay_Nested_PublicApi()
    {
        return _domainRelayMapper.Map<SourceNested, DestinationNested>(_nestedSource);
    }

    private sealed class SimpleBenchmarkProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<SourceSimple, DestinationSimple>();
        }
    }

    private sealed class NestedBenchmarkProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<SourceAddress, DestinationAddress>();
            configuration.CreateMap<SourceNested, DestinationNested>();
        }
    }
}

public sealed class SourceSimple
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public sealed class DestinationSimple
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public sealed class SourceAddress
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

public sealed class DestinationAddress
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}

public sealed class SourceNested
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SourceAddress Address { get; set; } = new();
}

public sealed class DestinationNested
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DestinationAddress Address { get; set; } = new();
}