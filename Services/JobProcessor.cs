using Entities;

namespace JobProcessing.Services;

public sealed class JobProcessor
{
    public async Task ProcessAsync(Job job, CancellationToken token)
    {
        try
        {
            job.Start();

            await Task.Delay(10_000, token);

            job.Complete();

        }
        catch (OperationCanceledException)
        {
            job.Cancel();
            Console.WriteLine("Operation was cancelled")
        }
        catch (Exception)
        {
            job.Fail();
            throw;
        }
    }
}