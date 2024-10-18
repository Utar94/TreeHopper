namespace TreeHopper.Models;

internal record PathSummary
{
  [JsonIgnore]
  public Specialization Source { get; }
  [JsonPropertyName("name")]
  public string SourceName => Source.Name;

  [JsonPropertyName("strong")]
  public List<string> Strong { get; set; } = [];

  [JsonPropertyName("likely")]
  public List<string> Likely { get; set; } = [];

  [JsonPropertyName("none")]
  public List<string> None { get; set; } = [];

  public PathSummary(Specialization source)
  {
    Source = source;
  }
}
