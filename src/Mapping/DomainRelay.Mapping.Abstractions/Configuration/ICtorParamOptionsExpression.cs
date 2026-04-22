using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface ICtorParamOptionsExpression<TSource, TParam>
{
    void MapFrom(Expression<Func<TSource, TParam>> sourceExpression);

    void NullSubstitute(TParam value);
}