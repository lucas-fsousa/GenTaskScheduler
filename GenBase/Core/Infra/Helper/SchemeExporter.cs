using GenTaskScheduler.Core.Abstractions.Providers;
using System.Text.RegularExpressions;

namespace GenTaskScheduler.Core.Infra.Helper;

/// <summary>
/// Utility to assist in exporting or obtaining database schemas.
/// </summary>
public sealed class SchemeExporter {
  private readonly ISchemeProvider _schemeProvider;

  private SchemeExporter(ISchemeProvider schemeProvider) => _schemeProvider = schemeProvider;

  public static SchemeExporter Create(ISchemeProvider schemeProvider) => new(schemeProvider);

  /// <summary>
  /// Extracts and exports the schematized scripts to create the database manually.
  /// </summary>
  /// <param name="filePath">Path of the SQL file to export.</param>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  public void ExportToFile(string filePath) { 
    if(string.IsNullOrEmpty(filePath))
      throw new ArgumentNullException(nameof(filePath));

    if(!Regex.IsMatch(filePath, @"\.sql^"))
      throw new InvalidOperationException($"Invalid file name or extension for [{filePath}]");

    File.WriteAllText(filePath, GetScript());
  }

  /// <summary>
  /// Extracts the script to create the database manually.
  /// </summary>
  /// <returns>A <see cref="string"/> with the schematized scripts</returns>
  public string GetScript() => _schemeProvider.GenerateSchemeScript();
}

