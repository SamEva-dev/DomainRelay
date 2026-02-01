using System.Text.Json;
using DomainRelay.EFCore.Outbox.Abstractions;

namespace DomainRelay.EFCore.Outbox.Serialization;

public sealed class SystemTextJsonOutboxSerializer : IOutboxSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonOutboxSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };
    }

    public string Serialize(object value, Type valueType)
        => JsonSerializer.Serialize(value, valueType, _options);

    public object Deserialize(string json, Type valueType)
        => JsonSerializer.Deserialize(json, valueType, _options)
           ?? throw new InvalidOperationException($"Failed to deserialize outbox payload as {valueType.FullName}.");
}
