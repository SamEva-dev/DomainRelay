using System.Linq.Expressions;
using System.Reflection;

namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class DestinationMemberTranslationMap
{
    public string DestinationMemberName { get; }
    public MemberInfo DestinationMember { get; }
    public Expression SourceExpressionBody { get; }

    public DestinationMemberTranslationMap(
        string destinationMemberName,
        MemberInfo destinationMember,
        Expression sourceExpressionBody)
    {
        DestinationMemberName = destinationMemberName;
        DestinationMember = destinationMember;
        SourceExpressionBody = sourceExpressionBody;
    }
}