using DomainRelay.Mapping.Resolution;

namespace DomainRelay.Mapping.Planning;

internal static class ConstructorResolver
{
    public static Func<object, object>? TryBuildFactory(Type sourceType, Type destinationType)
    {
        _ = sourceType;

        var ctor = destinationType.GetConstructor(Type.EmptyTypes);
        if (ctor is null)
        {
            return null;
        }

        return _ => Activator.CreateInstance(destinationType)!;
    }
}