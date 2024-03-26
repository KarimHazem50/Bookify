using Microsoft.AspNetCore.Identity;

namespace Bookify.Application.Services
{
    public interface IAuthService
    {
        Task<ApplicationUser?> GetByIdAsync(string? id);
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();
        Task<IEnumerable<IdentityRole>> GetRolesAsync();
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<ManageUserResponseDto> AddUserAsync(CreateUserDto dto, string createdById);
        Task<bool> AllowEmailAsync(string? id, string email);
        Task<bool> AllowUserNameAsync(string? id, string userName);
        Task<ApplicationUser?> ToggleStatusAsync(string id, string lastUpdatedById);
        Task<ManageUserResponseDto> ResetPasswordAsync(ApplicationUser user, string newPassword, string lastUpdatedById);
        Task<ManageUserResponseDto> UpdateUserAsync(ApplicationUser user, IList<string> selectedRoles, string lastUpdatedById);
        Task<ApplicationUser?> UnLockAsync(string id, string lastUpdatedById);
    }
}
