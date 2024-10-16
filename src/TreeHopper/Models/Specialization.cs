using CsvHelper.Configuration.Attributes;

namespace TreeHopper.Models;

internal class Specialization
{
  [Name("id")]
  public Guid Id { get; set; }

  [Name("tier")]
  public int Tier { get; set; }

  [Name("name")]
  public string Name { get; set; } = string.Empty;

  [Name("description")]
  public string Description { get; set; } = string.Empty;

  [Name("requirements")]
  public string Requirements { get; set; } = string.Empty;

  [Name("talents.mandatory")]
  public Guid? MandatoryTalentId { get; set; }

  public List<Guid> OptionalTalentIds { get; set; } = [];
  [Name("talents.optional")]
  public string OptionalTalentIdsSerialized
  {
    get => string.Join(',', OptionalTalentIds);
    set
    {
      OptionalTalentIds.Clear();

      string[] values = value.Split(',');
      foreach (string val in values)
      {
        if (Guid.TryParse(val, out Guid id))
        {
          OptionalTalentIds.Add(id);
        }
      }
    }
  }

  public List<string> OtherOptions { get; set; } = [];
  [Name("options")]
  public string OtherOptionsSerialized
  {
    get => string.Join(", ", OtherOptions);
    set
    {
      OtherOptions.Clear();
      OtherOptions.AddRange(value.Split(',').Select(val => val.Trim()));
    }
  }

  [Name("reservedTalent.name")]
  public string ReservedTalentName { get; set; } = string.Empty;

  [Name("reservedTalent.description")]
  public string ReservedTalentDescription { get; set; } = string.Empty;

  public override bool Equals(object? obj) => obj is Specialization specialization && specialization.Id == Id;
  public override int GetHashCode() => Id.GetHashCode();
  public override string ToString() => $"{Name} (Id={Id})";
}
