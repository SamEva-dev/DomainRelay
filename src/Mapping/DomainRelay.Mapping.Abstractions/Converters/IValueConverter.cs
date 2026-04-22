namespace DomainRelay.Mapping.Abstractions.Converters;

public interface IValueConverter<in TSourceMember>
{
    object? Convert(TSourceMember sourceMember);
}