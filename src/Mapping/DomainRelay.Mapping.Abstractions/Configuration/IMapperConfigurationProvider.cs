namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface IMapperConfigurationProvider
{
    void AssertConfigurationIsValid();
}