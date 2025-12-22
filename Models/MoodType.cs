namespace MindfulJournal.Models;

/// <summary>
/// Defines the various mood types available in the application.
/// </summary>
public enum MoodType
{
    /// <summary>
    /// No mood specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a very happy mood.
    /// </summary>
    VeryHappy = 1,

    /// <summary>
    /// Indicates a happy mood.
    /// </summary>
    Happy = 2,

    /// <summary>
    /// Indicates a calm mood.
    /// </summary>
    Calm = 3,

    /// <summary>
    /// Indicates a neutral mood.
    /// </summary>
    Neutral = 4,

    /// <summary>
    /// Indicates an anxious mood.
    /// </summary>
    Anxious = 5,

    /// <summary>
    /// Indicates a sad mood.
    /// </summary>
    Sad = 6,

    /// <summary>
    /// Indicates a very sad mood.
    /// </summary>
    VerySad = 7,

    /// <summary>
    /// Indicates an angry mood.
    /// </summary>
    Angry = 8
}

/// <summary>
/// Provides helper methods for retrieving mood-related display information.
/// </summary>
public static class MoodHelper
{
    /// <summary>
    /// Retrieves the emoji associated with the specified mood.
    /// </summary>
    /// <param name="mood">The mood type.</param>
    /// <returns>A string containing the emoji.</returns>
    public static string GetMoodEmoji(MoodType mood) => mood switch
    {
        MoodType.VeryHappy => "😄",
        MoodType.Happy => "🙂",
        MoodType.Calm => "😌",
        MoodType.Neutral => "😐",
        MoodType.Anxious => "😰",
        MoodType.Sad => "😢",
        MoodType.VerySad => "😭",
        MoodType.Angry => "😠",
        _ => "❓"
    };

    /// <summary>
    /// Retrieves the display name for the specified mood.
    /// </summary>
    /// <param name="mood">The mood type.</param>
    /// <returns>The string representation of the mood.</returns>
    public static string GetMoodName(MoodType mood) => mood switch
    {
        MoodType.VeryHappy => "Very Happy",
        MoodType.Happy => "Happy",
        MoodType.Calm => "Calm",
        MoodType.Neutral => "Neutral",
        MoodType.Anxious => "Anxious",
        MoodType.Sad => "Sad",
        MoodType.VerySad => "Very Sad",
        MoodType.Angry => "Angry",
        _ => "Unknown"
    };

    /// <summary>
    /// Retrieves the color hex code associated with the specified mood.
    /// </summary>
    /// <param name="mood">The mood type.</param>
    /// <returns>A string representing the color hex code.</returns>
    public static string GetMoodColor(MoodType mood) => mood switch
    {
        MoodType.VeryHappy => "#4CAF50",  // Green
        MoodType.Happy => "#8BC34A",       // Light Green
        MoodType.Calm => "#03A9F4",        // Light Blue
        MoodType.Neutral => "#9E9E9E",     // Grey
        MoodType.Anxious => "#FF9800",     // Orange
        MoodType.Sad => "#2196F3",         // Blue
        MoodType.VerySad => "#673AB7",     // Deep Purple
        MoodType.Angry => "#F44336",       // Red
        _ => "#757575"                      // Dark Grey
    };
}
