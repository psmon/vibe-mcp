using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using WebnoriMemory.Models;
using WebnoriMemory.Repositories;
using WebnoriMemory.Services;

namespace WebnoriMemory.Tools;

[McpServerToolType]
public static class WebnoriMemoryTools
{
    [McpServerTool]
    [Description("웹노리메모리에 내용을 저장합니다. 제목과 내용을 제공하면 저장하고, 내용에서 지역정보를 자동으로 추출하여 위경도로 변환합니다.")]
    public static async Task<string> SaveMemory(
        IServiceProvider serviceProvider,
        [Description("노트의 제목")] string title,
        [Description("노트의 내용")] string content)
    {
        var repository = serviceProvider.GetRequiredService<WebnoriRepository>();
        var geocodingService = serviceProvider.GetRequiredService<GeocodingService>();
        var vectorService = serviceProvider.GetRequiredService<VectorService>();

        // Extract location from content
        var fullText = $"{title} {content}";
        var (latitude, longitude, locationName) = geocodingService.ExtractLocationFromText(fullText);

        // Save note
        var note = await repository.SaveNoteAsync(title, content, latitude, longitude, locationName);

        // Generate and save embedding for the title
        note.TitleEmbedding = vectorService.GenerateEmbedding(title);
        
        var result = new
        {
            success = true,
            noteId = note.Id,
            title = note.Title,
            hasLocation = latitude.HasValue,
            locationName = locationName,
            latitude = latitude,
            longitude = longitude
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("웹노리메모리에서 텍스트로 검색합니다. FullTextSearch를 사용하여 제목과 내용에서 검색합니다.")]
    public static async Task<string> SearchMemory(
        IServiceProvider serviceProvider,
        [Description("검색할 텍스트")] string searchText)
    {
        var repository = serviceProvider.GetRequiredService<WebnoriRepository>();
        
        var notes = await repository.FullTextSearchAsync(searchText);
        
        var result = new
        {
            success = true,
            count = notes.Count,
            notes = notes.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                content = n.Content.Length > 200 ? n.Content.Substring(0, 200) + "..." : n.Content,
                createdAt = n.CreatedAt,
                hasLocation = n.Latitude.HasValue,
                locationName = n.LocationName
            })
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("특정 위치에 가까운 노트를 검색합니다. 예: '창원에 가까운 노트' 검색시 창원 근처의 노트를 찾습니다.")]
    public static async Task<string> SearchNearLocation(
        IServiceProvider serviceProvider,
        [Description("위치 쿼리 (예: '창원에 가까운 노트')")] string locationQuery)
    {
        var repository = serviceProvider.GetRequiredService<WebnoriRepository>();
        var geocodingService = serviceProvider.GetRequiredService<GeocodingService>();

        // Extract location from query
        var (latitude, longitude, locationName) = geocodingService.ExtractLocationFromText(locationQuery);
        
        if (!latitude.HasValue || !longitude.HasValue)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "위치를 찾을 수 없습니다. 한국의 도시명을 포함해주세요."
            });
        }

        // Search within 50km radius by default
        var notes = await repository.SearchNearLocationAsync(latitude.Value, longitude.Value, 50);
        
        var result = new
        {
            success = true,
            searchLocation = locationName,
            searchLatitude = latitude,
            searchLongitude = longitude,
            count = notes.Count,
            notes = notes.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                content = n.Content.Length > 200 ? n.Content.Substring(0, 200) + "..." : n.Content,
                locationName = n.LocationName,
                distance = n.Latitude.HasValue && n.Longitude.HasValue 
                    ? Math.Round(geocodingService.CalculateDistance(latitude.Value, longitude.Value, n.Latitude.Value, n.Longitude.Value), 2)
                    : (double?)null
            }).OrderBy(n => n.distance)
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("제목의 유사검색을 수행합니다. 벡터 임베딩을 사용하여 의미적으로 유사한 제목을 가진 노트를 찾습니다.")]
    public static async Task<string> SearchSimilarTitles(
        IServiceProvider serviceProvider,
        [Description("유사한 제목을 찾을 쿼리 제목")] string queryTitle)
    {
        var repository = serviceProvider.GetRequiredService<WebnoriRepository>();
        var vectorService = serviceProvider.GetRequiredService<VectorService>();

        // Get all notes
        var allNotes = await repository.GetAllNotesAsync();
        
        // Prepare notes with embeddings
        var notesWithEmbeddings = new List<(string Id, string Title, float[] Embedding)>();
        foreach (var note in allNotes)
        {
            var embedding = note.TitleEmbedding ?? vectorService.GenerateEmbedding(note.Title);
            notesWithEmbeddings.Add((note.Id!, note.Title, embedding));
        }

        // Find similar titles
        var similarNotes = vectorService.FindSimilarTitles(queryTitle, notesWithEmbeddings, 5);
        
        // Get full note details for similar notes
        var result = new
        {
            success = true,
            queryTitle = queryTitle,
            similarNotes = new List<object>()
        };

        foreach (var (id, similarity) in similarNotes)
        {
            var note = await repository.GetNoteByIdAsync(id);
            if (note != null)
            {
                ((List<object>)result.similarNotes).Add(new
                {
                    id = note.Id,
                    title = note.Title,
                    content = note.Content.Length > 200 ? note.Content.Substring(0, 200) + "..." : note.Content,
                    similarity = Math.Round(similarity, 4),
                    hasLocation = note.Latitude.HasValue,
                    locationName = note.LocationName
                });
            }
        }

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("웹노리메모리의 모든 노트 목록을 가져옵니다.")]
    public static async Task<string> ListAllMemories(
        IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<WebnoriRepository>();
        
        var notes = await repository.GetAllNotesAsync();
        
        var result = new
        {
            success = true,
            count = notes.Count,
            notes = notes.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                contentPreview = n.Content.Length > 100 ? n.Content.Substring(0, 100) + "..." : n.Content,
                createdAt = n.CreatedAt,
                hasLocation = n.Latitude.HasValue,
                locationName = n.LocationName
            }).OrderByDescending(n => n.createdAt)
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}