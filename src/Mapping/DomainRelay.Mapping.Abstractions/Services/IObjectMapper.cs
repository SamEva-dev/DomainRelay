using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Abstractions.Services;

public interface IObjectMapper
{
    TDestination Map<TDestination>(object source);

    TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> options);

    TDestination Map<TSource, TDestination>(TSource source);

    TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> options);

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> options);

    object? Map(object? source, Type sourceType, Type destinationType);

    object? Map(object? source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> options);

    object? Map(object? source, object destination, Type sourceType, Type destinationType);

    object? Map(object? source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> options);
}