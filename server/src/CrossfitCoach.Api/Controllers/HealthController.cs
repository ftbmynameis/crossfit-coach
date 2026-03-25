using CrossfitCoach.Api.Data;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CrossfitCoach.Api.Controllers;

/// <summary>
/// Provides health check endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly CrossfitCoachDbContext _db;

    public HealthController(CrossfitCoachDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns the health status of the API, including name and version.
    /// </summary>
    /// <returns>App name and version with a 200 OK status.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "1.0.0";

        return Ok(new HealthResponse("CrossFit Coach", version));
    }

    /// <summary>
    /// Confirms the database connection is healthy.
    /// </summary>
    /// <returns>200 OK if the database is reachable, 503 otherwise.</returns>
    [HttpGet("db")]
    [ProducesResponseType(typeof(DbHealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDb()
    {
        var canConnect = await _db.Database.CanConnectAsync();
        if (!canConnect)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "Database unavailable",
                    Detail = "Could not establish a connection to the database.",
                    Status = StatusCodes.Status503ServiceUnavailable
                });
        }

        return Ok(new DbHealthResponse("healthy"));
    }
}

/// <param name="AppName">The name of the application.</param>
/// <param name="Version">The current version of the application.</param>
public record HealthResponse(string AppName, string Version);

/// <param name="Status">Database connection status.</param>
public record DbHealthResponse(string Status);
