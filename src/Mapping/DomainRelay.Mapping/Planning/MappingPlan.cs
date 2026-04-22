namespace DomainRelay.Mapping.Planning;

internal sealed class MappingPlan
{
    public TypeMap TypeMap { get; }

    public MappingPlan(TypeMap typeMap)
    {
        TypeMap = typeMap;
    }
}