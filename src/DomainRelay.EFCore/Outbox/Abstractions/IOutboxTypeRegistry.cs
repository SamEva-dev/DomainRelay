namespace DomainRelay.EFCore.Outbox.Abstractions;

/// <summary>
/// Allowlist mapping from TypeKey -> CLR Type, to safely deserialize payloads.
/// </summary>
public interface IOutboxTypeRegistry
{
    bool TryResolve(string typeKey, out Type type);
    string GetTypeKey(Type type);
}
