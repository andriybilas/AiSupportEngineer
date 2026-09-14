namespace AiSupport.Application.Models
{
    public class EntityCreateResult<T>
    {
        public bool Succeeded { get; set; }
        public T? Entity { get; set; }
        public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

        public static EntityCreateResult<T> Ok(T entity)
        {
            return new EntityCreateResult<T>
            {
                Succeeded = true,
                Entity = entity,
                Errors = Array.Empty<string>()
            };
        }

        public static EntityCreateResult<T> Failed(IEnumerable<string> errors)
        {
            return new EntityCreateResult<T>
            {
                Succeeded = false,
                Entity = default,
                Errors = errors.ToList()
            };
        }

        public static EntityCreateResult<T> Failed(params string[] errors)
        {
            return Failed((IEnumerable<string>)errors);
        }
    }
}
