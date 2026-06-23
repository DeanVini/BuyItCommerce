using System.Threading.Channels;

namespace BuyItCommerce.Infrastructure.Outbox;

public sealed class OutboxSignal : IOutboxSignal
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = true });

    public ChannelReader<Guid> Reader => _channel.Reader;

    public void Notify(Guid messageId) => _channel.Writer.TryWrite(messageId);
}
