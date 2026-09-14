namespace AiSupport.Application.Models
{
    public class CredentialValidationResult
    {
        public bool Succeeded { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }

        public static CredentialValidationResult Ok(Guid userId, string userName)
        {
            return new CredentialValidationResult
            {
                Succeeded = true,
                UserId = userId,
                UserName = userName
            };
        }

        public static CredentialValidationResult Failed()
        {
            return new CredentialValidationResult
            {
                Succeeded = false
            };
        }
    }
}
