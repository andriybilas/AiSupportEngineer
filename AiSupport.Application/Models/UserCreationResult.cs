namespace AiSupport.Application.Models
{
    public class UserCreationResult
    {
        public bool Succeeded { get; set; }
        public Guid? UserId { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static UserCreationResult Ok(Guid userId)
        {
            return new UserCreationResult
            {
                Succeeded = true,
                UserId = userId,
                Errors = Array.Empty<string>()
            };
        }

        public static UserCreationResult Failed(IEnumerable<string> errors)
        {
            return new UserCreationResult
            {
                Succeeded = false,
                UserId = null,
                Errors = errors.ToList()
            };
        }
    }
}
