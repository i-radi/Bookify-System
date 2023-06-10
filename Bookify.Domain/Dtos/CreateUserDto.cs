namespace Bookify.Domain.Dtos;
public record CreateUserDto(
    string FullName,
    string UserName,
    string Email,
    string Password,
    IList<string> SelectedRoles
);