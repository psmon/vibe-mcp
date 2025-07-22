using Raven.Client.Documents.Indexes;

namespace WebnoriMemory.Models;

public class WebnoriNote
{
    public string? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Location data
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationName { get; set; }
    
    // Vector embedding for similarity search (dummy implementation)
    public float[]? TitleEmbedding { get; set; }
}

public class WebnoriNoteIndex : AbstractIndexCreationTask<WebnoriNote>
{
    public WebnoriNoteIndex()
    {
        Map = notes => from note in notes
                      select new
                      {
                          note.Title,
                          note.Content,
                          note.CreatedAt,
                          note.UpdatedAt,
                          note.Latitude,
                          note.Longitude,
                          note.LocationName
                      };

        // Full text search on Title and Content
        Index(x => x.Title, FieldIndexing.Search);
        Index(x => x.Content, FieldIndexing.Search);
    }
}