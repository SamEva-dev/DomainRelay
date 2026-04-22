namespace DomainRelay.Mapping.Internal;

internal static class Guard
{
    public static void AgainstNull(object? value, string paramName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}