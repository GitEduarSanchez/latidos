using Latidos.Models;
using System.Text.Json;

namespace Latidos.Services;

public class CompetitorService : ICompetitorService
{
    private static readonly List<CompetitorProfile> _competitors = new();
    private static readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private static bool _isLoaded;

    private static string StoragePath => Path.Combine(FileSystem.AppDataDirectory, "competidores.json");

    public async Task<List<CompetitorProfile>> SearchCompetitorsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<CompetitorProfile>();
        }

        await EnsureLoadedAsync();

        var normalizedQuery = query.Trim();

        await _gate.WaitAsync();
        try
        {
            return _competitors
                .Where(competitor =>
                    competitor.FullName.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ||
                    competitor.DocumentNumber.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                .OrderBy(competitor => competitor.FullName)
                .Take(8)
                .Select(Clone)
                .ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveCompetitorAsync(CompetitorProfile competitor)
    {
        await EnsureLoadedAsync();
        await _gate.WaitAsync();

        try
        {
            UpsertCompetitor(competitor);
            await SaveAllAsync();
        }
        finally
        {
            _gate.Release();
        }
    }

    private static void UpsertCompetitor(CompetitorProfile competitor)
    {
        var existing = _competitors.FirstOrDefault(saved =>
            saved.DocumentType.Equals(competitor.DocumentType, StringComparison.OrdinalIgnoreCase) &&
            saved.DocumentNumber.Equals(competitor.DocumentNumber, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            _competitors.Add(Clone(competitor));
            return;
        }

        existing.CompetitorNumber = competitor.CompetitorNumber;
        existing.FullName = competitor.FullName;
        existing.BirthDate = competitor.BirthDate;
        existing.PhotoPath = competitor.PhotoPath;
    }

    private static async Task EnsureLoadedAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        await _gate.WaitAsync();
        try
        {
            if (_isLoaded)
            {
                return;
            }

            if (File.Exists(StoragePath))
            {
                await using var stream = File.OpenRead(StoragePath);
                var savedCompetitors = await JsonSerializer.DeserializeAsync<List<CompetitorProfile>>(stream, _jsonOptions);

                if (savedCompetitors != null)
                {
                    foreach (var competitor in savedCompetitors)
                    {
                        UpsertCompetitor(competitor);
                    }
                }
            }

            _isLoaded = true;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task SaveAllAsync()
    {
        Directory.CreateDirectory(FileSystem.AppDataDirectory);

        await using var stream = File.Create(StoragePath);
        await JsonSerializer.SerializeAsync(stream, _competitors, _jsonOptions);
    }

    private static CompetitorProfile Clone(CompetitorProfile competitor)
    {
        return new CompetitorProfile
        {
            CompetitorNumber = competitor.CompetitorNumber,
            FullName = competitor.FullName,
            DocumentType = competitor.DocumentType,
            DocumentNumber = competitor.DocumentNumber,
            BirthDate = competitor.BirthDate,
            PhotoPath = competitor.PhotoPath
        };
    }
}
