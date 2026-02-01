using System.Diagnostics;
using DomainRelay.Abstractions;
using DomainRelay.Internal;

namespace DomainRelay.Diagnostics.Behaviors;

public sealed class DiagnosticsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
    {
        var name = TypeNameCache.GetFriendlyName(typeof(TRequest));

        using var activity = DomainRelayActivity.Source.StartActivity(
            $"DomainRelay.Send {name}",
            ActivityKind.Internal);

        activity?.SetTag("domainrelay.request", name);
        activity?.SetTag("domainrelay.response", TypeNameCache.GetFriendlyName(typeof(TResponse)));

        try
        {
            var res = await next().ConfigureAwait(false);
            activity?.SetTag("domainrelay.success", true);
            return res;
        }
        catch (Exception ex)
        {
            activity?.SetTag("domainrelay.success", false);
            activity?.SetTag("domainrelay.exception.type", ex.GetType().FullName);
            activity?.SetTag("domainrelay.exception.message", ex.Message);
            throw;
        }
    }
}
