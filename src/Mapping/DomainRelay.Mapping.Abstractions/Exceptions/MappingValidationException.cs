namespace DomainRelay.Mapping.Abstractions.Exceptions;

/// <summary>
/// Represents one or more mapping configuration validation errors.
/// </summary>
public sealed class MappingValidationException : MappingException
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingValidationException"/> class.
    /// </summary>
    /// <param name="error">The validation error.</param>
    public MappingValidationException(string error)
        : base(error)
    {
        Errors = new[] { error };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingValidationException"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public MappingValidationException(IEnumerable<string> errors)
        : base(BuildMessage(errors))
    {
        Errors = errors?.ToArray() ?? Array.Empty<string>();
    }

    private static string BuildMessage(IEnumerable<string> errors)
    {
        var errorList = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray()
                        ?? Array.Empty<string>();

        if (errorList.Length == 0)
        {
            return "Mapping configuration is invalid.";
        }

        return "Mapping configuration is invalid:" + Environment.NewLine
             + string.Join(Environment.NewLine, errorList.Select(e => "- " + e));
    }
}