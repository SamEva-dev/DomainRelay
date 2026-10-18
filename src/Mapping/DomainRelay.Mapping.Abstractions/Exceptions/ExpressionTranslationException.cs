namespace DomainRelay.Mapping.Abstractions.Exceptions;

/// <summary>Reports failure to translate an expression between mapped types.</summary>
public sealed class ExpressionTranslationException : MappingException
{
    /// <summary>Gets the source model type.</summary>
    public Type SourceType { get; }
    /// <summary>Gets the destination model type.</summary>
    public Type DestinationType { get; }
    /// <summary>Gets the optional expression involved in the translation failure.</summary>
    public string? ExpressionText { get; }

    /// <summary>Initializes a mapping error with its source and destination context.</summary>
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

    /// <summary>Initializes a mapping error with its source and destination context.</summary>
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