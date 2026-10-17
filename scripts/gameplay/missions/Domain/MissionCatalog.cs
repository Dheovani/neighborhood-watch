using System.Text.Json;

namespace NeighborhoodWatch.Gameplay.Missions.Domain;

public sealed class MissionCatalog
{
    private readonly Dictionary<string, MissionDefinition> _byId;

    public IReadOnlyList<MissionDefinition> Definitions { get; }

    public MissionCatalog(IEnumerable<MissionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        MissionDefinition[] entries = definitions.ToArray();
        if (entries.Length == 0)
        {
            throw new ArgumentException("At least one mission is required.", nameof(definitions));
        }

        _byId = new Dictionary<string, MissionDefinition>(StringComparer.Ordinal);
        foreach (MissionDefinition entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Id) ||
                string.IsNullOrWhiteSpace(entry.Title) ||
                string.IsNullOrWhiteSpace(entry.Instruction) ||
                string.IsNullOrWhiteSpace(entry.LocationId) ||
                string.IsNullOrWhiteSpace(entry.InfractionCategory) ||
                entry.OffenseActions is null || entry.OffenseActions.Count == 0 ||
                entry.OffenseActions.Any(string.IsNullOrWhiteSpace) ||
                entry.OffenseActions.Distinct(StringComparer.Ordinal).Count() != entry.OffenseActions.Count ||
                entry.CompletionPoints <= 0)
            {
                throw new ArgumentException($"Invalid mission definition: {entry.Id}", nameof(definitions));
            }

            if (!_byId.TryAdd(entry.Id, entry))
            {
                throw new ArgumentException($"Duplicate mission ID: {entry.Id}", nameof(definitions));
            }
        }

        Definitions = entries;
    }

    public static MissionCatalog FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        List<MissionDefinition>? entries = JsonSerializer.Deserialize<List<MissionDefinition>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return new MissionCatalog(entries ?? throw new ArgumentException("Mission JSON is empty.", nameof(json)));
    }
}
