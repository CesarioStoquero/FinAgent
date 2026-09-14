using System.Text.Json;
using AwesomeAssertions;
using Confluent.Kafka;
using FinAgent.Wallet.Domain;
using FinAgent.Wallet.Infrastructure.Messaging;
using FinAgent.Wallet.IntegrationTests.Fixtures;
using Xunit;

namespace FinAgent.Wallet.IntegrationTests.Messaging;

/// <summary>
/// T17 — `[requer contêiner]`. Sem Docker, cada teste se auto-pula via
/// <c>Assert.SkipUnless</c>. Consumidor de teste lê o tópico com poll de condição
/// (nunca <c>Thread.Sleep</c> fixo).
/// </summary>
public class KafkaEventPublisherTests(KafkaFixture fx) : IClassFixture<KafkaFixture>
{
    [Fact]
    public async Task Publish_AccountOpened_EntregaMensagemComKeyIgualAoAggregateId()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var accountId = Guid.NewGuid();
        var envelope = new EventEnvelope(
            accountId, 0, DateTimeOffset.UtcNow,
            new AccountOpened(accountId, Guid.NewGuid(), Currencies.Brl, AccountType.Customer));

        await using var publisher = new KafkaEventPublisher(fx.BootstrapAddress);
        await publisher.PublishAsync([envelope], ct);

        var record = await ConsumeOne(fx.BootstrapAddress, TimeSpan.FromSeconds(15));

        record.Message.Key.Should().Be(accountId.ToString());
    }

    [Fact]
    public async Task Publish_AccountOpened_MensagemTemEventTypeEVersion()
    {
        Assert.SkipUnless(DockerAvailability.IsAvailable, "Docker não está disponível nesta máquina.");
        var ct = TestContext.Current.CancellationToken;

        var accountId = Guid.NewGuid();
        var envelope = new EventEnvelope(
            accountId, 3, DateTimeOffset.UtcNow,
            new AccountOpened(accountId, Guid.NewGuid(), Currencies.Brl, AccountType.Customer));

        await using var publisher = new KafkaEventPublisher(fx.BootstrapAddress);
        await publisher.PublishAsync([envelope], ct);

        var record = await ConsumeOne(fx.BootstrapAddress, TimeSpan.FromSeconds(15));
        var message = JsonSerializer.Deserialize<WalletEventMessage>(record.Message.Value)!;

        message.EventType.Should().Be("wallet.account-opened");
        message.Version.Should().Be(3);
    }

    // Poll de condição (sem Sleep fixo): tenta consumir até achar UMA mensagem ou estourar o timeout.
    private static async Task<ConsumeResult<string, string>> ConsumeOne(string bootstrapServers, TimeSpan timeout)
    {
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = $"test-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }).Build();

        consumer.Subscribe(KafkaEventPublisher.Topic);

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
            if (result is not null)
            {
                return result;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException("Nenhuma mensagem consumida dentro do timeout.");
    }
}
