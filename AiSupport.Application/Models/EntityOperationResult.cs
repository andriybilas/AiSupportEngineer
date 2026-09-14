namespace AiSupport.Application.Models
{
    public class EntityOperationResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static EntityOperationResult Ok()
        {
            return new EntityOperationResult
            {
                Succeeded = true,
                NotFound = false,
                Errors = Array.Empty<string>()
            };
        }

        public static EntityOperationResult NotFoundResult(string message = "Entity not found.")
        {
            return new EntityOperationResult
            {
                Succeeded = false,
                NotFound = true,
                Errors = new[] { message }
            };
        }

        public static EntityOperationResult Failed(IEnumerable<string> errors)
        {
            return new EntityOperationResult
            {
                Succeeded = false,
                NotFound = false,
                Errors = errors.ToList()
            };
        }

        public static EntityOperationResult Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
