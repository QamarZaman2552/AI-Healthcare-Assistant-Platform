using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var existing = await _dbSet.FindAsync(new object[] { entity.Id }, cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException(
                $"{typeof(T).Name} with id {entity.Id} not found.");
        }

        var trackedEntry = _context.Entry(existing);

        foreach (var property in trackedEntry.Metadata.GetProperties())
        {
            var proposedValue = _context.Entry(entity).Property(property.Name).CurrentValue;
            var originalValue = trackedEntry.Property(property.Name).OriginalValue;

            if (Equals(proposedValue, originalValue))
                continue;

            trackedEntry.Property(property.Name).CurrentValue = proposedValue;
            trackedEntry.Property(property.Name).IsModified = true;
        }

        foreach (var navigation in trackedEntry.Metadata.GetNavigations())
        {
            var proposedValue = _context.Entry(entity).Navigation(navigation.Name).CurrentValue;
            if (proposedValue is not null)
                trackedEntry.Navigation(navigation.Name).CurrentValue = proposedValue;
        }
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
