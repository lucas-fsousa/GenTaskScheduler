using GenTaskScheduler.Core.Models.Common;
using System.Linq.Expressions;

namespace GenTaskScheduler.Core.Abstractions.Repository;

/// <summary>
/// Interface for the Task History Repository.
/// </summary>
public interface ITaskHistoryRepository : IDisposable {
  /// <summary>
  /// Retrieves the execution history of a task by its ID.
  /// </summary>
  /// <param name="taskId">Id of task for seach history</param>
  /// <param name="cancellationToken">Token for async executions</param>
  /// <returns>An <see cref="IEnumerable{T}"/></returns>
  public Task<IEnumerable<TaskExecutionHistory>> GetHistoryByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves a specific history record by execution ID.
  /// </summary>
  /// <param name="id">History ID</param>
  /// <param name="cancellationToken">Token for async executions</param>
  /// <returns><see cref="TaskExecutionHistory"/></returns>
  public Task<TaskExecutionHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Adds a new task execution history record.
  /// </summary>
  /// <param name="history">Value to add</param>
  /// <param name="autoCommit">if true, commit automatically, otherwise you will need to call <see cref="CommitAsync(CancellationToken)"/> manually.</param>
  /// <param name="cancellationToken">Token for async executions</param>
  /// <returns></returns>
  public Task AddAsync(TaskExecutionHistory history, bool autoCommit = true, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes a task execution history record by its ID.
  /// </summary>
  /// <param name="id">History ID</param>
  /// <param name="autoCommit">if true, commit automatically, otherwise you will need to call <see cref="CommitAsync(CancellationToken)"/> manually.</param>
  /// <param name="cancellationToken">Token for async executions</param>
  /// <returns></returns>
  public Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default);

  /// <summary>
  /// Commits the changes to the database.
  /// </summary>
  /// <param name="cancellationToken">Token for async executions</param>
  /// <returns></returns>
  public Task CommitAsync(CancellationToken cancellationToken = default);

  public Task DeleteAsync(Expression<Func<TaskExecutionHistory, bool>> filter, bool autoCommit = true, CancellationToken cancellationToken = default);
}
