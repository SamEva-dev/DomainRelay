using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Generation;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.Cache;
using DomainRelay.Mapping.Collections;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Diagnostics;
using DomainRelay.Mapping.Planning;
using DomainRelay.Mapping.Resolution;
using DomainRelay.Mapping.Validation;

namespace DomainRelay.Mapping.Engine;

internal sealed class ObjectMapper : IObjectMapper
{
    private readonly MappingConfiguration _configuration;
    private readonly TypeMapFactory _typeMapFactory;
    private readonly MappingPlanBuilder _planBuilder;
    private readonly CompiledMappingPlanBuilder _compiledPlanBuilder = new();
    private readonly MappingPlanCache _planCache;
    private readonly CompiledMappingPlanCache _compiledMappingPlanCache;
    private readonly MappingValidator _validator;
    private readonly TypeConverterRegistry _typeConverterRegistry;
    private readonly ICollectionMapper _collectionMapper;
    private readonly IDictionaryMapper _dictionaryMapper;
    private readonly MappingRuntimeOptions _runtimeOptions;
    private readonly IMappingDiagnosticsCollector _diagnosticsCollector;
    private readonly IGeneratedMappingRegistry? _generatedMappingRegistry;
    private readonly IServiceProvider? _serviceProvider;

    public ObjectMapper(
        MappingConfiguration configuration,
        TypeMapFactory typeMapFactory,
        MappingPlanBuilder planBuilder,
        MappingPlanCache planCache,
        CompiledMappingPlanCache compiledMappingPlanCache,
        MappingValidator validator,
        TypeConverterRegistry typeConverterRegistry,
        ICollectionMapper collectionMapper,
        IDictionaryMapper dictionaryMapper,
        MappingRuntimeOptions runtimeOptions,
        IMappingDiagnosticsCollector diagnosticsCollector,
        IGeneratedMappingRegistry? generatedMappingRegistry = null,
        IServiceProvider? serviceProvider = null)
    {
        _configuration = configuration;
        _typeMapFactory = typeMapFactory;
        _planBuilder = planBuilder;
        _planCache = planCache;
        _compiledMappingPlanCache = compiledMappingPlanCache;
        _validator = validator;
        _typeConverterRegistry = typeConverterRegistry;
        _collectionMapper = collectionMapper;
        _dictionaryMapper = dictionaryMapper;
        _runtimeOptions = runtimeOptions;
        _diagnosticsCollector = diagnosticsCollector;
        _generatedMappingRegistry = generatedMappingRegistry;
        _serviceProvider = serviceProvider;
    }

    public TDestination Map<TDestination>(object source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (TDestination)MapWithContext(source, source.GetType(), typeof(TDestination), new MappingContext(_serviceProvider))!;
    }

