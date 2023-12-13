namespace Bookify.Domain.Entities;
public class LocalizationSet
{
    public int Id { get; set; }
    public string Reference { get; set; } = null!;
    public ICollection<Localization> Localizations { get; set; } = new List<Localization>();
}