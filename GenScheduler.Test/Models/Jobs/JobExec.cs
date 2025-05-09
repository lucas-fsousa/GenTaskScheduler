using GenTaskScheduler.Core.Abstractions.Common;

namespace GenTaskScheduler.Test.Models.Jobs {
  internal class JobExec: IJob {
    public string JobName { get; set; } = null!;
    public async Task ExecuteJobAsync(CancellationToken cancellationToken = default) {
      Console.WriteLine($"waiting for exec the job: {JobName}");
      await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
      Console.WriteLine("completed");
    }
  }
}
