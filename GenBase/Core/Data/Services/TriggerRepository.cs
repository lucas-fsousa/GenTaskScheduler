using GenTaskScheduler.Core.Abstractions.Repository;
using GenTaskScheduler.Core.Data.Internal;
using GenTaskScheduler.Core.Enums;
using GenTaskScheduler.Core.Infra.Log;
using GenTaskScheduler.Core.Models.Triggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GenTaskScheduler.Core.Data.Services;
public class TriggerRepository(GenTaskSchedulerDbContext context, ILogger<ApplicationLogger> logger): ITriggerRepository {

  public async Task AddAsync(BaseTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      trigger.CreatedAt = DateTimeOffset.UtcNow;
      trigger.UpdatedAt = DateTimeOffset.UtcNow;
      trigger.Executions = 0;
      trigger.NextExecution = null;

      if(trigger.StartsAt < DateTimeOffset.UtcNow)
        throw new ArgumentException($"The param {trigger.StartsAt} cannot be in the past for triggers", nameof(trigger));

      await context.AddAsync(trigger, cancellationToken);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Trigger with Id {Id} added successfully", trigger.Id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on adding a new trigger with Id {Id}", trigger.Id);
    }
  }

  public async Task CommitAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken);

  public async Task DeleteAsync(Guid id, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      var trigger = await GetByIdAsync(id, cancellationToken) ?? throw new ArgumentException($"Trigger with ID {id} not found", nameof(id));
      context.BaseTriggers.Remove(trigger);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Trigger with Id {Id} deleted successfully", id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on deleting trigger with Id {Id}", id);
    }
  }

  public async Task<BaseTrigger?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) {
    try {
      return await context.BaseTriggers.FindAsync([id], cancellationToken);
    } catch(Exception ex) {
      logger.LogError(ex, "Error on getting trigger with Id {Id}", id);
      return null;
    }
  }

  public async Task<List<BaseTrigger>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default) {
    try {
      return await context.BaseTriggers.Where(t => t.TaskId == taskId).ToListAsync(cancellationToken);
    } catch(Exception ex) {
      logger.LogError(ex, "Error on getting triggers by taskId {Id}", taskId);
      return [];
    }
  }

  public async Task UpdateAsync(BaseTrigger trigger, bool autoCommit = true, CancellationToken cancellationToken = default) {
    try {
      trigger.UpdatedAt = DateTimeOffset.UtcNow;
      context.Update(trigger);
      if(autoCommit) {
        await CommitAsync(cancellationToken);
        logger.LogInformation("Trigger with Id {Id} updated successfully", trigger.Id);
      }
    } catch(Exception ex) {
      logger.LogError(ex, "Error on updating trigger with Id {Id}", trigger.Id);
    }
  }
  public void Dispose() {
    GC.SuppressFinalize(this);
    context.Dispose();
  }
}

