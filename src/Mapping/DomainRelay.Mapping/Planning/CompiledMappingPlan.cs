namespace DomainRelay.Mapping.Planning;

internal sealed class CompiledMappingPlan
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public CompiledMappingDelegate? MappingDelegate { get; }
    public bool IsExecutable => MappingDelegate is not null;
    public string? FailureReason { get; }

    public CompiledMappingPlan(
        Type sourceType,
        Type destinationType,
        CompiledMappingDelegate mappingDelegate)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        MappingDelegate = mappingDelegate;
        FailureReason = null;
    }

    private CompiledMappingPlan(
        Type sourceType,
        Type destinationType,
        string failureReason)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        MappingDelegate = null;
        FailureReason = failureReason;
    }

    public static CompiledMappingPlan Unavailable(
        Type sourceType,
        Type destinationType,
        string failureReason)
    {
        return new CompiledMappingPlan(sourceType, destinationType, failureReason);
    }
}