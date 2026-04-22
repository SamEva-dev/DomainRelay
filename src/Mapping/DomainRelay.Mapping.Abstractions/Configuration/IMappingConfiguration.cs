namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface IMappingConfiguration
{
    IMapExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

    IMapExpressionBase CreateMap(Type sourceType, Type destinationType);

    bool TryGetMap(Type sourceType, Type destinationType, out object? mapExpression);

    void AssertConfigurationIsValid();
}