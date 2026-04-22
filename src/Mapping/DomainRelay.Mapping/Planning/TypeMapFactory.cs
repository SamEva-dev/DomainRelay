using System.Reflection;
using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Internal;
using DomainRelay.Mapping.Resolution;

namespace DomainRelay.Mapping.Planning;

internal sealed class TypeMapFactory
{
    private readonly MappingConfiguration _configuration;

    public TypeMapFactory(MappingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public TypeMap Create(Type sourceType, Type destinationType)
    {
        if (_configuration.TryGetMap(sourceType, destinationType, out var mapExpressionObject))
        {
            if (mapExpressionObject is IMapExpressionBase && sourceType.IsGenericType && destinationType.IsGenericType)
            {
                return CreateOpenGenericTypeMap(sourceType, destinationType);
            }

            var destinationProperties = destinationType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(IsWritableMember)
                .ToArray();

            var sourceProperties = sourceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var typedMethod = typeof(TypeMapFactory)
                .GetMethod(nameof(CreateFromTypedMapExpression), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(sourceType, destinationType);

            return (TypeMap)typedMethod.Invoke(this, new object[] { mapExpressionObject!, sourceProperties, destinationProperties })!;
        }

        return CreateConventionalTypeMap(sourceType, destinationType);
    }

    private TypeMap CreateOpenGenericTypeMap(Type sourceType, Type destinationType)
    {
        var destinationProperties = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(IsWritableMember)
            .ToArray();

        var sourceProperties = sourceType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var memberMaps = new List<MemberMap>();

        foreach (var destinationProperty in destinationProperties)
        {
            if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
            {
                memberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    source => sourceProperty.GetValue(source),
                    contextValueResolver: null,
                    resolverType: null,
                    usesContextResolverType: false,
                    preCondition: null,
                    condition: null,
                    nullSubstitute: null,
                    ignored: false,
                    isExplicit: false));
                continue;
            }

            var flatteningResolver = FlatteningResolver.TryBuildResolver(sourceType, destinationProperty.Name);
            if (flatteningResolver is not null)
            {
                memberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    flatteningResolver,
                    contextValueResolver: null,
                    resolverType: null,
                    usesContextResolverType: false,
                    preCondition: null,
                    condition: null,
                    nullSubstitute: null,
                    ignored: false,
                    isExplicit: false));
            }
        }

        var constructionFactory = ConstructorResolver.TryBuildFactory(sourceType, destinationType);

        return new TypeMap(
            sourceType,
            destinationType,
            constructionFactory,
            new Dictionary<string, CtorParamMapDefinition>(),
            memberMaps,
            Array.Empty<Action<object, object>>(),
            Array.Empty<Action<object, object>>());
    }

    private TypeMap CreateConventionalTypeMap(Type sourceType, Type destinationType)
    {
        var destinationProperties = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(IsWritableMember)
            .ToArray();

        var sourceProperties = sourceType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var conventionalMemberMaps = new List<MemberMap>();

        foreach (var destinationProperty in destinationProperties)
        {
            if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
            {
                conventionalMemberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    source => sourceProperty.GetValue(source),
                    contextValueResolver: null,
                    resolverType: null,
                    usesContextResolverType: false,
                    preCondition: null,
                    condition: null,
                    nullSubstitute: null,
                    ignored: false,
                    isExplicit: false));
                continue;
            }

            var flatteningResolver = FlatteningResolver.TryBuildResolver(sourceType, destinationProperty.Name);
            if (flatteningResolver is not null)
            {
                conventionalMemberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    flatteningResolver,
                    contextValueResolver: null,
                    resolverType: null,
                    usesContextResolverType: false,
                    preCondition: null,
                    condition: null,
                    nullSubstitute: null,
                    ignored: false,
                    isExplicit: false));
            }
        }

        var constructionFactory = ConstructorResolver.TryBuildFactory(sourceType, destinationType);

        return new TypeMap(
            sourceType,
            destinationType,
            constructionFactory,
            new Dictionary<string, CtorParamMapDefinition>(),
            conventionalMemberMaps,
            Array.Empty<Action<object, object>>(),
            Array.Empty<Action<object, object>>());
    }

    private TypeMap CreateFromTypedMapExpression<TSource, TDestination>(
        MapExpression<TSource, TDestination> expression,
        Dictionary<string, PropertyInfo> sourceProperties,
        PropertyInfo[] destinationProperties)
    {
        var memberMaps = new List<MemberMap>();
        var ctorParamMaps = new Dictionary<string, CtorParamMapDefinition>(StringComparer.OrdinalIgnoreCase);
        var beforeMapActions = new List<Action<object, object>>();
        var afterMapActions = new List<Action<object, object>>();
        Func<object, object>? constructionFactory = null;

        ApplyIncludedBaseMaps<TSource, TDestination>(
            expression,
            memberMaps,
            ctorParamMaps,
            beforeMapActions,
            afterMapActions,
            ref constructionFactory);

        foreach (var destinationProperty in destinationProperties)
        {
            if (expression.MemberMaps.TryGetValue(destinationProperty.Name, out var explicitMember))
            {
                memberMaps.RemoveAll(m =>
                    string.Equals(m.DestinationMemberName, destinationProperty.Name, StringComparison.OrdinalIgnoreCase));

                if (explicitMember.Ignored)
                {
                    memberMaps.Add(new MemberMap(
                        destinationProperty.Name,
                        destinationProperty,
                        valueResolver: null,
                        contextValueResolver: null,
                        resolverType: null,
                        usesContextResolverType: false,
                        preCondition: null,
                        condition: null,
                        nullSubstitute: null,
                        ignored: true,
                        isExplicit: true));
                    continue;
                }

                Func<object, object?>? resolver = null;
                Func<object, object, IMappingContext, object?>? contextResolver = null;

                if (explicitMember.SourceResolver is not null)
                {
                    resolver = source => explicitMember.SourceResolver(source, Activator.CreateInstance(typeof(TDestination))!);
                }
                else if (explicitMember.ContextSourceResolver is not null)
                {
                    contextResolver = (source, destination, context) =>
                        explicitMember.ContextSourceResolver(source, destination, context);
                }
                else if (explicitMember.SourceExpression is not null)
                {
                    resolver = LambdaAdapterFactory.AdaptSourceResolver(explicitMember.SourceExpression);
                }

                Func<object, bool>? preCondition = explicitMember.PreCondition;

                Func<object, object, bool>? condition = null;
                if (explicitMember.Condition is not null)
                {
                    condition = LambdaAdapterFactory.AdaptCondition(
                        explicitMember.Condition,
                        typeof(TSource),
                        typeof(TDestination));
                }

                if (explicitMember.ValueConverter is not null && resolver is not null)
                {
                    var previousResolver = resolver;
                    resolver = source =>
                    {
                        var rawValue = previousResolver(source);
                        if (rawValue is null)
                        {
                            return explicitMember.NullSubstitute;
                        }

                        return ApplyValueConverter(explicitMember.ValueConverter, rawValue);
                    };
                }

                memberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    resolver,
                    contextResolver,
                    explicitMember.ResolverType,
                    explicitMember.UsesContextResolverType,
                    preCondition,
                    condition,
                    explicitMember.NullSubstitute,
                    ignored: false,
                    isExplicit: true));
                continue;
            }

            if (memberMaps.Any(m =>
                    string.Equals(m.DestinationMemberName, destinationProperty.Name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
            {
                memberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    source => sourceProperty.GetValue(source),
                    contextValueResolver: null,
                    resolverType: null,
                    usesContextResolverType: false,
                    preCondition: null,
                    condition: null,
                    nullSubstitute: null,
                    ignored: false,
                    isExplicit: false));
                continue;
            }

            var flatteningResolver = FlatteningResolver.TryBuildResolver(typeof(TSource), destinationProperty.Name);
            if (flatteningResolver is not null)
            {
                memberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    flatteningResolver,
                    contextValueResolver: null,
                    resolverType: null,
                    usesContextResolverType: false,
                    preCondition: null,
                    condition: null,
                    nullSubstitute: null,
                    ignored: false,
                    isExplicit: false));
            }
        }

        foreach (var ctorParamMap in expression.CtorParamMaps)
        {
            ctorParamMaps[ctorParamMap.Key] = ctorParamMap.Value;
        }

        if (expression.ConstructionFactory is not null)
        {
            constructionFactory = source => expression.ConstructionFactory((TSource)source)!;
        }
        else
        {
            constructionFactory ??= ConstructorResolver.TryBuildFactory(typeof(TSource), typeof(TDestination));
        }

        beforeMapActions.AddRange(AdaptBeforeMapActions(expression.BeforeMapActions));
        afterMapActions.AddRange(AdaptAfterMapActions(expression.AfterMapActions));

        return new TypeMap(
            typeof(TSource),
            typeof(TDestination),
            constructionFactory,
            ctorParamMaps,
            memberMaps,
            beforeMapActions,
            afterMapActions);
    }

    private void ApplyIncludedBaseMaps<TSource, TDestination>(
        MapExpression<TSource, TDestination> expression,
        List<MemberMap> memberMaps,
        Dictionary<string, CtorParamMapDefinition> ctorParamMaps,
        List<Action<object, object>> beforeMapActions,
        List<Action<object, object>> afterMapActions,
        ref Func<object, object>? constructionFactory)
    {
        foreach (var includedBaseMap in expression.IncludedBaseMaps)
        {
            if (!_configuration.TryGetMap(
                    includedBaseMap.BaseSourceType,
                    includedBaseMap.BaseDestinationType,
                    out var baseMapExpressionObject))
            {
                continue;
            }

            var baseDestinationProperties = includedBaseMap.BaseDestinationType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(IsWritableMember)
                .ToArray();

            var baseSourceProperties = includedBaseMap.BaseSourceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            var typedMethod = typeof(TypeMapFactory)
                .GetMethod(nameof(CreateFromTypedMapExpression), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(includedBaseMap.BaseSourceType, includedBaseMap.BaseDestinationType);

            var baseTypeMap = (TypeMap)typedMethod.Invoke(
                this,
                new object[] { baseMapExpressionObject!, baseSourceProperties, baseDestinationProperties })!;

            foreach (var baseMemberMap in baseTypeMap.MemberMaps)
            {
                var destinationProperty = typeof(TDestination).GetProperty(
                    baseMemberMap.DestinationMemberName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (destinationProperty is null || !IsWritableMember(destinationProperty))
                {
                    continue;
                }

                if (memberMaps.Any(m =>
                        string.Equals(m.DestinationMemberName, destinationProperty.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                memberMaps.Add(new MemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    baseMemberMap.ValueResolver,
                    baseMemberMap.ContextValueResolver,
                    baseMemberMap.ResolverType,
                    baseMemberMap.UsesContextResolverType,
                    baseMemberMap.PreCondition,
                    baseMemberMap.Condition,
                    baseMemberMap.NullSubstitute,
                    baseMemberMap.Ignored,
                    baseMemberMap.IsExplicit));
            }

            var baseCtorParamMapsProperty = baseMapExpressionObject!
                .GetType()
                .GetProperty("CtorParamMaps", BindingFlags.Public | BindingFlags.Instance);

            if (baseCtorParamMapsProperty?.GetValue(baseMapExpressionObject) is IReadOnlyDictionary<string, CtorParamMapDefinition> typedCtorMaps)
            {
                foreach (var kvp in typedCtorMaps)
                {
                    if (!ctorParamMaps.ContainsKey(kvp.Key))
                    {
                        ctorParamMaps[kvp.Key] = kvp.Value;
                    }
                }
            }

            if (constructionFactory is null)
            {
                var baseConstructionFactoryProperty = baseMapExpressionObject!
                    .GetType()
                    .GetProperty("ConstructionFactory", BindingFlags.Public | BindingFlags.Instance);

                var baseConstructionFactory = baseConstructionFactoryProperty?.GetValue(baseMapExpressionObject) as Delegate;
                if (baseConstructionFactory is not null)
                {
                    constructionFactory = source => baseConstructionFactory.DynamicInvoke(source)!;
                }
            }

            beforeMapActions.AddRange(baseTypeMap.BeforeMapActions);
            afterMapActions.AddRange(baseTypeMap.AfterMapActions);
        }
    }

    private static IReadOnlyList<Action<object, object>> AdaptBeforeMapActions<TSource, TDestination>(
        IEnumerable<Action<TSource, TDestination>> actions)
    {
        return actions
            .Select<Action<TSource, TDestination>, Action<object, object>>(action =>
                (src, dest) => action((TSource)src, (TDestination)dest))
            .ToList();
    }

    private static IReadOnlyList<Action<object, object>> AdaptAfterMapActions<TSource, TDestination>(
        IEnumerable<Action<TSource, TDestination>> actions)
    {
        return actions
            .Select<Action<TSource, TDestination>, Action<object, object>>(action =>
                (src, dest) => action((TSource)src, (TDestination)dest))
            .ToList();
    }

    private static object? ApplyValueConverter(object valueConverter, object rawValue)
    {
        var converterType = valueConverter.GetType();
        var method = converterType.GetMethod("Convert");
        return method!.Invoke(valueConverter, new[] { rawValue });
    }

    private static bool IsWritableMember(PropertyInfo property)
    {
        if (property.CanWrite)
        {
            return true;
        }

        var setMethod = property.SetMethod;
        if (setMethod is null)
        {
            return false;
        }

        return setMethod.ReturnParameter
            .GetRequiredCustomModifiers()
            .Any(modifier => modifier == typeof(System.Runtime.CompilerServices.IsExternalInit));
    }
}
