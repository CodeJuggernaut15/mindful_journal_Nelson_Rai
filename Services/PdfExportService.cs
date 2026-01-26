using MindfulJournal.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace MindfulJournal.Services;

/// <summary>
/// Provides functionality for exporting journal entries to PDF format.
/// </summary>
public class PdfExportService
{
    private readonly JournalService _journalService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfExportService"/> class.
    /// Sets the QuestPDF license to Community.
    /// </summary>
    /// <param name="journalService">The journal service for retrieving entries.</param>
    public PdfExportService(JournalService journalService)
    {
        _journalService = journalService;
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    /// <summary>
    /// Exports all journal entries to a PDF file.
    /// </summary>
    /// <returns>The file path of the generated PDF.</returns>
    public async Task<string> ExportAllToPdfAsync()
    {
        var entries = await _journalService.GetAllEntriesAsync();
        return GeneratePdf(entries, "All Journal Entries");
    }

    /// <summary>
    /// Exports journal entries within a specified date range to a PDF file.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>The file path of the generated PDF.</returns>
    public async Task<string> ExportByDateRangePdfAsync(DateTime startDate, DateTime endDate)
    {
        var entries = await _journalService.GetEntriesByDateRangeAsync(startDate, endDate);
        var title = $"Journal Entries ({startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy})";
        return GeneratePdf(entries, title);
    }

    private string GeneratePdf(List<JournalEntry> entries, string title)
    {
        var fileName = $"MindfulJournal_Export_{DateTime.Now:yyyy-MM-dd_HHmmss}.pdf";
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var exportPath = Path.Combine(documentsPath, "MindfulJournal Exports");
        
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        var filePath = Path.Combine(exportPath, fileName);

        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.SegoeUI));

                page.Header()
                    .Text(title)
                    .SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre)
                    .Column(x =>
                    {
                        if (!entries.Any())
                        {
                            x.Item().Text("No entries found for this period.").Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        }
                        else
                        {
                            foreach (var entry in entries)
                            {
                                x.Item().Element(container => ComposeEntry(container, entry));
                            }
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        })
        .GeneratePdf(filePath);

        return filePath;
    }

    private void ComposeEntry(QuestPDF.Infrastructure.IContainer container, JournalEntry entry)
    {
        container
            .PaddingBottom(1, QuestPDF.Infrastructure.Unit.Centimetre)
            .Decoration(decoration =>
            {
                decoration
                    .Before()
                    .PaddingBottom(5)
                    .BorderBottom(1)
                    .BorderColor(QuestPDF.Helpers.Colors.Grey.Medium)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(entry.Title).Bold().FontSize(14);
                            col.Item().Text($"{entry.CreatedAt:dddd, MMMM dd, yyyy} • {entry.CreatedAt:h:mm tt}")
                               .FontSize(10).FontColor(QuestPDF.Helpers.Colors.Black);
                        });

                        if (entry.Mood != MoodType.None)
                        {
                            row.AutoItem().Text($"{entry.MoodEmoji} {entry.MoodName}")
                               .FontSize(12).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                        }
                    });

                decoration
                    .Content()
                    .PaddingTop(10)
                    .Column(col =>
                    {
                        var content = StripMarkdown(entry.Content);
                        col.Item().Text(content).LineHeight(1.5f);

                        if (!string.IsNullOrWhiteSpace(entry.Tags))
                        {
                            col.Item().PaddingTop(5).Text($"Tags: {entry.Tags}")
                               .FontSize(9).Italic().FontColor(QuestPDF.Helpers.Colors.Black);
                        }
                    });
            });
    }

    private string StripMarkdown(string markdown)
    {
        if (string.IsNullOrEmpty(markdown)) return string.Empty;
        
        return markdown
            .Replace("**", "")
            .Replace("*", "")
            .Replace("# ", "")
            .Replace("## ", "")
            .Replace("### ", "")
            .Replace("> ", "")
            .Replace("[ ] ", "☐ ")
            .Replace("[x] ", "☑ ");
    }
}
