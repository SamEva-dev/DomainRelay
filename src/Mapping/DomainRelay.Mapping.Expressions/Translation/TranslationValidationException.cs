namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class TranslationValidationException : Exception
{
    public TranslationValidationException(string message) : base(message)
    {
    }
}