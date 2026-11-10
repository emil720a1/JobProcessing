using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using JobProcessing;
using App.ScopedService;


HostApplicationBuilder builder =
    Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<ScopedBackgroundService>();
builder.Services.AddScoped<IScopedProcessingService, DefaultScopedProcessingService>();

builder.Services.AddSingleton <JobProcessor>();
builder.Services.AddSingleton<MonitorLoop>();
builder.Services.AddHostedService<QueueHostedService>();


builder.Services.AddSingleton<IBackgroundTaskQueue>(_ =>
{
    if (!int.TryParse(
            builder.Configuration["QueueCapacity"],
            out var queueCapacity))
    {
        queueCapacity = 100;
    }

    return new DefaultBackgroundTaskQueue(queueCapacity);
});

IHost host = builder.Build();

MonitorLoop monitorLoop =
    host.Services.GetRequiredService<MonitorLoop>();


monitorLoop.StartMonitorLoop();

host.Run();