using GenTaskScheduler.Core.Abstractions.Common;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace GenTaskScheduler.Core.Infra.Helper;

internal static class TaskSerializer {
  private class TaskWrapper {
    public string TypeName { get; set; } = string.Empty;
    public object Payload { get; set; } = new();
  }

  private class SerializedObjectWrapper {
    public string TypeName { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
  }

  /// <summary>
  /// Serializes an <see cref="IJob"/> object into a byte array.
  /// </summary>
  /// <param name="job">Job for Serialization</param>
  /// <returns></returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  internal static byte[] Serialize(IJob? job) {
    var type = job?.GetType() ?? throw new ArgumentNullException(nameof(job), "The job cannot be null");

    var wrapper = new TaskWrapper() {
      TypeName = type.AssemblyQualifiedName ?? throw new InvalidOperationException($"The object {type.FullName} is not serializable"),
      Payload = job
    };

    return JsonSerializer.SerializeToUtf8Bytes(wrapper);
  }

  /// <summary>
  /// Deserializes a byte array into an IJob object.
  /// </summary>
  /// <param name="data"></param>
  /// <returns>Returns a Deserialized <see cref="IJob"/></returns>
  /// <exception cref="InvalidOperationException"></exception>
  internal static IJob Deserialize(byte[] data) {
    var wrapper = JsonSerializer.Deserialize<TaskWrapper>(data) ?? throw new InvalidOperationException("Error on diserialize payload data");
    var taskType = Type.GetType(wrapper.TypeName) ?? throw new InvalidOperationException($"The type {wrapper.TypeName} is not registered");
    var payload = JsonSerializer.Serialize(wrapper.Payload);
    var serialized = JsonSerializer.Deserialize(payload, taskType) as IJob;
    return serialized ?? throw new InvalidOperationException($"The payload connot be parsed");
  }

  /// <summary>
  /// Serializes an object into a byte array, wrapping it
  /// in a JSON structure that includes the object's type.
  /// </summary>
  /// <param name="data">The object to be serialized.</param>
  /// <returns>A byte array representing the serialized object and its type.</returns>
  internal static byte[]? SerializeObjectToBytes(object? data) {
    if(data is null)
      return [];

    var wrapper = new SerializedObjectWrapper {
      TypeName = data.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException("Type is not valid"),
      Data = JsonSerializer.SerializeToElement(data, data.GetType())
    };

    return JsonSerializer.SerializeToUtf8Bytes(wrapper);
  }

  /// <summary>
  /// Checks if a type is considered "simple" for serialization purposes.
  /// Simple types include C# primitives, enums, string, decimal, DateTime, etc.
  /// </summary>
  /// <param name="type">The type to check.</param>
  /// <returns>True if the type is simple, False otherwise.</returns>
  internal static bool IsSimpleType(Type type) {
    var pattern = @"^System\.[a-z0-9]+$";
    var match = Regex.IsMatch(type.FullName ?? string.Empty, pattern);
    return type.IsPrimitive || type.IsEnum || match;
  }

  /// <summary>
  /// Deserializes a byte array back into the original object.
  /// </summary>
  /// <param name="data">The bytes to be deserialized.</param>
  /// <returns>The deserialized object.</returns>
  internal static object? DeserializeBytesToObject(byte[]? data) {
    if(data is null || data.Length == 0)
      return null;

    var wrapper = JsonSerializer.Deserialize<SerializedObjectWrapper>(data)
        ?? throw new InvalidOperationException("Could not deserialize wrapper");

    var type = Type.GetType(wrapper.TypeName)
        ?? throw new InvalidOperationException($"Type {wrapper.TypeName} not found");

    var rawJson = wrapper.Data.GetRawText();

    return JsonSerializer.Deserialize(rawJson, type);
  }
}

