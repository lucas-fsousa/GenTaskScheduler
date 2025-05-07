namespace GenTaskScheduler.Core.Models.Common;

/// <summary>
/// Represents a range of integers. Similar to .NET <see cref="Range"/>, but with other internal purposes.
/// </summary>
/// <param name="Start">Start number</param>
/// <param name="End">End number</param>
public readonly record struct IntRange(int Start, int End) {
  /// <summary>
  /// Represents a range of integers with both start and end as zero.
  /// </summary>
  public static readonly IntRange Zero = new(0, 0);

  /// <summary>
  /// Gets integer values ​​within the stipulated range.
  /// </summary>
  /// <returns>
  /// An <see cref="IEnumerable{T}"/> that contains all the integer values within the specified range.
  /// If the range is represented by <see cref="IntRange.Zero"/>, it will return a sequence with a single value of 0.
  /// </returns>
  public IEnumerable<int> Expand() {
    if(this == Zero)
      return [0];

    var (from, to) = Start <= End ? (Start, End) : (End, Start);
    return Enumerable.Range(from, to - from + 1);
  }
}
