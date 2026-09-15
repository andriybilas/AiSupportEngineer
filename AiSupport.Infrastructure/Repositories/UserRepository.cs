using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;
using AiSupport.Domain.Models;
using AiSupport.Infrastructure.DataBase;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AiSupport.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(ApplicationDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<AppUser?> GetUserByIdAsync(Guid userId)
        {
            return await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<UserCreationResult> CreateUserAsync(
            string userName,
            string firstName,
            string lastName,
            string email,
            string password)
        {
            var user = new AppUser
            {
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Name = userName,
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            };

            var identityResult = await _userManager.CreateAsync(user, password);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors
                    .Select(e => e.Description)
                    .ToList();

                return UserCreationResult.Failed(errors);
            }

            return UserCreationResult.Ok(user.Id);
        }

        public async Task<UserUpdateResult> UpdateUserAsync(
            Guid userId,
            string? userName,
            string? email,
            string? firstName,
            string? lastName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return UserUpdateResult.NotFound();
            }

            if (userName != null)
            {
                user.UserName = userName;
                user.Name = userName;
            }

            if (email != null)
            {
                user.Email = email;
            }

            if (firstName != null)
            {
                user.FirstName = firstName;
            }

            if (lastName != null)
            {
                user.LastName = lastName;
            }

            user.UpdatedDateTime = DateTime.UtcNow;

            var identityResult = await _userManager.UpdateAsync(user);

            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors
                    .Select(e => e.Description)
                    .ToList();

                return UserUpdateResult.Failed(errors);
            }

            return UserUpdateResult.Ok();
        }

        public async Task<CredentialValidationResult> ValidateCredentialsAsync(
            string userNameOrEmail,
            string password)
        {
            var user = await _userManager.FindByNameAsync(userNameOrEmail);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userNameOrEmail);
            }

            if (user == null)
            {
                return CredentialValidationResult.Failed();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);

            if (!passwordValid)
            {
                return CredentialValidationResult.Failed();
            }

            var userName = user.UserName;

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = user.Email ?? string.Empty;
            }

            return CredentialValidationResult.Ok(user.Id, userName);
        }
    }
}
