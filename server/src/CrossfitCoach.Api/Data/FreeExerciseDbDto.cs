using System.Text.Json.Serialization;

namespace CrossfitCoach.Api.Data;

/// <summary>
/// Represents a single exercise entry from the free-exercise-db exercises.json.
/// Instructions and images are intentionally omitted.
/// </summary>
internal sealed class FreeExerciseDbDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("force")]
    public string? Force { get; set; }

    [JsonPropertyName("level")]
    public string? Level { get; set; }

    [JsonPropertyName("mechanic")]
    public string? Mechanic { get; set; }

    [JsonPropertyName("equipment")]
    public string? Equipment { get; set; }

    [JsonPropertyName("primaryMuscles")]
    public string[] PrimaryMuscles { get; set; } = [];

    [JsonPropertyName("secondaryMuscles")]
    public string[] SecondaryMuscles { get; set; } = [];

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}
