using AiSupport.Application.Models;
using AiSupport.Domain.Models;

namespace AiSupport.Application.Abstractions
{
    public interface IUserRepository
    {
        Task<UserCreationResult> CreateUserAsync(
            string userName,
            string firstName,
            string lastName,
            string email,
            string password);

        Task<AppUser?> GetUserByIdAsync(Guid userId);

        Task<UserUpdateResult> UpdateUserAsync(
            Guid userId,
            string? userName,
            string? email,
            string? firstName,
            string? lastName);
    }
}
