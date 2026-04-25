namespace DomainRelay.Mapping.Expressions.Dynamic;

/// <summary>
/// Represents a safe dynamic sort instruction expressed against destination members.
/// </summary>
/// <param name="MemberName">The destination member name to sort by.</param>
/// <param name="Direction">The sort direction.</param>
public sealed record DynamicSort(
    string MemberName,
    DynamicSortDirection Direction = DynamicSortDirection.Asc);