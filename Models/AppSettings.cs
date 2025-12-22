using SQLite;

namespace MindfulJournal.Models;

/// <summary>
/// Represents the application settings and configuration.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the unique identifier for the settings record.
    /// </summary>
    [PrimaryKey]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the application lock is enabled.
    /// </summary>
    public bool IsLockEnabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the hash of the rigorous PIN.
    /// </summary>
    [MaxLength(128)]
    public string PinHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the last time the application was locked.
    /// </summary>
    public DateTime? LastLockTime { get; set; }

    /// <summary>
    /// Gets or sets the duration in minutes before the application auto-locks.
    /// </summary>
    public int AutoLockTimeoutMinutes { get; set; } = 0;
}
