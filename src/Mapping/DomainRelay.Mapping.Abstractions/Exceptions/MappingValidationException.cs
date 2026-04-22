namespace DomainRelay.Mapping.Abstractions.Exceptions;

public sealed class MappingValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public MappingValidationException(string error)
        : base(error)
    {
        Errors = new[] { error };
    }

    public MappingValidationException(IEnumerable<string> errors)
        : base(BuildMessage(errors))
    {
        Errors = errors?.ToArray() ?? Array.Empty<string>();
    }

    private static string BuildMessage(IEnumerable<string> errors)
    {
        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray() ?? Array.Empty<string>();

        if (errorList.Length == 0)
        {
            return "Mapping configuration is invalid.";
        }

        return "Mapping configuration is invalid:" + Environment.NewLine
             + string.Join(Environment.NewLine, errorList.Select(e => "- " + e));
    }
}