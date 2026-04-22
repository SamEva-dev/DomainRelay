using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Tests;

public sealed class IntToStringValueConverter : IValueConverter<string>
{
    public object? Convert(string sourceMember)
    {
        return $"Age:{sourceMember}";
    }
}