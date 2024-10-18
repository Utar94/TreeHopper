namespace TreeHopper.Models;

internal record SpecializationPath
{
  [JsonIgnore]
  public Specialization Source { get; }
  [JsonPropertyName("source")]
  public string SourceName => Source.Name;

  [JsonIgnore]
  public Specialization Target { get; }
  [JsonPropertyName("target")]
  public string TargetName => Target.Name;

  [JsonPropertyName("kind")]
  public PathKind Kind
  {
    get
    {
      int threshold = 4 + Target.Tier;
      if (IsMandatoryTalentMatch == true)
      {
        threshold--;
      }

      if (IsMandatoryTalentMatch != false && OptionalTalentMatches >= threshold)
      {
        return PathKind.Strong;
      }
      else if ((IsMandatoryTalentMatch == false && OptionalTalentMatches >= threshold)
        || (IsMandatoryTalentMatch != false && OptionalTalentMatches >= (threshold - 1)))
      {
        return PathKind.Likely;
      }

      return PathKind.None;
    }
  }

  [JsonPropertyName("isMandatoryTalentMatch")]
  public bool? IsMandatoryTalentMatch { get; set; }

  [JsonPropertyName("optionalTalentMatches")]
  public int OptionalTalentMatches { get; set; }

  public SpecializationPath(Specialization source, Specialization target)
  {
    Source = source;
    Target = target;
  }
}
