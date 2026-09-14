namespace AiSupport.Application.Models
{
    public class UserUpdateResult
    {
        public bool Succeeded { get; set; }
        public bool UserNotFound { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static UserUpdateResult Ok()
        {
            return new UserUpdateResult
            {
                Succeeded = true,
                UserNotFound = false,
                Errors = Array.Empty<string>()
            };
        }

        public static UserUpdateResult NotFound()
        {
            return new UserUpdateResult
            {
                Succeeded = false,
                UserNotFound = true,
                Errors = new[] { "User not found." }
            };
        }

        public static UserUpdateResult Failed(IEnumerable<string> errors)
        {
            return new UserUpdateResult
            {
                Succeeded = false,
                UserNotFound = false,
                Errors = errors.ToList()
            };
        }

        public static UserUpdateResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
