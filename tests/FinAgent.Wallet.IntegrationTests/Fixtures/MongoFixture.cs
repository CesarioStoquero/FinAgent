using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Fixtures;

/// <summary>
/// Sobe um MongoDB real via Testcontainers (read model, ADR-0005). Sem Docker,
/// <see cref="InitializeAsync"/> não constrói nem inicia o container — os testes
/// checam <c>Assert.SkipUnless(DockerAvailability.IsAvailable, ...)</c> antes de
/// usar <see cref="Database"/>.
/// </summary>
public sealed class MongoFixture : IAsyncLifetime
{
    private MongoDbContainer? _container;

    public IMongoDatabase Database { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        if (!DockerAvailability.IsAvailable)
        {
            return;
        }

        _container = new MongoDbBuilder("mongo:7").Build();
        await _container.StartAsync();
        var client = new MongoClient(_container.GetConnectionString());
        Database = client.GetDatabase("wallet");
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
