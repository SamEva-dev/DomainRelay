namespace DomainRelay.Mapping.Abstractions.Exceptions;

public sealed class ExpressionTranslationException : MappingException
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public string? ExpressionText { get; }

    public ExpressionTranslationException(
        Type sourceType,
        Type destinationType,
        string message,
        string? expressionText = null)
        : base(BuildMessage(sourceType, destinationType, message, expressionText))
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        ExpressionText = expressionText;
    }

    public ExpressionTranslationException(
        Type sourceType,
        Type destinationType,
        string message,
        Exception innerException,
        string? expressionText = null)
        : base(BuildMessage(sourceType, destinationType, message, expressionText), innerException)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        ExpressionText = expressionText;
    }

    private static string BuildMessage(
        Type sourceType,
        Type destinationType,
        string message,
        string? expressionText)
    {
        var result =
            $"Expression translation failed. Source='{sourceType.FullName}', Destination='{destinationType.FullName}'. {message}";

        if (!string.IsNullOrWhiteSpace(expressionText))
        {
            result += $" Expression='{expressionText}'.";
        }

        return result;
    }
}