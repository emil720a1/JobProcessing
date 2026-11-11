using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Entities;
using JobProcessing.Services;

namespace JobProcessing;

public sealed class MonitorLoop
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<MonitorLoop> _logger;
    private readonly JobProcessor _jobProcessor;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly CancellationToken _cancellationToken;


    public MonitorLoop(
        IBackgroundTaskQueue taskQueue,
        ILogger<MonitorLoop> logger,
        JobProcessor jobProcessor,
        IHostApplicationLifetime applicationLifetime)
    {
        _jobProcessor = jobProcessor;
        _taskQueue = taskQueue;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _cancellationToken = applicationLifetime.ApplicationStopping;
    }

    public void StartMonitorLoop(){
        _logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");

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

                    Job job = new Job();

                    // Enqueue a background work item
                    await _taskQueue.QueueBackgroundWorkItemAsync(
                        token => new ValueTask(
                            _jobProcessor.ProcessAsync(job, token)));

                    break;
                case ConsoleKey.S:
                    _logger.LogInformation(
                        "Current queued work items: {QueuedCount}",
                        _taskQueue.QueuedCount);
                    break;
                case ConsoleKey.Q:
                    _logger.LogInformation("Shutting down...");
                    _applicationLifetime.StopApplication();
                    return;
                default:
                    _logger.LogInformation("Press W to add a work item to the background queue or S to show the current queued work items.");
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

        _logger.LogInformation("Queued work item {Guid} is starting.", guid);

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

            _logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
        }

        if (delayLoop is 3)
        {
            _logger.LogInformation("Queued Background Task {Guid} is complete.", guid);
        }
        else
        {
            _logger.LogInformation("Queued Background Task {Guid} was cancelled.", guid);
        }
    }
}
