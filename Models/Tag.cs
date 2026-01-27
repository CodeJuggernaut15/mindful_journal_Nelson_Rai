using SQLite;

namespace MindfulJournal.Models;

/// <summary>
/// Represents a predefined tag that can be applied to journal entries.
/// </summary>
public class Tag
{
    /// <summary>
    /// Gets or sets the unique identifier for the tag.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the tag.
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
