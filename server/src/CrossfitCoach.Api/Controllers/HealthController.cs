using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CrossfitCoach.Api.Controllers;

/// <summary>
/// Provides a simple health check endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
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
}

/// <param name="AppName">The name of the application.</param>
/// <param name="Version">The current version of the application.</param>
public record HealthResponse(string AppName, string Version);
