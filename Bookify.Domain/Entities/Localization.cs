namespace Bookify.Domain.Entities;
public class Localization
{
    public int LocalizationSetId { get; set; }
    public string CultureCode { get; set; } = null!;
    public string Value { get; set; } = null!;

    public LocalizationSet LocalizationSet { get; set; } = null!;
}