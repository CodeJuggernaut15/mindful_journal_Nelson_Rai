using MindfulJournal.Models;

namespace MindfulJournal.Services;

/// <summary>
/// Provides logic for managing predefined tags.
/// </summary>
public class TagService
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagService"/> class.
    /// </summary>
    /// <param name="databaseService">The database service instance.</param>
    public TagService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Initializes the database table for tags and seeds default tags if empty.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _databaseService.Database.CreateTableAsync<Tag>();
        
        var count = await _databaseService.Database.Table<Tag>().CountAsync();
        if (count == 0)
        {
            var defaultTags = new List<Tag>
            {
                new() { Name = "Gratitude" },
                new() { Name = "Reflection" },
                new() { Name = "Work" },
                new() { Name = "Family" },
                new() { Name = "Health" },
                new() { Name = "Ideas" },
                new() { Name = "Goals" },
                new() { Name = "Travel" }
            };

            await _databaseService.Database.InsertAllAsync(defaultTags);
        }
    }

    /// <summary>
    /// Retrieves all predefined tags.
    /// </summary>
    /// <returns>A list of all tags.</returns>
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _databaseService.Database
            .Table<Tag>()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new tag.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> AddTagAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return 0;

        var existing = await _databaseService.Database
            .Table<Tag>()
            .Where(t => t.Name.ToLower() == name.ToLower())
            .FirstOrDefaultAsync();

        if (existing != null)
            return 0;

        return await _databaseService.Database.InsertAsync(new Tag { Name = name });
    }

    /// <summary>
    /// Deletes a tag by its ID.
    /// </summary>
    /// <param name="id">The ID of the tag to delete.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> DeleteTagAsync(int id)
    {
        return await _databaseService.Database.DeleteAsync<Tag>(id);
    }
}
