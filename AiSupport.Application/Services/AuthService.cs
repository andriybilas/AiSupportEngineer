using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;

namespace AiSupport.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
            {
                errors.Add("UserNameOrEmail is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors.Add("Password is required.");
            }

            if (errors.Count > 0)
            {
                return LoginResult.Failed(errors);
            }

            var validation = await _userRepository.ValidateCredentialsAsync(
                request.UserNameOrEmail.Trim(),
                request.Password);

            if (!validation.Succeeded)
            {
                return LoginResult.Failed("Invalid username/email or password.");
            }

            var tokenResult = _jwtTokenService.CreateToken(
                validation.UserId!.Value,
                validation.UserName!);

            return LoginResult.Ok(
                tokenResult.Token,
                tokenResult.ExpiresAt,
                validation.UserId.Value,
                validation.UserName!);
        }
    }
}
