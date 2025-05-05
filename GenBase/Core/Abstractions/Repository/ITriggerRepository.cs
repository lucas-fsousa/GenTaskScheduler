using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace GenTaskScheduler.Core.Abstractions.Repository;

public interface ITriggerRepository : IDisposable {
  Task AddAsync(BaseTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task<BaseTrigger?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<BaseTrigger>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task UpdateAsync(BaseTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task UpdateAsync(Expression<Func<BaseTrigger, bool>> filter, Expression<Func<SetPropertyCalls<BaseTrigger>, SetPropertyCalls<BaseTrigger>>> updateExpression, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task CommitAsync(CancellationToken cancellationToken = default);
}
