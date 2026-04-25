namespace DomainRelay.Mapping.Expressions.Dynamic;

/// <summary>
/// Represents safe dynamic filtering and sorting options expressed against destination members.
/// </summary>
public sealed class DynamicQueryOptions
{
    /// <summary>
    /// Gets the filters to apply.
    /// </summary>
    public List<DynamicFilter> Filters { get; } = new();

    /// <summary>
    /// Gets the sorts to apply.
    /// </summary>
    public List<DynamicSort> Sorts { get; } = new();
}