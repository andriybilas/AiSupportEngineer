namespace AiSupport.Application.Models
{
    public class UpdateUserResult
    {
        public bool Succeeded { get; set; }
        public bool UserNotFound { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static UpdateUserResult Ok()
        {
            return new UpdateUserResult
            {
                Succeeded = true,
                UserNotFound = false,
                Errors = Array.Empty<string>()
            };
        }

        public static UpdateUserResult NotFound()
        {
            return new UpdateUserResult
            {
                Succeeded = false,
                UserNotFound = true,
                Errors = new[] { "User not found." }
            };
        }

        public static UpdateUserResult Failed(IEnumerable<string> errors)
        {
            return new UpdateUserResult
            {
                Succeeded = false,
                UserNotFound = false,
                Errors = errors.ToList()
            };
        }

        public static UpdateUserResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
