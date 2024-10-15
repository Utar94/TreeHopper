namespace TreeHopper;

internal class Startup
{
  private readonly IConfiguration _configuration;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public virtual void ConfigureServices(IServiceCollection services)
  {
    services.AddHostedService<Worker>();
  }
}
