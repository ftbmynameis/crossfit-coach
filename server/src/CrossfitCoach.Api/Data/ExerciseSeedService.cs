using System.Text.Json;
using CrossfitCoach.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CrossfitCoach.Api.Data;

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
        var dtos = await JsonSerializer.DeserializeAsync<FreeExerciseDbDto[]>(stream, JsonOptions);

        if (dtos is null || dtos.Length == 0)
        {
            logger.LogWarning("exercises.json is empty or could not be parsed — skipping seed.");
            return;
        }

        var exercises = dtos.Select(dto => new Exercise
        {
            ExternalId = dto.Id,
            Name = dto.Name,
            Force = dto.Force,
            Level = dto.Level,
            Mechanic = dto.Mechanic,
            Equipment = dto.Equipment,
            PrimaryMuscles = dto.PrimaryMuscles,
            SecondaryMuscles = dto.SecondaryMuscles,
            Category = dto.Category,
            IsCrossFit = false
        }).ToList();

        db.Exercises.AddRange(exercises);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} exercises from exercises.json.", exercises.Count);
    }
}
