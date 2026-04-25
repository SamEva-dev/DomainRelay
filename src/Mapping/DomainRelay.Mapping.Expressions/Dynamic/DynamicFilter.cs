namespace DomainRelay.Mapping.Expressions.Dynamic;

/// <summary>
/// Represents a safe dynamic filter instruction expressed against a destination member.
/// </summary>
/// <param name="MemberName">The destination member name to filter.</param>
/// <param name="Operator">The filter operation.</param>
/// <param name="Value">The comparison value.</param>
public sealed record DynamicFilter(
    string MemberName,
    DynamicFilterOperator Operator,
    object? Value);