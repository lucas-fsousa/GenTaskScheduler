namespace GenTaskScheduler.Core.Abstractions.Repository;
public interface IGlobalDbContextControl {
  Task CommitAsync(CancellationToken cancellationToken = default);
}

