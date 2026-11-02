using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobProcessing;

public sealed class QueueHostedService(
	IBackgroundTaskQueue taskQueue,
	ILogger<QueueHostedService> logger) 
	: BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
		logger.LogInformation(
			"{Name} is running. Tap W to add a work item to the background queue.",
			nameof(QueueHostedService));

		return ProcessTaskQueueAsync(stoppingToken);
    }

	private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
	{
        while (!stoppingToken.IsCancellationRequested)
		{
	    	try
	    	{
				Func<CancellationToken, ValueTask>? workItem =
		    		await taskQueue.DequeueAsync(stoppingToken);

				await workItem(stoppingToken);
	    	}
	    	catch (OperationCanceledException)
	    	{
				// Prevent throwing if stoppingToken was signaled
	    	}
	    	catch (Exception ex)
	    	{
				logger.LogError(ex, "Error occured execution task work item.");
	    	}
    	}
	}

	public override async Task StopAsync(CancellationToken stoppingToken)
	{
        logger.LogInformation(
	    $"{nameof(QueueHostedService)} is stopping.");
	
		await base.StopAsync(stoppingToken);
	}
}
    
