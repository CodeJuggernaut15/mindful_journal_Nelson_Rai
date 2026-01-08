using MindfulJournal.Models;

namespace MindfulJournal.Services;

/// <summary>
/// Provides logic for managing journal entries, including CRUD operations,
/// searching, filtering, and statistical analysis.
/// </summary>
public class JournalService
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalService"/> class.
    /// </summary>
    /// <param name="databaseService">The database service instance.</param>
    public JournalService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Retrieves all journal entries from the database, ordered by creation date descending.
    /// </summary>
    /// <returns>A list of all journal entries.</returns>
    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _databaseService.Database
            .Table<JournalEntry>()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a specific journal entry by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the journal entry.</param>
    /// <returns>The journal entry if found; otherwise, null.</returns>
    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _databaseService.Database
            .Table<JournalEntry>()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves the most recent journal entries up to a specified count.
    /// </summary>
    /// <param name="count">The number of entries to retrieve. Defaults to 5.</param>
    /// <returns>A list of recent journal entries.</returns>
    public async Task<List<JournalEntry>> GetRecentEntriesAsync(int count = 5)
    {
        return await _databaseService.Database
            .Table<JournalEntry>()
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Searches for journal entries that contain the specified term in the title, content, or tags.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>A list of matching journal entries.</returns>
    public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllEntriesAsync();

        var lowerSearch = searchTerm.ToLower();
        
        var allEntries = await GetAllEntriesAsync();
        
        return allEntries
            .Where(e => e.Title.ToLower().Contains(lowerSearch) || 
                        e.Content.ToLower().Contains(lowerSearch) ||
                        e.Tags.ToLower().Contains(lowerSearch))
            .ToList();
    }

    /// <summary>
    /// Retrieves journal entries filtered by a specific mood.
    /// </summary>
    /// <param name="mood">The mood to filter by.</param>
    /// <returns>A list of journal entries matching the specified mood.</returns>
    public async Task<List<JournalEntry>> GetEntriesByMoodAsync(MoodType mood)
    {
        return await _databaseService.Database
            .Table<JournalEntry>()
            .Where(e => e.Mood == mood)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves journal entries created within a specific date range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A list of journal entries within the specified range.</returns>
    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _databaseService.Database
            .Table<JournalEntry>()
            .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a journal entry for a specific date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>The first journal entry found for that date; otherwise, null.</returns>
    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        return await _databaseService.Database
            .Table<JournalEntry>()
            .Where(e => e.CreatedAt >= startOfDay && e.CreatedAt <= endOfDay)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new journal entry in the database.
    /// </summary>
    /// <param name="entry">The journal entry to create.</param>
    /// <returns>The number of rows affected.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the entry is null.</exception>
    public async Task<int> CreateEntryAsync(JournalEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        entry.UpdatedAt = null;
        return await _databaseService.Database.InsertAsync(entry);
    }

    /// <summary>
    /// Updates an existing journal entry in the database.
    /// </summary>
    /// <param name="entry">The journal entry to update.</param>
    /// <returns>The number of rows affected.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the entry is null.</exception>
    public async Task<int> UpdateEntryAsync(JournalEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        entry.UpdatedAt = DateTime.Now;
        return await _databaseService.Database.UpdateAsync(entry);
    }

    /// <summary>
    /// Deletes a journal entry from the database.
    /// </summary>
    /// <param name="entry">The journal entry to delete.</param>
    /// <returns>The number of rows affected.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the entry is null.</exception>
    public async Task<int> DeleteEntryAsync(JournalEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        return await _databaseService.Database.DeleteAsync(entry);
    }

    /// <summary>
    /// Deletes a journal entry by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the entry to delete.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> DeleteEntryByIdAsync(int id)
    {
        var entry = await GetEntryByIdAsync(id);
        if (entry != null)
            return await DeleteEntryAsync(entry);
        return 0;
    }

    /// <summary>
    /// Gets the total count of journal entries in the database.
    /// </summary>
    /// <returns>The total number of entries.</returns>
    public async Task<int> GetEntryCountAsync()
    {
        return await _databaseService.Database
            .Table<JournalEntry>()
            .CountAsync();
    }

    /// <summary>
    /// Generates statistics on mood distribution across all journal entries.
    /// </summary>
    /// <returns>A dictionary mapping mood types to their occurrence count.</returns>
    public async Task<Dictionary<MoodType, int>> GetMoodStatisticsAsync()
    {
        var entries = await GetAllEntriesAsync();
        return entries
            .GroupBy(e => e.Mood)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Calculates the current journaling streak in days.
    /// </summary>
    /// <param name="entries">The list of journal entries to analyze.</param>
    /// <returns>The number of consecutive days with at least one entry, ending today or yesterday.</returns>
    public int CalculateStreak(List<JournalEntry> entries)
    {
        if (entries == null || !entries.Any()) 
            return 0;

        var entryDates = entries
            .Select(e => e.CreatedAt.Date)
            .Distinct()
            .ToHashSet();

        if (!entryDates.Any()) 
            return 0;

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        DateTime currentCheckDate;
        
        if (entryDates.Contains(today))
        {
            currentCheckDate = today;
        }
        else if (entryDates.Contains(yesterday))
        {
            currentCheckDate = yesterday;
        }
        else
        {
            return 0;
        }

        var streakCount = 0;
        
        while (entryDates.Contains(currentCheckDate))
        {
            streakCount++;
            currentCheckDate = currentCheckDate.AddDays(-1);
        }

        return streakCount;
    }
}
