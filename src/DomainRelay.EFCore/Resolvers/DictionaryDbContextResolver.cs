using DomainRelay.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.EFCore.Resolvers;

/// <summary>
/// Map request types to DbContext via predicates or explicit type mapping.
/// </summary>
public sealed class DictionaryDbContextResolver : IDomainRelayDbContextResolver
{
    private readonly IServiceProvider _sp;
    private readonly List<(Func<Type, bool> Match, Type DbContextType)> _rules = new();

    public DictionaryDbContextResolver(IServiceProvider sp) => _sp = sp;

    public DictionaryDbContextResolver Map<TDbContext>(Func<Type, bool> match)
        where TDbContext : DbContext
    {
        _rules.Add((match, typeof(TDbContext)));
        return this;
    }

    public DbContext ResolveDbContext(Type requestType)
    {
        foreach (var (match, dbType) in _rules)
        {
            if (match(requestType))
            {
                return (DbContext)_sp.GetRequiredService(dbType);
            }
        }

        throw new InvalidOperationException(
            $"No DbContext mapping found for request type: {requestType.FullName}");
    }
}
