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
        var destinationProperties = typeMap.DestinationType
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(IsWritableMember)
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
        }

        if (mappedMemberNames.Count == 0)
        {
            foreach (var destinationProperty in destinationProperties)
            {
                errors.Add(
                    $"Required destination member '{typeMap.DestinationType.FullName}.{destinationProperty.Name}' cannot be mapped by convention, flattening, or explicit configuration.");
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
