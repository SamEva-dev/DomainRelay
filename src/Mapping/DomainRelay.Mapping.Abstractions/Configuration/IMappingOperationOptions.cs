namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Represents per-operation options passed to a mapping call.
/// </summary>
/// <remarks>
/// Use <see cref="Items"/> to pass runtime values to context-aware resolvers.
/// </remarks>
/// <example>
/// <code>
/// var dto = mapper.Map&lt;Tenant, TenantDto&gt;(
///     tenant,
///     opt =&gt; opt.Items["culture"] = "fr-FR");
/// </code>
/// </example>
public interface IMappingOperationOptions
{
    /// <summary>
    /// Gets a dictionary of runtime values available during the mapping operation.
    /// </summary>
    IDictionary<string, object?> Items { get; }
}