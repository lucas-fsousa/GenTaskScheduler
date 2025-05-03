using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Internal;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Infra.Log;
using GenTaskScheduler.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Data.Services;

public class TaskHistoryRepository(GenTaskSchedulerDbContext context, ILogger<ApplicationLogger> logger): ITaskHistoryRepository {
  public async Task<IEnumerable<TaskExecutionHistory>> GetHistoryByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default) {
    try {
      var tasks = await context.TaskExecutionsHistory
        .Where(h => h.TaskId == taskId)
        .OrderByDescending(h => h.EndedAt)
        .ToListAsync(cancellationToken);

      return tasks.Select(t => {
        t.ResultObject = TaskSerializer.DeserializeBytesToObject(t.ResultBlob);
        return t;
      });
    } catch(Exception ex) {
      logger.LogError(ex, "Error retrieving task history for task ID {TaskId}", taskId);
      return [];
    }
  }

  public async Task<TaskExecutionHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) {
    try {
      var task = await context.TaskExecutionsHistory.FindAsync([id], cancellationToken);
      if(task is not null)
        task.ResultObject = TaskSerializer.DeserializeBytesToObject(task.ResultBlob);

      return task;
    } catch(Exception ex) {
      logger.LogError(ex, "Error retrieving task history for ID {Id}", id);
      return null;
    }
  }

  public async Task AddAsync(TaskExecutionHistory history, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      if(history is null)
        throw new ArgumentNullException(nameof(history), "Task history cannot be null");

      await context.TaskExecutionsHistory.AddAsync(history, cancellationToken);

      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Task history added for task ID {TaskId}", history.TaskId);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on adding task history for task ID {TaskId}", history.TaskId);
    }
  }

  public async Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var history = await GetByIdAsync(id, cancellationToken) ?? throw new ArgumentException($"History with ID {id} not found", nameof(id));
      context.TaskExecutionsHistory.Remove(history);

      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Task history deleted for ID {Id}", id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on deleting task history for ID {Id}", id);
    }
  }

  public async Task CommitAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

  public void Dispose() {
    GC.SuppressFinalize(this);
    context.Dispose();
  }
}
