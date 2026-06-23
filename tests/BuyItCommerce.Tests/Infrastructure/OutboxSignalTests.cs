using BuyItCommerce.Infrastructure.Outbox;
using FluentAssertions;

namespace BuyItCommerce.Tests.Infrastructure;

public class OutboxSignalTests
{
    [Fact]
    public async Task Notify_WritesMessageId_ReaderReceivesIt()
    {
        var signal = new OutboxSignal();
        var messageId = Guid.NewGuid();

        signal.Notify(messageId);

        var received = await signal.Reader.ReadAsync();
        received.Should().Be(messageId);
    }

    [Fact]
    public void Notify_MultipleMessages_AllAvailableToReader()
    {
        var signal = new OutboxSignal();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        signal.Notify(first);
        signal.Notify(second);

        signal.Reader.TryRead(out var read1).Should().BeTrue();
        signal.Reader.TryRead(out var read2).Should().BeTrue();
        new[] { read1, read2 }.Should().BeEquivalentTo([first, second]);
    }
}
