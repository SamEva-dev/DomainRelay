using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Planning;

namespace DomainRelay.Mapping.Validation;

internal sealed class MappingValidator
{
    public void Validate(TypeMap typeMap)
    {
        var errors = new List<string>();

        ValidateConstruction(typeMap, errors);
        ValidateMembers(typeMap, errors);

        if (errors.Count > 0)
        {
            throw new MappingValidationException(errors);
        }
    }

    private static void ValidateConstruction(TypeMap typeMap, List<string> errors)
    {
        if (typeMap.ConstructionFactory is not null)
        {
            return;
        }

        if (typeMap.DestinationType.GetConstructor(Type.EmptyTypes) is not null)
        {
            return;
        }

        var hasWritableMembers = typeMap.DestinationType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Any(IsWritableMember);

        if (hasWritableMembers || typeMap.CtorParamMaps.Count > 0)
        {
            return;
        }

        errors.Add(
            $"Destination type '{typeMap.DestinationType.FullName}' has no public constructor, no writable members, and no construction factory.");
    }

    private static void ValidateMembers(TypeMap typeMap, List<string> errors)
    {
        // If the map contains custom lifecycle actions, we can't reliably infer which members
        // will be assigned. Don't block mapping execution in that case.
        if (typeMap.BeforeMapActions.Count > 0 || typeMap.AfterMapActions.Count > 0)
        {
            return;
        }

        var destinationProperties = typeMap.DestinationType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(IsWritableMember)
            .ToArray();

        var sourceProperties = typeMap.SourceType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        var configuredMembers = typeMap.MemberMaps
            .ToDictionary(m => m.DestinationMemberName, m => m, StringComparer.OrdinalIgnoreCase);
        var mappedMemberNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var destinationProperty in destinationProperties)
        {
            if (configuredMembers.TryGetValue(destinationProperty.Name, out var configuredMember))
            {
                if (configuredMember.Ignored)
                {
                    continue;
                }

                if (configuredMember.ValueResolver is null && configuredMember.ResolverType is null)
                {
                    errors.Add(
                        $"Destination member '{typeMap.DestinationType.FullName}.{destinationProperty.Name}' is explicitly configured but has no resolver.");
                }
                else
                {
                    mappedMemberNames.Add(destinationProperty.Name);
                }

                continue;
            }

            var sourceProperty = typeMap.SourceType.GetProperty(
                destinationProperty.Name,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

            if (sourceProperty is not null && sourceProperty.CanRead)
            {
                mappedMemberNames.Add(destinationProperty.Name);
                continue;
            }

            var flatteningResolver = Resolution.FlatteningResolver.TryBuildResolver(typeMap.SourceType, destinationProperty.Name);
            if (flatteningResolver is not null)
            {
                mappedMemberNames.Add(destinationProperty.Name);
                continue;
            }

            // Support simple unflattening (e.g. AddressCity -> Address.City).
            // ObjectMapper applies unflattening at runtime based on source member name prefixes.
            if (sourceProperties.Any(sp => sp.Name.StartsWith(destinationProperty.Name, StringComparison.OrdinalIgnoreCase)))
            {
                mappedMemberNames.Add(destinationProperty.Name);
                continue;
            }
        }

        // If nothing can be mapped, only flag an error when the destination has non-nullable
        // value-type members (can't be left as "default" safely in many cases).
        if (mappedMemberNames.Count == 0)
        {
            foreach (var destinationProperty in destinationProperties)
            {
                var memberType = destinationProperty.PropertyType;
                if (memberType.IsValueType && Nullable.GetUnderlyingType(memberType) is null)
                {
                    errors.Add(
                        $"Required destination member '{typeMap.DestinationType.FullName}.{destinationProperty.Name}' cannot be mapped by convention, flattening, or explicit configuration.");
                }
            }
        }
    }

    private static bool IsWritableMember(System.Reflection.PropertyInfo property)
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
