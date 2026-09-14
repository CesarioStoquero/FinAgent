using FinAgent.Wallet.Domain;

namespace FinAgent.Wallet.Infrastructure.Serialization;

/// <summary>
/// Mapa explícito nome-de-contrato &lt;-&gt; tipo de evento (plano D6/R1). O nome de
/// contrato é o que fica gravado no event store e no payload Kafka — NUNCA o nome
/// CLR do tipo, e NUNCA <c>TypeNameHandling</c>. Renomear a classe C# não pode
/// quebrar o replay (RN-3).
/// </summary>
internal static class EventTypeRegistry
{
    // Registrar aqui todo evento que precisa ser gravado/publicado. Esta feature só
    // exercita "wallet.account-opened" (abertura não move dinheiro).
    private static readonly IReadOnlyDictionary<string, Type> ContractToType = new Dictionary<string, Type>
    {
        ["wallet.account-opened"] = typeof(AccountOpened),
    };

    private static readonly IReadOnlyDictionary<Type, string> TypeToContract =
        ContractToType.ToDictionary(kv => kv.Value, kv => kv.Key);

    public static string ContractNameFor(IDomainEvent @event)
    {
        var type = @event.GetType();
        return TypeToContract.TryGetValue(type, out var contractName)
            ? contractName
            : throw new InvalidOperationException(
                $"Tipo de evento sem nome de contrato registrado em {nameof(EventTypeRegistry)}: {type.FullName}.");
    }

    public static Type TypeFor(string contractName)
        => ContractToType.TryGetValue(contractName, out var type)
            ? type
            : throw new InvalidOperationException(
                $"Nome de contrato de evento desconhecido em {nameof(EventTypeRegistry)}: '{contractName}'.");
}
