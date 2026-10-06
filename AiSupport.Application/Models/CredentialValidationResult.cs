namespace AiSupport.Application.Models
{
    public class CredentialValidationResult
    {
        public bool Succeeded { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public Guid TenantId { get; internal set; }

        public static CredentialValidationResult Ok(Guid userId, string userName, Guid tenantId)
        {
            return new CredentialValidationResult
            {
                Succeeded = true,
                UserId = userId,
                UserName = userName,
                TenantId = tenantId
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
