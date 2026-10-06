using System.Net.Mail;
using AiSupport.Application.Abstractions;
using AiSupport.Application.Models;

namespace AiSupport.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            ITenantRepository tenantRepository,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
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
                validation.UserName!,
                validation.TenantId);

            return LoginResult.Ok(
                tokenResult.Token,
                tokenResult.ExpiresAt,
                validation.UserId.Value,
                validation.UserName!);
        }

        /// <summary>
        /// Registers a user (UserName = Email) in an existing tenant.
        /// The tenant is stored on the user row itself, so creation is a single atomic insert.
        /// </summary>
        public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email?.Trim() ?? string.Empty;
            var errors = ValidateRegisterRequest(request, email);

            if (errors.Count > 0)
            {
                return RegisterResult.Failed(errors);
            }

            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);

            if (tenant == null)
            {
                return RegisterResult.NotFoundResult("Tenant not found.");
            }

            var emailRegistered = await _userRepository.IsEmailRegisteredAsync(email);

            if (emailRegistered)
            {
                return RegisterResult.ConflictResult("Email is already registered.");
            }

            var creationResult = await _userRepository.CreateUserAsync(
                email,
                string.Empty,
                string.Empty,
                email,
                request.Password,
                request.TenantId);

            if (!creationResult.Succeeded)
            {
                return RegisterResult.Failed(creationResult.Errors);
            }

            var userId = creationResult.UserId!.Value;

            var tokenResult = _jwtTokenService.CreateToken(
                userId,
                email,
                request.TenantId);

            return RegisterResult.Ok(
                userId,
                email,
                request.TenantId,
                tenant.Name,
                tokenResult.Token,
                tokenResult.ExpiresAt);
        }

        private static List<string> ValidateRegisterRequest(RegisterRequest request, string email)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("Email is required.");
            }
            else if (!IsValidEmail(email))
            {
                errors.Add("Email format is invalid.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors.Add("Password is required.");
            }

            var passwordsMatch = string.Equals(
                request.Password,
                request.ConfirmPassword,
                StringComparison.Ordinal);

            if (!passwordsMatch)
            {
                errors.Add("Passwords do not match.");
            }

            if (request.TenantId == Guid.Empty)
            {
                errors.Add("TenantId is required.");
            }

            return errors;
        }

        private static bool IsValidEmail(string email)
        {
            if (!MailAddress.TryCreate(email, out var address))
            {
                return false;
            }

            // Reject display-name forms such as "John <john@example.com>".
            return string.Equals(
                address.Address,
                email,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
