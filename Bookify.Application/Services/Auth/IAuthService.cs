using Microsoft.AspNetCore.Identity;

namespace Bookify.Application.Services;
public interface IAuthService
{
    Task<IEnumerable<ApplicationUser>> GetUsersAsync();
    Task<ApplicationUser?> GetUsersByIdAsync(string id);
    Task<IList<string>> GetUsersRolesAsync(ApplicationUser user);
    Task<ManageUserResponseDto> AddUserAsync(CreateUserDto dto, string createdById);
    Task<ManageUserResponseDto> UpdateUserAsync(ApplicationUser user, IEnumerable<string> selectedRoles, string updatedById);
    Task<ManageUserResponseDto> ResetPasswordAsync(ApplicationUser user, string password, string updatedById);
    Task<ApplicationUser?> ToggleUserStatusAsync(string id, string updatedById);
    Task<ApplicationUser?> UnlockUserAsync(string id);
    Task<bool> AllowUserNameAsync(string? id, string username);
    Task<bool> AllowEmailAsync(string? id, string email);

    Task<IEnumerable<IdentityRole>> GetRolesAsync();
}