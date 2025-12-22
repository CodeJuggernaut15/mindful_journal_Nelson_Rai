using SQLite;

namespace MindfulJournal.Models;

/// <summary>
/// Represents a single journal entry in the application.
/// </summary>
public class JournalEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the journal entry.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the entry.
    /// </summary>
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main content of the entry.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the date and time when the entry was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the primary mood associated with the entry.
    /// </summary>
    public MoodType Mood { get; set; } = MoodType.None;

    /// <summary>
    /// Gets or sets the secondary moods as a comma-separated string.
    /// </summary>
    [MaxLength(20)]
    public string SecondaryMoods { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of secondary moods.
    /// parsed from the <see cref="SecondaryMoods"/> property.
    /// </summary>
    [Ignore]
    public List<MoodType> SecondaryMoodList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(SecondaryMoods))
                return new List<MoodType>();
            
            return SecondaryMoods.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<MoodType>(s.Trim(), out var mood) ? mood : MoodType.None)
                .Where(m => m != MoodType.None)
                .Take(2)
                .ToList();
        }
        set
        {
            var moods = (value ?? new List<MoodType>())
                .Where(m => m != MoodType.None)
                .Take(2)
                .Select(m => ((int)m).ToString());
            SecondaryMoods = string.Join(",", moods);
        }
    }

    /// <summary>
    /// Gets a list containing both the primary mood and secondary moods.
    /// </summary>
    [Ignore]
    public List<MoodType> AllMoods
    {
        get
        {
            var moods = new List<MoodType>();
            if (Mood != MoodType.None)
                moods.Add(Mood);
            moods.AddRange(SecondaryMoodList);
            return moods;
        }
    }

    /// <summary>
    /// Gets or sets the category of the entry.
    /// </summary>
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the entry as a comma-separated string.
    /// </summary>
    [MaxLength(500)]
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of tags parsed from the <see cref="Tags"/> property.
    /// </summary>
    [Ignore]
    public List<string> TagList
    {
        get => string.IsNullOrWhiteSpace(Tags) 
            ? new List<string>() 
            : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(t => t.Trim())
                  .ToList();
        set => Tags = string.Join(",", value ?? new List<string>());
    }

    /// <summary>
    /// Gets a preview of the content, truncated to 100 characters.
    /// </summary>
    [Ignore]
    public string ContentPreview => Content.Length > 100 
        ? Content[..100] + "..." 
        : Content;

    /// <summary>
    /// Gets the emoji representation of the primary mood.
    /// </summary>
    [Ignore]
    public string MoodEmoji => MoodHelper.GetMoodEmoji(Mood);

    /// <summary>
    /// Gets the display name of the primary mood.
    /// </summary>
    [Ignore]
    public string MoodName => MoodHelper.GetMoodName(Mood);

    /// <summary>
    /// Gets the color code associated with the primary mood.
    /// </summary>
    [Ignore]
    public string MoodColor => MoodHelper.GetMoodColor(Mood);
}
