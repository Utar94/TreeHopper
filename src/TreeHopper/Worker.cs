using MediatR;
using TreeHopper.Models;
using TreeHopper.Queries;

namespace TreeHopper;

internal class Worker : BackgroundService
{
  private const string SpecializationPath = "data/specializations.csv";
  private const string TalentPath = "data/talents.csv";
  private static readonly Encoding _encoding = Encoding.UTF8;

  private static readonly JsonSerializerOptions _serializerOptions;
  static Worker()
  {
    _serializerOptions = new()
    {
      Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement),
      WriteIndented = true
    };
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

    Dictionary<string, Dictionary<string, SpecializationPath>> report = [];
    foreach (SpecializationPath path in paths)
    {
      if (!report.TryGetValue(path.Target.Name, out Dictionary<string, SpecializationPath>? sources))
      {
        sources = [];
        report[path.Target.Name] = sources;
      }

      sources[path.Source.Name] = path;
    }
    string json = JsonSerializer.Serialize(report, _serializerOptions);
    await File.WriteAllTextAsync("report.json", json, _encoding, cancellationToken);
    _logger.LogInformation("Saved report to file '{Path}'.", "report.json");

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
}
