namespace Bookify.Web.Core.Mapping;

public static class BaseLocalizationResolver
{
    public static string GetValue(string key, object source)
    {
        var currentCulture = CultureInfo.CurrentCulture.Name;
        var localizationSet = source.GetType().GetProperty(key)?.GetValue(source, null) as LocalizationSet;

        var localization = localizationSet?.Localizations.FirstOrDefault(l => l.CultureCode == currentCulture);

        return localization?.Value ?? string.Empty;
    }
}