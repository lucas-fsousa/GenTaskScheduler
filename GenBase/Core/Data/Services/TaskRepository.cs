using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Internal;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Log;
using GenTaskScheduler.Core.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenTaskScheduler.Core.Data.Services;

public class TaskRepository(GenTaskSchedulerDbContext context, ILogger<ApplicationLogger> logger, ITriggerRepository triggerRepository): ITaskRepository {
  public async Task AddAsync(ScheduledTask task, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      if(task is null)
        throw new ArgumentNullException(nameof(task), "Task cannot be null");

      if(task.Triggers is null || task.Triggers.Count == 0)
        throw new ArgumentException("Task must have at least one trigger", nameof(task));

      var existe = await context.ScheduledTasks.AnyAsync(t => t.Name.ToLower().Equals(task.Name.ToLower()) && t.IsActive, cancellationToken);
      if(existe)
        throw new ArgumentException($"Task with name [{task.Name}] already exists", nameof(task));

      foreach(var trigger in task.Triggers)
        context.Entry(trigger).State = EntityState.Added;

      task.CreatedAt = DateTimeOffset.UtcNow;
      task.UpdatedAt = DateTimeOffset.UtcNow;
      task.ExecutionStatus = ExecutionStatus.Ready;
      task.DependsOnTaskId = task.DependsOnTask?.Id ?? task.DependsOnTask?.Id;

      var tmpDependency = task.DependsOnTask;
      while(tmpDependency is not null) {
        tmpDependency.CreatedAt = DateTimeOffset.UtcNow;
        tmpDependency.UpdatedAt = DateTimeOffset.UtcNow;
        tmpDependency.ExecutionStatus = ExecutionStatus.Ready;
        tmpDependency = tmpDependency.DependsOnTask;
      }

      var triggers = task.Triggers;
      task.Triggers.Clear();
      foreach(var trigger in triggers) {
        await triggerRepository.AddAsync(trigger, false, cancellationToken);
        trigger.TaskId = task.Id;
      }
      
      await context.ScheduledTasks.AddAsync(task, cancellationToken);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Task with Id {Id} added successfully", task.Id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on adding task with Id {Id}", task.Id);
    }
  }

  public async Task CommitAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

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

  public async Task<List<ScheduledTask>> GetAllAsync(CancellationToken cancellationToken = default) {
    try {
      var tasks = await context.ScheduledTasks.ToListAsync(cancellationToken) ?? [];
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

  public async Task<ScheduledTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await context.ScheduledTasks.FindAsync([id], cancellationToken);

  public async Task UpdateAsync(ScheduledTask task, bool autoCommit = true ,CancellationToken cancellationToken = default) {
    try {
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

  public void Dispose() {
    GC.SuppressFinalize(this);
    context.Dispose();
  }
}
