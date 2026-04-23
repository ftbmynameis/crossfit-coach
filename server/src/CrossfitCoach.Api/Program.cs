using CrossfitCoach.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

if (File.Exists(".env"))
    DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CrossFit Coach API",
        Version = "v1"
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var allowedOrigin = builder.Configuration["ALLOWED_ORIGIN"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var connectionString = builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("DATABASE_URL environment variable is not set.");

var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
builder.Services.AddDbContext<CrossfitCoachDbContext>(options =>
    options.UseNpgsql(dataSource));

var app = builder.Build();

// Apply pending migrations and seed exercises on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CrossfitCoachDbContext>();
    await db.Database.MigrateAsync();

    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<CrossfitCoachDbContext>>();
    await ExerciseSeedService.SeedAsync(db, seedLogger);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
