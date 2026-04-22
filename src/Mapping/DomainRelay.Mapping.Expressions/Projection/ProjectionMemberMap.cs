using System.Linq.Expressions;
using System.Reflection;

namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class ProjectionMemberMap
{
    public string DestinationMemberName { get; }
    public PropertyInfo DestinationProperty { get; }
    public Expression? SourceExpressionBody { get; }
    public bool Ignored { get; }
    public bool IsExplicit { get; }
    public object? NullSubstitute { get; }

    public ProjectionMemberMap(
        string destinationMemberName,
        PropertyInfo destinationProperty,
        Expression? sourceExpressionBody,
        bool ignored,
        bool isExplicit,
        object? nullSubstitute)
    {
        DestinationMemberName = destinationMemberName;
        DestinationProperty = destinationProperty;
        SourceExpressionBody = sourceExpressionBody;
        Ignored = ignored;
        IsExplicit = isExplicit;
        NullSubstitute = nullSubstitute;
    }
}