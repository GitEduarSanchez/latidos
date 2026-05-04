using System.Text.Json;
using Latidos.Models;

namespace Latidos.Services;

public class ResultService : IResultService
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static readonly List<CompetitorResult> Results = new();
    private static bool _isLoaded;

    private static string StoragePath => Path.Combine(FileSystem.AppDataDirectory, "resultados.json");

    public async Task<List<CompetitorResult>> GetResultsByEventAsync(int eventId)
    {
        await EnsureLoadedAsync();

        await Gate.WaitAsync();
        try
        {
            return Results
                .Where(r => r.EventId == eventId)
                .Select(Clone)
                .ToList();
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task SaveResultsAsync(int eventId, IEnumerable<CompetitorResult> results)
    {
        await EnsureLoadedAsync();
        await Gate.WaitAsync();
        try
        {
            Results.RemoveAll(r => r.EventId == eventId);
            Results.AddRange(results.Select(Clone));
            await SaveAllAsync();
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task EnsureLoadedAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        await Gate.WaitAsync();
        try
        {
            if (_isLoaded)
            {
                return;
            }

            if (File.Exists(StoragePath))
            {
                await using var stream = File.OpenRead(StoragePath);
                var data = await JsonSerializer.DeserializeAsync<List<CompetitorResult>>(stream, JsonOptions);
                if (data != null)
                {
                    Results.Clear();
                    Results.AddRange(data.Select(Clone));
                }
            }

            _isLoaded = true;
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task SaveAllAsync()
    {
        Directory.CreateDirectory(FileSystem.AppDataDirectory);
        await using var stream = File.Create(StoragePath);
        await JsonSerializer.SerializeAsync(stream, Results, JsonOptions);
    }

    private static CompetitorResult Clone(CompetitorResult item)
    {
        return new CompetitorResult
        {
            EventId = item.EventId,
            CompetitorDocument = item.CompetitorDocument,
            CompetitorName = item.CompetitorName,
            CompetitorNumber = item.CompetitorNumber,
            OfficialTime = item.OfficialTime,
            UpdatedAt = item.UpdatedAt
        };
    }
}
