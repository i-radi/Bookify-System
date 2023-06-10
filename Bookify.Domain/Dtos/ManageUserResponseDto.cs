using Bookify.Domain.Entities;

namespace Bookify.Domain.Dtos;
public record ManageUserResponseDto(
    bool IsSucceeded,
    ApplicationUser? User,
    string? VerificationCode,
    IEnumerable<string>? Errors
);