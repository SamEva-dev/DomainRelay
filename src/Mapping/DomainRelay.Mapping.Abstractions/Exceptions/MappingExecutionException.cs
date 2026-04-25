namespace DomainRelay.Mapping.Abstractions.Exceptions;

/// <summary>
/// Represents a failure that occurred while executing a mapping operation.
/// </summary>
public sealed class MappingExecutionException : MappingException
{
    /// <summary>
    /// Gets the source type involved in the failed mapping operation.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the destination type involved in the failed mapping operation.
    /// </summary>
    public Type DestinationType { get; }

    /// <summary>
    /// Gets the destination member name involved in the failure, when available.
    /// </summary>
    public string? MemberName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingExecutionException"/> class.
    /// </summary>
    /// <param name="message">The mapping error message.</param>
    /// <param name="sourceType">The source type involved in the mapping.</param>
    /// <param name="destinationType">The destination type involved in the mapping.</param>
    /// <param name="memberName">The destination member name involved in the failure, when available.</param>
    /// <param name="innerException">The inner exception.</param>
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