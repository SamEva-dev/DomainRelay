namespace DomainRelay.EFCore.Outbox.Abstractions;

/// <summary>
/// Resolves outbox payload type names to CLR types and CLR types to stable names.
/// </summary>
/// <remarks>
/// The outbox stores payload type information as text. This registry allows applications
/// to control type naming and avoid relying directly on fragile assembly-qualified names.
/// </remarks>
public interface IOutboxTypeRegistry
{
    /// <summary>
    /// Gets a stable type name for the specified CLR type.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <param name="typeKey"></param>
    /// <returns>The stable type name.</returns>
    bool TryResolve(string typeKey, out Type type);

    /// <summary>
    /// Resolves a stable type name to a CLR type.
    /// </summary>
    /// <param name="type">The stable type name.</param>
    /// <returns>The resolved CLR type.</returns>
    string GetTypeKey(Type type);
}
