using System.Reflection;

namespace DomainRelay.Mapping.Expressions.Projection;

internal static class ProjectionConstructorResolver
{
    public static ConstructorInfo? TryResolve(Type destinationType, IReadOnlyList<ProjectionMemberMap> members)
    {
        var constructors = destinationType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .ToArray();

        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            var allResolvable = parameters.All(p =>
                members.Any(m => string.Equals(m.DestinationMemberName, p.Name, StringComparison.OrdinalIgnoreCase)));

            if (allResolvable)
            {
                return constructor;
            }
        }

        return null;
    }
}