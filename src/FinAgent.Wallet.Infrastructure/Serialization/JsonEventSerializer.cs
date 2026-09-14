using System.Text.Json;
using FinAgent.Wallet.Domain;

namespace FinAgent.Wallet.Infrastructure.Serialization;

/// <summary>
/// Serializa/desserializa eventos de domínio com <c>System.Text.Json</c>, usando o
/// nome de contrato do <see cref="EventTypeRegistry"/> — nunca o nome CLR (plano D6/R1).
/// </summary>
internal static class JsonEventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static (string EventType, string Payload) Serialize(IDomainEvent @event)
    {
        var contractName = EventTypeRegistry.ContractNameFor(@event);
        var payload = JsonSerializer.Serialize(@event, @event.GetType(), Options);
        return (contractName, payload);
    }

    public static IDomainEvent Deserialize(string eventType, string payload)
    {
        var type = EventTypeRegistry.TypeFor(eventType);
        var @event = (IDomainEvent?)JsonSerializer.Deserialize(payload, type, Options);
        return @event ?? throw new InvalidOperationException(
            $"Payload vazio ao desserializar o tipo de evento '{eventType}'.");
    }
}
