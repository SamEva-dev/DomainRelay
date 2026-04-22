using System.Reflection;

namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class ProjectionPlan
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public IReadOnlyList<ProjectionMemberMap> Members { get; }
    public ConstructorInfo? Constructor { get; }

    public ProjectionPlan(
        Type sourceType,
        Type destinationType,
        IReadOnlyList<ProjectionMemberMap> members,
        ConstructorInfo? constructor)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        Members = members;
        Constructor = constructor;
    }
}