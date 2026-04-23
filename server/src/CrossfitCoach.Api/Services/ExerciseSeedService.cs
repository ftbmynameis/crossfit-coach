using System.Text.Json;
using CrossfitCoach.Api.Data;
using CrossfitCoach.Api.Entities;
using CrossfitCoach.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CrossfitCoach.Api.Services;

/// <summary>
/// Syncs the bundled exercises.json (from free-exercise-db) into the Exercises table on startup.
/// Only runs if the table is empty.
/// </summary>
public static class ExerciseSeedService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Seeds the Exercises table from the bundled exercises.json if it is currently empty.
    /// </summary>
    /// <param name="db">The database context to seed into.</param>
    /// <param name="logger">Logger for progress and warning output.</param>
    public static async Task SeedAsync(CrossfitCoachDbContext db, ILogger logger)
    {
        if (await db.Exercises.AnyAsync())
        {
            logger.LogInformation("Exercise table already populated — skipping seed.");
            return;
        }

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "exercises.json");
        if (!File.Exists(jsonPath))
        {
            logger.LogWarning("exercises.json not found at {Path} — skipping seed.", jsonPath);
            return;
        }

        await using var stream = File.OpenRead(jsonPath);
        var models = await JsonSerializer.DeserializeAsync<ExerciseModel[]>(stream, JsonOptions);

        if (models is null || models.Length == 0)
        {
            logger.LogWarning("exercises.json is empty or could not be parsed — skipping seed.");
            return;
        }

        var exercises = models.Select(m => new ExerciseEntity
        {
            Id = Guid.NewGuid(),
            ExternalId = m.Id,
            Name = m.Name,
            Force = m.Force,
            Level = m.Level,
            Mechanic = m.Mechanic,
            Equipment = m.Equipment,
            PrimaryMuscles = m.PrimaryMuscles,
            SecondaryMuscles = m.SecondaryMuscles,
            Category = m.Category,
            IsCrossFit = false
        }).ToList();

        db.Exercises.AddRange(exercises);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} exercises from exercises.json.", exercises.Count);
    }
}
