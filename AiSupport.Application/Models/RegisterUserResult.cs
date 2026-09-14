namespace AiSupport.Application.Models
{
    public class RegisterUserResult
    {
        public bool Succeeded { get; set; }
        public Guid? UserId { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static RegisterUserResult Ok(Guid userId)
        {
            return new RegisterUserResult
            {
                Succeeded = true,
                UserId = userId,
                Errors = Array.Empty<string>()
            };
        }

        public static RegisterUserResult Failed(IEnumerable<string> errors)
        {
            return new RegisterUserResult
            {
                Succeeded = false,
                UserId = null,
                Errors = errors.ToList()
            };
        }

        public static RegisterUserResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
