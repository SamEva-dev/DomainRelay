namespace DomainRelay.Mapping.Abstractions.Generation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class GenerateMappingAttribute : Attribute
{
    public Type SourceType { get; }
    public Type DestinationType { get; }

    public GenerateMappingAttribute(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}