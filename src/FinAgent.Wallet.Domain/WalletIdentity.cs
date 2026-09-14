using System.Security.Cryptography;

namespace FinAgent.Wallet.Domain;

/// <summary>
/// Deriva o <c>walletId</c> deterministicamente do <c>ownerId</c> (plano D2): resolve
/// RF-4/RN-15 (no máximo uma carteira por titular) estruturalmente, sem tabela de
/// índice. Função pura, sem I/O — mesmo titular sempre produz o mesmo id.
/// </summary>
public static class WalletIdentity
{
    // Namespace fixo do FinAgent. NUNCA mude este valor: mudar quebraria a derivação
    // e deixaria carteiras já abertas órfãs (id calculado, não gravado, seria diferente).
    private static readonly Guid Namespace = Guid.Parse("2f6f6e8a-2b8a-4b7d-9d2a-2a6a8b6e6f6d");

    public static Guid ForOwner(Guid ownerId)
    {
        Span<byte> input = stackalloc byte[32];
        Namespace.TryWriteBytes(input[..16]);
        ownerId.TryWriteBytes(input[16..]);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(input, hash);

        Span<byte> idBytes = stackalloc byte[16];
        hash[..16].CopyTo(idBytes);

        // Bits de versão/variante ajustados (RFC 9562), só para marcar o id como
        // gerado por nome — não é exigido interoperar com nenhuma lib de UUID externa.
        idBytes[6] = (byte)((idBytes[6] & 0x0F) | 0x50); // versão 5 (name-based)
        idBytes[8] = (byte)((idBytes[8] & 0x3F) | 0x80); // variante RFC 9562

        return new Guid(idBytes);
    }
}
