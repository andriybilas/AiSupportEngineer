namespace AiSupport.Application.Abstractions
{
    public interface IApplicationTransaction : IAsyncDisposable
    {
        Task CommitAsync();

        Task RollbackAsync();
    }
}
