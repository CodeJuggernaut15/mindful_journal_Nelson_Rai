using MindfulJournal.Models;
using System.Text;

namespace MindfulJournal.Services;

/// <summary>
/// Provides functionality for exporting journal entries to text files.
/// </summary>
public class ExportService
{
    private readonly JournalService _journalService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportService"/> class.
    /// </summary>
    /// <param name="journalService">The journal service for retrieving entries.</param>
    public ExportService(JournalService journalService)
    {
        _journalService = journalService;
    }

    /// <summary>
    /// Exports all journal entries to a formatted text string.
    /// </summary>
    /// <returns>A string containing the formatted export content.</returns>
    public async Task<string> ExportAllToTextAsync()
    {
        var entries = await _journalService.GetAllEntriesAsync();
        return FormatEntriesAsText(entries);
    }

    /// <summary>
    /// Exports journal entries within a specified date range to a text string.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>A formatted export string.</returns>
    public async Task<string> ExportByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var entries = await _journalService.GetEntriesByDateRangeAsync(startDate, endDate);
        return FormatEntriesAsText(entries);
    }

    /// <summary>
    /// Exports journal entries with a specific mood to a text string.
    /// </summary>
    /// <param name="mood">The mood filter.</param>
    /// <returns>A formatted export string.</returns>
    public async Task<string> ExportByMoodAsync(MoodType mood)
    {
        var entries = await _journalService.GetEntriesByMoodAsync(mood);
        return FormatEntriesAsText(entries);
    }

    /// <summary>
    /// Saves the provided content to a file in the Documents or LocalApplicationData folder.
    /// </summary>
    /// <param name="content">The content to save.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>The full path to the saved file.</returns>
    public async Task<string> SaveToFileAsync(string content, string fileName)
    {
        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var journalExportsPath = Path.Combine(documentsPath, "MindfulJournal Exports");
            
            if (!Directory.Exists(journalExportsPath))
            {
                Directory.CreateDirectory(journalExportsPath);
            }

            var filePath = Path.Combine(journalExportsPath, fileName);
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            
            return filePath;
        }
        catch (UnauthorizedAccessException)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var journalExportsPath = Path.Combine(appDataPath, "MindfulJournal", "Exports");
            
            if (!Directory.Exists(journalExportsPath))
            {
                Directory.CreateDirectory(journalExportsPath);
            }

            var filePath = Path.Combine(journalExportsPath, fileName);
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            
            return filePath;
        }
    }

    /// <summary>
    /// Opens the folder containing the exported file.
    /// </summary>
    /// <param name="filePath">The path of the file.</param>
    public async Task OpenExportFolderAsync(string filePath)
    {
        try
        {
            var folderPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
        }
        catch
        {
            // Ignore errors if opening folder fails
        }
    }

    /// <summary>
    /// Gets the path of the default export folder.
    /// </summary>
    /// <returns>The folder path.</returns>
    public string GetExportsFolderPath()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documentsPath, "MindfulJournal Exports");
    }

    /// <summary>
    /// Generates a unique filename for an export based on the current timestamp.
    /// </summary>
    /// <returns>A filename string.</returns>
    public string GenerateFileName()
    {
        return $"MindfulJournal_Export_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt";
    }

    private string FormatEntriesAsText(List<JournalEntry> entries)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                    MINDFUL JOURNAL EXPORT                     ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"Exported on: {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}");
        sb.AppendLine($"Total Entries: {entries.Count}");
        sb.AppendLine();
        sb.AppendLine("────────────────────────────────────────────────────────────────");
        sb.AppendLine();

        if (!entries.Any())
        {
            sb.AppendLine("No entries to export.");
            return sb.ToString();
        }

        var entriesByMonth = entries
            .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
            .OrderByDescending(g => g.Key.Year)
            .ThenByDescending(g => g.Key.Month);

        foreach (var monthGroup in entriesByMonth)
        {
            var monthDate = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1);
            sb.AppendLine($"═══ {monthDate:MMMM yyyy} ═══");
            sb.AppendLine();

            foreach (var entry in monthGroup.OrderByDescending(e => e.CreatedAt))
            {
                sb.AppendLine($"📅 {entry.CreatedAt:dddd, MMMM dd, yyyy 'at' h:mm tt}");
                sb.AppendLine($"📝 {entry.Title}");
                
                if (entry.Mood != MoodType.None)
                {
                    sb.AppendLine($"   Mood: {entry.MoodEmoji} {entry.MoodName}");
                }

                if (entry.TagList.Any())
                {
                    sb.AppendLine($"   Tags: {string.Join(", ", entry.TagList)}");
                }

                sb.AppendLine();
                sb.AppendLine(entry.Content);
                sb.AppendLine();
                sb.AppendLine("────────────────────────────────────────────────────────────────");
                sb.AppendLine();
            }
        }

        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("                    End of Export");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        return sb.ToString();
    }
}
