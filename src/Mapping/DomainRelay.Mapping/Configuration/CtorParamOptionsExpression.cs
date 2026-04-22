using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Configuration;

internal sealed class CtorParamOptionsExpression<TSource, TParam>
    : ICtorParamOptionsExpression<TSource, TParam>
{
    private readonly string _parameterName;
    private Expression<Func<TSource, TParam>>? _sourceExpression;
    private TParam? _nullSubstitute;

    public CtorParamOptionsExpression(string parameterName)
    {
        _parameterName = parameterName;
    }

    public void MapFrom(Expression<Func<TSource, TParam>> sourceExpression)
    {
        ArgumentNullException.ThrowIfNull(sourceExpression);
        _sourceExpression = sourceExpression;
    }

    public void NullSubstitute(TParam value)
    {
        _nullSubstitute = value;
    }

    public CtorParamMapDefinition Build()
    {
        return new CtorParamMapDefinition(
            _parameterName,
            _sourceExpression,
            _nullSubstitute);
    }
}