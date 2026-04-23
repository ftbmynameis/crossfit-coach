namespace CrossfitCoach.Api.Entities;

/// <summary>
/// Database entity representing an exercise from the exercise library.
/// The full free-exercise-db dataset is synced on startup; the CrossFit-relevant
/// subset is flagged via <see cref="IsCrossFit"/>.
/// </summary>
public class ExerciseEntity
{
    /// <summary>Database primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>Stable identifier from free-exercise-db (e.g. "Barbell_Deadlift").</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Human-readable exercise name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Movement direction: push, pull, static, or null.</summary>
    public string? Force { get; set; }

    /// <summary>Skill level: beginner, intermediate, or expert.</summary>
    public string? Level { get; set; }

    /// <summary>Compound or isolation, or null.</summary>
    public string? Mechanic { get; set; }

    /// <summary>Equipment required (e.g. barbell, kettlebells, body only).</summary>
    public string? Equipment { get; set; }

    /// <summary>Primary muscles targeted.</summary>
    public string[] PrimaryMuscles { get; set; } = [];

    /// <summary>Secondary muscles targeted.</summary>
    public string[] SecondaryMuscles { get; set; } = [];

    /// <summary>Exercise category (e.g. strength, olympic weightlifting, plyometrics).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// True if this exercise is part of the curated CrossFit-relevant subset.
    /// Only CrossFit exercises are shown in workout definition and logging flows.
    /// </summary>
    public bool IsCrossFit { get; set; }
}
