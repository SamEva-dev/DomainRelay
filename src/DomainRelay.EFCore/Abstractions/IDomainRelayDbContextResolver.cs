using Microsoft.EntityFrameworkCore;

namespace DomainRelay.EFCore.Abstractions;

/// <summary>
/// Resolves which DbContext should be used for a given request type.
/// Enables multi-DbContext transaction behavior.
/// </summary>
public interface IDomainRelayDbContextResolver
{
    DbContext ResolveDbContext(Type requestType);
}
