using System.Collections.Concurrent;
using DomainRelay.EFCore.Outbox.Abstractions;

namespace DomainRelay.EFCore.Outbox;

/// <summary>
/// Default allowlist registry. You must register event types explicitly.
/// </summary>
public sealed class OutboxTypeRegistry : IOutboxTypeRegistry
{
    private readonly ConcurrentDictionary<string, Type> _byKey = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Type, string> _byType = new();

    public OutboxTypeRegistry Register<T>(string? typeKey = null)
    {
        var t = typeof(T);
        var key = typeKey ?? t.FullName ?? t.Name;

        _byKey[key] = t;
        _byType[t] = key;

        return this;
    }

    public bool TryResolve(string typeKey, out Type type) => _byKey.TryGetValue(typeKey, out type!);

    public string GetTypeKey(Type type)
    {
        if (_byType.TryGetValue(type, out var key))
            return key;

        throw new InvalidOperationException(
            $"Type '{type.FullName}' is not registered in OutboxTypeRegistry. Register it explicitly.");
    }
}
