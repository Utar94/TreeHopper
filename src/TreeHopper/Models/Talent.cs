using CsvHelper.Configuration.Attributes;

namespace TreeHopper.Models;

internal class Talent
{
  [Name("id")]
  public Guid Id { get; set; }

  [Name("tier")]
  public int Tier { get; set; }

  [Name("name")]
  public string Name { get; set; } = string.Empty;

  [Name("description")]
  public string Description { get; set; } = string.Empty;

  [Name("allowMultiplePurchases")]
  public bool AllowMultiplePurchases { get; set; }

  [Name("requiredTalentId")]
  public Guid? RequiredTalentId { get; set; }

  public override bool Equals(object? obj) => obj is Talent talent && talent.Id == Id;
  public override int GetHashCode() => Id.GetHashCode();
  public override string ToString() => $"{Name} (Id={Id})";
}
