namespace FinAgent.Wallet.Domain;

/// <summary>
/// Erro de regra de negócio do domínio da carteira. Base de toda exceção que
/// representa uma violação de invariante (nunca um erro técnico/infra).
/// </summary>
public class DomainException(string message) : Exception(message);
