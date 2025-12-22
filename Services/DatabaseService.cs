using SQLite;
using MindfulJournal.Models;

namespace MindfulJournal.Services;

/// <summary>
/// Handles the initialization and connection to the SQLite database.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _databasePath;

    /// <summary>
    /// Gets the active database connection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the database has not been initialized.</exception>
    public SQLiteAsyncConnection Database => _database ?? throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseService"/> class.
    /// Sets up the database path.
    /// </summary>
    public DatabaseService()
    {
        var appDataPath = FileSystem.AppDataDirectory;
        _databasePath = Path.Combine(appDataPath, "mindfuljournal.db");
    }

    /// <summary>
    /// Asynchronously initializes the database connection and creates tables if they do not exist.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        
        await _database.CreateTableAsync<JournalEntry>();
        
        System.Diagnostics.Debug.WriteLine($"Database initialized at: {_databasePath}");
    }

    /// <summary>
    /// Gets the full file path to the database.
    /// </summary>
    /// <returns>The database file path.</returns>
    public string GetDatabasePath() => _databasePath;
}
