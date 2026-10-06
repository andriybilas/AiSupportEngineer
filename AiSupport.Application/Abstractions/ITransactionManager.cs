namespace AiSupport.Application.Abstractions
{
    public interface ITransactionManager
    {
        Task<IApplicationTransaction> BeginTransactionAsync();
    }
}
