namespace AiSupport.Application.Models
{
    public class CreateAppServiceRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
