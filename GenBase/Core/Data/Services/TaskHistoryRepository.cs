﻿using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Internal;
using GenTaskScheduler.Core.Infra.Helper;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace GenTaskScheduler.Core.Data.Services;

public class TaskHistoryRepository(GenTaskSchedulerDbContext context, ILogger<ApplicationLogger> logger): ITaskHistoryRepository {

  ///<inheritdoc />
  public async Task<IEnumerable<TaskExecutionHistory>> GetHistoryByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default) {
    try {
      var tasks = await context.TaskExecutionsHistory
        .Where(h => h.TaskId == taskId)
        .OrderByDescending(h => h.EndedAt)
        .ToListAsync(cancellationToken);

      return tasks;
    } catch(Exception ex) {
      logger.LogError(ex, "Error retrieving task history for task ID {TaskId}", taskId);
      return [];
    }
  }

  ///<inheritdoc />
  public async Task<TaskExecutionHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) {
    try {
      var task = await context.TaskExecutionsHistory.FindAsync([id], cancellationToken);
      return task;
    } catch(Exception ex) {
      logger.LogError(ex, "Error retrieving task history for ID {Id}", id);
      return null;
    }
  }

  ///<inheritdoc />
  ///<exception cref="ArgumentNullException"></exception>"
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

  ///<inheritdoc />
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

  ///<inheritdoc />
  public async Task DeleteAsync(Expression<Func<TaskExecutionHistory, bool>> filter, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var rowsModifieds = await context.TaskExecutionsHistory.Where(filter).ExecuteDeleteAsync(cancellationToken);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        if(rowsModifieds > 0) {
          logger.LogInformation("Tasks history deleted successfully. {rowsModifieds} rows affected", rowsModifieds);
          return;
        }
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on delete tasks history by filter");
    }
  }

  ///<inheritdoc />
  public async Task CommitAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

  ///<inheritdoc />
  public void Dispose() {
    GC.SuppressFinalize(this);
    context.Dispose();
  }
}
