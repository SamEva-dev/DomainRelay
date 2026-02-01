using System.Collections.Concurrent;
using DomainRelay.Abstractions;
using DomainRelay.Exceptions;
using DomainRelay.Internal;
using DomainRelay.Options;

namespace DomainRelay;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;
    private readonly DomainRelayOptions _options;

    private static readonly ConcurrentDictionary<(Type Req, Type Res), RequestHandlerWrapper> RequestWrappers = new();

    public Mediator(IServiceProvider sp, DomainRelayOptions options)
    {
        _sp = sp;
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var reqType = request.GetType();
        var resType = typeof(TResponse);

        var wrapper = RequestWrappers.GetOrAdd((reqType, resType), static key =>
        {
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(key.Req, key.Res);
            return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        });

        try
        {
            var result = await wrapper.Handle(_sp, request, ct).ConfigureAwait(false);
            return (TResponse)result!;
        }
        catch (Exception ex) when (_options.WrapExceptions)
        {
            throw new DomainRelayException($"DomainRelay.Send failed for {TypeNameCache.GetFriendlyName(reqType)}.", ex);
        }
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification
    {
        if (notification is null) throw new ArgumentNullException(nameof(notification));

        try
        {
            // IMPORTANT: do NOT cache handler instances. Resolve each call.
            var obj = _sp.GetService(typeof(IEnumerable<INotificationHandler<TNotification>>));
            var handlers = obj as IEnumerable<INotificationHandler<TNotification>>
                ?? Array.Empty<INotificationHandler<TNotification>>();

            await _options.PublishStrategy
                .Publish(handlers.ToArray(), notification, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (_options.WrapExceptions)
        {
            throw new DomainRelayException(
                $"DomainRelay.Publish failed for {TypeNameCache.GetFriendlyName(typeof(TNotification))}.",
                ex);
        }
    }
}
