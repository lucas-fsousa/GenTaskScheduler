using GenTaskScheduler.Core.Models.Triggers;

namespace GenTaskScheduler.Core.Abstractions.Repository;

public interface ITriggerRepository : IDisposable {
  Task AddAsync(BaseTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task<BaseTrigger?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<BaseTrigger>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task UpdateAsync(BaseTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task CommitAsync(CancellationToken cancellationToken = default);
}
