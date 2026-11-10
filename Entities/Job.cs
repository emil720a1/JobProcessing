using System;

namespace Entities;

public sealed class Job
{
    public Job()
    {
        Id = Guid.NewGuid();
        Status = JobStatus.Queued;
        CreatedAt = DateTime.UtcNow;
        RetryCount = 0;
    }

    public Guid Id { get; private set; }

    public JobStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public int RetryCount { get; private set; }

    public void Start() 
    {
        Status = JobStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete() 
    {
        Status = JobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail() 
    {
        Status = JobStatus.Failed;
    }

    public void Cancel() 
    {
        Status = JobStatus.Cancelled;
    }

}
