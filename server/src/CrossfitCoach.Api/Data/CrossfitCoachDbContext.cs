using Microsoft.EntityFrameworkCore;

namespace CrossfitCoach.Api.Data;

public class CrossfitCoachDbContext : DbContext
{
    public CrossfitCoachDbContext(DbContextOptions<CrossfitCoachDbContext> options)
        : base(options)
    {
    }
}
