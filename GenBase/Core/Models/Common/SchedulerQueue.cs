using GenTaskScheduler.Core.Infra.Configurations;
using System.Collections.Concurrent;

namespace GenTaskScheduler.Core.Models.Common;
internal class SchedulerQueue<T> where T : class {
  private readonly ConcurrentQueue<T> _queue = new();
  private readonly ConcurrentDictionary<Guid, byte> _idSet = new();
  private readonly Func<T, Guid> _idSelector;
  internal int Count => _queue.Count;
  
  internal SchedulerQueue(Func<T, Guid> idSelector) => _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));

  internal bool Enqueue(T item) {
    if(_idSet.TryAdd(_idSelector(item), 0)) {
      _queue.Enqueue(item);
      return true;
    }
    return false;
  }

  internal bool TryDequeue(out T? item) {
    if(_queue.TryDequeue(out item)) {
      var id = _idSelector(item);
      _idSet.TryRemove(id, out _);
      return true;
    }

    return false;
  }

  internal bool Contains(Guid id) => _idSet.ContainsKey(id);
  
  internal bool CompareEquals(T item) => _queue.Any(x => x.Equals(item));
}
