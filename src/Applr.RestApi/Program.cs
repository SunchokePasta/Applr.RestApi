using Applr.RestApi.Data;
using Applr.RestApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Console + a rolling daily file under ./logs, anchored to the app's own
// content root rather than the process's working directory -- same
// reasoning as TrackrScraper/Scraper.py's logging setup: whatever starts
// this (Task Scheduler, IIS, a plain `dotnet run` from a different cwd)
// shouldn't change where the log file ends up.
var logDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDirectory);

builder.Host.UseSerilog((context, configuration) => configuration
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logDirectory, "log-.txt"),
        rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("JobFinderDb")
    ?? throw new InvalidOperationException(
        "Missing ConnectionStrings:JobFinderDb -- set it in appsettings.Development.json " +
        "(gitignored) or an environment variable before running.");

builder.Services.AddDbContext<ApplrDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IJobRepository, JobRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
