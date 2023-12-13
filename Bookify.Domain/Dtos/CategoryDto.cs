namespace Bookify.Domain.Dtos;
public record CategoryDto(
    int Id,
    string Name,
    bool IsDeleted,
    DateTime CreatedOn,
    DateTime? LastUpdatedOn
);