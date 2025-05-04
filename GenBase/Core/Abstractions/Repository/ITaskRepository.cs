using GenTaskScheduler.Core.Models.Common;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace GenTaskScheduler.Core.Abstractions.Repository;

public interface ITaskRepository : IDisposable {
  Task AddAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task<ScheduledTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<ScheduledTask>> GetAllAsync(Expression<Func<ScheduledTask, bool>>? filter = null, CancellationToken cancellationToken = default);
  Task UpdateAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task CommitAsync(CancellationToken cancellationToken = default);
  Task UpdateAsync(Expression<Func<ScheduledTask, bool>> filter, Expression<Func<SetPropertyCalls<ScheduledTask>, SetPropertyCalls<ScheduledTask>>> updateExpression,  bool autoCommit = true, CancellationToken cancellationToken = default);
}
