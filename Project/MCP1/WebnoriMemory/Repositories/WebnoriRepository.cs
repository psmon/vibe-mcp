using Raven.Client.Documents;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using WebnoriMemory.Models;
using WebnoriMemory.Services;

namespace WebnoriMemory.Repositories;

public class WebnoriRepository
{
    private readonly RavenDbService _ravenDbService;

    public WebnoriRepository(RavenDbService ravenDbService)
    {
        _ravenDbService = ravenDbService;
    }

    public async Task<WebnoriNote> SaveNoteAsync(string title, string content, double? latitude = null, double? longitude = null, string? locationName = null)
    {
        using var session = _ravenDbService.DocumentStore.OpenAsyncSession();
        
        var note = new WebnoriNote
        {
            Title = title,
            Content = content,
            Latitude = latitude,
            Longitude = longitude,
            LocationName = locationName
        };

        await session.StoreAsync(note);
        await session.SaveChangesAsync();
        
        return note;
    }

    public async Task<List<WebnoriNote>> FullTextSearchAsync(string searchText)
    {
        using var session = _ravenDbService.DocumentStore.OpenAsyncSession();
        
        var query = session.Query<WebnoriNote, WebnoriNoteIndex>()
            .Search(x => x.Title, searchText)
            .Search(x => x.Content, searchText, options: SearchOptions.Or)
            .Customize(x => x.WaitForNonStaleResults());

        return await query.ToListAsync();
    }

    public async Task<List<WebnoriNote>> SearchNearLocationAsync(double latitude, double longitude, double radiusInKm)
    {
        using var session = _ravenDbService.DocumentStore.OpenAsyncSession();
        
        var query = session.Query<WebnoriNote>()
            .Spatial(
                r => r.Point(x => x.Latitude, x => x.Longitude),
                criteria => criteria.WithinRadius(radiusInKm, latitude, longitude))
            .Customize(x => x.WaitForNonStaleResults());

        return await query.ToListAsync();
    }

    public async Task<List<WebnoriNote>> GetAllNotesAsync()
    {
        using var session = _ravenDbService.DocumentStore.OpenAsyncSession();
        return await session.Query<WebnoriNote>().ToListAsync();
    }

    public async Task<WebnoriNote?> GetNoteByIdAsync(string id)
    {
        using var session = _ravenDbService.DocumentStore.OpenAsyncSession();
        return await session.LoadAsync<WebnoriNote>(id);
    }
}