    public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        return (TDestination)MapWithContext(source, source.GetType(), typeof(TDestination), context)!;
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (TDestination)MapWithContext(source!, typeof(TSource), typeof(TDestination), new MappingContext(_serviceProvider))!;
    }

    public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        return (TDestination)MapWithContext(source!, typeof(TSource), typeof(TDestination), context)!;
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        MapWithContext(source!, destination!, typeof(TSource), typeof(TDestination), new MappingContext(_serviceProvider));
        return destination;
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        MapWithContext(source!, destination!, typeof(TSource), typeof(TDestination), context);
        return destination;
    }

    public object? Map(object? source, Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(destinationType);

        return MapWithContext(source, sourceType, destinationType, new MappingContext(_serviceProvider));
    }

    public object? Map(object? source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(destinationType);
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        return MapWithContext(source, sourceType, destinationType, context);
    }

    public object? Map(object? source, object destination, Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(destinationType);

        return MapWithContext(source, destination, sourceType, destinationType, new MappingContext(_serviceProvider));
    }

    public object? Map(object? source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(destinationType);
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        return MapWithContext(source, destination, sourceType, destinationType, context);
    }

    private MappingContext CreateContext(Action<IMappingOperationOptions> configure)
    {
        var options = new MappingOperationOptions();
        configure(options);

        return new MappingContext(
            _serviceProvider,
            new Dictionary<string, object?>(options.Items, StringComparer.OrdinalIgnoreCase));
    }

    private object? MapWithContext(object source, Type sourceType, Type destinationType, MappingContext context)
    {
        var inheritanceResolution = InheritanceMapResolver.TryResolveMoreSpecificMap(
            _configuration,
            sourceType,
            source.GetType(),
            destinationType);

        if (inheritanceResolution is not null)
        {
            sourceType = inheritanceResolution.Value.SourceType;
            destinationType = inheritanceResolution.Value.DestinationType;
        }

        if (context.TryGetVisited(source, destinationType, out var existing))
        {
            return existing;
        }

        if (_dictionaryMapper.CanMap(sourceType, destinationType))
        {
            return _dictionaryMapper.MapDictionary(
                source,
                destination: null,
                sourceType,
                destinationType,
                (nestedSource, nestedSourceType, nestedDestinationType) =>
                    MapWithContext(nestedSource, nestedSourceType, nestedDestinationType, context));
        }

        if (_collectionMapper.CanMap(sourceType, destinationType))
        {
            return _collectionMapper.MapCollection(
                source,
                destinationCollection: null,
                sourceType,
                destinationType,
                (nestedSource, nestedSourceType, nestedDestinationType) =>
                    MapWithContext(nestedSource, nestedSourceType, nestedDestinationType, context));
        }

        try
        {
            if (_generatedMappingRegistry is not null &&
                _generatedMappingRegistry.TryGetGeneratedMapper(sourceType, destinationType, out var generatedMapper) &&
                generatedMapper is not null)
            {
                if (_runtimeOptions.EnableDiagnostics)
                {
                    _diagnosticsCollector.Add(new MappingDiagnostic
                    {
                        Category = "GeneratedPlan",
                        Message = "Using source-generated mapper.",
                        SourceType = sourceType,
                        DestinationType = destinationType
                    });
                }

                var generatedResult = generatedMapper(source);
                context.RegisterVisited(source, destinationType, generatedResult);
                return generatedResult;
            }

            var plan = _planCache.GetOrAdd(sourceType, destinationType, () =>
            {
                var typeMap = _typeMapFactory.Create(sourceType, destinationType);
                _validator.Validate(typeMap);
                return _planBuilder.Build(typeMap);
            });

            if (_runtimeOptions.EnableDiagnostics)
            {
                _diagnosticsCollector.Add(new MappingDiagnostic
                {
                    Category = "Plan",
                    Message = "Using mapping plan.",
                    SourceType = sourceType,
                    DestinationType = destinationType
                });
            }

            var compiledPlan = GetCompiledPlan(plan.TypeMap);
            if (TryExecuteCompiledPlan(compiledPlan, source, null, sourceType, destinationType, context, out var compiledResult))
            {
                ApplySimpleUnflattening(source, sourceType, compiledResult!);
                return compiledResult;
            }

            var result = ExecutePlan(plan, source, null, context);
            ApplySimpleUnflattening(source, sourceType, result);
            return result;
        }
        catch (Exception ex)
        {
            HandleFailure("Failed to map object.", sourceType, destinationType, null, ex);
            return null;
        }
    }

    private object? MapWithContext(object source, object destination, Type sourceType, Type destinationType, MappingContext context)
    {
        if (_dictionaryMapper.CanMap(sourceType, destinationType))
        {
            return _dictionaryMapper.MapDictionary(
                source,
                destination,
                sourceType,
                destinationType,
                (nestedSource, nestedSourceType, nestedDestinationType) =>
                    MapWithContext(nestedSource, nestedSourceType, nestedDestinationType, context));
        }

        if (_collectionMapper.CanMap(sourceType, destinationType))
        {
            return _collectionMapper.MapCollection(
                source,
                destination,
                sourceType,
                destinationType,
                (nestedSource, nestedSourceType, nestedDestinationType) =>
                    MapWithContext(nestedSource, nestedSourceType, nestedDestinationType, context));
        }

        try
        {
            var plan = _planCache.GetOrAdd(sourceType, destinationType, () =>
            {
                var typeMap = _typeMapFactory.Create(sourceType, destinationType);
                _validator.Validate(typeMap);
                return _planBuilder.Build(typeMap);
            });

            if (_runtimeOptions.EnableDiagnostics)
            {
                _diagnosticsCollector.Add(new MappingDiagnostic
                {
                    Category = "Plan",
                    Message = "Using mapping plan.",
                    SourceType = sourceType,
                    DestinationType = destinationType
                });
            }

            var compiledPlan = GetCompiledPlan(plan.TypeMap);
            if (TryExecuteCompiledPlan(compiledPlan, source, destination, sourceType, destinationType, context, out var compiledResult))
            {
                ApplySimpleUnflattening(source, sourceType, destination);
                return compiledResult;
            }

            var result = ExecutePlan(plan, source, destination, context);
            ApplySimpleUnflattening(source, sourceType, destination);
            return result;
        }
        catch (Exception ex)
        {
            HandleFailure("Failed to map object into existing destination.", sourceType, destinationType, null, ex);
            return null;
        }
    }

    private CompiledMappingPlan GetCompiledPlan(TypeMap typeMap)
    {
        return _compiledMappingPlanCache.GetOrAdd(
            typeMap.SourceType,
            typeMap.DestinationType,
            () => BuildCompiledPlan(typeMap));
    }

    private CompiledMappingPlan BuildCompiledPlan(TypeMap typeMap)
    {
        if (!_runtimeOptions.EnableFastPathCompilation)
        {
            return CompiledMappingPlan.Unavailable(
                typeMap.SourceType,
                typeMap.DestinationType,
                "Fast-path compilation is disabled.");
        }

        try
        {
            return _compiledPlanBuilder.Build(typeMap);
        }
        catch (Exception ex)
        {
            return CompiledMappingPlan.Unavailable(
                typeMap.SourceType,
                typeMap.DestinationType,
                ex.Message);
        }
    }

    private bool TryExecuteCompiledPlan(
        CompiledMappingPlan compiledPlan,
        object source,
        object? destination,
        Type sourceType,
        Type destinationType,
        MappingContext context,
        out object? result)
    {
        result = null;

        if (!compiledPlan.IsExecutable || compiledPlan.MappingDelegate is null)
        {
            if (_runtimeOptions.EnableDiagnostics)
            {
                _diagnosticsCollector.Add(new MappingDiagnostic
                {
                    Category = "CompiledPlan",
                    Message = $"Compiled mapping plan unavailable: {compiledPlan.FailureReason}",
                    SourceType = sourceType,
                    DestinationType = destinationType
                });
            }

            return false;
        }

        if (_runtimeOptions.EnableDiagnostics)
        {
            _diagnosticsCollector.Add(new MappingDiagnostic
            {
                Category = "CompiledPlan",
                Message = "Using compiled mapping plan.",
                SourceType = sourceType,
                DestinationType = destinationType
            });
        }

        try
        {
            result = compiledPlan.MappingDelegate(source, destination, context);
            return true;
        }
        catch (Exception ex)
        {
            if (_runtimeOptions.EnableDiagnostics)
            {
                _diagnosticsCollector.Add(new MappingDiagnostic
                {
                    Category = "CompiledPlanFallback",
                    Message = $"Compiled mapping plan failed and runtime plan will be used: {ex.Message}",
                    SourceType = sourceType,
                    DestinationType = destinationType
                });
            }

            result = null;
            return false;
        }
    }

    private object ExecutePlan(MappingPlan plan, object source, object? destination, MappingContext context)
    {
        var typeMap = plan.TypeMap;

        var destinationObject = destination ?? CreateDestination(typeMap, source, context);

        context.RegisterVisited(source, typeMap.DestinationType, destinationObject);

        foreach (var beforeAction in typeMap.BeforeMapActions)
        {
            beforeAction(source, destinationObject);
        }

        foreach (var memberMap in typeMap.MemberMaps)
        {
            if (memberMap.Ignored)
            {
                continue;
            }

            if (memberMap.PreCondition is not null && !memberMap.PreCondition(source))
            {
                continue;
            }

            if (memberMap.Condition is not null && !memberMap.Condition(source, destinationObject))
            {
                continue;
            }

            object? rawValue;

            if (memberMap.ResolverType is not null)
            {
                var resolver = _serviceProvider?.GetService(memberMap.ResolverType)
                    ?? throw new InvalidOperationException(
                        $"No service registered for resolver type '{memberMap.ResolverType.FullName}'.");

                var resolveMethod = memberMap.ResolverType.GetMethod("Resolve");

                rawValue = memberMap.UsesContextResolverType
                    ? resolveMethod!.Invoke(resolver, new object[] { source, destinationObject, context })
                    : resolveMethod!.Invoke(resolver, new object[] { source, destinationObject });
            }
            else if (memberMap.ContextValueResolver is not null)
            {
                rawValue = memberMap.ContextValueResolver(source, destinationObject, context);
            }
            else
            {
                rawValue = memberMap.ValueResolver?.Invoke(source);
            }

            object? valueToAssign = rawValue;
            if (valueToAssign is null && memberMap.NullSubstitute is not null)
            {
                valueToAssign = memberMap.NullSubstitute;
            }

            var destinationProperty = memberMap.DestinationProperty;
            var destinationMemberType = destinationProperty.PropertyType;

            if (valueToAssign is not null)
            {
                var valueRuntimeType = valueToAssign.GetType();

                if (!destinationMemberType.IsAssignableFrom(valueRuntimeType))
                {
                    if (_typeConverterRegistry.TryConvert(valueToAssign, valueRuntimeType, destinationMemberType, out var converted))
                    {
                        valueToAssign = converted;
                    }
                    else if (IsSimpleType(destinationMemberType))
                    {
                        throw new InvalidOperationException(
                            $"No converter available for simple destination member '{destinationProperty.DeclaringType?.FullName}.{destinationProperty.Name}' from '{valueRuntimeType.FullName}' to '{destinationMemberType.FullName}'.");
                    }
                    else
                    {
                        var existingDestinationMember = destinationProperty.GetValue(destinationObject);
                        valueToAssign = existingDestinationMember is not null
                            ? MapWithContext(valueToAssign, existingDestinationMember, valueRuntimeType, destinationMemberType, context)
                            : MapWithContext(valueToAssign, valueRuntimeType, destinationMemberType, context);
                    }
                }
                else if (!IsSimpleType(destinationMemberType))
                {
                    var existingDestinationMember = destinationProperty.GetValue(destinationObject);
                    if (existingDestinationMember is not null)
                    {
                        valueToAssign = MapWithContext(valueToAssign, existingDestinationMember, valueRuntimeType, destinationMemberType, context);
                    }
                }
            }

            if (valueToAssign is null)
            {
                if (!destinationMemberType.IsValueType || Nullable.GetUnderlyingType(destinationMemberType) is not null)
                {
                    destinationProperty.SetValue(destinationObject, null);
                }

                continue;
            }

            destinationProperty.SetValue(destinationObject, valueToAssign);
        }

        foreach (var afterAction in typeMap.AfterMapActions)
        {
            afterAction(source, destinationObject);
        }

        return destinationObject;
    }

    private object CreateDestination(TypeMap typeMap, object source, MappingContext context)
    {
        if (typeMap.ConstructionFactory is not null)
        {
            return typeMap.ConstructionFactory(source);
        }

        var sourceType = source.GetType();

        var sourceProperties = sourceType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var memberMaps = typeMap.MemberMaps
            .Where(m => !m.Ignored)
            .ToDictionary(m => m.DestinationProperty.Name, m => m, StringComparer.OrdinalIgnoreCase);

        _configuration.TryGetMap(typeMap.SourceType, typeMap.DestinationType, out var mapExpressionObject);

        var ctorParamDefinitions = mapExpressionObject?
            .GetType()
            .GetProperty("CtorParamMaps", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?
            .GetValue(mapExpressionObject) as System.Collections.IDictionary;

        var ctor = typeMap.DestinationType.GetConstructor(Type.EmptyTypes);
        if (ctor is not null)
        {
            return Activator.CreateInstance(typeMap.DestinationType)!;
        }

        var constructors = typeMap.DestinationType
            .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .ToArray();

        foreach (var candidate in constructors)
        {
            var parameters = candidate.GetParameters();
            var args = new object?[parameters.Length];
            var canUse = true;

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterType = parameter.ParameterType;

                object? rawValue = null;

                if (ctorParamDefinitions is not null && ctorParamDefinitions.Contains(parameter.Name!))
                {
                    var ctorParamDefinition = ctorParamDefinitions[parameter.Name!];
                    var sourceExpressionProperty = ctorParamDefinition?.GetType().GetProperty("SourceExpression");
                    var nullSubstituteProperty = ctorParamDefinition?.GetType().GetProperty("NullSubstitute");

                    var sourceExpression = sourceExpressionProperty?.GetValue(ctorParamDefinition) as System.Linq.Expressions.LambdaExpression;
                    var nullSubstitute = nullSubstituteProperty?.GetValue(ctorParamDefinition);

                    if (sourceExpression is not null)
                    {
                        var compiled = sourceExpression.Compile();
                        rawValue = compiled.DynamicInvoke(source);
                    }

                    if (rawValue is null && nullSubstitute is not null)
                    {
                        rawValue = nullSubstitute;
                    }
                }
                else if (memberMaps.TryGetValue(parameter.Name ?? string.Empty, out var memberMap) && memberMap.ValueResolver is not null)
                {
                    rawValue = memberMap.ValueResolver(source);
                }
                else if (parameter.Name is not null && sourceProperties.TryGetValue(parameter.Name, out var sourceProperty))
                {
                    rawValue = sourceProperty.GetValue(source);
                }
                else if (parameter.HasDefaultValue)
                {
                    rawValue = parameter.DefaultValue;
                }

                if (rawValue is null)
                {
                    if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) is null)
                    {
                        canUse = false;
                        break;
                    }

                    args[i] = null;
                    continue;
                }

                var rawRuntimeType = rawValue.GetType();
                if (parameterType.IsAssignableFrom(rawRuntimeType))
                {
                    args[i] = rawValue;
                    continue;
                }

                if (_typeConverterRegistry.TryConvert(rawValue, rawRuntimeType, parameterType, out var converted))
                {
                    args[i] = converted;
                    continue;
                }

                if (IsSimpleType(parameterType))
                {
                    canUse = false;
                    break;
                }

                args[i] = MapWithContext(rawValue, rawRuntimeType, parameterType, context);
            }

            if (!canUse)
            {
                continue;
            }

            return Activator.CreateInstance(typeMap.DestinationType, args)!;
        }

        throw new InvalidOperationException(
            $"No suitable constructor available for '{typeMap.DestinationType.FullName}' and no construction factory configured.");
    }

    private void HandleFailure(
        string message,
        Type sourceType,
        Type destinationType,
        string? memberName,
        Exception ex)
    {
        if (_runtimeOptions.EnableDiagnostics)
        {
            _diagnosticsCollector.Add(new MappingDiagnostic
            {
                Category = "ExecutionError",
                Message = $"{message} ({ex.Message})",
                SourceType = sourceType,
                DestinationType = destinationType,
                MemberName = memberName
            });
        }

        if (_runtimeOptions.ThrowOnMappingFailure)
        {
            throw new MappingExecutionException(message, sourceType, destinationType, memberName, ex);
        }
    }

    private static void ApplySimpleUnflattening(object source, Type sourceType, object destination)
    {
        var destinationType = destination.GetType();
        var destinationProperties = destinationType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var sourceProperties = sourceType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        foreach (var sourceProperty in sourceProperties)
        {
            if (destinationProperties.ContainsKey(sourceProperty.Name))
            {
                continue;
            }

            var value = sourceProperty.GetValue(source);
            UnflatteningResolver.TryAssign(destination, sourceProperty.Name, value);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        var actualType = Nullable.GetUnderlyingType(type) ?? type;

        return actualType.IsPrimitive
               || actualType.IsEnum
               || actualType == typeof(string)
               || actualType == typeof(decimal)
               || actualType == typeof(DateTime)
               || actualType == typeof(DateTimeOffset)
               || actualType == typeof(Guid)
               || actualType == typeof(TimeSpan);
    }
}
