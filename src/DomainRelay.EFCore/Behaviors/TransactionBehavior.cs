using DomainRelay.Abstractions;
using DomainRelay.EFCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DomainRelay.EFCore.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDomainRelayDbContextResolver _resolver;

    public TransactionBehavior(IDomainRelayDbContextResolver resolver)
        => _resolver = resolver;

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
    {
        var db = _resolver.ResolveDbContext(typeof(TRequest));

        // Already in transaction => just continue
        if (db.Database.CurrentTransaction is not null)
            return await next().ConfigureAwait(false);

        await using var tx = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        var response = await next().ConfigureAwait(false);
        await tx.CommitAsync(ct).ConfigureAwait(false);
        return response;
    }
}
    