using CrossfitCoach.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossfitCoach.Api.Controllers;

/// <summary>
/// Provides endpoints for accessing the exercise library.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly CrossfitCoachDbContext _db;

    /// <summary>
    /// Initialises a new instance of <see cref="ExercisesController"/>.
    /// </summary>
    public ExercisesController(CrossfitCoachDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all exercises in the exercise library.
    /// </summary>
    /// <returns>A list of all exercises.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExerciseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var exercises = await _db.Exercises
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseResponse(
                e.Id,
                e.ExternalId,
                e.Name,
                e.Force,
                e.Level,
                e.Mechanic,
                e.Equipment,
                e.PrimaryMuscles,
                e.SecondaryMuscles,
                e.Category,
                e.IsCrossFit))
            .ToListAsync();

        return Ok(exercises);
    }
}

/// <param name="Id">Database primary key.</param>
/// <param name="ExternalId">Stable identifier from free-exercise-db.</param>
/// <param name="Name">Human-readable exercise name.</param>
/// <param name="Force">Movement direction: push, pull, static, or null.</param>
/// <param name="Level">Skill level: beginner, intermediate, or expert.</param>
/// <param name="Mechanic">Compound or isolation, or null.</param>
/// <param name="Equipment">Equipment required.</param>
/// <param name="PrimaryMuscles">Primary muscles targeted.</param>
/// <param name="SecondaryMuscles">Secondary muscles targeted.</param>
/// <param name="Category">Exercise category.</param>
/// <param name="IsCrossFit">True if part of the curated CrossFit-relevant subset.</param>
public record ExerciseResponse(
    Guid Id,
    string ExternalId,
    string Name,
    string? Force,
    string? Level,
    string? Mechanic,
    string? Equipment,
    string[] PrimaryMuscles,
    string[] SecondaryMuscles,
    string Category,
    bool IsCrossFit);
