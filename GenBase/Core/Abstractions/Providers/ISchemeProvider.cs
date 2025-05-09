namespace GenTaskScheduler.Core.Abstractions.Providers;

/// <summary>
/// Interface to assist in creating scripts containing database schemas.
/// </summary>
public interface ISchemeProvider {

  /// <summary>
  /// Generate the SQL script with the schemas used to create the database.
  /// </summary>
  /// <returns>returns a <see cref="string"/> with the scheme data.</returns>
  string GenerateSchemeScript();
}

