namespace TreeHopper;

internal class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;

  public Worker(ILogger<Worker> logger)
  {
    _logger = logger;
  }

  protected override Task ExecuteAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Worker running at {Timestamp}.", DateTime.Now);
    return Task.CompletedTask;
  }
}
