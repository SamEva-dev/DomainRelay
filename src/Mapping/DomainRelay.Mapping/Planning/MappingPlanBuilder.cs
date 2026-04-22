namespace DomainRelay.Mapping.Planning;

internal sealed class MappingPlanBuilder
{
    public MappingPlan Build(TypeMap typeMap)
    {
        return new MappingPlan(typeMap);
    }
}