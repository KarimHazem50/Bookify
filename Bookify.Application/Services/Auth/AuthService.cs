using Microsoft.AspNetCore.Identity;

namespace Bookify.Application.Services
{
    internal class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string? id)
        {
            return await _userManager.FindByIdAsync(id);
        }
        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }
        public async Task<IEnumerable<IdentityRole>> GetRolesAsync()
        {
            return await _roleManager.Roles.ToListAsync();
        }
        public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<ManageUserResponseDto> AddUserAsync(CreateUserDto dto, string createdById)
        {
            ApplicationUser user = new()
            {
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                CreatedById = createdById
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, dto.SelectedRoles);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return new ManageUserResponseDto(IsSucceeded: true, User: user, VerificationCode: code, Errors: null);
            }

            return new ManageUserResponseDto(IsSucceeded: false, User: null, VerificationCode: null, Errors: result.Errors.Select(e => e.Description));
        }
        public async Task<bool> AllowEmailAsync(string? id, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user is null || user.Id == id;
        }
        public async Task<bool> AllowUserNameAsync(string? id, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user is null || user.Id == id;
        }
        public async Task<ApplicationUser?> ToggleStatusAsync(string id, string lastUpdatedById)
        {
            var user = await GetByIdAsync(id);
            if (user is null)
                return null;

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedById = lastUpdatedById;
            user.LastUpdatedOn = DateTime.Now;
            await _userManager.UpdateAsync(user);

            if (user.IsDeleted)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            return user;
        }
        public async Task<ManageUserResponseDto> ResetPasswordAsync(ApplicationUser user, string newPassword, string lastUpdatedById)
        {
            var currentPassword = user.PasswordHash;
            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, newPassword);

            if (result.Succeeded)
            {
                user.LastUpdatedOn = DateTime.Now;
                user.LastUpdatedById = lastUpdatedById;
                await _userManager.UpdateAsync(user);
                return new ManageUserResponseDto(IsSucceeded: true, User: user, VerificationCode: null, Errors: null);
            }

            return new ManageUserResponseDto(IsSucceeded: false, User: user, VerificationCode: null, Errors: result.Errors.Select(e => e.Description));
        }
        public async Task<ManageUserResponseDto> UpdateUserAsync(ApplicationUser user, IList<string> selectedRoles, string lastUpdatedById)
        {
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);

                await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
                await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));

                await _userManager.UpdateSecurityStampAsync(user);

                return new ManageUserResponseDto(IsSucceeded: true, User: user, VerificationCode: null, Errors: null);
            }

            return new ManageUserResponseDto(IsSucceeded: false, User: user, VerificationCode: null, Errors: result.Errors.Select(e => e.Description));
        }
        public async Task<ApplicationUser?> UnLockAsync(string id, string lastUpdatedById)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return null;

            var isLocked = await _userManager.IsLockedOutAsync(user);
            if (isLocked)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                user.LastUpdatedOn = DateTime.Now;
                user.LastUpdatedById = lastUpdatedById;
                await _userManager.UpdateAsync(user);
            }
            return user;
        }
    }
}
