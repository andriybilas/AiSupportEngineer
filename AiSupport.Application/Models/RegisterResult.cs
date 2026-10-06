namespace AiSupport.Application.Models
{
    public class RegisterResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public bool Conflict { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public Guid? TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static RegisterResult Ok(
            Guid userId,
            string email,
            Guid tenantId,
            string tenantName,
            string token,
            DateTime expiresAt)
        {
            return new RegisterResult
            {
                Succeeded = true,
                UserId = userId,
                Email = email,
                TenantId = tenantId,
                TenantName = tenantName,
                Token = token,
                ExpiresAt = expiresAt,
                Errors = Array.Empty<string>()
            };
        }

        public static RegisterResult NotFoundResult(string message)
        {
            return new RegisterResult
            {
                Succeeded = false,
                NotFound = true,
                Errors = new[] { message }
            };
        }

        public static RegisterResult ConflictResult(string message)
        {
            return new RegisterResult
            {
                Succeeded = false,
                Conflict = true,
                Errors = new[] { message }
            };
        }

        public static RegisterResult Failed(IEnumerable<string> errors)
        {
            return new RegisterResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }

        public static RegisterResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
