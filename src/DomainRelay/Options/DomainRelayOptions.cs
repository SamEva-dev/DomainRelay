using DomainRelay.Publish;

namespace DomainRelay.Options;

/// <summary>
/// Global options controlling DomainRelay mediator runtime behavior.
/// </summary>
/// <remarks>
/// These options are configured when calling <c>AddDomainRelay</c>.
/// </remarks>
/// <example>
/// <code>
/// services.AddDomainRelay(
///     configureOptions: options =>
///     {
///         options.WrapExceptions = true;
///         options.PublishStrategy = new ParallelPublishStrategy();
///     });
/// </code>
/// </example>
public sealed class DomainRelayOptions
{
    /// <summary>
    /// Gets or sets the notification publish strategy used by <c>IMediator.Publish</c>.
    /// </summary>
    /// <remarks>
    /// The default strategy is <see cref="SequentialPublishStrategy"/>, which invokes notification
    /// handlers one after another.
    /// </remarks>
    public IPublishStrategy PublishStrategy { get; set; } = new SequentialPublishStrategy();

    /// <summary>
    /// Gets or sets whether DomainRelay should wrap handler exceptions in <see cref="Exceptions.DomainRelayException"/>.
    /// </summary>
    /// <remarks>
    /// When set to <see langword="true"/>, exceptions thrown by handlers are wrapped to provide
    /// a consistent mediator-level error surface.
    /// When set to <see langword="false"/>, original exceptions are propagated.
    /// </remarks>
    public bool WrapExceptions { get; set; } = true;
}