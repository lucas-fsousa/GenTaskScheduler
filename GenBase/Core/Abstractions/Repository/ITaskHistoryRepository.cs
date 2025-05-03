using GenTaskScheduler.Core.Models.Common;

namespace GenTaskScheduler.Core.Abstractions.Repository;

internal interface ITaskHistoryRepository : IDisposable {
  internal Task<IEnumerable<TaskExecutionHistory>> GetHistoryByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
  internal Task<TaskExecutionHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  internal Task AddAsync(TaskExecutionHistory history, bool autoCommit = true, CancellationToken cancellationToken = default);
  internal Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default);

  Task CommitAsync(CancellationToken cancellationToken = default);
}
