using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Abstractions.Repository;

public interface ITaskRepository : IDisposable {
  Task AddAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task<ScheduledTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<List<ScheduledTask>> GetAllAsync(CancellationToken cancellationToken = default);
  Task UpdateAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default);
  Task CommitAsync(CancellationToken cancellationToken = default);
}
