using System.Threading.Channels;

namespace JobProcessing;

public class DefaultBackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask>> _queue;
    private int _queuedCount;

    public int QueuedCount => Volatile.Read(ref _queuedCount);

    public DefaultBackgroundTaskQueue(int capacity)
    {
        BoundedChannelOptions options = new(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ValueTask> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        Interlocked.Increment(ref _queuedCount);

        try
        {
            await _queue.Writer.WriteAsync(workItem);
        }
        catch
        {
            Interlocked.Decrement(ref _queuedCount);
            throw;
        } 
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        Func<CancellationToken, ValueTask>? workItem =
            await _queue.Reader.ReadAsync(cancellationToken);

        Interlocked.Decrement(ref _queuedCount);

        return workItem;
    }
}
