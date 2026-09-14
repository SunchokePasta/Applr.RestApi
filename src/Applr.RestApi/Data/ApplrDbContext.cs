using System.Text.Json;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Applr.RestApi.Data;

public sealed class ApplrDbContext(DbContextOptions<ApplrDbContext> options)
    : DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // raw_cells is a MySQL `json` column holding a plain array of
        // strings. Data Annotations alone can't express "serialize this
        // as JSON", so this is the one bit of Fluent API config the
        // model needs -- everything else is handled by attributes on
        // Job itself.
        modelBuilder.Entity<Job>()
            .Property(j => j.RawCells)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null),
                new ValueComparer<List<string>?>(
                    (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                    v => v == null ? 0 : v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
                    v => v == null ? null : v.ToList()));
    }
}
