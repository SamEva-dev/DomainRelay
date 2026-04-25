namespace DomainRelay.Mapping.Abstractions.Converters;

/// <summary>
/// Converts values between source and destination types at runtime.
/// </summary>
/// <remarks>
/// Type converters are useful for reusable conversions that apply to a pair of types,
/// for example <see cref="string"/> to <see cref="Guid"/>, enum to string,
/// or custom value objects to primitive DTO values.
/// </remarks>
public interface ITypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert between the specified types.
    /// </summary>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The runtime destination type.</param>
    /// <returns>
    /// <see langword="true"/> when this converter supports the conversion; otherwise, <see langword="false"/>.
    /// </returns>
    bool CanConvert(Type sourceType, Type destinationType);

    /// <summary>
    /// Converts the specified source value.
    /// </summary>
    /// <param name="source">The source value. May be <see langword="null"/>.</param>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The runtime destination type.</param>
    /// <returns>The converted destination value.</returns>
    object? Convert(object? source, Type sourceType, Type destinationType);
}