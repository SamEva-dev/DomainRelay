using DomainRelay.Publish;

namespace DomainRelay.Options;

/// <summary>
/// Global options for DomainRelay runtime behavior.
/// </summary>
public sealed class DomainRelayOptions
{
    /// <summary>
    /// Publish strategy used by <see cref="Abstractions.IMediator.Publish{TNotification}"/>.
    /// </summary>
    public IPublishStrategy PublishStrategy { get; set; } = new SequentialPublishStrategy();

    /// <summary>
    /// If true, wraps exceptions with a DomainRelayException for consistent error surface.
    /// </summary>
    public bool WrapExceptions { get; set; } = true;
}
