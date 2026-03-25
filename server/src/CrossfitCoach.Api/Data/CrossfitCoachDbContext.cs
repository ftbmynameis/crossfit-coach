using Microsoft.EntityFrameworkCore;

namespace CrossfitCoach.Api.Data;

/// <summary>
/// EF Core database context for CrossFit Coach.
/// </summary>
public class CrossfitCoachDbContext : DbContext
{
    /// <summary>
    /// Initialises a new instance of <see cref="CrossfitCoachDbContext"/>.
    /// </summary>
    public CrossfitCoachDbContext(DbContextOptions<CrossfitCoachDbContext> options)
        : base(options)
    {
    }
}
