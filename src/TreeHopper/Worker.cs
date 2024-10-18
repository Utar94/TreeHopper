using MediatR;
using TreeHopper.Models;
using TreeHopper.Queries;

namespace TreeHopper;

internal class Worker : BackgroundService
{
  private const string ReportPath = "reports/paths.json";
  private const string SpecializationPath = "data/specializations.csv";
  private const string TalentPath = "data/talents.csv";
  private static readonly Encoding _encoding = Encoding.UTF8;

  private static readonly JsonSerializerOptions _serializerOptions;
  static Worker()
  {
    _serializerOptions = new()
    {
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      WriteIndented = true
    };
    _serializerOptions.Converters.Add(new JsonStringEnumConverter());
  }

  private readonly IHostApplicationLifetime _hostApplicationLifetime;
  private readonly ILogger<Worker> _logger;
  private readonly IServiceProvider _serviceProvider;

  public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger, IServiceProvider serviceProvider)
  {
    _hostApplicationLifetime = hostApplicationLifetime;
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Worker running at {Timestamp}.", DateTime.Now);

    using IServiceScope scope = _serviceProvider.CreateScope();
    IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

    IReadOnlyCollection<Talent> talents = await mediator.Send(new ReadTalentsQuery(TalentPath, _encoding), cancellationToken);
    _logger.LogInformation("Read {TalentCount} {TalentText} from file '{Path}'.", talents.Count, talents.Count <= 1 ? "talent" : "talents", TalentPath);

    IReadOnlyCollection<Specialization> specializations = await mediator.Send(new ReadSpecializationsQuery(SpecializationPath, _encoding), cancellationToken);
    _logger.LogInformation("Read {SpecializationCount} {SpecializationText} from file '{Path}'.", specializations.Count, specializations.Count <= 1 ? "specialization" : "specializations", SpecializationPath);

    IReadOnlyDictionary<Guid, Talent> talentsById = talents.ToDictionary(x => x.Id, x => x).AsReadOnly();

    List<SpecializationPath> paths = [];
    foreach (Specialization target in specializations)
    {
      if (target.Tier == 2 || target.Tier == 3)
      {
        int sourceTier = target.Tier - 1;
        IReadOnlyCollection<Talent> mandatoryTalentTree = BuildTalentTree(target.MandatoryTalentId, talentsById);
        IReadOnlyCollection<IReadOnlyCollection<Talent>> optionalTalentTrees = target.OptionalTalentIds.Select(id => BuildTalentTree(id, talentsById)).ToArray().AsReadOnly();

        foreach (Specialization source in specializations)
        {
          if (source.Tier == sourceTier)
          {
            SpecializationPath path = new(source, target);
            paths.Add(path);

            if (mandatoryTalentTree.Count > 0)
            {
              path.IsMandatoryTalentMatch = mandatoryTalentTree.Any(x => x.Id == source.MandatoryTalentId || source.OptionalTalentIds.Contains(x.Id));
            }

            foreach (IReadOnlyCollection<Talent> optionalTalentTree in optionalTalentTrees)
            {
              if (optionalTalentTree.Any(x => x.Id == source.MandatoryTalentId || source.OptionalTalentIds.Contains(x.Id)))
              {
                path.OptionalTalentMatches++;
              }
            }
          }
        }
      }
    }

    Report report = new();
    report.Details.AddRange(paths);

    Dictionary<string, PathSummary> tier1 = [];
    Dictionary<string, PathSummary> tier2 = [];
    foreach (SpecializationPath path in paths)
    {
      PathSummary? summary = null;
      string key = Normalize(path.Source.Name);
      if (path.Source.Tier == 1)
      {
        if (!tier1.TryGetValue(key, out summary))
        {
          summary = new(path.Source);
          tier1[key] = summary;
        }
      }
      else if (path.Source.Tier == 2)
      {
        if (!tier2.TryGetValue(key, out summary))
        {
          summary = new(path.Source);
          tier2[key] = summary;
        }
      }

      if (summary != null)
      {
        switch (path.Kind)
        {
          case PathKind.Likely:
            summary.Likely.Add(path.Target.Name);
            break;
          case PathKind.Strong:
            summary.Strong.Add(path.Target.Name);
            break;
          default:
            summary.None.Add(path.Target.Name);
            break;
        }
      }
    }
    report.Tier1.AddRange(tier1.Values);
    report.Tier2.AddRange(tier2.Values);

    string json = JsonSerializer.Serialize(report, _serializerOptions);
    await File.WriteAllTextAsync(ReportPath, json, _encoding, cancellationToken);
    _logger.LogInformation("Saved report to file '{Path}'.", ReportPath);

    _hostApplicationLifetime.StopApplication();
  }

  private static IReadOnlyCollection<Talent> BuildTalentTree(Guid? id, IReadOnlyDictionary<Guid, Talent> talentsById)
  {
    List<Talent> path = [];

    while (id.HasValue)
    {
      if (!talentsById.TryGetValue(id.Value, out Talent? talent))
      {
        throw new InvalidOperationException($"The talent 'Id={id}' could not be found.");
      }

      path.Add(talent);

      id = talent.RequiredTalentId;
    }

    return path;
  }

  private static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
