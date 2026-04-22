namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class ExpressionTranslationPlan
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public IReadOnlyDictionary<string, DestinationMemberTranslationMap> Members { get; }

    public ExpressionTranslationPlan(
        Type sourceType,
        Type destinationType,
        IReadOnlyDictionary<string, DestinationMemberTranslationMap> members)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        Members = members;
    }
}