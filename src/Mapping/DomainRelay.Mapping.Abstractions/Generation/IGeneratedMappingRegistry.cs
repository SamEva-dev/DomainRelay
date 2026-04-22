namespace DomainRelay.Mapping.Abstractions.Generation;

public interface IGeneratedMappingRegistry
{
    bool TryGetGeneratedMapper(Type sourceType, Type destinationType, out Func<object, object>? mapper);
}