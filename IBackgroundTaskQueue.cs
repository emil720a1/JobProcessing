using System.Threading;

namespace JobProcessing;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ValueTask> workItem);

    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken CancellationToken);

    int QueuedCount { get; }
}