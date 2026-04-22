using DomainRelay.Mapping.Abstractions.Services;

namespace DomainRelay.Mapping.Abstractions.Resolvers;

public interface IContextValueResolver<in TSource, in TDestination, out TDestMember>
{
    TDestMember Resolve(TSource source, TDestination destination, IMappingContext context);
}