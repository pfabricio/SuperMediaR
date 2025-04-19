using Microsoft.EntityFrameworkCore;
using SuperMediaR.Core.Interfaces;

namespace SuperMediaR.Pipeline.Behaviors;

public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly DbContext _dbContext;

    public TransactionScopeBehavior(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response;

        // Se já tem transação ativa, apenas executa
        if (_dbContext.Database.CurrentTransaction != null)
            return await next();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            response = await next();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return response;
    }
}