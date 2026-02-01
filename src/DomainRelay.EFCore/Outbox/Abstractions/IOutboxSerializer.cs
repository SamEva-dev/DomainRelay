namespace DomainRelay.EFCore.Outbox.Abstractions;

public interface IOutboxSerializer
{
    string Serialize(object value, Type valueType);
    object Deserialize(string json, Type valueType);
}
