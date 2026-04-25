namespace DomainRelay.Mapping.Abstractions.Converters;

/// <summary>
/// Converts a source member value before assigning it to a destination member.
/// </summary>
/// <typeparam name="TSourceMember">The source member type.</typeparam>
/// <remarks>
/// Value converters are configured at member level with <c>ConvertUsing</c>.
/// </remarks>
/// <example>
/// <code>
/// public sealed class TrimConverter : IValueConverter&lt;string&gt;
/// {
///     public object? Convert(string sourceMember) =&gt; sourceMember?.Trim();
/// }
/// </code>
/// </example>
public interface IValueConverter<in TSourceMember>
{
    /// <summary>
    /// Converts the specified source member value.
    /// </summary>
    /// <param name="sourceMember">The source member value.</param>
    /// <returns>The converted value assigned to the destination member.</returns>
    object? Convert(TSourceMember sourceMember);
}