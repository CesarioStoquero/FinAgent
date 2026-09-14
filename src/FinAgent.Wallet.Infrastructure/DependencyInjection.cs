using FinAgent.Wallet.Domain.Ports;
using FinAgent.Wallet.Infrastructure.Dev;
using FinAgent.Wallet.Infrastructure.Messaging;
using FinAgent.Wallet.Infrastructure.Persistence;
using FinAgent.Wallet.Infrastructure.Projections;
using FinAgent.Wallet.Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Npgsql;

namespace FinAgent.Wallet.Infrastructure;

/// <summary>
/// Registro dos adapters da Infrastructure (template canônico
/// <c>reference/project-layout.md</c>). Quando <c>Wallet:Store == "InMemory"</c>,
/// registra os adapters de dev (plano D5) — conveniência de execução local sem
/// Docker, NUNCA o default de produção. Todos os adapters são <c>internal</c>,
/// expostos só por aqui.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddWalletInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var store = configuration["Wallet:Store"];

        return string.Equals(store, "InMemory", StringComparison.OrdinalIgnoreCase)
            ? AddInMemoryAdapters(services)
            : AddRealAdapters(services, configuration);
    }

    // Plano D5: conveniência de execução local sem Docker — NUNCA o default.
    private static IServiceCollection AddInMemoryAdapters(IServiceCollection services)
    {
        services.AddSingleton<InMemoryWalletProjectionStore>();
        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();
        services.AddSingleton<IWalletReadModel, InMemoryWalletReadModel>();
        return services;
    }

    private static IServiceCollection AddRealAdapters(IServiceCollection services, IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres não configurada.");
        var mongoConnectionString = configuration.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("ConnectionStrings:Mongo não configurada.");
        var mongoDatabaseName = configuration["Wallet:Mongo:Database"] ?? "wallet";
        var kafkaBootstrapServers = configuration["Wallet:Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Wallet:Kafka:BootstrapServers não configurado.");

        // Postgres (event store, ADR-0007). A migração (ADR-0011) é aplicada pela Api
        // (fora deste método), não aqui — DI não deve fazer I/O bloqueante. O
        // WalletEventStoreDbContext existe só para isso: DEFINIR/migrar o schema.
        // PostgresEventStore continua em Npgsql cru para LoadAsync/AppendAsync.
        services.AddSingleton(_ => NpgsqlDataSource.Create(postgresConnectionString));
        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddDbContext<WalletEventStoreDbContext>(options => options.UseNpgsql(postgresConnectionString));

        // Kafka (ADR-0004).
        services.AddSingleton<IEventPublisher>(_ => new KafkaEventPublisher(kafkaBootstrapServers));

        // Mongo (read model, ADR-0005).
        services.AddSingleton<IMongoDatabase>(_ => new MongoClient(mongoConnectionString).GetDatabase(mongoDatabaseName));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoDatabase>().GetCollection<AccountDocument>("accounts"));
        services.AddSingleton<LedgerProjection>();
        services.AddSingleton<IWalletReadModel, MongoWalletReadModel>();

        // Projeção hospedada no processo da Api (plano D8).
        services.AddHostedService(sp =>
            new WalletProjectionWorker(kafkaBootstrapServers, sp.GetRequiredService<LedgerProjection>()));

        return services;
    }
}
