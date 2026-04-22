using DomainRelay.Mapping.Internal;

namespace DomainRelay.Mapping.Resolution;

internal static class NestedMappingResolver
{
    public static bool ShouldUseNestedMapping(Type sourceType, Type destinationType)
    {
        return !TypeHelper.IsSimpleType(sourceType)
               && !TypeHelper.IsSimpleType(destinationType)
               && !TypeHelper.IsEnumerable(sourceType)
               && !TypeHelper.IsEnumerable(destinationType)
               && !TypeHelper.IsDictionary(sourceType)
               && !TypeHelper.IsDictionary(destinationType);
    }
}