using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobProcessing;

public sealed class MonitorLoop(
    IBackgroundTaskQueue taskQueue,
    ILogger<MonitorLoop> logger,
    IHostApplicationLifetime applicationLifetime)
{
    private readonly CancellationToken _cancellationToken =
        applicationLifetime.ApplicationStopping;

    public void StartMonitorLoop(){
        logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");

        // Run a console user input loop in a background thread
        Task.Run(async () => await MonitorAsync());
    }

    private async ValueTask MonitorAsync()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            var keyStroke = Console.ReadKey();

            switch (keyStroke.Key)
            {
                case ConsoleKey.W:
                    // Enqueue a background work item
                    await taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
                    break;
                case ConsoleKey.S:
                    logger.LogInformation(
                        "Current queued work items: {QueuedCount}",
                        taskQueue.QueuedCount);
                    break;
                case ConsoleKey.Q:
                    logger.LogInformation("Shutting down...");
                    applicationLifetime.StopApplication();
                    return;
                default:
                    logger.LogInformation("Press W to add a work item to the background queue or S to show the current queued work items.");
                    break;
            }
        }
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken token)
    {
        // Simulate three 5-second tasks to complete
        // for each enqueued work item

        int delayLoop = 0;
        var guid = Guid.NewGuid();

        logger.LogInformation("Queued work item {Guid} is starting.", guid);

        while (!token.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            ++ delayLoop;

            logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
        }

        if (delayLoop is 3)
        {
            logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
        }
        else
        {
            logger.LogInformation("Queued Background Task {Guid} was cancelled.", guid);
        }
    }
}
