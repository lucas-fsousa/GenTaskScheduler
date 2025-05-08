using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Internal;
using GenTaskScheduler.Core.Infra.Logger;
using GenTaskScheduler.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace GenTaskScheduler.Core.Data.Services;

public class TaskRepository(GenTaskSchedulerDbContext context, ILogger<ApplicationLogger> logger, ITriggerRepository triggerRepository): ITaskRepository {

  ///<inheritdoc/>
  public async Task AddAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var now = DateTimeOffset.UtcNow;
      if(task is null)
        throw new ArgumentNullException(nameof(task), "Task cannot be null");

      if(task.Triggers is null || task.Triggers.Count <= 0)
        throw new ArgumentException("Task must have at least one trigger", nameof(task));

      var existe = await context.ScheduledTasks.AnyAsync(t => t.Name.ToLower().Equals(task.Name.ToLower()) && t.IsActive, cancellationToken);
      if(existe)
        throw new ArgumentException($"Task with name [{task.Name}] already exists", nameof(task));

      if(task.DependsOnTask is not null)
        await AddAsync(task.DependsOnTask, false, cancellationToken);

      task.UpdatedAt = now;
      task.CreatedAt = now;
      task.ExecutionStatus = Enums.GenSchedulerTaskStatus.Ready.ToString();
      task.DependsOnTaskId = task.DependsOnTask?.Id ?? task.DependsOnTaskId;
      task.DependsOnTask = null;
      task.NextExecution = task.Triggers.OrderBy(t => t.NextExecution).First(t => t.NextExecution is not null).NextExecution ?? DateTimeOffset.MinValue;

      var triggers = task.Triggers.ToList();
      task.Triggers.Clear();
      await context.ScheduledTasks.AddAsync(task, cancellationToken);

      foreach(var trigger in triggers) {
        await triggerRepository.AddAsync(trigger, false, cancellationToken);
        trigger.TaskId = task.Id;
      }

      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Task with Id {Id} added successfully", task.Id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on adding task with Id {Id}", task.Id);
    }
  }

  ///<inheritdoc/>
  public async Task CommitAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

  ///<inheritdoc/>
  public async Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var task = await GetByIdAsync(id, cancellationToken) ?? throw new ArgumentException($"Task with ID {id} not found", nameof(id));
      context.ScheduledTasks.Remove(task);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Task with Id {Id} deleted successfully", id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on deleting task with Id {Id}", id);
    }
  }

  ///<inheritdoc/>
  public async Task<List<ScheduledTask>> GetAllAsync(Expression<Func<ScheduledTask, bool>>? filter = null, CancellationToken cancellationToken = default) {
    try {
      var tasksQuery = context.ScheduledTasks.AsQueryable();

      if(filter is not null)
        tasksQuery = tasksQuery.Where(filter);

      var tasks = await tasksQuery.Select(task => new ScheduledTask {
        Id = task.Id,
        Name = task.Name,
        NextExecution = task.NextExecution,
        LastExecution = task.LastExecution,
        ExecutionStatus = task.ExecutionStatus,
        AutoDelete = task.AutoDelete,
        IsActive = task.IsActive,
        BlobArgs = task.BlobArgs,
        MaxExecutionTime = task.MaxExecutionTime,
        Triggers = task.Triggers,
        DependsOnStatus = task.DependsOnStatus,
        DependsOnTaskId = task.DependsOnTaskId,
        DependsOnTask = task.DependsOnTask,
        LastExecutionHistoryId = task.LastExecutionHistoryId,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
        LastExecutionHistory = task.ExecutionHistory.FirstOrDefault(h => h.Id == task.LastExecutionHistoryId)
      }).ToListAsync(cancellationToken);

      foreach(var task in tasks) {
        if(task.DependsOnTask is null)
          continue;

        foreach(var reference in context.Entry(task).References)
          await reference.LoadAsync(cancellationToken);
      }

      return tasks;
    } catch(Exception ex) {
      logger.LogError(ex, "Error on getting all tasks");
      return [];
    }
  }

  ///<inheritdoc/>
  public async Task<ScheduledTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await context.ScheduledTasks.FindAsync([id], cancellationToken);

  ///<inheritdoc/>
  public async Task UpdateAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      foreach(var item in task.Triggers)
        context.BaseTriggers.Entry(item).State = EntityState.Unchanged;

      task.NextExecution = task.Triggers.OrderBy(t => t.NextExecution).FirstOrDefault(t => t.NextExecution is not null)?.NextExecution ?? DateTimeOffset.MinValue;
      task.UpdatedAt = DateTimeOffset.UtcNow;
      context.ScheduledTasks.Update(task);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Task with Id {Id} updated successfully", task.Id);
      }

    } catch(Exception ex) {
      logger.LogError(ex, "Error on updating task with Id {Id}", task.Id);
    }
  }

  ///<inheritdoc/>
  public async Task UpdateAsync(Expression<Func<ScheduledTask, bool>> filter, Expression<Func<SetPropertyCalls<ScheduledTask>, SetPropertyCalls<ScheduledTask>>> updateExpression, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var rowsModifieds = await context.ScheduledTasks.Where(filter).ExecuteUpdateAsync(updateExpression, cancellationToken);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        if(rowsModifieds > 0) {
          logger.LogInformation("Tasks updated successfully. {rowsModifieds} rows affected", rowsModifieds);
          return;
        }
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on updating tasks by filter");
    }
  }

  ///<inheritdoc/>
  public async Task DeleteAsync(Expression<Func<ScheduledTask, bool>> filter, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var rowsModifieds = await context.ScheduledTasks.Where(filter).ExecuteDeleteAsync(cancellationToken);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        if(rowsModifieds > 0) {
          logger.LogInformation("Tasks deleted successfully. {rowsModifieds} rows affected", rowsModifieds);
          return;
        }
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on delete tasks by filter");
    }
  }


  public void Dispose() {
    GC.SuppressFinalize(this);
    context.Dispose();
  }
}
