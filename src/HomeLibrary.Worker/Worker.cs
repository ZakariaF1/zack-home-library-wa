namespace HomeLibrary.Worker;

/// <summary>
/// Placeholder hosted service. Book-import consumption is added in a later change.
/// </summary>
public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Home Library worker started (import consumer not wired yet)");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
