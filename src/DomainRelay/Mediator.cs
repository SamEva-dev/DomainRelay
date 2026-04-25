using System.Collections.Concurrent;
using DomainRelay.Abstractions;
using DomainRelay.Exceptions;
using DomainRelay.Internal;
using DomainRelay.Options;

namespace DomainRelay;

/// <summary>
/// Default implementation of <see cref="IMediator"/>.
/// </summary>
/// <remarks>
/// <para>
/// The mediator sends requests to exactly one request handler and publishes notifications
/// to zero, one or many notification handlers.
/// </para>
/// <para>
/// Handler wrappers are cached by request and response type, while handler instances are resolved
/// from the configured <see cref="IServiceProvider"/> for each call.
/// </para>
/// </remarks>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;
    private readonly DomainRelayOptions _options;

    private static readonly ConcurrentDictionary<(Type Req, Type Res), RequestHandlerWrapper> RequestWrappers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="sp">The service provider used to resolve handlers and pipeline behaviors.</param>
    /// <param name="options">The DomainRelay runtime options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public Mediator(IServiceProvider sp, DomainRelayOptions options)
    {
        _sp = sp;
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task Send(IRequest request, CancellationToken ct = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var reqType = request.GetType();
        var resType = typeof(Unit);

        var wrapper = RequestWrappers.GetOrAdd((reqType, resType), static key =>
        {
            var wrapperType = typeof(VoidRequestHandlerWrapper<>).MakeGenericType(key.Req);
            return (RequestHandlerWrapper)Activator.CreateInstance(wrapperType)!;
        });

        try
        {
            await wrapper.Handle(_sp, request, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (_options.WrapExceptions)
        {
            throw new DomainRelayException($"DomainRelay.Send failed for {TypeNameCache.GetFriendlyName(reqType)}.", ex);
        }
    }

    /// <inheritdoc />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification
    {
        if (notification is null) throw new ArgumentNullException(nameof(notification));

        try
        {
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