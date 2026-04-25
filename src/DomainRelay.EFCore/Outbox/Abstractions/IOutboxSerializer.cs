namespace DomainRelay.EFCore.Outbox.Abstractions;

public interface IOutboxSerializer
{
    /// <summary>
    /// Serializes the specified payload object.
    /// </summary>
    /// <param name="value">The payload to serialize.</param>
    /// <param name="valueType">The value type.</param>
    /// <returns>A serialized string representation of the payload.</returns>
    string Serialize(object value, Type valueType);

    /// <summary>
    /// Deserializes a payload string to the specified runtime type.
    /// </summary>
    /// <param name="payload">The serialized payload.</param>
    /// <param name="valueType">The expected payload type.</param>
    /// <returns>The deserialized payload object.</returns>
    object Deserialize(string payload, Type valueType);
}
