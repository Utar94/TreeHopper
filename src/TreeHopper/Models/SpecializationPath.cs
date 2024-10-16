namespace TreeHopper.Models;

internal record SpecializationPath
{
  [JsonIgnore]
  public Specialization Source { get; }
  [JsonIgnore]
  public Specialization Target { get; }

  public bool? IsMandatoryTalentMatch { get; set; }
  public int OptionalTalentMatches { get; set; }

  public SpecializationPath(Specialization source, Specialization target)
  {
    Source = source;
    Target = target;
  }
}
