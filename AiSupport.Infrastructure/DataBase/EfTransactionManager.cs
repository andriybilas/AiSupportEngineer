using AiSupport.Application.Abstractions;

namespace AiSupport.Infrastructure.DataBase
{
    public class EfTransactionManager : ITransactionManager
    {
        private readonly ApplicationDbContext _db;

        public EfTransactionManager(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IApplicationTransaction> BeginTransactionAsync()
        {
            var transaction = await _db.Database.BeginTransactionAsync();

            return new EfApplicationTransaction(transaction);
        }
    }
}
