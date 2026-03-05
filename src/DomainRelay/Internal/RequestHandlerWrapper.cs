using DomainRelay.Abstractions;

namespace DomainRelay.Internal;

internal abstract class RequestHandlerWrapper
{
    public abstract Task<object?> Handle(IServiceProvider sp, object request, CancellationToken ct);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapper
    where TRequest : IRequest<TResponse>
{
    public override Task<object?> Handle(IServiceProvider sp, object request, CancellationToken ct)
        => HandleTyped(sp, (TRequest)request, ct);

    private static Task<object?> HandleTyped(IServiceProvider sp, TRequest request, CancellationToken ct)
    {
        var handler = (IRequestHandler<TRequest, TResponse>)sp.GetService(typeof(IRequestHandler<TRequest, TResponse>))!
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).FullName}.");

        var behaviors = (IEnumerable<IPipelineBehavior<TRequest, TResponse>>)sp.GetService(typeof(IEnumerable<IPipelineBehavior<TRequest, TResponse>>))!
            ?? Array.Empty<IPipelineBehavior<TRequest, TResponse>>();

        HandlerDelegate<TResponse> invokeHandler = () => handler.Handle(request, ct);

        foreach (var behavior in behaviors.Reverse())
        {
            var next = invokeHandler;
            invokeHandler = () => behavior.Handle(request, ct, next);
        }

        return Box(invokeHandler);

        static async Task<object?> Box(HandlerDelegate<TResponse> d)
            => await d().ConfigureAwait(false);
    }
}
