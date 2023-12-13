namespace Bookify.Domain.Dtos;
public record LocalizationDto(
    string CultureCode,
    string Value
);