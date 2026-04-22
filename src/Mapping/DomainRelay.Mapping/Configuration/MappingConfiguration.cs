using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Models;
using DomainRelay.Mapping.Planning;
using DomainRelay.Mapping.Validation;

namespace DomainRelay.Mapping.Configuration;

public sealed class MappingConfiguration : IMappingConfiguration, IMapperConfigurationProvider
{
    private readonly Dictionary<TypePair, object> _closedMaps = new();
    private readonly List<IMapExpressionBase> _openMaps = new();

    public IMapExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var pair = new TypePair(typeof(TSource), typeof(TDestination));

        if (_closedMaps.ContainsKey(pair))
        {
            throw new MappingConfigurationException(
                $"A mapping from '{typeof(TSource).FullName}' to '{typeof(TDestination).FullName}' is already registered.");
        }

        var expression = new MapExpression<TSource, TDestination>();
        _closedMaps[pair] = expression;

        return new MapExpressionWrapper<TSource, TDestination>(this, expression);
    }

    public IMapExpressionBase CreateMap(Type sourceType, Type destinationType)
    {
        if (!sourceType.IsGenericTypeDefinition || !destinationType.IsGenericTypeDefinition)
        {
            throw new MappingConfigurationException(
                "CreateMap(Type, Type) is reserved for open generic registrations.");
        }

        var expression = (IMapExpressionBase)new OpenGenericMapExpression(sourceType, destinationType);
        _openMaps.Add(expression);

        return expression;
    }

    public bool TryGetMap(Type sourceType, Type destinationType, out object? mapExpression)
    {
        if (_closedMaps.TryGetValue(new TypePair(sourceType, destinationType), out mapExpression))
        {
            return true;
        }

        mapExpression = _openMaps.FirstOrDefault(m =>
            sourceType.IsGenericType &&
            destinationType.IsGenericType &&
            sourceType.GetGenericTypeDefinition() == m.SourceType &&
            destinationType.GetGenericTypeDefinition() == m.DestinationType);

        return mapExpression is not null;
    }

    internal IReadOnlyDictionary<TypePair, object> GetAll() => _closedMaps;

    internal IReadOnlyList<IMapExpressionBase> GetOpenMaps() => _openMaps;

    internal IMapExpression<TDestination, TSource> CreateReverseMap<TSource, TDestination>(
        MapExpression<TSource, TDestination> sourceMap)
    {
        var reverse = CreateMap<TDestination, TSource>();

        foreach (var member in sourceMap.MemberMaps.Values)
        {
            ReverseMemberConfigurator.TryConfigureReverseMember<TSource, TDestination>(reverse, member);
        }

        return reverse;
    }

    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();
        var typeMapFactory = new TypeMapFactory(this);
        var validator = new MappingValidator();

        foreach (var map in _closedMaps.Keys)
        {
            try
            {
                var typeMap = typeMapFactory.Create(map.SourceType, map.DestinationType);
                validator.Validate(typeMap);
            }
            catch (Exception ex)
            {
                errors.Add(
                    $"Mapping '{map.SourceType.FullName}' -> '{map.DestinationType.FullName}' is invalid: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            throw new MappingValidationException(errors);
        }
    }
}