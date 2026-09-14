using AwesomeAssertions;
using FinAgent.Wallet.Infrastructure.Projections;
using FinAgent.Wallet.Infrastructure.ReadModel;
using FinAgent.Wallet.IntegrationTests.Fixtures;
using MongoDB.Driver;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.ReadModel;

/// <summary>
/// T19 — `[requer contêiner]`. Sem Docker, cada teste se auto-pula via
/// <c>Assert.SkipUnless</c>.
/// </summary>
public class MongoWalletReadModelTests(MongoFixture fx) : IClassFixture<MongoFixture>
{
    [Fact]
    public async Task GetById_DeCarteiraProjetada_DevolveWalletViewComSaldoZero()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var collection = CollectionFor(fx);
        var accountId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        await collection.InsertOneAsync(new AccountDocument
        {
            Id = accountId,
            OwnerId = ownerId,
            Currency = "BRL",
            BalanceCents = 0,
            Version = 0,
        }, cancellationToken: ct);

        var readModel = new MongoWalletReadModel(collection);
        var view = await readModel.GetByIdAsync(accountId, ct);

        view.Should().NotBeNull();
        view!.WalletId.Should().Be(accountId);
        view.OwnerId.Should().Be(ownerId);
        view.BalanceCents.Should().Be(0);
    }

    [Fact]
    public async Task GetById_DeIdNuncaAberto_DevolveNull()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var readModel = new MongoWalletReadModel(CollectionFor(fx));

        var view = await readModel.GetByIdAsync(Guid.NewGuid(), ct);

        view.Should().BeNull();
    }

    private static IMongoCollection<AccountDocument> CollectionFor(MongoFixture fx)
        => fx.Database.GetCollection<AccountDocument>($"accounts_{Guid.NewGuid():N}");
}
