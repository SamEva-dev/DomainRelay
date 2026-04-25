namespace DomainRelay.Mapping.Abstractions.Services;

/// <summary>
/// Represents the runtime context of a mapping operation.
/// </summary>
/// <remarks>
/// The mapping context exposes dependency injection services, runtime items and cycle-tracking
/// information used to avoid infinite recursion when mapping object graphs.
/// </remarks>
public interface IMappingContext
{
    /// <summary>
    /// Gets the service provider available during the current mapping operation, when any.
    /// </summary>
    IServiceProvider? ServiceProvider { get; }

    /// <summary>
    /// Gets runtime values passed to the current mapping operation.
    /// </summary>
    /// <remarks>
    /// Values are usually provided through <c>IMappingOperationOptions.Items</c>.
    /// </remarks>
    IReadOnlyDictionary<string, object?> Items { get; }

    /// <summary>
    /// Attempts to retrieve a previously mapped destination object for the specified source object
    /// and destination type.
    /// </summary>
    /// <param name="source">The source object currently being mapped.</param>
    /// <param name="destinationType">The destination type being created.</param>
    /// <param name="destination">The previously mapped destination object, when found.</param>
    /// <returns><see langword="true"/> when a mapped destination object already exists.</returns>
    bool TryGetVisited(object source, Type destinationType, out object? destination);

    /// <summary>
    /// Registers a mapped destination object for cycle tracking.
    /// </summary>
    /// <param name="source">The source object being mapped.</param>
    /// <param name="destinationType">The destination type being created.</param>
    /// <param name="destination">The mapped destination object.</param>
    void RegisterVisited(object source, Type destinationType, object destination);
}