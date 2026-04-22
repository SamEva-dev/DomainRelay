namespace DomainRelay.Mapping.Abstractions.Exceptions;

public sealed class MappingExecutionException : Exception
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public string? MemberName { get; }

    public MappingExecutionException(
        string message,
        Type sourceType,
        Type destinationType,
        string? memberName,
        Exception innerException)
        : base(BuildMessage(message, sourceType, destinationType, memberName, innerException), innerException)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        MemberName = memberName;
    }

    private static string BuildMessage(
        string message,
        Type sourceType,
        Type destinationType,
        string? memberName,
        Exception innerException)
    {
        var baseMessage =
            $"{message} Source='{sourceType.FullName}', Destination='{destinationType.FullName}'";

        if (!string.IsNullOrWhiteSpace(memberName))
        {
            baseMessage += $", Member='{memberName}'";
        }

        if (!string.IsNullOrWhiteSpace(innerException.Message))
        {
            baseMessage += $". Inner: {innerException.Message}";
        }

        return baseMessage;
    }
}