namespace TreeHopper.Models;

internal record Report
{
  [JsonPropertyName("tier1")]
  public List<PathSummary> Tier1 { get; set; } = [];

  [JsonPropertyName("tier2")]
  public List<PathSummary> Tier2 { get; set; } = [];

  [JsonPropertyName("details")]
  public List<SpecializationPath> Details { get; set; } = [];
}
