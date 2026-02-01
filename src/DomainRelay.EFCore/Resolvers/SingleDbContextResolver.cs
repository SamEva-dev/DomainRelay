using DomainRelay.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DomainRelay.EFCore.Resolvers;

public sealed class SingleDbContextResolver<TDbContext> : IDomainRelayDbContextResolver
    where TDbContext : DbContext
{
    private readonly TDbContext _db;

    public SingleDbContextResolver(TDbContext db) => _db = db;

    public DbContext ResolveDbContext(Type requestType) => _db;
}
