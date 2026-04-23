using System.Text.Json.Serialization;

namespace CrossfitCoach.Api.Models;

/// <summary>
/// Represents a single exercise entry deserialized from the free-exercise-db exercises.json.
/// Instructions and images are intentionally omitted.
/// </summary>
public sealed class ExerciseModel
{
    /// <summary>Stable identifier from free-exercise-db (e.g. "Barbell_Deadlift").</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable exercise name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Movement direction: push, pull, static, or null.</summary>
    [JsonPropertyName("force")]
    public string? Force { get; set; }

    /// <summary>Skill level: beginner, intermediate, or expert.</summary>
    [JsonPropertyName("level")]
    public string? Level { get; set; }

    /// <summary>Compound or isolation, or null.</summary>
    [JsonPropertyName("mechanic")]
    public string? Mechanic { get; set; }

    /// <summary>Equipment required (e.g. barbell, kettlebells, body only).</summary>
    [JsonPropertyName("equipment")]
    public string? Equipment { get; set; }

    /// <summary>Primary muscles targeted.</summary>
    [JsonPropertyName("primaryMuscles")]
    public string[] PrimaryMuscles { get; set; } = [];

    /// <summary>Secondary muscles targeted.</summary>
    [JsonPropertyName("secondaryMuscles")]
    public string[] SecondaryMuscles { get; set; } = [];

    /// <summary>Exercise category (e.g. strength, olympic weightlifting, plyometrics).</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}
