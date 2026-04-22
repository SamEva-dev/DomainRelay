using System.Linq.Expressions;

namespace DomainRelay.Mapping.Configuration;

internal sealed class CtorParamMapDefinition
{
    public string ParameterName { get; }
    public LambdaExpression? SourceExpression { get; }
    public object? NullSubstitute { get; }

    public CtorParamMapDefinition(
        string parameterName,
        LambdaExpression? sourceExpression,
        object? nullSubstitute)
    {
        ParameterName = parameterName;
        SourceExpression = sourceExpression;
        NullSubstitute = nullSubstitute;
    }
}