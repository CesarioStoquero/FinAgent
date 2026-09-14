using AwesomeAssertions;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Infrastructure.Messaging;
using FinAgent.Wallet.Infrastructure.Projections;
using FinAgent.Wallet.Infrastructure.ReadModel;
using FinAgent.Wallet.IntegrationTests.Fixtures;
using MongoDB.Driver;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Messaging;

/// <summary>
/// T20 — `[requer contêiner]`, ponta a ponta (Kafka + Mongo). Sem Docker, cada teste
/// se auto-pula via <c>Assert.SkipUnless</c>. Poll de condição, nunca <c>Thread.Sleep</c>.
/// </summary>
public class WalletProjectionWorkerTests(KafkaFixture kafkaFx, MongoFixture mongoFx)
    : IClassFixture<KafkaFixture>, IClassFixture<MongoFixture>
{
    [Fact]
    public async Task AoConsumirAccountOpened_ReadModelPassaAConterACarteira()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var (worker, readModel) = await StartWorker(ct);
        try
        {
            var accountId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            await PublishAccountOpened(accountId, ownerId, ct);

            var view = await Eventually(
                () => readModel.GetByIdAsync(accountId, ct),
                v => v is not null,
                TimeSpan.FromSeconds(20));

            view!.WalletId.Should().Be(accountId);
            view.OwnerId.Should().Be(ownerId);
            view.BalanceCents.Should().Be(0);
        }
        finally
        {
            await worker.StopAsync(ct);
            worker.Dispose();
        }
    }

    [Fact]
    public async Task AoReentregarAMesmaMensagem_ReadModelPermaneceIgual()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var (worker, readModel) = await StartWorker(ct);
        try
        {
            var accountId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var envelope = new EventEnvelope(
                accountId, 0, DateTimeOffset.UtcNow,
                new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer));

            await using (var publisher = new KafkaEventPublisher(kafkaFx.BootstrapAddress))
            {
                await publisher.PublishAsync([envelope], ct); // 1ª entrega
                await publisher.PublishAsync([envelope], ct); // reentrega (at-least-once)
            }

            var view = await Eventually(
                () => readModel.GetByIdAsync(accountId, ct),
                v => v is not null,
                TimeSpan.FromSeconds(20));

            view!.BalanceCents.Should().Be(0); // reprocessar não altera nada
        }
        finally
        {
            await worker.StopAsync(ct);
            worker.Dispose();
        }
    }

    private async Task PublishAccountOpened(Guid accountId, Guid ownerId, CancellationToken ct)
    {
        await using var publisher = new KafkaEventPublisher(kafkaFx.BootstrapAddress);
        await publisher.PublishAsync(
            [new EventEnvelope(accountId, 0, DateTimeOffset.UtcNow, new AccountOpened(accountId, ownerId, Currencies.Brl, AccountType.Customer))],
            ct);
    }

    private async Task<(WalletProjectionWorker Worker, MongoWalletReadModel ReadModel)> StartWorker(CancellationToken ct)
    {
        var collection = mongoFx.Database.GetCollection<AccountDocument>($"accounts_{Guid.NewGuid():N}");
        var projection = new LedgerProjection(collection);
        var readModel = new MongoWalletReadModel(collection);
        var worker = new WalletProjectionWorker(kafkaFx.BootstrapAddress, projection);

        await worker.StartAsync(ct);

        return (worker, readModel);
    }

    // Helper: repete até a condição ou estoura o timeout. Substitui Thread.Sleep.
    private static async Task<T?> Eventually<T>(Func<Task<T?>> get, Func<T?, bool> cond, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var v = await get();
            if (cond(v))
            {
                return v;
            }

            await Task.Delay(200);
        }

        throw new TimeoutException("Condição não satisfeita dentro do timeout.");
    }
}
