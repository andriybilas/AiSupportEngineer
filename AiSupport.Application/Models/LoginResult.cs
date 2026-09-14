namespace AiSupport.Application.Models
{
    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static LoginResult Ok(string token, DateTime expiresAt, Guid userId, string userName)
        {
            return new LoginResult
            {
                Succeeded = true,
                Token = token,
                ExpiresAt = expiresAt,
                UserId = userId,
                UserName = userName,
                Errors = Array.Empty<string>()
            };
        }

        public static LoginResult Failed(IEnumerable<string> errors)
        {
            return new LoginResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }

        public static LoginResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
