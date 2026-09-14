using Testcontainers.Kafka;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Fixtures;

/// <summary>
/// Sobe um Kafka real via Testcontainers (ADR-0004), modo KRaft (sem ZooKeeper). Sem
/// Docker, <see cref="InitializeAsync"/> não constrói nem inicia o container — os
/// testes checam <c>Assert.SkipUnless(DockerAvailability.IsAvailable, ...)</c> antes
/// de ler <see cref="BootstrapAddress"/>.
/// </summary>
public sealed class KafkaFixture : IAsyncLifetime
{
    private KafkaContainer? _container;

    public string BootstrapAddress => _container!.GetBootstrapAddress();

    public async ValueTask InitializeAsync()
    {
        if (!DockerAvailability.IsAvailable)
        {
            return;
        }

        _container = new KafkaBuilder("confluentinc/cp-kafka:7.7.0").WithKRaft().Build();
        await _container.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
