namespace DomainRelay.Mapping.Abstractions.Exceptions;

public sealed class ProjectionConfigurationException : MappingException
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public IReadOnlyList<string> Errors { get; }

    public ProjectionConfigurationException(
        Type sourceType,
        Type destinationType,
        IEnumerable<string> errors)
        : base(BuildMessage(sourceType, destinationType, errors))
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        Errors = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray()
                 ?? Array.Empty<string>();
    }

    public ProjectionConfigurationException(
        Type sourceType,
        Type destinationType,
        string error)
        : this(sourceType, destinationType, new[] { error })
    {
    }

    private static string BuildMessage(
        Type sourceType,
        Type destinationType,
        IEnumerable<string> errors)
    {
        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray()
                        ?? Array.Empty<string>();

        var header =
            $"Projection configuration is invalid. Source='{sourceType.FullName}', Destination='{destinationType.FullName}'.";

        if (errorList.Length == 0)
        {
            return header;
        }

        return header
               + Environment.NewLine
               + string.Join(Environment.NewLine, errorList.Select(e => "- " + e));
    }
}